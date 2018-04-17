using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 物体挂不挂button无所谓，挂button只是为了显示点击的效果
/// </summary>

public enum ClickEventType
{
    None,
    Tool,
    Undo,
    Redo,
    ChangeSize,
    ResetZoom,
    Delete,
    TakePhote,
    LeaveMsg,
    ReviewMsg,
    ChangeColor,
    ChangeBg,
    ChangAlpha
}

public enum DownEventType
{
    None,
    OnZoomInPress,
    OnZoomOutPress,
}

public enum UpEvnetType
{
    None,
    OnZoomInPressRelease,
    OnZoomOutPressRelease,
}


public class UIEventController : MonoBehaviour
{
    public ClickEventType clickType = ClickEventType.None;
    public DownEventType downType = DownEventType.None;
    public UpEvnetType upType = UpEvnetType.None;
    private Tool tool;
    private Sprite spr;
    private Slider slider;

    private void Awake()
    {
        tool = this.GetComponent<Tool>();
        if (this.GetComponent<Image>() != null)
            spr = this.GetComponent<Image>().sprite;
        slider = this.GetComponent<Slider>();
        if (slider !=null)
        {
            slider.minValue = 0.2f;
            slider.maxValue = 1;
        }
    }

    private void Start()
    {
        EventTriggerListener.Get(this.gameObject).onClick = OnPointerClick;
        EventTriggerListener.Get(this.gameObject).onDown = OnPointerDown;
        EventTriggerListener.Get(this.gameObject).onUp = OnPointerUp;
        EventTriggerListener.Get(this.gameObject).onEnter = OnPointerEnter;
        EventTriggerListener.Get(this.gameObject).onExit = OnPointerExit;
        EventTriggerListener.Get(this.gameObject).onLongPress = OnLongPress;
    }

    private void OnLongPress()
    {

    }

    private void OnPointerClick(GameObject go)
    {
        
    }

    private void ChangeEvent()
    {
        switch (clickType)
        {
            case ClickEventType.None:
                break;

            case ClickEventType.Tool:
                UIEventManager.ToolClickEvent(tool);
                break;

            case ClickEventType.Undo:
                UIEventManager.UndoEvent();
                break;

            case ClickEventType.Redo:
                UIEventManager.RedoEvent();
                break;

            case ClickEventType.ChangeSize:
                UIEventManager.ChangeSizeEvent(tool);
                break;

            case ClickEventType.ResetZoom:
                UIEventManager.ResetZoom();
                break;

            case ClickEventType.Delete:
                UIEventManager.Delete();
                break;

            case ClickEventType.TakePhote:
                UIEventManager.TakePhoto();
                break;

            case ClickEventType.LeaveMsg:
                UIEventManager.LeaveMsg();
                break;

            case ClickEventType.ReviewMsg:
                UIEventManager.ReviewMsg();
                break;

            case ClickEventType.ChangeColor:
                UIEventManager.ChangColorEvent(spr);
                break;

            case ClickEventType.ChangeBg:
                UIEventManager.ChangBg(tool);
                break;

            case ClickEventType.ChangAlpha:
                slider.onValueChanged.RemoveAllListeners();
                if (slider != null)
                    slider.onValueChanged.AddListener(OnVauluChanged);
                break;

            default:
                break;
        }
    }

    private void OnVauluChanged(float value)
    {
        UIEventManager.ChangAlpha(value);
    }

    private void OnPointerDown(GameObject go)
    {
        UIEventManager.PointDownEvent();
        ChangeEvent();
        if (downType == DownEventType.OnZoomInPress)
        {
            UIEventManager.OnZoomInPress();
        }
        else if (downType == DownEventType.OnZoomOutPress)
        {
            UIEventManager.OnZoomOutPress();
        }
    }

    private void OnPointerUp(GameObject go)
    {
        UIEventManager.PointUpEvent();
        if (upType == UpEvnetType.OnZoomInPressRelease)
        {
            UIEventManager.OnZoomInPressRelease();
        }
        else if (upType == UpEvnetType.OnZoomOutPressRelease)
        {
            UIEventManager.OnZoomOutPressRelease();
        }
    }

    private void OnPointerEnter(GameObject go)
    {
        UIEventManager.PointEnterEvent();
    }

    private void OnPointerExit(GameObject go)
    {
        UIEventManager.PointExitEvent();  
    }
}
