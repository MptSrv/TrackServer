using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MptService.Track.Server
{
    /// <summary>
    /// Описывает GPS-данные (навигационную отметку) от навигационных устройств
    /// </summary>
    [Table("gps_log")]
    public class GpsDatum
    {      
        [Column("gps_id")]
        public int GpsDatumId { get; private set; }

        [Column("station_id")]
        public Guid StationId { get; private set; }

        [Column("received_time")]
        public DateTime ReceivedTime { get; private set; }

        [Column("latitude")]
        public double Latitude { get; private set; }

        [Column("longitude")]
        public double Longitude { get; private set; }

        [Column("speed")]
        public int Speed { get; private set; }

        /// <summary>
        /// Создает экземпляр навигационной отметки
        /// </summary>
        /// <param name="stationId">ID станции, от которой поступили навигационные данные</param>
        /// <param name="receivedTime">Временная отметка</param>
        /// <param name="latitude">Широта, в градусах</param>
        /// <param name="longitude">Долгота, в градусах</param>
        /// <param name="speed">Скорость, км/ч</param>
        public GpsDatum(Guid stationId, DateTime receivedTime, double latitude, double longitude, int speed)
        {
            StationId = stationId;
            ReceivedTime = receivedTime;
            Latitude = latitude;
            Longitude = longitude;
            Speed = speed;
        }

        public override string ToString()
        {
            return string.Format("StationID = {0}, ReceivedTime = {1}, Latitude = {2}, Longitude = {3}, Speed = {4}", StationId, ReceivedTime, Latitude, Longitude, Speed);
        }        
    }
}
