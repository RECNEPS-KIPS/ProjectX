// author:KIPKIPS
// describe:二进制文件工具类

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Framework.Common
{
    /// <summary>
    /// 二进制工具
    /// </summary>
    public static class BinaryUtils
    {
        /// <summary>
        /// 将对象转换为byte数组
        /// </summary>
        /// <param name="obj">被转换对象</param>
        /// <returns>转换后byte数组</returns>
        public static byte[] Object2Bytes<T>(T obj)
        {
            using var ms = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            var buff = ms.GetBuffer();
            return buff;
        }
 
        /// <summary>
        /// 将byte数组转换成对象
        /// </summary>
        /// <param name="buff">被转换byte数组</param>
        /// <returns>转换完成后的对象</returns>
        public static T Bytes2Object<T>(byte[] buff) where T : class
        {
            using var ms = new MemoryStream(buff);
            IFormatter formatter = new BinaryFormatter();
            var obj = formatter.Deserialize(ms) as T;
            return obj;
        }
    }
}