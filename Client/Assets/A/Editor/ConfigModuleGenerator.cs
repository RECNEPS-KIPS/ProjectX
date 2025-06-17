using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

public class ConfigModuleGenerator : EditorWindow
{
    private string targetPath = "Assets/GameResources/Resources/Config";
    private string outputPath = "Assets/Scripts/Module/GameLogic/GameConfigModule.gen.cs";
    
    [MenuItem("Tools/CodeGen/ConfigModuleGenerator")]
    public static void ShowWindow()
    {
        GetWindow<ConfigModuleGenerator>("配置文件生成器");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("配置文件生成器", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(true);
        targetPath = EditorGUILayout.TextField("扫描路径", targetPath);
        EditorGUI.EndDisabledGroup();
        if (GUILayout.Button("选择路径", GUILayout.Width(80)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("选择配置文件路径", "Assets", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                // 将绝对路径转换为相对于项目的路径
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    targetPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    targetPath = selectedPath;
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(true);
        outputPath = EditorGUILayout.TextField("输出文件路径", outputPath);
        EditorGUI.EndDisabledGroup();
        if (GUILayout.Button("选择路径", GUILayout.Width(80)))
        {
            string defaultName = Path.GetFileName(outputPath);
            string defaultDirectory = Path.GetDirectoryName(outputPath);
            string selectedPath = EditorUtility.SaveFilePanel("选择输出文件路径", defaultDirectory, defaultName, "cs");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                // 将绝对路径转换为相对于项目的路径
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    outputPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    outputPath = selectedPath;
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("生成配置加载代码"))
        {
            GenerateConfigLoaderCode();
        }
    }
    
    private void GenerateConfigLoaderCode()
    {
        List<string> voClasses = FindAllVOFiles(targetPath);
        if (voClasses.Count == 0)
        {
            EditorUtility.DisplayDialog("错误", "在指定路径下未找到任何xxVO.bytes文件", "确定");
            return;
        }
        
        string code = GenerateCode(voClasses);
        File.WriteAllText(outputPath, code);
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("成功", $"已成功生成配置加载代码，共 {voClasses.Count} 个配置文件。", "确定");
    }
    
    private List<string> FindAllVOFiles(string path)
    {
        List<string> voClasses = new List<string>();
        
        // 获取目录下所有.bytes文件
        string[] files = Directory.GetFiles(path, "*.bytes", SearchOption.AllDirectories);
        
        // 筛选出xxVO.bytes格式的文件
        Regex regex = new Regex(@"(\w+VO)\.bytes$");
        foreach (string file in files)
        {
            Match match = regex.Match(file);
            if (match.Success)
            {
                string voClassName = match.Groups[1].Value;
                voClasses.Add(voClassName);
                
                // 获取相对路径(不包含扩展名)
                string relativePath = file.Replace(Application.dataPath, "Assets");
                string configPath = relativePath.Substring(0, relativePath.Length - 6); // 移除.bytes扩展名
                
                // 将Windows路径分隔符替换为通用格式
                configPath = configPath.Replace('\\', '/');
                
                // 简化路径，只保留父目录和文件名
                string[] pathParts = configPath.Split('/');
                if (pathParts.Length >= 2)
                {
                    string parentDir = pathParts[pathParts.Length - 2];
                    string fileName = pathParts[pathParts.Length - 1];
                    configPath = $"{parentDir}/{fileName}";
                }
                
                // 存储VO类名和对应的配置路径
                voClassPaths[voClassName] = configPath;
            }
        }
        
        return voClasses;
    }
    
    private Dictionary<string, string> voClassPaths = new Dictionary<string, string>();
    
    private string GenerateCode(List<string> voClasses)
    {
        StringBuilder sb = new StringBuilder();
        
        sb.AppendLine("namespace GameLogic");
        sb.AppendLine("{");
        sb.AppendLine("    public partial class GameConfigModule");
        sb.AppendLine("    {");
        sb.AppendLine("        public void LoadConfig()");
        sb.AppendLine("        {");
        
        foreach (string voClass in voClasses)
        {
            string configPath = voClassPaths[voClass];
            sb.AppendLine($"            LoadConfigFromPath<{voClass}>(\"{configPath}\");");
        }
        
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        
        return sb.ToString();
    }
}