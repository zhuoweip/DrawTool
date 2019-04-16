using DevelopEngine;

public class AIManager : MonoSingleton<AIManager> {

    public AIType type = AIType.BaiDu;
    [EnumLabel("模板类型")]
    public Template template = Template.cf_yuren_guangtou;

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

    public string AIOcrDetect(byte[] bytes, OcrType ocrType = OcrType.GENERAL_BASIC)
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

    public string AIBodyAnalysisDetector(byte[] bytes, BodyType bodyType = BodyType.GESTURE)
    {
        string result = string.Empty;
        if (TcpManager.IsOnLine())//在线检测
        {
            switch (bodyType)
            {
                case BodyType.BODYANALYSIS:
                    result = BodyAnalysisDetector.Instance.BodyAnalysis(bytes).ToString();
                    break;
                case BodyType.BODYATTR:
                    result = BodyAnalysisDetector.Instance.BodyAttr(bytes).ToString();
                    break;
                case BodyType.BODYNUM:
                    result = BodyAnalysisDetector.Instance.BodyNum(bytes).ToString();
                    break;
                case BodyType.GESTURE:
                    UnityEngine.Debug.LogError("GESTURE"); 
                    result = BodyAnalysisDetector.Instance.Gesture(bytes).ToString();
                    break;
                case BodyType.BODYSEG:
                    result = BodyAnalysisDetector.Instance.BodySeg(bytes).ToString();
                    break;
                case BodyType.DRIVERBEHAVIOR:
                    result = BodyAnalysisDetector.Instance.DriverBehavior(bytes).ToString();
                    break;
                case BodyType.BODYTRACKING:
                    result = BodyAnalysisDetector.Instance.BodyTracking(bytes).ToString();
                    break;
                default:
                    break;
            }

        }
        return result;
    }
}
