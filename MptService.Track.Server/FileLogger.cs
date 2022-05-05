using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace MptService.Track.Server
{
    class FileLogger : ILogger
    {
        private string _filePath;
        private static object _lock = new object();
        public FileLogger(string path)
        {
            _filePath = path;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter != null)
            {
                lock (_lock)
                {
                    DateTime currentDateTime = DateTime.Now;
                    string filePath = string.Format("{0}_{1:yyyyMMdd}.txt", _filePath, currentDateTime);
                    //File.AppendAllText(filePath, currentDateTime.ToString("hh:mm:ss")  + formatter(state, exception) + Environment.NewLine);
                    File.AppendAllText(filePath, string.Format("{0:HH:mm:ss}: {1}{2}", currentDateTime, formatter(state, exception), Environment.NewLine));
                    //StreamWriter sw = new StreamWriter(filePath);
                    //sw.WriteLine(string.Format("{0:hh:mm:ss}: {1}{2}", currentDateTime, formatter(state, exception), Environment.NewLine));
                    //sw.Flush();
                    //sw.Close();
                }
            }
        }

        public static void Clear(string path)
        {
            DateTime currentTime = DateTime.Now;
            var files = Directory.GetFiles(path);
            foreach (string item in files)
            {
                FileInfo fileInfo = new FileInfo(item);
                TimeSpan timeSpan = currentTime - fileInfo.CreationTime;
                if (timeSpan.TotalDays > 10) // 10 последних дней
                {
                    fileInfo.Delete();
                }
            }
        }      
    }

    public class FileLoggerProvider : ILoggerProvider
    {
        private string _path;
        public FileLoggerProvider(string path)
        {
            _path = path;
        }
        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(_path);
        }

        public void Dispose()
        {
        }
    }

    public static class FileLoggerExtensions
    {
        public static ILoggerFactory AddFile(this ILoggerFactory factory,
                                        string filePath)
        {
            factory.AddProvider(new FileLoggerProvider(filePath));
            return factory;
        }
    }
}
