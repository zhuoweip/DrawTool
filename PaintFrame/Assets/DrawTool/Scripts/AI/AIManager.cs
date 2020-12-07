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

    /// <summary>
    /// 人脸搜索
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="faceSearchType"></param>
    /// <returns></returns>
    public string AIFaceSearch(byte[] bytes,FaceSearchType faceSearchType = FaceSearchType.OneN)
    {
        string result = string.Empty;
        switch (faceSearchType)
        {
            case FaceSearchType.OneN:
                result = FaceDetector.Instance.FaceSearch(bytes);
                break;
            case FaceSearchType.MN:
                result = FaceDetector.Instance.FaceMultiSearch(bytes);
                break;
            default:
                break;
        }
        return result;
    }

    /// <summary>
    /// 人脸比对
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public bool FaceMatch(byte[] bytes)
    {
        return FaceDetector.Instance.FaceMatch(bytes);
    }

    /// <summary>
    /// 人脸融合
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="rImg"></param>
    public void AIFaceMerge(byte[] bytes,UnityEngine.UI.RawImage rImg)
    {
        FacialRecognition.Instance.FaceMerge(bytes, rImg, template);
    }

    /// <summary>
    /// Ocr识别
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="ocrType"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 人体分析
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="bodyType"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 组识别，获取，添加用户
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="type"></param>
    public void FaceGroupDetect(byte[] bytes, FaceGroupDetectType type)
    {
        switch (type)
        {
            case FaceGroupDetectType.FaceCreatGroup:
                FaceDetector.Instance.FaceCreatGroup();
                break;
            case FaceGroupDetectType.GetGroupList:
                FaceDetector.Instance.GetUserList();
                break;
            case FaceGroupDetectType.SignUpNewPerson:
                FaceDetector.Instance.SignUpNewPerson(bytes);
                break;
            default:
                break;
        }
    }
}
