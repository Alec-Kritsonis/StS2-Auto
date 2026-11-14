using System;
using System.IO;
using Godot;

namespace StS2_Auto.StS2_AutoCode.Logging;

internal enum AutoPlayLogLevel
{
    Info,
    Debug
}

internal static class AutoPlayLogger
{
    private static readonly object Sync = new();
    private static StreamWriter? _writer;
    private static AutoPlayLogLevel _logLevel = AutoPlayLogLevel.Info;

    public static string LogPath { get; } = Path.Combine(
        System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
        "SlayTheSpire2",
        "logs",
        "Sts2_Auto",
        "auto.log");

    public static void Initialize()
    {
        lock (Sync)
        {
            if (_writer != null)
                return;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
                _writer = new StreamWriter(
                    new FileStream(LogPath, FileMode.Create, System.IO.FileAccess.Write, FileShare.Read))
                {
                    AutoFlush = true
                };
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                GD.PrintErr($"Could not open log file '{LogPath}': {ex}");
                return;
            }
        }

        postInfo("Logger initialized.");
        postInfo($"Log file: {LogPath}");
    }

    public static void postInfo(string message) => Write("INFO", message);

    public static void postDebug(string message)
    {
        if (_logLevel == AutoPlayLogLevel.Debug)
            Write("DEBUG", message);
    }

    public static void postWarn(string message) => Write("WARN", message);

    public static void postError(string message) => Write("ERROR", message);

    public static void CycleLogLevel()
    {
        _logLevel = _logLevel == AutoPlayLogLevel.Info
            ? AutoPlayLogLevel.Debug
            : AutoPlayLogLevel.Info;
        postInfo($"Log level set to {_logLevel.ToString().ToUpperInvariant()} (F9 to cycle).");
    }

    private static void Write(string level, string message)
    {
        lock (Sync)
        {
            if (_writer == null)
            {
                GD.PrintErr($"Logger is unavailable; message not written: {message}");
                return;
            }

            try
            {
                _writer.WriteLine(
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}");
            }
            catch (Exception ex) when (ex is IOException or ObjectDisposedException)
            {
                GD.PrintErr($"Failed writing to '{LogPath}': {ex}");
                _writer.Dispose();
                _writer = null;
            }
        }
    }

    public static void WriteSeparator(string label)
    {
        postInfo($"==================== {label} ====================");
    }
}
