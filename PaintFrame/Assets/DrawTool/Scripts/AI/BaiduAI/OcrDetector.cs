using DevelopEngine;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OcrDetector : MonoSingleton<OcrDetector>
{
    string APPID = "15757780";
    string APIKey = "BH3VIXOv98pBphZhHSZqcRFD";
    string SecretKey = "Qzv464UlVDjreu9pkumWV8Zf0T3XBEwT";

    private void Awake()
    {
        System.Net.ServicePointManager.ServerCertificateValidationCallback +=
               delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                           System.Security.Cryptography.X509Certificates.X509Chain chain,
                           System.Net.Security.SslPolicyErrors sslPolicyErrors)
               {
                   return true; // **** Always accept
               };

        Cf.Instance().setOcrAppInfo(APIKey, SecretKey);
    }

    public JObject Handwriting(byte[] bytes)
    {
        try
        {
            return BaiDu.handwriting(bytes);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }

    public JObject GeneralBasic(byte[] bytes)
    {
        try
        {
            return BaiDu.general_basic(bytes);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }

    public JObject AccurateBasic(byte[] bytes)
    {
        try
        {
            return BaiDu.accurate_basic(bytes);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }

    public JObject GeneralEnhanced(byte[] bytes)
    {
        try
        {
            return BaiDu.general_enhanced(bytes);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            throw;
        }
    }
}
