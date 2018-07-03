using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Baidu.Aip.Face;
using UnityEngine.UI;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System;
using DevelopEngine;

//参考演示demo,用的是旧的sdk
//http://www.manew.com/forum.php?mod=viewthread&tid=109347 
//https://blog.csdn.net/dark00800/article/details/78191431

public enum ImgType
{
    /// <summary>
    /// 图片的base64值
    /// </summary>
    BASE64 = 1,
    /// <summary>
    /// 图片的 URL地址
    /// </summary>
    URL,
    /// <summary>
    /// 人脸图片的唯一标识
    /// </summary>
    FACE_TOKEN
}

public class FaceDetector : MonoSingleton<FaceDetector> {

    private Face client;                                        // 用来调用百度AI接口
    string APIKey = "B6ZGNfAXbfvdV2xo0vYox4lQ";                  
    string SecretKey = "glrXsvIKb8cESeyAooRSgR7dk1QmEqgg";       

    private Dictionary<string, object> options;                 // 返回的数据
    private string imageType = Enum.GetName(typeof(ImgType),1);                        // 图片类型
    private JObject result;                                     // 接收返回的结果

    private void Awake()
    {
        System.Net.ServicePointManager.ServerCertificateValidationCallback +=
               delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                           System.Security.Cryptography.X509Certificates.X509Chain chain,
                           System.Net.Security.SslPolicyErrors sslPolicyErrors)
               {
                   return true; // **** Always accept
               };

        client = new Face(APIKey, SecretKey);
        options = new Dictionary<string, object>()
        {
            {"face_field", "age,beauty,expression,faceshape,gender,glasses,landmark,race,quality,facetype"},
            {"max_face_num", 1},
            {"face_type", "LIVE"}
        };
    }

    public void DetectLocalImg(string imgName)
    {
        byte[] image = File.ReadAllBytes(Application.streamingAssetsPath + "/" + imgName + ".jpg");
        FaceDetect(image);
    }

    private string GetImgStr(byte[] bytes)
    {
        return System.Convert.ToBase64String(bytes);
    }

    public JObject FaceDetect(byte[] img)
    {
        try
        {
            result = client.Detect(GetImgStr(img), imageType, options);
            return result;
            //ReDrawDetectImg(result);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }

    /// <summary>
    /// 根据检测点重绘图像
    /// </summary>
    /// <param name="result"></param>
    private void ReDrawDetectImg(JObject result,RawImage rImg, Texture2D texture)
    {
        var width = texture.width;
        var height = texture.height;
        var mask = new Texture2D(width, height);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
                mask.SetPixel(i, j, texture.GetPixel(i, j));
        }

        var r = result["result"]["face_list"];
        foreach (var value in r)
        {
            var landmarks = value["landmark72"];
            foreach (var lm in landmarks)
            {
                var x = (int)(float.Parse(lm["x"].ToString()));
                var y = height - (int)(float.Parse(lm["y"].ToString()));

                for (int i = x; i <= x + 10; i++)//改变取值点的范围可以增加绘制点的数量，使得颜色更深 int i = x - 1; i <= x + 1; i++
                {
                    for (int j = y; j <= y + 10; j++)
                    {
                        mask.SetPixel(i, j, Color.black);
                    }
                }
            }
        }
        mask.Apply();
        rImg.texture = mask;
    }

    #region 获取信息

    public string GetExpressionStr(string expressionStr)
    {
        string expressionType = string.Empty;
        if (expressionStr == "none")
            expressionType = "冷漠";
        else if (expressionStr == "smile")
            expressionType = "微笑";
        else if (expressionStr == "laugh")
            expressionType = "大笑";
        return expressionType;
    }

    public string GetRaceStr(string raceStr)
    {
        string raceType = string.Empty;
        if (raceStr == "yellow")
            raceType = "黄种人";
        else if (raceStr == "white")
            raceType = "白种人";
        else if (raceStr == "black")
            raceType = "黑种人";
        else if (raceStr == "arabs")
            raceType = "阿拉伯人";
        return raceType;
    }

    public string GetGlassStr(string glassStr)
    {
        string glasstype = string.Empty;
        if (glassStr == "none")
            glasstype = "无";
        else if (glassStr == "common")
            glasstype = "普通眼镜";
        else if (glassStr == "sun")
            glasstype = "墨镜";
        return glasstype;
    }

    #endregion
}
