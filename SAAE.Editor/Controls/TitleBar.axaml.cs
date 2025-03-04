﻿using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SAAE.Editor.Controls;

public partial class TitleBar : UserControl {

    public Window Window = null!;
    
    public TitleBar() {
        InitializeComponent();
        DataContext = this;
    }
    
    private void BeginDrag(object? sender, PointerPressedEventArgs e) {
        Window.BeginMoveDrag(e);
    }

    private void Minimize(object? sender, RoutedEventArgs e) {
        if (Design.IsDesignMode) {
            return;
        }

        Window.WindowState = WindowState.Minimized;
    }

    private void Maximize(object? sender, RoutedEventArgs e) {
        if (Design.IsDesignMode) {
            return;
        }

        if (Window.CanResize) {
            Window.WindowState = Window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
    }

    private void Close(object? sender, RoutedEventArgs e) {
        if (Design.IsDesignMode) {
            return;
        }
        
        Window.Close();
    }
}