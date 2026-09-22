using CommunityToolkit.Mvvm.Input;
using H.NotifyIcon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using YTM_Presence.Handler;

namespace YTM_Presence;

public sealed partial class TrayIcon : UserControl, INotifyPropertyChanged
{
    // normal WinRT-friendly property with explicit INotifyPropertyChanged implementation
    private bool _isStartupEnabled;
    private readonly TaskbarIcon _taskbarIcon;
    public bool IsStartupEnabled        
    {
        get => _isStartupEnabled;
        set => SetProperty(ref _isStartupEnabled, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
        storage = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    public TrayIcon()
    {
        InitializeComponent();
        _taskbarIcon = TaskbarIcon;
        _ = LoadStartupState();
    }

    private async Task LoadStartupState()
    {
        var startupTask = await StartupTask.GetAsync("YTMPresenceStartup");
        IsStartupEnabled = startupTask.State == StartupTaskState.Enabled;
    }

    // [RelayCommand] automatycznie tworzy publiczną komendę o nazwie OpenWindowCommand
    [RelayCommand]
    public void OpenWindow()
    {
        var wnd = App.MainWindow;
        if (wnd == null) return;

        var dq = wnd.DispatcherQueue;
        if (dq == null) return;

        dq.TryEnqueue(() =>
        {
            try
            {
                if (wnd.Visible)
                    return;
                wnd.Activate();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        });
    }

    // [RelayCommand] tworzy komendę ToggleStartupCommand. Obsługuje asynchroniczność (Task).
    [RelayCommand]
    public async Task ToggleStartup()
    {
        Debug.WriteLine("te31452t");

        if (!IsStartupEnabled)
        {
            await StartupManager.DisableStartup();
        }
        else
        {
            await StartupManager.EnableStartup();
        }

        var startupState = await StartupManager.GetState();
        IsStartupEnabled = startupState == StartupTaskState.Enabled;
    }

    // [RelayCommand] tworzy komendę ExitApplicationCommand
    [RelayCommand]
    public void ExitApplication()
    {
        App.Cleanup();
        Dispose();
        Application.Current.Exit();
    }

    public void Dispose()
    {
        _taskbarIcon?.Dispose();
    }
}