using System;
using System.Collections.Generic;
using System.Text;

namespace MptService.Track.Server
{
    /// <summary>
    /// Вспомогательные функции для преобразования отображения объектов
    /// </summary>
    static class ConvertUtilities
    {        
        /// <summary>
        /// Преобразование массива байтов в строковое представление hex-формата
        /// </summary>
        /// <param name="byteArray">массив байтов</param>
        /// <returns></returns>
        public static string ByteArrayToString(byte[] byteArray)
        {
            string result = "";           
            for (int i = 0; i < byteArray.Length; i++)
                result += string.Format("{0:X2} ", byteArray[i]);
            return result;
        }
    }
}
