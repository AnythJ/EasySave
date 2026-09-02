using Avalonia.Media;

namespace EasySave.Services
{
    public record SyncStatus(string Text, IBrush Color);

    public static class SyncStatusEvaluator
    {
        private static readonly IBrush UpToDateColor = new SolidColorBrush(Color.Parse("#4FBF8F"));
        private static readonly IBrush OutdatedColor = new SolidColorBrush(Color.Parse("#E0667A"));
        private static readonly IBrush UnknownColor = Brushes.Gray;

        public static SyncStatus Evaluate(string? driveLatestFileId, string? lastKnownFileId)
        {
            if (driveLatestFileId == null)
                return new SyncStatus(string.Empty, UnknownColor);

            return driveLatestFileId == lastKnownFileId
                ? new SyncStatus("Up to date", UpToDateColor)
                : new SyncStatus("Outdated - newer version on Drive", OutdatedColor);
        }
    }
}
