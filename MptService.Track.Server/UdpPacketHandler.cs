using System;
using System.Collections.Generic;
using System.Text;

namespace MptService.Track.Server
{
    /// <summary>
    /// Обработчик UDP-пакетов
    /// </summary>
    static class UdpPacketHandler
    {
        /// <summary>
        /// Коэффициент для преобразования морских миль в километры
        /// </summary>
        private const double _nauticalMileCoefficient = 1.852;

        /// <summary>
        /// Является ли пакет сообщением формата радиостанции Tait
        /// </summary>
        /// <param name="data">сообщение</param>
        /// <returns></returns>
        public static bool IsTait(byte[] data)
        {
            return (data.Length == 31) && (data[8] == 0x0C) && (data[9] == 0x10) && (data[10] == 0x07);
        }

        /// <summary>
        /// Получение идентификатора радиостанции в формате MPT-1327 (префикс, номер)
        /// </summary>
        /// <param name="data">Навигационное сообщение</param>
        /// <returns></returns>
        public static (int prefix, int number) GetMptIdentifier1327(byte[] data)
        {
            int prefix = data[3];
            int number = (data[7] << 8) + data[6];

            return (prefix, number);
        }        

        /// <summary>
        /// Создает экземпляр навигационной отметки
        /// </summary>
        /// <param name="stationId">ID радиостанции, от которой поступили навигационные данные</param>
        /// <param name="data">Навигационное сообщение</param>
        /// <returns></returns>
        public static GpsDatum GetGpsDatum(Guid stationId, byte[] data)
        {
            // примера пакета данных: 3A 1F 02 00 16 01 63 02 0C 10 07 4E B0 4C 8F C4 06 A0 00 00 00 00 00 00 21 00 A2 A3 FF A7 B7

            int latitudeDegree = ((data[11] & 0x1F) << 2) + ((data[12] & 0xC0) >> 6);
            int latitudeMinute = (data[12] & 0x3F);
            int latitudeKiloMinute = (data[13] << 2) + ((data[14] & 0xC0) >> 6);
            double latitudeSecond = (latitudeKiloMinute / 1000.0) * 60;
            double latitude = latitudeDegree + latitudeMinute / 60.0 + latitudeSecond / 3600.0;

            if ((data[11] & 0x20) > 0)
            {
                latitude *= -1;
            }

            int longitudeDegree = ((data[14] & 0x1F) << 3) + ((data[15] & 0xE0) >> 5);
            int longitudeMinute = ((data[15] & 0x1F) << 1) + ((data[16] & 0x80) >> 7);
            int longitudeKiloMinute = ((data[16] & 0x7F) << 3) + ((data[17] & 0xE0) >> 5);
            double lonSeconds = (longitudeKiloMinute / 1000.0) * 60;
            double longitude = longitudeDegree + longitudeMinute / 60.0 + lonSeconds / 3600.0;
            if ((data[14] & 0x20) > 0)
            {
                longitude *= -1;
            }

            int speed = ((data[18] & 0x0F) << 4) + ((data[19] & 0xF0) >> 4);
            speed = (int)(speed * _nauticalMileCoefficient);

            return new GpsDatum(stationId, DateTime.UtcNow, latitude, longitude, speed);
        }

        #region Проверка контрольной суммы
        /// <summary>
        /// Верна ли контрольная сумма
        /// </summary>
        /// <param name="data">массив байтов</param>
        /// <returns></returns>
        public static bool IsChecksumCorrect(byte[] data)
        {            
            // Контрольная сумма байтов должна быть равна 0 по модулю 256
            return GetChecksum(data, 0, data.Length) == 0x00;
        }

        /// <summary>
        /// Функция вычисления контрольной суммы байт по модулю 256.
        /// </summary>
        /// <param name="data">массив данных</param>
        /// <param name="offset">смещение в массиве</param>
        /// <param name="count">длина сообщения</param>
        /// <returns></returns>
        private static byte GetChecksum(byte[] data, int offset, int count)
        {
            byte sum = 0;
            for (int i = offset; i < count; i++)
                sum += data[i];
            return sum;
        }
        #endregion
    }
}
