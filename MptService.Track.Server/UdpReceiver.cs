using Microsoft.Extensions.Logging;
using MptService.Track.Server.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MptService.Track.Server
{
    /// <summary>
    /// Получатель навигационных UDP-сообщений
    /// </summary>
    class UdpReceiver
    {
        private UdpClient _udpClient;
        private CancellationTokenSource _tokenSource;
        private Task _task;
        private ApplicationContext _applicationContext;
        private List<Station> _stations;

        /// <summary>
        /// Сокет для приема UDP-пакетов
        /// </summary>
        private readonly IPEndPoint _endPoint = new IPEndPoint(IPAddress.Any, 4100);

        /// <summary>
        /// Минимальная длина валидного пакета
        /// </summary>
        private const int _minPacketLength = 8;

        private ILogger _logger;

        /// <summary>
        /// Произошла критическая ошибка при прослушивании UDP-порта
        /// </summary>
        public bool IsCrashed { get; private set; }
        
        public UdpReceiver(ApplicationContext applicationContext, ILogger logger)
        {
            _udpClient = new UdpClient(_endPoint);
            _tokenSource = new CancellationTokenSource();
            _task = new Task(() => Receive(_tokenSource.Token), TaskCreationOptions.LongRunning); // выделяем прослушивание UDP-порта в отдельный Task (поток)
            //UdpInit();

            _applicationContext = applicationContext;
            _stations = _applicationContext.Stations.ToList(); // получаем [однократно, при запуске] список станций
                                                               // 
            _logger = logger;
            //_logger.LogInformation("UdpReceiver - constructor");
        }

        //private void UdpInit()
        //{        
        //    if (_udpClient != null)
        //    {
        //        _udpClient.Close();                
        //        _udpClient = null;
        //    }
        //    _udpClient = new UdpClient(_endPoint);
        //    _tokenSource = new CancellationTokenSource();
        //    _task = new Task(() => Receive(_tokenSource.Token), TaskCreationOptions.LongRunning); // выделяем прослушивание UDP-порта в отдельный Task (поток)
        //}

        public void Start()
        {            
            _task.Start();
            _logger.LogInformation("UdpReceiver => Start");
        }

        public void Stop()
        {
            _logger.LogInformation("UdpReceiver => Stop");
            _tokenSource.Cancel();
            _udpClient.Close();            
        }

        private void Receive(CancellationToken token)
        {            
            IPEndPoint remoteEndPoint = null;
            try
            {
                while (!token.IsCancellationRequested)
                { 
                    // =================================
                    //if (DateTime.Now.Minute % 5 == 0)
                    //{
                    //    throw new Exception("Test Exception");
                    //}
                    // =================================
                    byte[] bytes = _udpClient.Receive(ref remoteEndPoint);                    

                    if (!UdpPacketHandler.IsChecksumCorrect(bytes) || bytes.Length < _minPacketLength)
                    {
                        _logger.LogInformation("Ошибка в контрольной сумме (или пакет неверного формата)");
                        continue;
                    }

                    var mpt1327 = UdpPacketHandler.GetMptIdentifier1327(bytes);
                    //_logger.LogInformation(ConvertUtilities.ByteArrayToString(bytes) + $" ({mpt1327.prefix}, {mpt1327.number})");
                    //_logger.LogInformation($"Получен пакет от ({mpt1327.prefix}, {mpt1327.number})");
                    string logline = ConvertUtilities.ByteArrayToString(bytes) + $" ({mpt1327.prefix}, {mpt1327.number})";
                    var station = _stations.FirstOrDefault(t => t.Prefix1327 == mpt1327.prefix && t.Number1327 == mpt1327.number);

                    if (UdpPacketHandler.IsTait(bytes))
                    {                        
                        if (station == null)
                        {
                            // Console.WriteLine($"UNKNOWN station: Prefix1327 = {mpt1327.prefix}, Number1327 {mpt1327.number}");
                            //_logger.LogWarning($"UNKNOWN station: Prefix1327 = {mpt1327.prefix}, Number1327 = {mpt1327.number}");
                            logline += $" [UNKNOWN station: Prefix1327 = {mpt1327.prefix}, Number1327 = {mpt1327.number}]";
                            _logger.LogInformation(logline);
                            if (mpt1327.number >= 2300 && mpt1327.number <= 2999) // номер флота 3150
                            {
                                int prefix = mpt1327.prefix + 200;
                                int number = mpt1327.number - 2100; // IBI для флота 3150
                                Station unknownStation = new Station(Guid.Empty, prefix, 3150, number, $"Tait {number} (Флот 3150)", 4); // 4 - номер подразделения "Флот 3150"
                                _applicationContext.Stations.Add(unknownStation);
                                _applicationContext.SaveChanges();

                                _stations = _applicationContext.Stations.ToList(); // получаем [однократно, при обновлении] список станций
                            }                            
                            continue;
                        }

                        GpsDatum gps = UdpPacketHandler.GetGpsDatum(station.StationId, bytes);

                        _applicationContext.GpsData.Add(gps);
                        _applicationContext.SaveChanges();

                        //_logger.LogInformation("TAIT");
                        logline += " [TAIT]";
                    }
                    else if (bytes.Length == 10 && bytes[8] == 0x15)
                    {                        
                        logline += " [MAN DOWN]";
                        Alarm alarm = new Alarm(0, station.StationId, DateTime.UtcNow, 1); // alarmType=1 => ManDown
                        _applicationContext.Alarms.Add(alarm);
                        _applicationContext.SaveChanges();
                    }

                    _logger.LogInformation(logline);
                }
            }
            catch (Exception ex)
            {
                // Console.WriteLine("Receive error: " + ex.Message);
                _logger.LogError("ERROR (UdpReceiver.Receive): " + ex.Message);

                IsCrashed = !token.IsCancellationRequested;
            }
            finally
            {
                _udpClient.Close(); // TODO?
            }
        }        
    }
}
