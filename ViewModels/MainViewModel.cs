using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Helpers;
using EasySave.Models;
using EasySave.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EasySave.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly SaveSyncManager _syncService;
    private readonly ConfigService _configService = new();
    private readonly IFilePickerService _filePickerService;

    private string? _folderId;

    [ObservableProperty]
    private ObservableCollection<GameConfig> games = new();

    [ObservableProperty]
    private GameConfig? selectedGame;

    [ObservableProperty]
    private string newGameName = string.Empty;

    [ObservableProperty]
    private string statusText = "Ready";

    [ObservableProperty]
    private ObservableCollection<Google.Apis.Drive.v3.Data.File> recentSaves = new();

    [ObservableProperty]
    private ObservableCollection<string> selectedFiles = new();

    [ObservableProperty]
    private string downloadDestinationPath = string.Empty;

    [ObservableProperty]
    private string syncStatusText = string.Empty;

    [ObservableProperty]
    private IBrush syncStatusColor = Brushes.Gray;

    public MainViewModel(SaveSyncManager syncService, IFilePickerService filePickerService)
    {
        _syncService = syncService;
        _filePickerService = filePickerService;
        Games = new ObservableCollection<GameConfig>(_configService.Load());
        if (Games.Count > 0)
            SelectedGame = Games[0];
    }

    partial void OnSelectedGameChanged(GameConfig? value)
    {
        _ = OnGameChangedAsync(value);
    }

    private async Task OnGameChangedAsync(GameConfig? game)
    {
        if (game == null) return;

        DownloadDestinationPath = game.DownloadDestinationPath;
        SelectedFiles = new ObservableCollection<string>(game.LastSelectedFiles);

        try
        {
            StatusText = "Loading...";
            _folderId = await _syncService.GetOrCreateFolderAsync(game.Name);
            await RefreshRecentSaves();
            StatusText = "Ready";
        }
        catch (Exception ex)
        {
            StatusText = $"Failed to load game: {ex.Message}";
        }

        UpdateSyncStatus();
    }

    [RelayCommand]
    private async Task PickFiles()
    {
        var files = await _filePickerService.PickFilesAsync("Select Save Files");
        SelectedFiles = new ObservableCollection<string>(files);

        if (SelectedGame != null)
        {
            SelectedGame.LastSelectedFiles = SelectedFiles.ToList();
            SaveConfig();
        }

        UpdateSyncStatus();
    }

    [RelayCommand]
    private async Task PickDownloadFolder()
    {
        var folder = await _filePickerService.PickFolderAsync("Select Download Destination");
        if (folder == null) return;

        DownloadDestinationPath = folder;

        if (SelectedGame != null)
        {
            SelectedGame.DownloadDestinationPath = DownloadDestinationPath;
            SaveConfig();
        }

        UpdateSyncStatus();
    }

    [RelayCommand]
    private void AddGame()
    {
        if (string.IsNullOrWhiteSpace(NewGameName)) return;

        var game = new GameConfig { Name = NewGameName };
        Games.Add(game);
        SaveConfig();
        NewGameName = string.Empty;
        SelectedGame = game;
    }

    [RelayCommand]
    private async Task Upload()
    {
        if (SelectedGame == null) { StatusText = "Select a game first"; return; }
        if (SelectedFiles.Count == 0) { StatusText = "No files selected"; return; }

        try
        {
            StatusText = "Uploading...";
            var fileId = await _syncService.UploadSaveAsync(SelectedFiles.ToList(), _folderId!, SelectedGame.Name);
            SelectedGame.LastKnownDriveFileId = fileId;
            SaveConfig();
            StatusText = "Upload complete";
            await RefreshRecentSaves();
        }
        catch (Google.GoogleApiException ex) { StatusText = $"Upload failed: {GoogleErrorHelper.Describe(ex)}"; }
        catch (IOException) { StatusText = "Upload failed: file in use or inaccessible"; }
        catch (Exception ex) { StatusText = $"Upload failed: {ex.Message}"; }
    }

    [RelayCommand]
    private async Task DownloadLatest()
    {
        if (RecentSaves.Count == 0) { StatusText = "No saves available"; return; }
        if (string.IsNullOrEmpty(DownloadDestinationPath)) { StatusText = "Pick a download folder first"; return; }

        if (Directory.Exists(DownloadDestinationPath) && Directory.GetFileSystemEntries(DownloadDestinationPath).Length > 0)
        {
            var owner = (App.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            var dialog = new EasySave.Views.ConfirmDialog($"This will overwrite matching files\n\nContinue?");
            await dialog.ShowDialog(owner!);
            if (!dialog.Result) { StatusText = "Download cancelled"; return; }
        }

        try
        {
            StatusText = "Downloading...";
            await _syncService.DownloadSaveAsync(RecentSaves[0].Id, DownloadDestinationPath);
            SelectedGame!.LastKnownDriveFileId = RecentSaves[0].Id;
            SaveConfig();
            StatusText = "Download complete";
        }
        catch (Google.GoogleApiException ex) { StatusText = $"Download failed: {GoogleErrorHelper.Describe(ex)}"; }
        catch (Exception ex) { StatusText = $"Download failed: {ex.Message}"; }

        UpdateSyncStatus();
    }

    private async Task RefreshRecentSaves()
    {
        var saves = await _syncService.GetRecentSavesAsync(_folderId!, 5);
        RecentSaves = new ObservableCollection<Google.Apis.Drive.v3.Data.File>(saves);
        UpdateSyncStatus();
    }

    private void UpdateSyncStatus()
    {
        var driveLatestId = RecentSaves.Count > 0 ? RecentSaves[0].Id : null;
        var status = SyncStatusEvaluator.Evaluate(driveLatestId, SelectedGame?.LastKnownDriveFileId);
        SyncStatusText = status.Text;
        SyncStatusColor = status.Color;
    }

    private void SaveConfig() => _configService.Save(Games.ToList());
}
