using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Collections.Generic;

public class BaiDu
{
    /// <summary>
    /// 人脸检测 detectface
    /// </summary>
    /// <param name="bytes">图片流</param>
    /// <returns></returns>
    public static JObject detectface(byte[] bytes)
    {
        return Cf.Instance().client.Detect(SCY.Utility.ByteArrToStr64(bytes), "BASE64", Cf.Instance().options);
    }

    /// <summary>
    /// 人脸检测
    /// </summary>
    /// <param name="url">图片网络地址</param>
    /// <returns></returns>
    public static JObject detectface(string url)
    {
        return Cf.Instance().client.Detect(url, "URL", Cf.Instance().options);
    }

    /// <summary>
    /// 创建用户组
    /// </summary>
    public static void creatgroup()
    {
        var groupId = "group1";
        var result = Cf.Instance().client.GroupAdd(groupId);
        //Debug.Log(result);
    }

    /// <summary>
    /// 删除用户组
    /// </summary>
    public static void groupdelete()
    {
        var groupId = "group1";
        var result = Cf.Instance().client.GroupDelete(groupId);
        //Debug.Log(result);
    }

    /// <summary>
    /// 组列表查询
    /// </summary>
    public static JObject getgrouplist()
    {
        // 可选参数
        var options = new Dictionary<string, object> { { "start", 0 }, { "length", 50 } };
        var result = Cf.Instance().client.GroupGetlist(options);
        //Debug.Log(result);
        return result;
    }

    /// <summary>
    /// 人脸注册
    /// </summary>
    /// <param name="bytes"></param>
    public static void addface(byte[] bytes)
    {
        var image = SCY.Utility.ByteArrToStr64(bytes);
        var groupId = "group1";
        var userId = "user1";
        //可选参数options
        var options = new Dictionary<string, object>{
        {"user_info", "user's info"},
        {"quality_control", "NORMAL"},//图片质量控制
        {"liveness_control", "LOW"}};//活体检测控制
        var result = Cf.Instance().client.UserAdd(image, "BASE64", groupId, userId, options);
        //Debug.Log(result);
    }

    /// <summary>
    /// 人脸更新
    /// </summary>
    /// <param name="bytes"></param>
    public static void updateface(byte[] bytes)
    {
        var image = SCY.Utility.ByteArrToStr64(bytes); ;
        var imageType = "BASE64";
        var groupId = "group1";
        var userId = "user1";
        //可选参数options
        var options = new Dictionary<string, object>{
        {"user_info", "user's info"},
        {"quality_control", "NORMAL"},
        {"liveness_control", "LOW"}};
        var result = Cf.Instance().client.UserUpdate(image, imageType, groupId, userId,options);
        //Debug.Log(result);
    }

    /// <summary>
    /// 人脸删除
    /// </summary>
    /// <param name="token"></param>
    public static void deleteface(string token)
    {
        var userId = "user1";
        var groupId = "group1";
        var faceToken = token;
        var result = Cf.Instance().client.FaceDelete(userId, groupId, faceToken);
        //Debug.Log(result);
    }

    /// <summary>
    /// 用户信息查询
    /// </summary>
    /// <returns></returns>
    public static JObject userget()
    {
        var groupId = "group1";
        var userId = "user1";
        var result = Cf.Instance().client.UserGet(userId, groupId);
        //Debug.Log(result);
        return result;
    }

    /// <summary>
    /// 获取用户人脸列表
    /// </summary>
    public static JObject getfacelist()
    {
        var groupId = "group1";
        var userId = "user1";
        var result = Cf.Instance().client.FaceGetlist(userId, groupId);
        //Debug.Log(result);
        return result;
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    public static void userdelete()
    {
        var groupId = "group1";
        var userId = "user1";
        var result = Cf.Instance().client.UserDelete(groupId, userId);
        //Debug.Log(result);
    }

    /// <summary>
    /// 人脸对比
    /// </summary>
    /// <param name="bytes">摄像头数据</param>
    /// <param name="token">人脸库的token</param>
    /// <returns></returns>
    public static bool facecompare(byte[] bytes, string token)
    {
        var faces = new JArray
        {
            new JObject
            {
                {"image", SCY.Utility.ByteArrToStr64(bytes)},
                {"image_type", "BASE64"},
                {"face_type", "LIVE"},
                {"quality_control", "LOW"},
                {"liveness_control", "NONE"},
            },
            new JObject
            {
                {"image", token},
                {"image_type", "FACE_TOKEN"},
                {"face_type", "LIVE"},
                {"quality_control", "LOW"},
                {"liveness_control", "NONE"},
            }
        };
        var result = Cf.Instance().client.Match(faces);
        //Debug.Log(result);
        string errorMsg = result["error_msg"].ToString();
        if (errorMsg == "SUCCESS")
        {
            decimal score = SCY.Utility.JTokenToDecimal(result["result"]["score"]);
            if (score > 80)
                return true;
            else
                return false;
        }
        else
        {
            Debug.LogError(errorMsg);
            return false;
        }
    }
}
