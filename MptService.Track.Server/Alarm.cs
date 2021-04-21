using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MptService.Track.Server
{
    /// <summary>
    /// Описывает сигнал оповещения ("Man Down" и т.п.)
    /// </summary>
    [Table("alarms")]
    public class Alarm
    {
        [Column("alarm_id")]
        public int AlarmId { get; private set; }

        [Column("station_id")]
        public Guid StationId { get; private set; }

        [Column("received_time")]
        public DateTime ReceivedTime { get; private set; }

        [Column("alarm_type")]
        public int AlarmType { get; private set; }

        public Alarm(int alarmId, Guid stationId, DateTime receivedTime, int alarmType)
        {
            AlarmId = alarmId;
            StationId = stationId;
            ReceivedTime = receivedTime;
            AlarmType = alarmType;
        }
    }
}
