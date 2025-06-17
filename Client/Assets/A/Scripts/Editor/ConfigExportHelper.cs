#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using MemoryPack;
using GameUtils;
using UnityEditor;
using UnityEngine;

namespace GameFramework
{
    // 自定义字符串数组转换器
    public class StringArrayConverter : DefaultTypeConverter
    {
        private const string Delimiter = "|";

        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new string[0];
            }

            // 移除前后的引号（如果存在）
            if (text.StartsWith("\"") && text.EndsWith("\""))
            {
                text = text.Substring(1, text.Length - 2);
            }

            // 使用分隔符分割字符串
            return text.Split(new[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries);
        }

        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            if (value == null)
            {
                return string.Empty;
            }

            string[] array = value as string[];
            if (array == null)
            {
                return string.Empty;
            }

            return string.Join(Delimiter, array);
        }
    }

    public class ConfigExportHelper : EditorWindow
    {
        [MenuItem("Tools/ConfigExportHelper")]
        public static void ShowWindow()
        {
            GetWindow<ConfigExportHelper>("ConfigExportHelper");
        }

        static string defaultConfigPath = "";
        static string outputConfigPath = "Assets/GameResources/Config";
        
        // 文件选择状态
        private Dictionary<string, bool> csvFileSelections = new Dictionary<string, bool>();
        private Vector2 scrollPosition;
        private bool selectAll = false;
        private string[] csvFiles = new string[0];
        private const string PrefsKeyPrefix = "CSVExporter_";

        private void OnEnable()
        {
            // 加载上次保存的路径
            defaultConfigPath = EditorPrefs.GetString(PrefsKeyPrefix + "SourcePath", defaultConfigPath);
            outputConfigPath = EditorPrefs.GetString(PrefsKeyPrefix + "OutputPath", outputConfigPath);
            
            // 如果路径有效，加载文件列表
            if (!string.IsNullOrEmpty(defaultConfigPath) && Directory.Exists(defaultConfigPath))
            {
                LoadCsvFiles();
            }
        }
        
        private void OnGUI()
        {
            GUILayout.Label("配置表导出工具", EditorStyles.boldLabel);

            // 源文件路径选择
            GUILayout.BeginHorizontal();
            GUILayout.Label("CSV源文件路径:", GUILayout.Width(120));
            GUILayout.Label(defaultConfigPath.CheckIsNullOrEmpty() ? "当前路径为空" : defaultConfigPath, EditorStyles.textField);
            if (GUILayout.Button("浏览...", GUILayout.Width(60)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("选择CSV文件路径", defaultConfigPath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    defaultConfigPath = selectedPath;
                    EditorPrefs.SetString(PrefsKeyPrefix + "SourcePath", defaultConfigPath);
                    LoadCsvFiles();
                }
            }
            GUILayout.EndHorizontal();

            // 输出路径选择
            GUILayout.BeginHorizontal();
            GUILayout.Label("导出目标路径:", GUILayout.Width(120));
            outputConfigPath = EditorGUILayout.TextField(outputConfigPath);
            if (GUILayout.Button("浏览...", GUILayout.Width(60)))
            {
                string selectedPath = EditorUtility.SaveFolderPanel("选择导出目标路径", outputConfigPath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // 转换为相对于Assets的路径
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        selectedPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    }
                    outputConfigPath = selectedPath;
                    EditorPrefs.SetString(PrefsKeyPrefix + "OutputPath", outputConfigPath);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            
            // 显示CSV文件列表
            if (csvFiles.Length > 0)
            {
                GUILayout.BeginHorizontal();
                bool newSelectAll = EditorGUILayout.ToggleLeft("全选", selectAll, GUILayout.Width(60));
                if (newSelectAll != selectAll)
                {
                    selectAll = newSelectAll;
                    foreach (var file in csvFiles)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        csvFileSelections[fileName] = selectAll;
                        SaveSelectionState(fileName, selectAll);
                    }
                }
                GUILayout.EndHorizontal();
                
                GUILayout.Label("CSV文件列表:", EditorStyles.boldLabel);
                
                // 创建一个可变大小的滚动区域，自适应窗口高度
                float scrollHeight = position.height - 200; // 根据窗口高度动态计算滚动区域高度
                if (scrollHeight < 150) scrollHeight = 150; // 设置最小高度
                
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUI.skin.box, 
                    GUILayout.ExpandHeight(true), GUILayout.MinHeight(scrollHeight));
                
                // 文件列表内容
                foreach (var file in csvFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    
                    GUILayout.BeginHorizontal(EditorStyles.helpBox);
                    bool isSelected = csvFileSelections.ContainsKey(fileName) ? csvFileSelections[fileName] : false;
                    bool newSelection = EditorGUILayout.ToggleLeft(fileName, isSelected, GUILayout.ExpandWidth(true));
                    
                    if (newSelection != isSelected)
                    {
                        csvFileSelections[fileName] = newSelection;
                        SaveSelectionState(fileName, newSelection);
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
                
                GUILayout.Label($"共 {csvFiles.Length} 个CSV文件，已选择 {GetSelectedCount()} 个", EditorStyles.miniLabel);
            }
            else if (!defaultConfigPath.CheckIsNullOrEmpty())
            {
                GUILayout.Label("所选文件夹中没有CSV文件", EditorStyles.boldLabel);
            }

            GUILayout.Space(10);

            GUI.enabled = !defaultConfigPath.CheckIsNullOrEmpty() && !outputConfigPath.CheckIsNullOrEmpty() && csvFiles.Length > 0 && HasSelectedFiles();
            if (GUILayout.Button("导出配置表", GUILayout.Height(30)))
            {
                ExportConfig();
            }
            GUI.enabled = true;
        }
        
        private bool HasSelectedFiles()
        {
            foreach (var selection in csvFileSelections.Values)
            {
                if (selection) return true;
            }
            return false;
        }
        
        private int GetSelectedCount()
        {
            int count = 0;
            foreach (var selection in csvFileSelections.Values)
            {
                if (selection) count++;
            }
            return count;
        }
        
        private void LoadCsvFiles()
        {
            if (string.IsNullOrEmpty(defaultConfigPath) || !Directory.Exists(defaultConfigPath))
            {
                csvFiles = new string[0];
                return;
            }
            
            csvFiles = Directory.GetFiles(defaultConfigPath, "*.csv");
            
            // 加载保存的选择状态
            csvFileSelections.Clear();
            foreach (var file in csvFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                bool isSelected = EditorPrefs.GetBool(PrefsKeyPrefix + fileName, true);
                csvFileSelections[fileName] = isSelected;
            }
            
            // 检查是否所有文件都被选中
            selectAll = csvFiles.Length > 0;
            foreach (var selection in csvFileSelections.Values)
            {
                if (!selection)
                {
                    selectAll = false;
                    break;
                }
            }
        }
        
        private void SaveSelectionState(string fileName, bool isSelected)
        {
            EditorPrefs.SetBool(PrefsKeyPrefix + fileName, isSelected);
        }

        private void ExportConfig()
        {
            List<string> selectedFiles = new List<string>();
            foreach (var file in csvFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                if (csvFileSelections.ContainsKey(fileName) && csvFileSelections[fileName])
                {
                    selectedFiles.Add(file);
                }
            }
            
            int totalFiles = selectedFiles.Count;
            
            if (totalFiles == 0)
            {
                EditorUtility.DisplayDialog("导出失败", "没有选择任何CSV文件进行导出", "确定");
                return;
            }

            // 确保输出目录存在
            if (!Directory.Exists(outputConfigPath))
            {
                Directory.CreateDirectory(outputConfigPath);
            }
            
            int successCount = 0;
            int failedCount = 0;
            List<string> failedFiles = new List<string>();

            try
            {
                for (int i = 0; i < totalFiles; i++)
                {
                    var csvFile = selectedFiles[i];
                    string fileName = Path.GetFileNameWithoutExtension(csvFile);
                    
                    // 显示进度条
                    float progress = (float)i / totalFiles;
                    bool cancelled = EditorUtility.DisplayCancelableProgressBar(
                        "导出配置表", 
                        $"正在处理: {fileName} ({i+1}/{totalFiles})", 
                        progress);
                        
                    if (cancelled)
                    {
                        EditorUtility.DisplayDialog("导出取消", "用户取消了导出操作", "确定");
                        break;
                    }
                    
                    string outputFilePath = Path.Combine(outputConfigPath, $"{fileName}.bytes");

                    try
                    {
                        using (var reader = new StreamReader(csvFile))
                        {
                            var config = new CsvConfiguration(CultureInfo.InvariantCulture);
                            using (var csv = new CsvReader(reader, config))
                            {
                                // 注册字符串数组转换器
                                csv.Context.TypeConverterCache.AddConverter(typeof(string[]), new StringArrayConverter());
                                var allAssembly = AppDomain.CurrentDomain.GetAssemblies();
                                Type voType = Type.GetType($"GameLogic.{fileName}");
                                foreach (var assembly in allAssembly)
                                {
                                    // 遍历所有 Assembly 中的类型
                                    foreach (var type in assembly.GetTypes())
                                    {
                                        if (type.Name == fileName)
                                        {
                                            voType = type;
                                            break;
                                        }
                                    }
                                }
                                if (voType == null)
                                {
                                    Debug.LogError($"GameLogic 命名空间下找不到类型: {fileName}");
                                    failedFiles.Add(fileName);
                                    failedCount++;
                                    continue;
                                }
                                dynamic records = ConvertList(csv.GetRecords(voType), voType);
                                File.WriteAllBytes(outputFilePath, MemoryPackSerializer.Serialize(records));
                            }
                        }
                        Debug.Log($"导出配置表 {fileName} 成功，路径: {outputFilePath}");
                        successCount++;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"导出配置表 {fileName} 失败: {e.Message}\n{e.StackTrace}");
                        failedFiles.Add(fileName);
                        failedCount++;
                    }
                }
            }
            finally
            {
                // 清除进度条
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.Refresh();
            
            // 显示完成对话框
            string message = $"导出完成！\n成功: {successCount} 个文件\n失败: {failedCount} 个文件";
            if (failedCount > 0)
            {
                message += "\n\n失败的文件:\n" + string.Join("\n", failedFiles);
            }
            
            EditorUtility.DisplayDialog("导出结果", message, "确定");
        }

        public static object ConvertList(IEnumerable<dynamic> source, Type targetType)
        {
            // 使用反射将 List<object> 转换为对应类型的 List<T>
            var genericListType = typeof(List<>).MakeGenericType(targetType);
            var listInstance = Activator.CreateInstance(genericListType);

            foreach (var item in source)
            {
                genericListType.GetMethod("Add")?.Invoke(listInstance, new[] { item });
            }
            return listInstance;
        }
    }
}

#endif