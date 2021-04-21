using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MptService.Track.Server
{
    /// <summary>
    /// Описывает радиостанцию
    /// </summary>
    [Table("stations")]
    public class Station
    {
        [Column("station_id")]
        public Guid StationId { get; private set; }

        [Column("mpt_prefix")]
        public int Prefix { get; private set; }

        [Column("mpt_fleet")]
        public int Fleet { get; private set; }

        [Column("mpt_number")]
        public int Number { get; private set; }

        [Column("title")]
        public string Title { get; private set; }

        [Column("division_id")]
        public int? DivisionId { get; private set; }

        public Station(Guid stationId, int prefix, int fleet, int number, string title, int? divisionId)
        {
            StationId = stationId;
            Prefix = prefix;
            Fleet = fleet;
            Number = number;
            Title = title;
            DivisionId = divisionId;
        }

        /// <summary>
        /// Префикс радиостанции в формате MPT-1327
        /// </summary>
        [NotMapped]       
        public int Prefix1327
        {
            get
            {
                return Prefix - 200;
            }
        }

        /// <summary>
        /// Номер радиостанции в формате MPT-1327
        /// </summary>
        [NotMapped]
        public int Number1327
        {
            get
            {
                int IBI = (Fleet - 2000) * 2;
                return Number + IBI - 200;
            }
        }

        //public Station(Guid stationId, int prefix, int fleet, int number, string title, int? divisionId)
        //{
        //    StationId = stationId;
        //    Prefix = prefix;
        //    Fleet = fleet;
        //    Number = number;
        //    Title = title;
        //    DivisionId = divisionId;
        //}

        public override string ToString()
        {
            return $"{Title} ({Prefix}/{Fleet}/{Number}) [{StationId}]";            
        }

        //public static List<Station> Init()
        //{
        //    var result = new List<Station>();

            
        //    for (int i = 203; i <= 210; i++)
        //    {
        //        result.Add(new Station(0, 200, 2450, i, $"Tait {i}", 1));
        //    }

        //    for (int i = 282; i <= 313; i++)
        //    {
        //        result.Add(new Station(0, 200, 2450, i, $"Tait {i}", 1));
        //    }

        //    for (int i = 500; i <= 587; i++)
        //    {
        //        result.Add(new Station(0, 200, 2450, i, $"Tait {i}", 1));
        //    }

        //    for (int i = 611; i <= 639; i++)
        //    {
        //        result.Add(new Station(0, 200, 2450, i, $"Tait {i}", 1));
        //    }

        //    for (int i = 700; i <= 709; i++)
        //    {
        //        result.Add(new Station(0, 200, 2450, i, $"Tait {i}", 2));
        //    }
        //    for (int i = 840; i <= 858; i++)
        //    {
        //        result.Add(new Station(0, 200, 2450, i, $"Tait {i}", 2));
        //    }

        //    for (int i = 600; i <= 610; i++)
        //    {
        //        result.Add(new Station(0, 200, 2450, i, $"Tait {i}", 3));
        //    }

        //    result.Add(new Station(0, 200, 2450, 710, $"Tait {710}", 3));
        //    result.Add(new Station(0, 200, 2450, 777, $"Tait {777}", 3));
        //    result.Add(new Station(0, 200, 2450, 839, $"Tait {839}", 3));

        //    for (int i = 859; i <= 898; i++)
        //    {
        //        result.Add(new Station(0, 200, 2450, i, $"Tait {i}", 3));
        //    }
        //    result.Add(new Station(0, 200, 2450, 200, $"Tait {200}", 3));

        //    return result;
        //}
    }
}
