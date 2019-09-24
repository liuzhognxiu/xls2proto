using UnityEngine;
using System.Collections;
using ProtoBuf;
using System.IO;
using tnt_deploy;

public class Test : MonoBehaviour
{
    void Start()
    {
        GOODS_INFO_ARRAY goods_infos = ReadOneDataConfig<GOODS_INFO_ARRAY>("goods_info");
        Debug.Log("goods_id==================" + goods_infos.items[0].name);
    }

    private T ReadOneDataConfig<T>(string FileName)
    {
        FileStream fileStream;
        fileStream = GetDataFileStream(FileName);
        if (null != fileStream)
        {
            T t = Serializer.Deserialize<T>(fileStream);
            fileStream.Close();
            return t;
        }

        return default(T);
    }
    private FileStream GetDataFileStream(string fileName)
    {
        string filePath = GetDataConfigPath(fileName);
        if (File.Exists(filePath))
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            return fileStream;
        }

        return null;
    }
    private string GetDataConfigPath(string fileName)
    {
        return Application.streamingAssetsPath + "/DataConfig/" + fileName + ".data";
    }
}