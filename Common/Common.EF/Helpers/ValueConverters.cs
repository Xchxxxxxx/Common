using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Common.EF.Helpers
{
    public static class ValueConverters
    {
        /// <summary>
        /// 创建枚举到字符串转换器
        /// </summary>
        public static ValueConverter<TEnum, string> CreateEnumToStringConverter<TEnum>()
            where TEnum : Enum
        {
            return new ValueConverter<TEnum, string>(
                v => v.ToString(),
                v => (TEnum)Enum.Parse(typeof(TEnum), v)
            );
        }

        /// <summary>
        /// 创建枚举到数值转换器
        /// </summary>
        public static ValueConverter<TEnum, int> CreateEnumToNumberConverter<TEnum>()
            where TEnum : Enum
        {
            return new ValueConverter<TEnum, int>(
                v => Convert.ToInt32(v),
                v => (TEnum)Enum.ToObject(typeof(TEnum), v)
            );
        }

        /// <summary>
        /// 创建逗号分隔字符串转列表转换器
        /// </summary>
        public static ValueConverter<List<string>, string> CreateCommaSeparatedListConverter()
        {
            return new ValueConverter<List<string>, string>(
                v => string.Join(",", v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            );
        }

        /// <summary>
        /// 创建JSON转换器
        /// </summary>
        public static ValueConverter<T, string> CreateJsonConverter<T>()
        {
            return new ValueConverter<T, string>(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }),
                v => JsonSerializer.Deserialize<T>(v, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) /*?? throw new InvalidOperationException($"Cannot deserialize JSON to {typeof(T).Name}")*/
            );
        }

        /// <summary>
        /// 创建日期到时间戳转换器
        /// </summary>
        public static ValueConverter<DateTime, long> CreateDateTimeToTicksConverter()
        {
            return new ValueConverter<DateTime, long>(
                v => v.Ticks,
                v => new DateTime(v, DateTimeKind.Utc)
            );
        }

        /// <summary>
        /// 创建日期到字符串转换器
        /// </summary>
        public static ValueConverter<DateTime, string> CreateDateTimeToStringConverter(string format = "yyyy-MM-dd HH:mm:ss")
        {
            return new ValueConverter<DateTime, string>(
                v => v.ToString(format),
                v => DateTime.ParseExact(v, format, null)
            );
        }

        /// <summary>
        /// 创建加密转换器
        /// </summary>
        public static ValueConverter<string, string> CreateEncryptedConverter(string encryptionKey)
        {
            return new ValueConverter<string, string>(
                v => Encrypt(v, encryptionKey),
                v => Decrypt(v, encryptionKey)
            );
        }

        private static string Encrypt(string text, string key)
        {
            // 实现你的加密逻辑
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(text));
        }

        private static string Decrypt(string encrypted, string key)
        {
            // 实现你的解密逻辑
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encrypted));
        }
    }
}
