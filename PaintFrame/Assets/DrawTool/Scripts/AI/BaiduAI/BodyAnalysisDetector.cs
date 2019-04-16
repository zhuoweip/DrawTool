using Newtonsoft.Json.Linq;
using UnityEngine;
using DevelopEngine;

public class BodyAnalysisDetector : MonoSingleton<BodyAnalysisDetector> {

    string APPID = "16030010";
    string APIKey = "uOXuYEBh6MnFESbwQd5n97kX";
    string SecretKey = "z1ND5oINGGEzoR7SGrDyrGFbGl7WFtFN";

    private void Awake()
    {
        System.Net.ServicePointManager.ServerCertificateValidationCallback +=
               delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                           System.Security.Cryptography.X509Certificates.X509Chain chain,
                           System.Net.Security.SslPolicyErrors sslPolicyErrors)
               {
                   return true; // **** Always accept
               };

        Cf.Instance().setBodyAnalysisAppInfo(APIKey, SecretKey);
    }

    public JObject BodyAnalysis(byte[] bytes)
    {
        try
        {
            return BaiDu.bodyAnalysis(bytes);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }

    public JObject BodyAttr(byte[] bytes)
    {
        try
        {
            return BaiDu.bodyAttr(bytes);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }

    public JObject BodyNum(byte[] bytes)
    {
        try
        {
            return BaiDu.bodyNum(bytes);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }

    public JObject Gesture(byte[] bytes)
    {
        try
        {
            return BaiDu.gesture(bytes);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }

    public JObject BodySeg(byte[] bytes)
    {
        try
        {
            return BaiDu.bodySeg(bytes);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }

    public JObject DriverBehavior(byte[] bytes)
    {
        try
        {
            return BaiDu.driverBehavior(bytes);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }

    public JObject BodyTracking(byte[] bytes)
    {
        try
        {
            return BaiDu.bodyTracking(bytes);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }
}
