﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models;
using SAAE.Editor.Services;
using SAAE.Editor.Views;
using SAAE.Engine;
using SAAE.Engine.Common;

namespace SAAE.Editor.ViewModels;

public partial class ProjectFileVisualItem : ObservableObject {

    public ProjectFileVisualItem(ProjectFile project, ICommand openCommand) {
        ProjectFile = project;
        OpenCommand = openCommand;
    }
    
    [ObservableProperty] private ProjectFile projectFile;
    [ObservableProperty] private ICommand openCommand;
}

public partial class ProjectSelectionViewModel : BaseViewModel {

    public ProjectSelectionView view = null!; // isso eh feio mas nao quero fazer um role pro filepicker
    private readonly ProjectService projectService = App.Services.GetRequiredService<ProjectService>();

    public bool Cancelled { get; private set; } = false;
    
    [ObservableProperty]
    private string searchQuery = string.Empty;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EmptyRecentProjects))]
    private ObservableCollection<ProjectFileVisualItem> filteredRecentProjects = [];
    private readonly List<ProjectFileVisualItem> allRecentProjects = [];
    
    [ObservableProperty]
    private bool isCreatingProject;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DirectoryNotice))]
    [NotifyPropertyChangedFor(nameof(CanCreateProject))]
    private string newProjectName = string.Empty;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DirectoryNotice))]
    [NotifyPropertyChangedFor(nameof(CanCreateProject))]
    private string newProjectPath = string.Empty;

    private List<OperatingSystemType> allOperatingSystems = null!;
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(HasAvailableOperatingSystems))]
    private ObservableCollection<OperatingSystemType> operatingSystems = [];
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCreateProject))]
    private int selectedOperatingSystemIndex = -1;

    [ObservableProperty] private ObservableCollection<Architecture> isas = [];
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCreateProject))]
    [NotifyPropertyChangedFor(nameof(HasAvailableOperatingSystems))]
    private int selectedIsaIndex = -1;
    
    public bool EmptyRecentProjects => FilteredRecentProjects.Count == 0;
    
    public bool CanCreateProject => !string.IsNullOrWhiteSpace(NewProjectName) 
                                    && !string.IsNullOrWhiteSpace(NewProjectPath)
                                    && SelectedOperatingSystemIndex >= 0
                                    && SelectedIsaIndex >= 0;
    
    public bool HasAvailableOperatingSystems => OperatingSystems.Count > 0;
    

    private readonly TaskCompletionSource<bool> projectSelectionTask = new();
    
    public string DirectoryNotice {
        get {
            string path = Path.Combine(SanitizeProjectPath(NewProjectPath), SanitizeProjectName(NewProjectName));
            return string.Format(Localization.ProjectResources.DirectoryResultNoticeValue, path);
        }
    }

    public ProjectSelectionViewModel() {
        foreach (ProjectFile project in projectService.GetRecentProjects()) {
            filteredRecentProjects.Add(new ProjectFileVisualItem(project, OpenProjectCommand));
            allRecentProjects.Add(new ProjectFileVisualItem(project, OpenProjectCommand));
        }
    }
    
    partial void OnSearchQueryChanged(string value) {
        // atualiza a lista de projetos recentes
        filteredRecentProjects.Clear();
        foreach (ProjectFileVisualItem proj in allRecentProjects) {
            bool nameCheck = proj.ProjectFile.ProjectName.Contains(value, StringComparison.OrdinalIgnoreCase);
            bool pathCheck = proj.ProjectFile.ProjectPath.Contains(value, StringComparison.OrdinalIgnoreCase);
            
            if (nameCheck || pathCheck) {
                filteredRecentProjects.Add(proj);
            }
        }
    }

    public Task WaitForProjectSelection() {
        return projectSelectionTask.Task;
    }
    
    [RelayCommand]
    private void NewProjectStart() {
        IsCreatingProject = true;
        allOperatingSystems = OperatingSystemManager.GetAvailableOperatingSystems().ToList();
        OperatingSystems = new ObservableCollection<OperatingSystemType>(allOperatingSystems);
        Isas = [Architecture.Mips, Architecture.RiscV, Architecture.Arm];
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasAvailableOperatingSystems)));
    }

    [RelayCommand]
    private void NewProjectReturn()
    {
        IsCreatingProject = false;
    }
    
    partial void OnSelectedIsaIndexChanged(int value) {
        if (value < 0 || value >= allOperatingSystems.Count) {
            return;
        }
        Console.WriteLine("Selected ISA changed");
        Architecture isa = Isas[value];
        IEnumerable<OperatingSystemType> oss = allOperatingSystems
            .Where(x => x.CompatibleArchitecture == isa);
        OperatingSystems.Clear();
        foreach (OperatingSystemType os in oss) {
            OperatingSystems.Add(os);
        }
        if (OperatingSystems.Count > 0) {
            SelectedOperatingSystemIndex = 0;
        }else {
            SelectedOperatingSystemIndex = -1;
        }

        Console.WriteLine("Now with {0} OSes and selection at {1}", OperatingSystems.Count, SelectedOperatingSystemIndex);
    }

    [RelayCommand]
    private async Task NewProjectEnd() {
        string path = SanitizeProjectPath(NewProjectPath);
        string name = SanitizeProjectName(NewProjectName);
        string projectFilePath = Path.Combine(path, name, name+".asmproj");
        OperatingSystemType os = OperatingSystems[SelectedOperatingSystemIndex];
        Architecture isa = Isas[SelectedIsaIndex];
        ProjectFile project = await projectService.CreateProjectAsync(projectFilePath, name, os, isa);
        projectService.SetCurrentProject(project);
        IsCreatingProject = false;
        if(!projectSelectionTask.Task.IsCompleted) {
            projectSelectionTask.SetResult(true);
        }
    }
    
    [RelayCommand]
    private async Task OpenProjectDialog() {
        if (!view.StorageProvider.CanOpen) {
            Console.WriteLine("FilePicker nao eh suportado nessa plataforma!");
            return;
        }
        
        IReadOnlyList<IStorageFile> result = await view.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
            Title = Localization.ProjectResources.SelectProjectPickerValue,
            AllowMultiple = false,
            SuggestedFileName = "project.asmproj", 
            FileTypeFilter = [
                new FilePickerFileType(Localization.ProjectResources.AsmProjectsValue) {
                    Patterns = [ "*.asmproj" ], 
                }
            ]
        });

        if (result.Count != 1) {
            return;
        }

        string path = result[0].Path.AbsolutePath;
        await OpenProject(path);
    }
    
    [RelayCommand]
    private async Task OpenProject(string path) {
        if (!path.EndsWith(".asmproj")) {
            // esse check nao precisaria, mas melhor garantir
            return;
        }

        ProjectFile? project = await projectService.OpenProject(path);
        if (project is null) {
            // msg de erro ao usuario
            return;
        }
        projectService.SetCurrentProject(project);
        if(!projectSelectionTask.Task.IsCompleted) {
            projectSelectionTask.SetResult(true);
        }
    }

    private static string SanitizeProjectName(string name) {
        return Path.GetInvalidFileNameChars().Aggregate(name, (current, illegal) => current.Replace(illegal.ToString(), ""));
    }

    private static string SanitizeProjectPath(string path) {
        return Path.GetInvalidPathChars().Aggregate(path, (current, illegal) => current.Replace(illegal.ToString(), ""));
    }
    
    public void OverrideTaskCompletion() {
        Cancelled = true;
        if(!projectSelectionTask.Task.IsCompleted) {
            projectSelectionTask.SetResult(true);
        }
    }
}