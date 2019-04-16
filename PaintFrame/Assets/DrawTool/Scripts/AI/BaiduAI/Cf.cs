using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Baidu.Aip.Face;
using Baidu.Aip.Ocr;
using Baidu.Aip.BodyAnalysis;

public class Cf{

    public string APIKey { get; set; }
    public string SECRET_KEY { get; set; }
    public Face faceClient;
    public Ocr ocrClient;
    public Body bodyClient;

    private static Cf instance = null;

    private Cf() { }

    public static Cf Instance()
    {
        if (instance == null)
        {
            instance = new Cf();
        }
        return instance;
    }

    /// <summary>
    /// 初始化 应用信息
    /// </summary>
    /// <param name="appid"></param>
    /// <param name="secretId"></param>
    /// <param name="secretKey"></param>
    /// <param name="userid"></param>
    public void setAppInfo(string apiKey, string secretKey)
    {
        this.faceClient = new Face(apiKey, secretKey);
    }

    public void setOcrAppInfo(string apiKey, string secretKey)
    {
        this.ocrClient = new Ocr(apiKey, secretKey);
    }

    public void setBodyAnalysisAppInfo(string apiKey, string secretKey)
    {
        this.bodyClient = new Body(apiKey, secretKey);
    }
}
