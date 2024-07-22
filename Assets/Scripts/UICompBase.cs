using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using UnityEngine;

public interface IUIComp
{
    public void OnComponentInit();
    public void OnComponentShow();
    public void OnComponentHide();
    public void OnComponentDestroy();
}
public class UICompBase : MonoBehaviour, IUIComp
{
    public virtual void OnComponentInit()
    {

    }

    public virtual void OnComponentShow()
    {

    }

    public virtual void SetData(EventArgsPack args)
    {

    }

    public virtual void OnComponentDestroy()
    {

    }

    public virtual void OnComponentHide()
    {

    }




}
