﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SAAE.Editor.Models;

namespace SAAE.Editor.ViewModels;

public partial class SplashScreenViewModel : BaseViewModel {

    private const string GithubUrl = "https://github.com/Agentew04/SAAE/raw/refs/heads/clang-bin/";
    private readonly string configurationDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".saae");

    private readonly string userPreferencesPath;

    private UserPreferences preferences;

    public SplashScreenViewModel() {
        userPreferencesPath = Path.Combine(configurationDirectory, "config.json");
    }
    
    public async Task Initialize() {
        if(!Directory.Exists(configurationDirectory) || !File.Exists(userPreferencesPath)) {
            Directory.CreateDirectory(configurationDirectory);
            
            // write default configuration
            StatusText = "Definindo configurações padrão";
            UserPreferences defaultConfig = GetDefaultPreferences();
            await File.WriteAllTextAsync(userPreferencesPath, JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions() {
                WriteIndented = true
            }));
        }
        
        // read stored configuration
        string configJson = await File.ReadAllTextAsync(userPreferencesPath);
        preferences = JsonSerializer.Deserialize<UserPreferences>(configJson) ?? GetDefaultPreferences();

        StatusText = "Checking for compiler...";
        // clang
        (bool hasCompiler, bool hasLinker) = CheckCompiler();
        if (!hasCompiler || !hasLinker) {
            await DownloadCompiler(!hasCompiler, !hasLinker);
        }

        StatusText = "Done!";
        await Task.Delay(1000);
        await Task.Delay(1000);
        await Task.Delay(1000);
    }

    [ObservableProperty]
    private string statusText;

    private (bool hasCompiler, bool hasLinker) CheckCompiler() {
        // TODO: check if compiler is installed e usar o do usuario se possivel
        bool appCompiler = File.Exists(Path.Combine(preferences.CompilerPath, "clang.exe"));
        bool appLinker = File.Exists(Path.Combine(preferences.CompilerPath, "ld.lld.exe"));
        return (appCompiler, appLinker);
    }
    
    private async Task DownloadCompiler(bool getCompiler, bool getLinker) {
        // get structure of remote repo
        StatusText = "Checking compiler availability for current platform";
        using HttpClient http = new();
        string repoStructureJson = await http.GetStringAsync(GithubUrl + "structure.json");
        using JsonDocument repoStructure = JsonDocument.Parse(repoStructureJson);
        string os = OperatingSystem.IsWindows() ? "windows" : "linux";
        string arch = Environment.Is64BitOperatingSystem ? "x64" : "x86";
        
        // get compiler and linker path in remote repo
        JsonElement info;
        try {
            info = repoStructure.RootElement
                .GetProperty(os)
                .GetProperty(arch);
        }
        catch (KeyNotFoundException) {
            // erro, plataforma nao suportada
            // eh disparado em linux 32bits, macos, arm etc
            return;
        }
        
        bool available = info.GetProperty("available").GetBoolean();
        if (!available) {
            // plataforma nao disponivel ainda
            return;
        }
        
        // download
        string? compilerPath = info.GetProperty("clang").GetString();
        string? linkerPath = info.GetProperty("lld").GetString();

        if (compilerPath is null || linkerPath is null) {
            // eh o fim. :(
            return;
        }

        if (compilerPath.StartsWith('/')) {
            compilerPath = compilerPath[1..];
        }
        if (linkerPath.StartsWith('/')) {
            linkerPath = linkerPath[1..];
        }
        compilerPath = GithubUrl + compilerPath;
        linkerPath = GithubUrl + linkerPath;
        
        if (!Directory.Exists(preferences.CompilerPath)) {
            Directory.CreateDirectory(preferences.CompilerPath);
        }
        
        StatusText = "Downloading "+(getCompiler ? "compiler" : "") + (getCompiler && getLinker ? " and " : "") + (getLinker ? "linker" : "");
        TextProgress progress = new() { vm = this };
        Task compilerTask = Task.Run(async () => {
            if (!getCompiler) {
                return;
            }

            MemoryStream ms = new();
            using HttpResponseMessage response =
                await http.GetAsync(compilerPath, HttpCompletionOption.ResponseHeadersRead);
            long? contentLength = response.Content.Headers.ContentLength;
            progress.max += contentLength ?? 0;
            progress.isCompilerUsed = true;

            await using Stream download = await response.Content.ReadAsStreamAsync(default);
            Progress<long> otherProgress = new(progress.Report);
            if (contentLength.HasValue) {
                // posso reportar
                await download.CopyToAsync(ms, 81920, otherProgress);
                progress.Report(1);
            }
            else {
                // n sei o tamanho total, so faz o download
                await download.CopyToAsync(ms);
            }

            ms.Seek(0, SeekOrigin.Begin);
            using ZipArchive archive = new(ms, ZipArchiveMode.Read);
            ZipArchiveEntry? entry = archive.GetEntry("clang.exe");
            if (entry is null) {
                return;
            }
            await using Stream entryStream = entry.Open();
            progress.max += entry.Length;
            progress.isCompilerDownloading = false;
            await using var fs = new FileStream(Path.Combine(preferences.CompilerPath, "clang.exe"),FileMode.OpenOrCreate);
            await entryStream.CopyToAsync(fs, 81920, otherProgress);
            progress.isCompilerDownloading = true;
        });
        Task linkerTask = Task.Run(async () => {
            if (!getLinker) {
                return;
            }
            
            using MemoryStream ms = new();
            using HttpResponseMessage response =
                await http.GetAsync(linkerPath, HttpCompletionOption.ResponseHeadersRead);
            long? contentLength = response.Content.Headers.ContentLength;
            progress.max += contentLength ?? 0;
            progress.isLinkerUsed = true;

            await using Stream download = await response.Content.ReadAsStreamAsync(default);
            Progress<long> otherProgress = new(progress.Report);
            if (contentLength.HasValue) {
                // posso reportar
                await download.CopyToAsync(ms, 81920, otherProgress);
                progress.Report(1);
            }
            else {
                // n sei o tamanho total, so faz o download
                await download.CopyToAsync(ms);
            }

            ms.Seek(0, SeekOrigin.Begin);
            using ZipArchive archive = new(ms, ZipArchiveMode.Read);
            ZipArchiveEntry? entry = archive.GetEntry("ld.lld.exe");
            if (entry is null) {
                return;
            }
            progress.max += entry.Length;
            progress.isLinkerDownloading = false;
            await using Stream entryStream = entry.Open();
            await using var fs = new FileStream(Path.Combine(preferences.CompilerPath, "ld.lld.exe"),FileMode.OpenOrCreate);
            await entryStream.CopyToAsync(fs, 81920, otherProgress);
            progress.isLinkerDownloading = true;
        });

        await Task.WhenAll(compilerTask, linkerTask);
        StatusText = "Download finished";
    }
    
    private UserPreferences GetDefaultPreferences() => new UserPreferences() {
        CompilerPath = Path.Combine(configurationDirectory, "compiler")
    };

    private class TextProgress : IProgress<long> {

        public SplashScreenViewModel vm;
        public long max = 0;
        public bool isCompilerUsed = false;
        public bool isLinkerUsed = false;
        public bool isCompilerDownloading = true;
        public bool isLinkerDownloading = true;

        private int smoothingValues = 30;
        private List<double> values = new();
        
        public void Report(long value) {
            double relative = (double)value / (double)max;
            if (values.Count < smoothingValues) {
                values.Add(relative);
            }
            else {
                values.RemoveAt(0);
                values.Add(relative);
            }
            
            double smoothed = values.Average();
            
            vm.StatusText = ((isCompilerUsed && isCompilerDownloading) || (isLinkerUsed && isLinkerDownloading) ? "Downloading " : "Extracting ") + smoothed.ToString("P");
        }
    }
    
    
}