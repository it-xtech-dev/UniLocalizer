using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UniLocalizer.Localizer.Model;

namespace UniLocalizer.Extensions
{
    /// <summary>
    /// Localization extensions
    /// TODO: can be moved / refactored to more general namespace.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Safely gets dictionary value (without throwing an exception when item not found).
        /// </summary>
        /// <typeparam name="TK">Dictionary key type.</typeparam>
        /// <typeparam name="TV">Dictioanry value type</typeparam>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The key to be searched inside dictionary</param>
        /// <param name="defaultValue">The default value that will be returned when key not found inside dictionary.</param>
        /// <returns></returns>
        public static TV GetValue<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV defaultValue = default(TV))
        {
            TV value;
            return dict.TryGetValue(key, out value) ? value : defaultValue;
        }

        /// <summary>
        /// Gets MD5 hash for given string.
        /// </summary>
        /// <param name="inputString">String to be hashed</param>
        /// <returns>Hased string as array of bytes</returns>
        public static byte[] GetHash(this string inputString)
        {
            HashAlgorithm algorithm = MD5.Create();  //or use SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        /// <summary>
        /// Gets MD5 hash for given string.
        /// </summary>
        /// <param name="inputString">String to be hashed</param>
        /// <returns>Hased string HEX text</returns>
        public static string GetHashString(this string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
    }
}
