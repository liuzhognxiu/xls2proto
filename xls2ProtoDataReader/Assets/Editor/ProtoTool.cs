using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;

public class ProtoTool
{
    /// <summary>
    /// 呼叫流程
    /// </summary>
    /// <param name="processName"></param>
    /// <param name="param"></param>
    /// <returns>成功true</returns>
    static bool CallProcess(string processName, string param)
    {
        ProcessStartInfo process = new ProcessStartInfo
        {
            CreateNoWindow = false,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            FileName = processName,
            Arguments = param,
        };

        Process p = Process.Start(process);
        p.WaitForExit();

        string error = p.StandardError.ReadToEnd();
        if (!string.IsNullOrEmpty(error))
        {
            UnityEngine.Debug.LogError(processName + " " + param + "  ERROR! " + "\n" + error);

            string output = p.StandardOutput.ReadToEnd();
            if (!string.IsNullOrEmpty(output))
            {
                UnityEngine.Debug.Log(output);
            }

            return false;
        }
        return true;
    }

    static bool ProcessTableProto(string name)
    {
        string param = string.Format("-i:{0}.proto -o:{0}.cs -p:detectMissing", name);
        if (CallProcess("protogen.exe", param))
        {
            File.Copy(@".\" + name + ".cs", tableClientPath + name + ".cs", true);
            File.Delete(@".\" + name + ".cs");
            return true;
        }

        return false;
    }


    /// <summary>编译选中的proto文件</summary>
    [MenuItem("Tools/根据Proto生成CS文件")]
    static void ProtogenSelectedProtoFile()
    {
        string fileName = EditorUtility.OpenFilePanel("Select Proto File", protoPath, "proto");

        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogWarning("Please select one proto file");
            return;
        }

        string fileDirectory = Path.GetDirectoryName(fileName);
        string defaultDirectory = Directory.GetCurrentDirectory();

        try
        {
            Directory.SetCurrentDirectory(fileDirectory);

            string name = Path.GetFileNameWithoutExtension(fileName);
            string outputPath = Application.dataPath + "/CSTable";
            CallProcess("protoc", string.Format("{0}.proto --descriptor_set_out={0}.protodesc", name));
            CallProcess("protogen", string.Format("-i:{0}.protodesc -o:{1}/{0}.cs -p:detectMissing", name, outputPath));


            Directory.SetCurrentDirectory(defaultDirectory);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            Directory.SetCurrentDirectory(defaultDirectory);
        }
    }

    /// <summary>读选中的表格</summary>
    [MenuItem("Tools/选择XLS生成数据文件")]
    static void ReadSelectedTable()
    {
        //string excelPath = EditorUtility.OpenFilePanel("Select Proto File", Path.GetFullPath("../table/workbook"), "xls");
        string excelPath = EditorUtility.OpenFilePanel("Select Table File", tablePath + "/worktable", "xls");

        if (string.IsNullOrEmpty(excelPath))
        {
            Debug.LogWarning("Please select one xls file");
            return;
        }

        //string excelDirectory = Path.GetDirectoryName(excelPath);
        string projectDirectory = Directory.GetCurrentDirectory();
        string excelName = Path.GetFileNameWithoutExtension(excelPath);
        string bytesName = excelName.ToLower();

        try
        {
            //Directory.SetCurrentDirectory("../table");
            Directory.SetCurrentDirectory(tablePath);

            Debug.Log(tablePath + string.Format(@"xls_deploy_tool.py  {0}  {0}.xls", excelName));
            if (CallProcess("python", string.Format(@"xls_deploy_tool.py  {0} .\worktable\{0}.xls", excelName)))
            {
                File.Copy(@".\" + bytesName + ".data", tableBytePath + bytesName + ".data", true);
                File.Delete(@".\" + bytesName + ".data");
                File.Delete(@".\" + bytesName + ".txt");
                File.Delete(@".\" + bytesName + ".proto");
                File.Delete(@".\" + bytesName + ".py");
                File.Delete(@".\" + bytesName + ".pyc");
            }

            Directory.SetCurrentDirectory(projectDirectory);

        }
        catch (Exception e)
        {
            Debug.LogError(e);
            Directory.SetCurrentDirectory(projectDirectory);
        }
    }


    #region DataPath

    private static string tableClientPath = Application.dataPath + "/Table/";

    public static string protoPath
    {
        get
        {
            string curPath = Application.dataPath;

            string str1 = "xls2ProtoDataReader/Assets";

            if (curPath.Contains(str1))
            {
                return curPath.Replace(str1, "proto");
            }
            return string.Empty;
        }
    }

    private static string tablePath
    {
        get
        {
            string curPath = Application.dataPath;

            string str1 = "xls2ProtoDataReader/Assets";

            if (curPath.Contains(str1))
            {
                Debug.Log(curPath.Replace(str1, "XlsTable"));
                return curPath.Replace(str1, "XlsTable");
            }

            Debug.Log("get proto  path  faile");
            return string.Empty;
        }
    }

    private static string tableBytePath
    {
        get
        {
            string curPath = Application.dataPath;

            string path = string.Empty;

            string str1 = "xls2ProtoDataReader/Assets";

            if (curPath.Contains(str1))
            {
                path = str1;

                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    return "";
                }
                else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
                {
                    return "";
                }
                else
                {
                    if (Contant.IsInEditor)
                    {
                        return curPath.Replace(path, "xls2ProtoDataReader/Assets/StreamingAssets/DataConfig");
                    }
                    return curPath.Replace(path, "Tabledata/");
                }
            }
            return string.Empty;
        }
    }

    #endregion

}
