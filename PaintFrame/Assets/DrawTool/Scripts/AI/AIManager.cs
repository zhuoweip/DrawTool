using DevelopEngine;

public class AIManager : MonoSingleton<AIManager> {

    public AIType type = AIType.BaiDu;
    [EnumLabel("模板类型")]
    public Template template = Template.cf_yuren_guangtou;
    [EnumLabel("模板类型")]
    public OcrType ocrType = OcrType.GENERAL_BASIC;

    public string AIFaceDetect(byte[] bytes)
    {
        string result = string.Empty;
        if (TcpManager.IsOnLine())//在线检测
        {
            if (type == AIType.Tencent)
            {
                result = FacialRecognition.Instance.FaceDect(bytes);
            }
            else
            {
                result = FaceDetector.Instance.FaceDetect(bytes).ToString();
            }
        }
        return result;
    }

    public void AIFaceMerge(byte[] bytes,UnityEngine.UI.RawImage rImg)
    {
        FacialRecognition.Instance.FaceMerge(bytes, rImg, template);
    }

    public string AIOcrDetect(byte[] bytes)
    {
        string result = string.Empty;
        if (TcpManager.IsOnLine())//在线检测
        {
            switch (ocrType)
            {
                case OcrType.GENERAL_BASIC:
                    result = OcrDetector.Instance.GeneralBasic(bytes).ToString();
                    break;
                case OcrType.ACCURATE_BASIC:
                    result = OcrDetector.Instance.AccurateBasic(bytes).ToString();
                    break;
                case OcrType.GENERAL_ENHANCED:
                    result = OcrDetector.Instance.GeneralEnhanced(bytes).ToString();
                    break;
                case OcrType.HANDWRITING:
                    result = OcrDetector.Instance.Handwriting(bytes).ToString();
                    break;
                default:
                    break;
            }
            
        }
        return result;
    }
}
