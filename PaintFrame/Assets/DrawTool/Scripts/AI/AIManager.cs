using DevelopEngine;

public class AIManager : MonoSingleton<AIManager> {

    public AIType type = AIType.Tencent;
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
}
