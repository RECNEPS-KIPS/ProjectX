// author:KIPKIPS
// describe:封装随机类

using System;

namespace Framework.Core.Manager
{
    /// <summary>
    /// 加密随机数
    /// </summary>
    public class EncryptRandom
    {
        private static Random _random = new Random();

        private EncryptRandom()
        {
        }

        /// <summary>
        /// 加密随机数
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int RandomNum(int max = 1024)
        {
            return _random.Next(0, max < 0 ? 1024 : max);
        }
    }
}