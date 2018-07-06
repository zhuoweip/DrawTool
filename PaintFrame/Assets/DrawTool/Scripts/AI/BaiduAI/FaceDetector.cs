using DevelopEngine;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

//参考演示demo,用的是旧的sdk
//http://www.manew.com/forum.php?mod=viewthread&tid=109347 
//https://blog.csdn.net/dark00800/article/details/78191431

public class FaceDetector : MonoSingleton<FaceDetector>
{
    string APPID = "11450771";
    string APIKey = "B6ZGNfAXbfvdV2xo0vYox4lQ";
    string SecretKey = "glrXsvIKb8cESeyAooRSgR7dk1QmEqgg";

    private void Awake()
    {
        System.Net.ServicePointManager.ServerCertificateValidationCallback +=
               delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                           System.Security.Cryptography.X509Certificates.X509Chain chain,
                           System.Net.Security.SslPolicyErrors sslPolicyErrors)
               {
                   return true; // **** Always accept
               };

        Cf.Instance().setAppInfo(APIKey, SecretKey);
    }

    public void DetectLocalImg(string imgName)
    {
        byte[] bytes = File.ReadAllBytes(Application.streamingAssetsPath + "/" + imgName + ".jpg");
        FaceDetect(bytes);
    }

    /// <summary>人脸检测</summary>
    public JObject FaceDetect(byte[] bytes)
    {
        try
        {
            return BaiDu.detectface(bytes);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }

    /// <summary>注册用户组</summary>
    public void FaceCreatGroup()
    {
        try
        {
            BaiDu.creatgroup();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>获取用户组</summary>
    public void GetGroupList()
    {
        try
        {
            BaiDu.getgrouplist();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>注册用户</summary>
    public void SignUpNewPerson(byte[] bytes)
    {
        try
        {
            BaiDu.addface(bytes);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>获取用户列表</summary>
    public JObject GetFaceList()
    {
        try
        {
            return BaiDu.getfacelist();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>对比人脸库</summary>
    public bool FaceMatch(byte[] bytes)
    {
        JsonParse.BaiduFaceList.Result result = JsonParse.BaiduFaceList.ParseJsonFaceList(GetFaceList().ToString()).result;
        List<string> tokenList = new List<string>();

        for (int i = 0; i < result.face_list.Length; i++)
        {
            tokenList.Add(result.face_list[i].face_token);
        }
        try
        {
            for (int j = 0; j < tokenList.Count; j++)
            {
                if (BaiDu.facecompare(bytes, tokenList[j]))
                    return true;
            }
            return false;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>根据检测点重绘图像</summary>
    public void ReDrawDetectImg(JObject result, RawImage rImg, Texture2D texture)
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
