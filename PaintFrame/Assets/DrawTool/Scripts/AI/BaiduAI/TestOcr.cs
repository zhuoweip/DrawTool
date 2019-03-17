using Baidu.Aip.Ocr;
using System;
using UnityEngine;
using DevelopEngine;
using System.IO;
using System.Collections.Generic;

public class TestOcr : MonoSingleton<TestOcr>
{
    const string APP_ID = "15757780";
    const string API_KEY = "BH3VIXOv98pBphZhHSZqcRFD";
    const string SECRET_KEY = "Qzv464UlVDjreu9pkumWV8Zf0T3XBEwT";

    Ocr client;

    void Awake()
    {
        client = new Ocr(API_KEY, SECRET_KEY);
        client.Timeout = 60000;  // 修改超时时间
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            GeneralBasicDemo(null);
        }
    }

    void Start()
    {
        //调用文字识别函数
        //GeneralBasicDemo();
    }

    public void GeneralBasicDemo(byte[] images)
    {
        //读取对应"图片文件路径"的图片文件
        byte[] image = File.ReadAllBytes(Application.streamingAssetsPath + "/timg.jpg");
        var options = new Dictionary<string, object>{
        {"language_type", "CHN_ENG"},
        {"detect_direction", "true"},
        {"detect_language", "true"},
        {"probability", "true"}
    };
        Debug.LogError(image.Length);
        // 调用通用文字识别, 图片参数为本地图片，可能会抛出网络等异常，请使用try/catch捕获
        try
        {
            //调取API是哦图片文字
            var result = client.GeneralBasic(image, options);
            //打印获取到的结果
            Debug.Log(result);
        }
        catch (Exception e)
        {
            //打印异常信息
            Debug.Log("异常：" + e);
        }
    }
}
