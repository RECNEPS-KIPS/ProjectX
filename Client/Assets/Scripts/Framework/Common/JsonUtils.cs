// author:KIPKIPS
// describe:json工具类

using System;
using System.IO;
using Newtonsoft.Json;

namespace Framework.Common
{
    /// <summary>
    /// JSON工具
    /// </summary>
    public static class JsonUtils
    {
        /// <summary>
        /// AnalysisJson
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T LoadJsonByPath<T>(string path)
        {
            var filePath = Environment.CurrentDirectory + "/" + path;
            //print(filePath);
            //读取文件
            var reader = new StreamReader(filePath);
            var jsonStr = @reader.ReadToEnd();
            reader.Close();
            //字符串转换为DataSave对象
            var data = JsonConvert.DeserializeObject<T>(@jsonStr);
            return data;
        }
    }
}