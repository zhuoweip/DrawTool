using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EventTriggerListener : EventTrigger
{
    public delegate void VoidDelegateParam(GameObject go);
    public VoidDelegateParam onClick;
    public VoidDelegateParam onDown;
    public VoidDelegateParam onEnter;
    public VoidDelegateParam onExit;
    public VoidDelegateParam onUp;
    public VoidDelegateParam onSelect;
    public VoidDelegateParam onUpdateSelect;
    public delegate void VoidDelegate();
    public VoidDelegate onLongPress;//长按事件


    private bool isPointDown;
    private bool longPressTriggered;
    private float durationThreshold =1f;
    private float timePressStarted;


    static public EventTriggerListener Get(GameObject go)
    {
        EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
        if (listener == null) listener = go.AddComponent<EventTriggerListener>();
        return listener;
    }

    private void Update()
    {
        if (isPointDown && !longPressTriggered)
        {
            if (Time.time - timePressStarted > durationThreshold)
            {
                longPressTriggered = true;
                if (onLongPress !=null)
                    onLongPress();
            }
        }
    }


    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!longPressTriggered)
        {
            if (onClick != null) onClick(gameObject);
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (onDown != null)
        {
            timePressStarted = Time.time;
            isPointDown = true;
            longPressTriggered = false;

            onDown(gameObject);
        } 
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (onEnter != null) onEnter(gameObject);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (onExit != null)
        {
            isPointDown = false;
            onExit(gameObject);
        } 
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (onUp != null)
        {
            isPointDown = false;
            onUp(gameObject);
        } 
    }

    public override void OnSelect(BaseEventData eventData)
    {
        if (onSelect != null) onSelect(gameObject);
    }

    public override void OnUpdateSelected(BaseEventData eventData)
    {
        if (onUpdateSelect != null) onUpdateSelect(gameObject);
    }

}