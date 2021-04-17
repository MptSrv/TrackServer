using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MptService.Track.Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MptService.Track.Server
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private readonly ApplicationContext _applicationContext;

        /// <summary>
        /// Обработчик входящих UDP-сообщений от навигационных контроллеров
        /// </summary>
        private UdpReceiver _udpReceiver;

        public Worker(ILogger<Worker> logger, ApplicationContext applicationContext)
        {
            _logger = logger;

            _applicationContext = applicationContext;            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(Timeout.Infinite, stoppingToken); // TODO: периодические действия?     
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            // Инициализация и запуск
            _udpReceiver = new UdpReceiver(_applicationContext);
            _udpReceiver.Start();                       

            _logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);
            
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _udpReceiver.Stop();       

            _logger.LogInformation("Worker stopped at: {time}", DateTimeOffset.Now);
            return base.StopAsync(cancellationToken);
        }
    }
}
