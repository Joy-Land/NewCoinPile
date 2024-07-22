using Joyland.GamePlay;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using UnityEngine;

public interface IUIView
{

    public int UIOrder { get; set; }
    public bool IsActive { get; set; }
    public void OnViewAwake(EventArgsPack args);
    public void OnViewShow(EventArgsPack args);
    public void OnViewUpdate();
    public void OnViewUpdateBySecond();
    public void OnViewHide();
    public void OnViewDestroy();
}
public class UIViewBase : MonoBehaviour, IUIView
{

    public Animator animator;
    public virtual int UIOrder { get; set; }

    public bool IsActive { get; set; }


    public virtual void SetAnimatorNode()
    {

    }

    public virtual void OnEnable()
    {
        IsActive = true;
    }

    public virtual void OnDisable() 
    {
        IsActive = false;
    }

    public virtual void OnViewAwake(EventArgsPack args)
    {

    }

    public virtual void OnViewShow(EventArgsPack args)
    {

    }

    public virtual void OnViewDestroy()
    {

    }

    public virtual void OnViewHide()
    {

    }

    public virtual void OnViewUpdate()
    {

    }

    public virtual void OnViewUpdateBySecond()
    {

    }


}
