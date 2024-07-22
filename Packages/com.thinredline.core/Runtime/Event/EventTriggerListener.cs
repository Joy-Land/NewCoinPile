using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class EventTriggerListener : EventTrigger
{
    public delegate void VoidEventHandler(GameObject go);

    /// <summary>
    /// 点击事件
    /// </summary>
    public VoidEventHandler GoToClick;

    /// <summary>
    /// 按下事件
    /// </summary>
    public VoidEventHandler GoToDown;

    /// <summary>
    /// 抬起事件
    /// </summary>
    public VoidEventHandler GoToUp;

    /// <summary>
    /// 进入UI事件
    /// </summary>
    public VoidEventHandler GoToOnEnter;

    /// <summary>
    /// 离开UI事件
    /// </summary>
    public VoidEventHandler GoToExit;

    /// <summary>
    /// 开始拖拽
    /// </summary>
    public VoidEventHandler GoToBeginDrag;

    /// <summary>
    /// 拖拽中
    /// </summary>
    public VoidEventHandler GoToDrag;

    /// <summary>
    /// 拖拽结束
    /// </summary>
    public VoidEventHandler GoToEndDrag;

    /// <summary>
    /// 物体选中时
    /// </summary>
    public VoidEventHandler GoToSelect;

    /// <summary>
    /// 物体被取消选中时
    /// </summary>
    public VoidEventHandler GoToDeselect;

    /// <summary>
    /// 拖拽结束(拖拽结束后的位置(即鼠标位置)如果有物体，则那个物体调用)
    /// </summary>
    public VoidEventHandler GoToDrop;

    /// <summary>
    /// 滚轮滚动
    /// </summary>
    public VoidEventHandler GoToScroll;

    /// <summary>
    /// 选中的物体每帧开始调用
    /// </summary>
    public VoidEventHandler GoToUpdateSelect;

    /// <summary>
    /// 取消按钮被按下时(与InputManager里的Cancel按键相对应，PC上默认的是Esc键)，前提条件是物体被选中
    /// </summary>
    public VoidEventHandler GoToCanel;

    /// <summary>
    /// 拖拽时的初始化，跟IPointerDownHandler差不多，在按下时调用 
    /// </summary>
    public VoidEventHandler GoToInitializePotentialDrag;

    /// <summary>
    /// 临时对象
    /// </summary>
    private static GameObject tempGo;

    /// <summary>
    /// 得到要添加的游戏对象
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    static public EventTriggerListener Get(GameObject go)
    {
        EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
        if (listener == null)
        {
            listener = go.AddComponent<EventTriggerListener>();
            tempGo = go;
            print("为空，以添加成功");
        }

        return listener;
    }

    /// <summary>
    /// 添加可定义操作的
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="act">事件执行的方法</param>
    public void AddListener(/*GameObject go,*/ EventTriggerType eventType, UnityAction<BaseEventData> act)
    {
        EventTrigger trigger = tempGo.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            tempGo.AddComponent<EventTrigger>();
        }
        if (trigger.triggers.Count == 0)
        {
            trigger.triggers = new List<Entry>();
        }
        //定义所要绑定的事件类型   
        EventTrigger.Entry entry = new EventTrigger.Entry();
        //设置事件类型    
        entry.eventID = eventType;
        //定义回调函数    
        UnityAction<BaseEventData> callback = new UnityAction<BaseEventData>(act);
        //设置回调函数    
        entry.callback.AddListener(callback);
        //添加事件触发记录到GameObject的事件触发组件    
        trigger.triggers.Add(entry);
    }

    public void RemoveAllListener()
    {
        EventTrigger trigger = tempGo.GetComponent<EventTrigger>();
        if (trigger != null)
        {
            trigger.triggers.Clear();
            GameObject.Destroy(trigger);
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        if (GoToBeginDrag != null)
            GoToBeginDrag(gameObject);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (GoToOnEnter != null)
        {
            GoToOnEnter(gameObject);
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        if (GoToExit != null)
        {
            GoToExit(gameObject);
        }
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (GoToDown != null)
        {
            GoToDown(gameObject);
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        if (GoToClick != null)
        {
            GoToClick(gameObject);
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if (GoToUp != null)
        {
            GoToUp(gameObject);
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        if (GoToDrag != null)
        {
            GoToDrag(gameObject);
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        if (GoToEndDrag != null)
        {
            GoToEndDrag(gameObject);
        }
    }

    public override void OnCancel(BaseEventData eventData)
    {
        base.OnCancel(eventData);
        if (GoToCanel != null)
        {
            GoToCanel(gameObject);
        }
    }

    public override void OnDrop(PointerEventData eventData)
    {
        base.OnDrop(eventData);
        if (GoToDrop != null)
        {
            GoToDrop(gameObject);
        }
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        if (GoToDeselect != null)
        {
            GoToDeselect(gameObject);
        }
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        if (GoToSelect != null)
        {
            GoToSelect(gameObject);
        }
    }

    public override void OnScroll(PointerEventData eventData)
    {
        base.OnScroll(eventData);
        if (GoToScroll != null)
        {
            GoToScroll(gameObject);
        }
    }

    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        base.OnInitializePotentialDrag(eventData);
        if (GoToInitializePotentialDrag != null)
        {
            GoToInitializePotentialDrag(gameObject);
        }
    }

    public override void OnUpdateSelected(BaseEventData eventData)
    {
        base.OnUpdateSelected(eventData);
        if (GoToUpdateSelect != null)
        {
            GoToUpdateSelect(gameObject);
        }
    }
}


