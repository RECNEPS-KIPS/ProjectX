// author:KIPKIPS
// describe:封装PlayerPrefs 使用异或算法加密
using System;
using UnityEngine;

namespace Framework.Core.Utility {
    public static class PlayerPrefsUtility {
        static readonly byte[] _secretKey = { 217, 134, 151, 168, 185, 202, 129, 135, 150, 130, 141, 201, 210, 167, 198, 169 };
        public static bool isEncrypt = true;
        public static string logTag = "PlayerPrefsExtend";
        //删除所有本地存储的内容
        public static void DeleteAll() {
            PlayerPrefs.DeleteAll();
        }

        //删除指定key值
        public static void DeleteKey(string key) {
            PlayerPrefs.DeleteKey(key);
        }

        //获取float
        public static float GetFloat(string key, float defaultValue = 0.0F) {
            if (!HasKey(key)) {
                LogManager.LogWarning(logTag, $"The key {key} does not exist");
                return defaultValue;
            }
            if (isEncrypt) {
                string encryptedStr = PlayerPrefs.GetString(key);
                string decryptedStr = Decrypt(encryptedStr);
                float value = 0;
                if (float.TryParse(decryptedStr, out value)) {
                    return value;
                } else {
                    LogManager.LogWarning(logTag, $"Key {key} GetFloat Failed");
                }
            }
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        //获取int
        public static int GetInt(string key, int defaultValue = 0) {
            if (!HasKey(key)) {
                LogManager.LogWarning(logTag, $"The key {key} does not exist  ");
                return defaultValue;
            }
            if (isEncrypt) {
                string encryptedStr = PlayerPrefs.GetString(key);
                string decryptedStr = Decrypt(encryptedStr);
                int value = 0;
                if (int.TryParse(decryptedStr, out value)) {
                    return value;
                } else {
                    LogManager.LogWarning(logTag, $"Key {key} GetInt Failed");
                }
            }
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        //获取string
        public static string GetString(string key, string defaultValue = "") {
            if (!HasKey(key)) {
                LogManager.LogWarning(logTag, $"The key {key} does not exist  ");
                return defaultValue;
            }
            if (isEncrypt) {
                string encryptedStr = PlayerPrefs.GetString(key);
                return Decrypt(encryptedStr);
            }
            return PlayerPrefs.GetString(key, defaultValue);
        }

        //获取bool
        public static bool GetBool(string key, bool defaultValue = false) {
            if (!HasKey(key)) {
                LogManager.LogWarning(logTag, $"The key {key} does not exist  ");
                return defaultValue;
            }
            int value = PlayerPrefs.GetInt(key, Convert.ToInt32(defaultValue));
            if (value == 1)
                return true;
            else return false;
        }

        //设置float
        public static void SetFloat(string key, float value) {
            if (isEncrypt) {
                string encryptedStr = Encrypt(value.ToString());
                PlayerPrefs.SetString(key, encryptedStr);
            } else {
                PlayerPrefs.SetFloat(key, value);
            }
        }

        //设置int
        public static void SetInt(string key, int value) {
            if (isEncrypt) {
                string encryptedStr = Encrypt(value.ToString());
                PlayerPrefs.SetString(key, encryptedStr);
            } else {
                PlayerPrefs.SetInt(key, value);
            }
        }

        //设置bool
        public static void SetBool(string key, bool state) {
            PlayerPrefs.SetInt(key, Convert.ToInt32(state));
        }
        public static void SetString(string key, string value) {
            if (isEncrypt) {
                string encryptedStr = Encrypt(value);
                PlayerPrefs.SetString(key, encryptedStr);
            } else {
                PlayerPrefs.SetString(key, value);
            }
        }

        //是否存在key
        public static bool HasKey(string key) {
            return PlayerPrefs.HasKey(key);
        }

        //保存到磁盘
        public static void Save() {
            PlayerPrefs.Save();
        }
        /// 加密内容 返回存储的string
        static string Encrypt(string content) {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(content);
            data = XorEncrypt(data);
            string saveStr = Convert.ToBase64String(data, 0, data.Length);
            return saveStr;
        }

        //解密存储的内容
        static string Decrypt(string content) {
            byte[] data = Convert.FromBase64String(content);
            data = XorEncrypt(data);
            content = System.Text.Encoding.UTF8.GetString(data);
            return content;
        }

        //异或加密
        private static byte[] XorEncrypt(byte[] data) {
            int d = 0;
            int k = 0;
            while (d < data.Length) {
                while (k < _secretKey.Length && d < data.Length) {
                    data[d] = (byte)(data[d] ^ _secretKey[k]);
                    d++;
                    k++;
                }
                k = 0;
            }
            return data;
        }
    }
}