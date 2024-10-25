using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace export {
    internal class export {
        private static int maxLayer;
        private static readonly bool isMinify = false;
        private static string splicerN = "";
        private static string splicerT = "";
        private static readonly Dictionary<string, string> typeDict = new Dictionary<string, string>();
        private static void Main(string[] args) {
            // args = new string[] { "s" }; //TODO:test
            splicerN = isMinify ? "" : "\n";
            splicerT = isMinify ? "" : "\t";
            if (args.Length > 0) {
                // // string root = "G:/GUARDIAN_OFFICE/Config/";//TODO:test
                // string root = "C:/GUARDIAN_OFFICE/Config/"; //TODO:test
                string root = args[0].Replace('\\', '/');
                string csvPath = root + "csv";
                string jsonPath = root + "json";
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Configuration table directory : {csvPath}");
                Console.WriteLine($"Export target directory : {jsonPath}\n");
                //删除导出目录下的所有文件
                DirectoryInfo jsonDir = new DirectoryInfo(jsonPath);
                FileSystemInfo[] jsonFiles = jsonDir.GetFileSystemInfos();
                foreach (FileSystemInfo i in jsonFiles) {
                    //判断是否文件夹
                    if (i is DirectoryInfo) {
                        DirectoryInfo subDir = new DirectoryInfo(i.FullName);
                        //删除子目录和文件
                        subDir.Delete(true);
                    } else {
                        //删除指定文件
                        File.Delete(i.FullName);
                    }
                }
                Console.WriteLine("The old configuration table is cleared\n");
                Console.ForegroundColor = ConsoleColor.White;
                //获取目录下所有文件
                DirectoryInfo csvDir = new DirectoryInfo(csvPath);
                FileSystemInfo[] csvFiles = csvDir.GetFileSystemInfos();
                int ignoreLine = 3;
                foreach (var fileSystemInfo in csvFiles) {
                    var fileInfo = (FileInfo)fileSystemInfo;
                    if (fileInfo.Extension != ".csv") continue;
                    string str;
                    try {
                        using (StreamReader csvSR = new StreamReader(fileInfo.FullName, new UTF8Encoding())) {
                            str = csvSR.ReadToEnd();
                            csvSR.Close();
                        }
                        //以UTF-8 NO-BOM格式重新写入文件
                        Encoding newEncoding = new UTF8Encoding(false);
                        using (StreamWriter csvSW = new StreamWriter(fileInfo.FullName, false, newEncoding)) {
                            csvSW.Write(str);
                            csvSW.Close();
                            //Console.WriteLine(str);
                        }
                        string strLine = string.Empty;
                        FileStream csvFS = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);
                        StreamReader formatCsvSR = new StreamReader(csvFS, Encoding.Default);
                        int row = 0;
                        string[] keys = null;
                        string[] types = null;
                        string[] values;
                        string json = string.Empty;
                        string obj;
                        json += "[";
                        int curLayer;
                        //int commentNum = 0;
                        typeDict.Clear();
                        if (!typeDict.ContainsKey("__metatable")) {
                            typeDict.Add("__metatable", "true");
                        }
                        HashSet<int> validKeyIndexes = new HashSet<int>();
                        using (Microsoft.VisualBasic.FileIO.TextFieldParser csvReader = new Microsoft.VisualBasic.FileIO.TextFieldParser(csvFS, Encoding.Default)) {
                            csvReader.SetDelimiters(new string[] { "," });
                            csvReader.HasFieldsEnclosedInQuotes = true;
                            csvReader.TrimWhiteSpace = true;
                            while (!csvReader.EndOfData) {
                                string[] arrayLine = csvReader.ReadFields();
                                // arrayLine 中就是每行的数据
                                Console.WriteLine("arrayLine:  " + arrayLine.Length);
                                if (row != ignoreLine && row != 0) {
                                    if (row == 1) {
                                        keys = arrayLine;
                                        List<string> keyList = new List<string>();
                                        for (int i = 0; i < arrayLine.Length; i++) {
                                            if (keys[i].StartsWith('_')) continue;
                                            keyList.Add(keys[i]);
                                            validKeyIndexes.Add(i);
                                        }
                                        keys = keyList.ToArray();
                           
                                        //Console.WriteLine(keys.Length);
                                    }
                                    if (row == 2) {
                                        types = arrayLine;
                                        List<string> typeList = new List<string>();
                                        for (int i = 0; i < types.Length; i++) {
                                            if (validKeyIndexes.Contains(i)) {
                                                typeList.Add(types[i]);
                                            }
                                        }
                                        types = typeList.ToArray();
                                        //Console.WriteLine(types.Length);
                                    }
                                    if (row >= 4) {
                                        values = arrayLine;
                                        List<string> valueList = new List<string>();
                                        for (int i = 0; i < values.Length; i++) {
                                            if (validKeyIndexes.Contains(i)) {
                                                valueList.Add(values[i]);
                                            }
                                        }
                                        values = valueList.ToArray();
                                        //Console.WriteLine(values.Length);
                                        curLayer = 1;
                                        obj = splicerN + GetTableAndRecord(curLayer) + "{" + splicerN;
                                        for (int i = 0; i < values.Length; i++) {
                                            // Console.WriteLine(values[i]);
                                            curLayer = 2;
                                            obj += GetTableAndRecord(curLayer);
                                            string value = types[i] switch {
                                                "int" => values[i] == "" ? "0" : values[i],
                                                "float" => values[i] == "" ? "0" : values[i],
                                                "bool" => values[i] == "" ? "false" : values[i].ToLower(),
                                                "string" => $"\"{values[i]}\"",
                                                "vector2" => GetNormalList(values[i], ','),
                                                "vector3" => GetNormalList(values[i], ','),
                                                "list<int>" => GetNormalList(values[i], '|'),
                                                "list<float>" => GetNormalList(values[i], '|'),
                                                "list<bool>" => GetNormalList(values[i], '|'),
                                                "list<string>" => GetStringList(values[i], '|'),
                                                "list<list<int>>" => GetNormalNestList(values[i], curLayer),
                                                "list<list<float>>" => GetNormalNestList(values[i], curLayer),
                                                "list<list<bool>>" => GetNormalNestList(values[i], curLayer),
                                                "list<list<string>>" => GetStringNestList(values[i], curLayer),
                                                "dict<int,int>" => GetDict(values[i], curLayer),
                                                "dict<int,float>" => GetDict(values[i], curLayer),
                                                "dict<int,bool>" => GetDict(values[i], curLayer),
                                                "dict<int,string>" => GetDict(values[i], curLayer, true),
                                                "dict<float,int>" => GetDict(values[i], curLayer),
                                                "dict<float,float>" => GetDict(values[i], curLayer),
                                                "dict<float,bool>" => GetDict(values[i], curLayer),
                                                "dict<float,string>" => GetDict(values[i], curLayer, true),
                                                "dict<string,int>" => GetDict(values[i], curLayer),
                                                "dict<string,float>" => GetDict(values[i], curLayer),
                                                "dict<string,bool>" => GetDict(values[i], curLayer),
                                                "dict<string,string>" => GetDict(values[i], curLayer, true),
                                                _ => string.Empty
                                            };
                                            obj += $"\"{keys[i]}\":{value}";
                                            obj += (i == values.Length - 1) ? splicerN : ("," + splicerN);
                                            // if ("Scene.csv" == fileInfo.Name ) {
                                            //     Console.WriteLine(fileInfo.Name + " obj:" + obj);
                                            // }
                                            if (!typeDict.ContainsKey(keys[i])) {
                                                typeDict.Add(keys[i], types[i]);
                                            }
                                        }
                                        curLayer = 1;
                                        obj += GetTableAndRecord(curLayer) + "},";
                                        json += obj;
                                    }
                                    //Console.WriteLine("第"+row+"行:"+strLine);
                                }
                                row++;
                            }
                        }
                        // curLayer = 1;
                        // metatable
                        string dt = splicerT + splicerT;
                        json += splicerN + splicerT + "{";
                        string typeStr = "";
                        int idx = 0;
                        string suffix;
                        // Console.WriteLine(typeDict.Count);
                        foreach (KeyValuePair<string, string> kvp in typeDict) {
                            idx++;
                            suffix = idx == typeDict.Count ? "" : ",";
                            typeStr += $"{splicerN}{dt}\"{kvp.Key}\":\"{kvp.Value}\"{suffix}";
                        }
                        json += typeStr + splicerN + splicerT + "}" + splicerN + "]";
                        for (int i = 0; i <= maxLayer; i++) {
                            json = json.Replace("," + splicerN + GetTable(i) + "]", splicerN + GetTable(i) + "]");
                        }
                        // end metatable
                        formatCsvSR.Close();
                        string exportFile = fileInfo.FullName.Replace("csv", "json");
                        if (!File.Exists(exportFile)) {
                            Console.WriteLine($"Create file : {exportFile}");
                            FileStream createFS = File.Create(exportFile);
                            createFS.Close();
                        }
                        try {
                            using (FileStream jsonFS = File.Create(exportFile)) {
                                byte[] info = new UTF8Encoding(true).GetBytes(json);
                                // 向文件中写入一些信息。
                                jsonFS.Write(info, 0, info.Length);
                                jsonFS.Close();
                            }
                            Console.WriteLine($"Export finished : {exportFile}");
                        } catch (Exception ex) {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(ex.ToString());
                        }
                        typeDict.Clear();
                        //Console.WriteLine(json);
                    } catch (Exception ex) {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(ex.ToString());
                    }
                }
                // Console.WriteLine(jsonPath);
                // Console.WriteLine(root);
                Console.ForegroundColor = ConsoleColor.Green;
                string targetPath = root.Replace("/Config", "");
                Console.WriteLine($"\nThe target export directory is {targetPath}Client/Config\n");
                DirectoryInfo targetDir = new DirectoryInfo(targetPath + "Client/Config");
                FileSystemInfo[] targetFiles = targetDir.GetFileSystemInfos();
                foreach (FileSystemInfo i in targetFiles) {
                    //判断是否文件夹
                    if (i is DirectoryInfo) {
                        DirectoryInfo subDir = new DirectoryInfo(i.FullName);
                        //删除子目录和文件
                        subDir.Delete(true);
                    } else {
                        //删除指定文件
                        File.Delete(i.FullName);
                    }
                }
                Console.WriteLine("\nThe export directory is cleared\n");
                // Console.WriteLine(targetPath);
                Console.ForegroundColor = ConsoleColor.White;
                ListFiles(new DirectoryInfo(jsonPath), targetPath + "Client/Config", "json");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Copy finished\n");
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Any input to close the console...");
            Console.ReadKey();
            Environment.Exit(0);
        }

        /// <summary>
        /// 查找固定后缀文件
        /// </summary>
        /// <param name="info"></param>
        /// <param name="tarDir"></param>
        /// <param name="fileType"></param>
        /// <returns></returns>
        public static void ListFiles(FileSystemInfo info, string tarDir, string fileType) {
            if (!info.Exists)
                return;
            DirectoryInfo dir = info as DirectoryInfo;
            //不是目录
            if (dir == null)
                return;
            FileSystemInfo[] files = dir.GetFileSystemInfos();
            foreach (var t in files) {
                if (t is FileInfo file) {
                    //Console.WriteLine(file.Extension);
                    //if (file.Extension == "."+ fileType) {
                    if (file.Extension != "." + fileType) continue;
                    Console.WriteLine("Copied : " + file.FullName);
                    string desPath = tarDir + "/" + file.Name;
                    file.CopyTo(desPath, true); //允许覆盖文件
                } else { //子目录递归查找
                    ListFiles(t, tarDir, fileType);
                }
            }
        }
        // dict<T,T>导出方案
        /// <summary>
        /// </summary>
        /// <param name="str">解析串</param>
        /// <param name="layer">层级</param>
        /// <returns>list对应的嵌套的json字符串</returns>
        public static string GetDict(string str, int layer, bool isString = false) {
            // Console.WriteLine(str);
            string res = "{";
            string[] arr = str.Split('|');
            int invalidNum = CalculateInvalidNum(arr);
            for (int i = 0; i < arr.Length; i++) {
                if (arr[i] != string.Empty) {
                    res += (splicerN + GetTable(layer + 1) + GetDictKeyValuePair(arr[i], '#', isString) + (i == arr.Length - 1 - invalidNum ? "" : ","));
                }
            }
            res += splicerN + GetTable(layer) + "}";
            return res;
        }
        public static string GetDictKeyValuePair(string str, char split, bool isString = false) {
            string[] arr = str.Split(split);
            string res = "";
            if (arr.Length == 2) {
                res = "\"" + arr[0] + "\":" + (isString ? "\"" + arr[1] + "\"" : arr[1]);
            }
            return res;
        }
        // 嵌套list<list<数值型>>导出方案
        /// <summary>
        /// </summary>
        /// <param name="str">解析串</param>
        /// <param name="layer">层级</param>
        /// <returns>list对应的嵌套的json字符串</returns>
        static string GetNormalNestList(string str, int layer) {
            string[] arr = str.Split('|');
            string res = string.Empty;
            int invalidNum = CalculateInvalidNum(arr);
            res += "[";
            for (int i = 0; i < arr.Length; i++) {
                if (arr[i] != string.Empty) {
                    res += (splicerN + GetTable(layer + 1) + GetNormalList(arr[i], '#') + (i == arr.Length - 1 - invalidNum ? "" : ","));
                }
            }
            res += splicerN + GetTable(layer) + "]";
            return res;
        }
        //嵌套list<list<string>>导出方案
        /// <summary>
        /// </summary>
        /// <param name="str">解析串</param>
        /// <param name="layer">层级</param>
        /// <returns>list对应的嵌套的json字符串</returns>
        static string GetStringNestList(string str, int layer) {
            string[] arr = str.Split('|');
            string res = string.Empty;
            int invalidNum = CalculateInvalidNum(arr);
            res += "[";
            for (int i = 0; i < arr.Length; i++) {
                if (arr[i] != string.Empty) {
                    res += (splicerN + GetTable(layer + 1) + GetStringList(arr[i], '#') + (i == arr.Length - 1 - invalidNum ? "" : ","));
                }
            }
            res += splicerN + GetTable(layer) + "]";
            return res;
        }
        /// <summary>
        /// 根据指定类型(数值型)拆分字符串为数组并拼接成json数组
        /// </summary>
        /// <param name="str">解析串</param>
        /// <param name="split">划分字符</param>
        /// <returns>list对应的嵌套的json字符串</returns>
        static string GetNormalList(string str, char split) {
            string[] arr = str.Split(split);
            string res = string.Empty;
            int invalidNum = CalculateInvalidNum(arr);
            for (int i = 0; i < arr.Length; i++) {
                if (arr[i] != string.Empty) {
                    res += (arr[i] + (i == arr.Length - 1 - invalidNum ? "" : ","));
                }
            }
            return "[" + res + "]";
        }
        /// <summary>
        /// 根据指定类型(字符型)拆分字符串为数组并拼接成json数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        static string GetStringList(string str, char split) {
            string[] arr = str.Split(split);
            string res = string.Empty;
            int invalidNum = CalculateInvalidNum(arr);
            for (int i = 0; i < arr.Length; i++) {
                if (arr[i] != string.Empty) {
                    res += (("\"" + arr[i] + "\"") + (i == arr.Length - 1 - invalidNum ? "" : ","));
                }
            }
            return "[" + res + "]";
        }
        /// <summary>
        /// 拼接\t并记录最大拼接数量
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        static string GetTableAndRecord(int layer) {
            if (layer > maxLayer) {
                maxLayer = layer;
            }
            return GetTable(layer);
        }
        /// <summary>
        /// 拼接\t
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        static string GetTable(int layer) {
            string str = "";
            for (int i = 0; i < layer; i++) {
                str += splicerT;
            }
            return str;
        }
        /// <summary>
        /// 计算无效元素数量
        /// </summary>
        /// <param name="strs"></param>
        /// <returns></returns>
        static int CalculateInvalidNum(string[] strs) {
            int num = 0;
            for (int i = 0; i < strs.Length; i++) {
                if (strs[i] == string.Empty) {
                    num++;
                }
            }
            return num;
        }
    }
}