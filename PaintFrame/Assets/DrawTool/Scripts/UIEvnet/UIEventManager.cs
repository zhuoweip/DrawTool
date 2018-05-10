using UnityEngine;
using DevelopEngine;


public static class UIEventManager
{
    public static void PointDownEvent()
    {
        DrawManager drawManager = GameObject.FindObjectOfType<DrawManager>();
        drawManager.isForbid = true;
        drawManager.ReleaseAll();
    }

    public static void PointUpEvent()
    {
        GameObject.FindObjectOfType<DrawManager>().isForbid = false;
    }

    public static void PointEnterEvent()
    {
    }

    public static void PointExitEvent()
    {
    }

    /// <summary>
    /// 点击更换工具
    /// </summary>
    /// <param name="tool"></param>
    public static void ToolClickEvent(Tool tool)
    {
        GameObject.FindObjectOfType<DrawManager>().SelectTool(tool);
    }

    /// <summary>
    /// 撤回
    /// </summary>
    public static void UndoEvent()
    {
        GameObject.FindObjectOfType<History>().UnDo();
    }

    /// <summary>
    /// 重做
    /// </summary>
    public static void RedoEvent()
    {
        GameObject.FindObjectOfType<History>().Redo();
    }

    /// <summary>
    /// 改变工具大小
    /// </summary>
    /// <param name="size"></param>
    public static void ChangeSizeEvent(Tool tool)
    {
        GameObject.FindObjectOfType<DrawManager>().ChangeSize(tool);
    }

    /// <summary>
    /// 改变工具颜色
    /// </summary>
    /// <param name="tool"></param>
    public static void ChangColorEvent(Sprite spr)
    {
        GameObject.FindObjectOfType<DrawManager>().ChangColor(spr);
    }

    /// <summary>
    /// 改变背景图
    /// </summary>
    /// <param name="index"></param>
    public static void ChangBg(Tool tool)
    {
        GameObject.FindObjectOfType<DrawManager>().ChangBg(tool); 
    }

    /// <summary>
    /// 改变透明度
    /// </summary>
    /// <param name="value"></param>
    public static void ChangAlpha(float value)
    {
        GameObject.FindObjectOfType<DrawManager>().ChangAlpha(value);
    }

    #region 尺寸缩进
    public static void OnZoomInPress()
    {
        GameObject.FindObjectOfType<CameraZoom>().OnZoomInPress();
    }

    public static void OnZoomInPressRelease()
    {
        GameObject.FindObjectOfType<CameraZoom>().OnZoomInPressRelease();
    }

    public static void OnZoomOutPress()
    {
        GameObject.FindObjectOfType<CameraZoom>().OnZoomOutPress();
    }

    public static void OnZoomOutPressRelease()
    {
        GameObject.FindObjectOfType<CameraZoom>().OnZoomOutPressRelease();
    }

    public static void ResetZoom()
    {
        GameObject.FindObjectOfType<CameraZoom>().ResetZoom();
    }
    #endregion

    /// <summary>
    /// 删除模式
    /// </summary>
    public static void Delete()
    {
        GameObject.FindObjectOfType<DrawManager>().Delete();
    }

    public static void TakePhoto()
    {
        GameObject.FindObjectOfType<GetCamera>().TakePhoto();
    }

    /// <summary>
    ///上传留言
    /// </summary>
    public static void LeaveMsg()
    {
        //GameObject.FindObjectOfType<DrawManager>().ShowCursor(false);
        GameObject.FindObjectOfType<GetCamera>().Save();
    }

    /// <summary>
    /// 留言回顾
    /// </summary>
    public static void ReviewMsg()
    {
        //GameObject.FindObjectOfType<DrawManager>().ShowCursor(false);
        GameObject.FindObjectOfType<GetCamera>().ImgGet();
    }

    /// <summary>
    /// 打印
    /// </summary>
    public static void Print()
    {
        GameObject.FindObjectOfType<WebPrint>().PrintScreen();
    }
}
