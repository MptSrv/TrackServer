using MptService.Track.Server.Data;
using System;
using System.Collections.Generic;
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
        
        public UdpReceiver(ApplicationContext applicationContext)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 4100);
            _udpClient = new UdpClient(endPoint);
            _tokenSource = new CancellationTokenSource();
            _task = new Task(() => Receive(_tokenSource.Token), TaskCreationOptions.LongRunning); // выделяем прослушивание UDP-порта в отдельный Task (поток)
            _applicationContext = applicationContext;
            _stations = _applicationContext.Stations.ToList(); // получаем [однократно, при запуске] список станций
        }

        public void Start()
        {            
            _task.Start();
        }

        public void Stop()
        {
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
                    byte[] bytes = _udpClient.Receive(ref remoteEndPoint);

                    if (!UdpPacketHandler.IsChecksumCorrect(bytes) || !UdpPacketHandler.IsTait(bytes))
                        continue;

                    var mpt1327 = UdpPacketHandler.GetMptIdentifier1327(bytes);
                    
                    var station = _stations.FirstOrDefault(t => t.Prefix1327 == mpt1327.prefix && t.Number1327 == mpt1327.number);
                    if (station == null)
                    {                        
                        // Console.WriteLine($"UNKNOWN station: Prefix1327 = {mpt1327.prefix}, Number1327 {mpt1327.number}");
                        continue;
                    }
                   
                    GpsDatum gps = UdpPacketHandler.GetGpsDatum(station.StationId, bytes);
                    
                    _applicationContext.GpsData.Add(gps);
                    _applicationContext.SaveChanges();

                    // Console.WriteLine($"Received: {gps} from {remoteEndPoint.Address}");                    
                }
            }
            catch (Exception ex)
            {
                // Console.WriteLine("Receive error: " + ex.Message);
            }
            finally
            {
                // Console.WriteLine("And finally!");                
                _udpClient.Close(); // TODO?
            }
        }        
    }
}
