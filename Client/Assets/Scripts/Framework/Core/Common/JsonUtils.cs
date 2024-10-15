// author:KIPKIPS
// describe:json工具类
using System;
using System.IO;
using Newtonsoft.Json;

namespace Framework.Common {
    public static class JsonUtils {
        //AnalysisJson
        public static T LoadJsonByPath<T>(string path) {
            string filePath = Environment.CurrentDirectory + "/" + path;
            //print(filePath);
            //读取文件
            StreamReader reader = new StreamReader(filePath);
            string jsonStr = @reader.ReadToEnd();
            reader.Close();
            //字符串转换为DataSave对象
            T data = JsonConvert.DeserializeObject<T>(@jsonStr);
            return data;
        }
    }
}