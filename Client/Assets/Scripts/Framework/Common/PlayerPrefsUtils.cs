// author:KIPKIPS
// describe:封装PlayerPrefs 使用异或算法加密

using System;
using System.Globalization;
using UnityEngine;

namespace Framework.Common
{
    /// <summary>
    /// 本地存储工具
    /// </summary>
    public static class PlayerPrefsUtility
    {
        private static readonly byte[] SecretKey =
            { 217, 134, 151, 168, 185, 202, 129, 135, 150, 130, 141, 201, 210, 167, 198, 169 };

        /// <summary>
        /// 是否加密
        /// </summary>
        public static bool IsEncrypt = true;

        private const string LOGTag = "PlayerPrefsExtend";

        /// <summary>
        /// 删除所有本地存储的内容
        /// </summary>
        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }

        /// <summary>
        /// 删除指定key值
        /// </summary>
        /// <param name="key"></param>
        public static void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        /// <summary>
        /// 获取float
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static float GetFloat(string key, float defaultValue = 0.0F)
        {
            if (!HasKey(key))
            {
                LogManager.LogWarning(LOGTag, $"The key {key} does not exist");
                return defaultValue;
            }

            if (IsEncrypt)
            {
                var encryptedStr = PlayerPrefs.GetString(key);
                var decryptedStr = Decrypt(encryptedStr);
                if (float.TryParse(decryptedStr, out var value))
                {
                    return value;
                }

                LogManager.LogWarning(LOGTag, $"Key {key} GetFloat Failed");
            }

            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        /// <summary>
        /// 获取int
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int GetInt(string key, int defaultValue = 0)
        {
            if (!HasKey(key))
            {
                LogManager.LogWarning(LOGTag, $"The key {key} does not exist  ");
                return defaultValue;
            }

            if (IsEncrypt)
            {
                var encryptedStr = PlayerPrefs.GetString(key);
                var decryptedStr = Decrypt(encryptedStr);
                if (int.TryParse(decryptedStr, out var value))
                {
                    return value;
                }

                LogManager.LogWarning(LOGTag, $"Key {key} GetInt Failed");
            }

            return PlayerPrefs.GetInt(key, defaultValue);
        }

        /// <summary>
        /// 获取string
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetString(string key, string defaultValue = "")
        {
            if (HasKey(key))
                return IsEncrypt ? Decrypt(PlayerPrefs.GetString(key)) : PlayerPrefs.GetString(key, defaultValue);
            LogManager.LogWarning(LOGTag, $"The key {key} does not exist  ");
            return defaultValue;
        }

        /// <summary>
        /// 获取bool
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static bool GetBool(string key, bool defaultValue = false)
        {
            if (!HasKey(key))
            {
                LogManager.LogWarning(LOGTag, $"The key {key} does not exist  ");
                return defaultValue;
            }

            var value = PlayerPrefs.GetInt(key, Convert.ToInt32(defaultValue));
            return value == 1;
        }

        /// <summary>
        /// 设置float
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetFloat(string key, float value)
        {
            if (IsEncrypt)
            {
                PlayerPrefs.SetString(key, Encrypt(value.ToString(CultureInfo.InvariantCulture)));
            }
            else
            {
                PlayerPrefs.SetFloat(key, value);
            }
        }

        /// <summary>
        /// 设置int
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetInt(string key, int value)
        {
            if (IsEncrypt)
            {
                PlayerPrefs.SetString(key, Encrypt(value.ToString()));
            }
            else
            {
                PlayerPrefs.SetInt(key, value);
            }
        }

        /// <summary>
        /// 设置bool
        /// </summary>
        /// <param name="key"></param>
        /// <param name="state"></param>
        public static void SetBool(string key, bool state)
        {
            PlayerPrefs.SetInt(key, Convert.ToInt32(state));
        }

        /// <summary>
        /// 设置string
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetString(string key, string value)
        {
            if (IsEncrypt)
            {
                PlayerPrefs.SetString(key, Encrypt(value));
            }
            else
            {
                PlayerPrefs.SetString(key, value);
            }
        }

        /// <summary>
        /// 是否存在key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        /// <summary>
        /// 保存到磁盘
        /// </summary>
        public static void Save()
        {
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 加密内容 返回存储的string
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string Encrypt(string content)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(content);
            data = XorEncrypt(data);
            var saveStr = Convert.ToBase64String(data, 0, data.Length);
            return saveStr;
        }

        /// <summary>
        /// 解密存储的内容
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string Decrypt(string content)
        {
            var data = Convert.FromBase64String(content);
            data = XorEncrypt(data);
            content = System.Text.Encoding.UTF8.GetString(data);
            return content;
        }

        /// <summary>
        /// 异或加密
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static byte[] XorEncrypt(byte[] data)
        {
            var d = 0;
            var k = 0;
            while (d < data.Length)
            {
                while (k < SecretKey.Length && d < data.Length)
                {
                    data[d] = (byte)(data[d] ^ SecretKey[k]);
                    d++;
                    k++;
                }

                k = 0;
            }

            return data;
        }
    }
}