using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Baidu.Aip.Face;

public class Cf{

    public string APIKey { get; set; }
    public string SECRET_KEY { get; set; }
    public Face client;
    public Dictionary<string, object> options;

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
        this.client = new Face(apiKey, secretKey);
        this.options = new Dictionary<string, object>()
        {
            {"face_field", "age,beauty,expression,faceshape,gender,glasses,landmark,race,quality,facetype"},
            {"max_face_num", 1},
            {"face_type", "LIVE"}
        };
    }
}
