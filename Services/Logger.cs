using System;
using System.IO;
using System.Threading;

namespace IsleServerLauncher.Services
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }

    public interface ILogger
    {
        void Debug(string message);
        void Info(string message);
        void Warning(string message, Exception? ex = null);
        void Error(string message, Exception? ex = null);
        void Critical(string message, Exception ex);
    }

    public class FileLogger : ILogger
    {
        private readonly string _logFilePath;
        private readonly LogLevel _minLevel;
        private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);

        public FileLogger(string logDirectory, LogLevel minLevel = LogLevel.Info)
        {
            _minLevel = minLevel;
            
            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd");
            _logFilePath = Path.Combine(logDirectory, $"launcher_{timestamp}.log");
        }

        public void Debug(string message) => Log(LogLevel.Debug, message);
        public void Info(string message) => Log(LogLevel.Info, message);
        public void Warning(string message, Exception? ex = null) => Log(LogLevel.Warning, message, ex);
        public void Error(string message, Exception? ex = null) => Log(LogLevel.Error, message, ex);
        public void Critical(string message, Exception ex) => Log(LogLevel.Critical, message, ex);

        private void Log(LogLevel level, string message, Exception? ex = null)
        {
            if (level < _minLevel)
                return;

            try
            {
                _writeLock.Wait();
                try
                {
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    string logEntry = $"[{timestamp}] [{level}] {message}";
                    
                    if (ex != null)
                    {
                        logEntry += $"\n  Exception: {ex.GetType().Name}: {ex.Message}";
                        logEntry += $"\n  StackTrace: {ex.StackTrace}";
                        
                        if (ex.InnerException != null)
                        {
                            logEntry += $"\n  Inner Exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}";
                        }
                    }

                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                }
                finally
                {
                    _writeLock.Release();
                }
            }
            catch
            {
                // Can't log if logging fails - fail silently to prevent app crash
            }
        }
    }
}
