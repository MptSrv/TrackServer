using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MptService.Track.Server.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MptService.Track.Server
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private readonly ILogger _fileLogger;

        private readonly ApplicationContext _applicationContext;

        private readonly string _logDirectory;

        /// <summary>
        /// Обработчик входящих UDP-сообщений от навигационных контроллеров
        /// </summary>
        private UdpReceiver _udpReceiver;

        public Worker(ILogger<Worker> logger, ApplicationContext applicationContext, ILoggerFactory loggerFactory)
        {
            _logger = logger;

            var companyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MptService");
            _logDirectory = Path.Combine(companyPath, "TrackServer");
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
            var logFilePath = Path.Combine(_logDirectory, "Log");

            
            loggerFactory.AddFile(logFilePath);            
            _fileLogger = loggerFactory.CreateLogger("FileLogger");
            //_fileLogger.LogInformation(companyPath);

            _applicationContext = applicationContext;            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                FileLogger.Clear(_logDirectory);
                _logger.LogInformation("Logs cleared at: {time}", DateTimeOffset.Now);
                //_fileLogger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken); // TODO: периодические действия?     
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            // Инициализация и запуск
            _udpReceiver = new UdpReceiver(_applicationContext, _logger);
            _udpReceiver.Start();                       

            _logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);
            //_fileLogger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _udpReceiver.Stop();       

            _logger.LogInformation("Worker stopped at: {time}", DateTimeOffset.Now);
            //_fileLogger.LogInformation("Worker stopped at: {time}", DateTimeOffset.Now);
            return base.StopAsync(cancellationToken);
        }
    }
}
