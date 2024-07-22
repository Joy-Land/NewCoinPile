using Joyland.GamePlay;
using System;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using UnityEngine;
using UnityEngine.UI;

public class UINewHome : UIViewBase
{
    private int _UIOrder = 100;
    public override int UIOrder
    {
        get { return _UIOrder; }
        set { _UIOrder = value; }
    }

    public Button Btn_Close;
    public GameObject Obj_Container;

    public Image cc = null;

    public override void OnViewAwake(EventArgsPack args)
    {
        base.OnViewAwake(args);

        Btn_Close = GameObject.Find("UINewHome(Clone)/Btn_Close").GetComponent<Button>();
        Obj_Container = GameObject.Find("UINewHome(Clone)/Obj_Container");


        //PlayEffect("view_open", () =>
        //{
        //    console.error("fasdfasdfasdf");
        //});
    }

    public override void SetAnimatorNode()
    {
        base.SetAnimatorNode();
        if(this.animator == null)
        {

            this.animator = Obj_Container.GetComponent<Animator>() ?? Obj_Container.AddComponent<Animator>();
        }
    }

    public int PlayEffect(string stateName, Action callback = null)
    {
        var animator = Obj_Container.GetComponent<Animator>();

        animator.Update(0f);
        animator.Play(stateName, -1);

        if (callback == null) return 0;

        StartCoroutine(DelayRunEffectCallback(animator, stateName, callback));

        //CoroutineManager.Instance

        return 0;
    }

    public IEnumerator DelayRunEffectCallback(Animator animator, string stateName, Action callback)
    {
        // 状态机的切换发生在帧的结尾
        yield return new WaitForEndOfFrame();

        var info = animator.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName(stateName)) yield return null;

        yield return new WaitForSeconds(info.length);
        callback();
    }


    public void OnAnimationFinished()
    {
        console.error("fasdfsadf");
    }

    public override void OnViewShow(EventArgsPack args)
    {
        base.OnViewShow(args);
        RegistEvent();
    }
    public override void OnViewUpdate()
    {
        base.OnViewUpdate();

    }
    public override void OnViewUpdateBySecond()
    {
        base.OnViewUpdateBySecond();

    }
    public override void OnViewHide()
    {
        base.OnViewHide();

        UnregistEvent();
    }
    public override void OnViewDestroy()
    {
        base.OnViewDestroy();

    }

    public void RegistEvent()
    {
        Btn_Close.onClick.AddListener(OnBtn_CloseClicked);
    }


    public void OnBtn_CloseClicked()
    {
        UIManager.Instance.CloseUI(this);
    }

    public void UnregistEvent()
    {
        Btn_Close.onClick.RemoveListener(OnBtn_CloseClicked);
    }




}
