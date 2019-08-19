using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Collections.Generic;

public class BaiDu
{
    #region 人脸识别
    /// <summary>
    /// 人脸检测 detectface
    /// </summary>
    /// <param name="bytes">图片流</param>
    /// <returns></returns>
    public static JObject detectface(byte[] bytes)
    {
        var options = new Dictionary<string, object>()
        {
            {"face_field", "age,beauty,expression,faceshape,gender,glasses,landmark,race,quality,facetype"},
            {"max_face_num", 1},
            {"face_type", "LIVE"}
        };
        return Cf.Instance().faceClient.Detect(SCY.Utility.ByteArrToStr64(bytes), "BASE64", options);
    }

    /// <summary>
    /// 人脸检测
    /// </summary>
    /// <param name="url">图片网络地址</param>
    /// <returns></returns>
    public static JObject detectface(string url)
    {
        var options = new Dictionary<string, object>()
        {
            {"face_field", "age,beauty,expression,faceshape,gender,glasses,landmark,race,quality,facetype"},
            {"max_face_num", 1},
            {"face_type", "LIVE"}
        };
        return Cf.Instance().faceClient.Detect(url, "URL", options);
    }

    /// <summary>
    /// 创建用户组
    /// </summary>
    public static void creatgroup()
    {
        var groupId = "group1";
        var result = Cf.Instance().faceClient.GroupAdd(groupId);
        //Debug.Log(result);
    }

    /// <summary>
    /// 删除用户组
    /// </summary>
    public static void groupdelete()
    {
        var groupId = "group1";
        var result = Cf.Instance().faceClient.GroupDelete(groupId);
        //Debug.Log(result);
    }

    /// <summary>
    /// 组列表查询
    /// </summary>
    public static JObject getgrouplist()
    {
        // 可选参数
        var options = new Dictionary<string, object> { { "start", 0 }, { "length", 50 } };
        var result = Cf.Instance().faceClient.GroupGetlist(options);
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
        var result = Cf.Instance().faceClient.UserAdd(image, "BASE64", groupId, userId, options);
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
        var result = Cf.Instance().faceClient.UserUpdate(image, imageType, groupId, userId,options);
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
        var result = Cf.Instance().faceClient.FaceDelete(userId, groupId, faceToken);
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
        var result = Cf.Instance().faceClient.UserGet(userId, groupId);
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
        var result = Cf.Instance().faceClient.FaceGetlist(userId, groupId);
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
        var result = Cf.Instance().faceClient.UserDelete(groupId, userId);
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
        var result = Cf.Instance().faceClient.Match(faces);
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

    #endregion

    #region 文字识别
    /// <summary>
    /// 手写检测
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static JObject handwriting(byte[] bytes)
    {
        var options = new Dictionary<string, object>()
        {
            {"recognize_granularity", "big"}
        };
        return Cf.Instance().ocrClient.Handwriting(bytes, options);
    }

    /// <summary>
    /// 通用文字识别
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static JObject general_basic(byte[] bytes)
    {
        //可选参数options
        var options = new Dictionary<string, object>{
            {"detect_language", "false"},
        };
        var result = Cf.Instance().ocrClient.GeneralBasic(bytes, options);
        return result;
    }

    /// <summary>
    /// 通用文字识别（高精度版）
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static JObject accurate_basic(byte[] bytes)
    {
        //可选参数options
        var options = new Dictionary<string, object>{
            {"detect_direction","true"},
            {"detect_language", "true"},
        };
        var result = Cf.Instance().ocrClient.AccurateBasic(bytes, options);
        return result;
    }

    /// <summary>
    /// 通用文字识别（含生僻字版）
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static JObject general_enhanced(byte[] bytes)
    {
        //可选参数options
        //var options = new Dictionary<string, object>{
        //    {"detect_language", "false"},
        //};
        var result = Cf.Instance().ocrClient.AccurateBasic(bytes/*, options*/);
        return result;
    }

    #endregion

    #region 人体分析
    /// <summary>
    /// 人体关键点识别
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static JObject bodyAnalysis(byte[] bytes)
    {
        return Cf.Instance().bodyClient.BodyAnalysis(bytes);
    }

    /// <summary>
    /// 人体检测与属性识别
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static JObject bodyAttr(byte[] bytes)
    {
        return Cf.Instance().bodyClient.BodyAttr(bytes);
    }

    /// <summary>
    /// 人流量统计
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static JObject bodyNum(byte[] bytes)
    {
        return Cf.Instance().bodyClient.BodyNum(bytes);
    }

    /// <summary>
    /// 手势识别
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static JObject gesture(byte[] bytes)
    {
        return Cf.Instance().bodyClient.Gesture(bytes);
    }

    /// <summary>
    /// 人像分割
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static JObject bodySeg(byte[] bytes)
    {
        var options = new Dictionary<string, object>{
        {"type", "foreground"}};
        /*
        可以通过设置type参数，自主设置返回哪些结果图，避免造成带宽的浪费
        1）可选值说明：
        labelmap - 二值图像，需二次处理方能查看分割效果
        scoremap - 人像前景灰度图
        foreground - 人像前景抠图，透明背景
        2）type 参数值可以是可选值的组合，用逗号分隔；如果无此参数默认输出全部3类结果图
         */
        return Cf.Instance().bodyClient.BodySeg(bytes, options);
    }

    /// <summary>
    /// 驾驶行为分析
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static JObject driverBehavior(byte[] bytes)
    {
        return Cf.Instance().bodyClient.DriverBehavior(bytes);
    }

    /// <summary>
    /// 人流量统计-动态版
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static JObject bodyTracking(byte[] bytes,string dynamic = "true")
    {
        return Cf.Instance().bodyClient.BodyTracking(bytes, dynamic);
    }
    #endregion
}
