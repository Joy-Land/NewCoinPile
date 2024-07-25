using Joyland.GamePlay;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UISetting : UIViewBase
{
    private int _UIOrder = 100;
    public override int UIOrder
    {
        get { return _UIOrder; }
        set { _UIOrder = value; }
    }

    public Image Img_Bg;
    public GameObject Obj_Container;
    public Image Img_ContainerBg;
    public Image Img_Title;
    public Text Txt_Title;
    public Button Btn_Close;
    public GameObject Obj_AudioMusic;
    public Image Img_AudioMusic;
    public Text Txt_AudioMusic;
    public Button Btn_AudioMusic;
    public GameObject Obj_AudioMusicOff;
    public GameObject Obj_AudioMusicOn;
    public GameObject Obj_AudioEffect;
    public Image Img_AudioEffect;
    public Text Txt_AudioEffect;
    public Button Btn_AudioEffect;
    public GameObject Obj_AudioEffectOff;
    public GameObject Obj_AudioEffectOn;
    public GameObject Obj_Vibrate;
    public Image Img_Vibrate;
    public Text Txt_Vibrate;
    public Button Btn_Vibrate;
    public GameObject Obj_VibrateOff;
    public GameObject Obj_VibrateOn;
    public GameObject Obj_OutGame;
    public GameObject Obj_InGame;
    public Button Btn_Leave;


    private bool m_AudioEffectFlag = true;
    private bool m_AudioMusicFlag = true;
    private bool m_VibrateFlag = true;
    public override void OnViewAwake(EventArgsPack args)
    {
        base.OnViewAwake(args);
        Img_Bg = transform.Find("Img_Bg").GetComponent<Image>();
        Obj_Container = transform.Find("Obj_Container").gameObject;
        Img_ContainerBg = transform.Find("Obj_Container/Img_ContainerBg").GetComponent<Image>();
        Img_Title = transform.Find("Obj_Container/Img_Title").GetComponent<Image>();
        Txt_Title = transform.Find("Obj_Container/Img_Title/Txt_Title").GetComponent<Text>();
        Btn_Close = transform.Find("Obj_Container/Btn_Close").GetComponent<Button>();
        Obj_AudioMusic = transform.Find("Obj_Container/Obj_AudioMusic").gameObject;
        Img_AudioMusic = transform.Find("Obj_Container/Obj_AudioMusic/Img_AudioMusic").GetComponent<Image>();
        Txt_AudioMusic = transform.Find("Obj_Container/Obj_AudioMusic/Txt_AudioMusic").GetComponent<Text>();
        Btn_AudioMusic = transform.Find("Obj_Container/Obj_AudioMusic/Btn_AudioMusic").GetComponent<Button>();
        Obj_AudioMusicOff = transform.Find("Obj_Container/Obj_AudioMusic/Btn_AudioMusic/Obj_AudioMusicOff").gameObject;
        Obj_AudioMusicOn = transform.Find("Obj_Container/Obj_AudioMusic/Btn_AudioMusic/Obj_AudioMusicOn").gameObject;
        Obj_AudioEffect = transform.Find("Obj_Container/Obj_AudioEffect").gameObject;
        Img_AudioEffect = transform.Find("Obj_Container/Obj_AudioEffect/Img_AudioEffect").GetComponent<Image>();
        Txt_AudioEffect = transform.Find("Obj_Container/Obj_AudioEffect/Txt_AudioEffect").GetComponent<Text>();
        Btn_AudioEffect = transform.Find("Obj_Container/Obj_AudioEffect/Btn_AudioEffect").GetComponent<Button>();
        Obj_AudioEffectOff = transform.Find("Obj_Container/Obj_AudioEffect/Btn_AudioEffect/Obj_AudioEffectOff").gameObject;
        Obj_AudioEffectOn = transform.Find("Obj_Container/Obj_AudioEffect/Btn_AudioEffect/Obj_AudioEffectOn").gameObject;
        Obj_Vibrate = transform.Find("Obj_Container/Obj_Vibrate").gameObject;
        Img_Vibrate = transform.Find("Obj_Container/Obj_Vibrate/Img_Vibrate").GetComponent<Image>();
        Txt_Vibrate = transform.Find("Obj_Container/Obj_Vibrate/Txt_Vibrate").GetComponent<Text>();
        Btn_Vibrate = transform.Find("Obj_Container/Obj_Vibrate/Btn_Vibrate").GetComponent<Button>();
        Obj_VibrateOff = transform.Find("Obj_Container/Obj_Vibrate/Btn_Vibrate/Obj_VibrateOff").gameObject;
        Obj_VibrateOn = transform.Find("Obj_Container/Obj_Vibrate/Btn_Vibrate/Obj_VibrateOn").gameObject;
        Obj_OutGame = transform.Find("Obj_Container/Obj_OutGame").gameObject;
        Obj_InGame = transform.Find("Obj_Container/Obj_InGame").gameObject;
        Btn_Leave = transform.Find("Obj_Container/Obj_InGame/Btn_Leave").GetComponent<Button>();

        Img_Bg.GetComponent<RectTransform>().offsetMin = UIManager.Instance.FullOffset.offsetMin;
        Img_Bg.GetComponent<RectTransform>().offsetMax = UIManager.Instance.FullOffset.offsetMax;

        m_VibrateFlag = J.Minigame.EnableVibrate;
        m_AudioEffectFlag = AudioManager.Instance.EffectMusicEnable;
        m_AudioMusicFlag = AudioManager.Instance.BackgroundMusicEnable;

    }
    public override void SetAnimatorNode()
    {
        base.SetAnimatorNode();

        if (this.animator == null)
        {
            if(Obj_Container.GetComponent<Animator>() == null)
            {
                this.animator = Obj_Container.AddComponent<Animator>();
            }
        }

    }

    public override void OnViewShow(EventArgsPack args)
    {
        base.OnViewShow(args);
        RegistEvent();

        UpdateStatus();
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
        Btn_AudioMusic.onClick.AddListener(OnBtn_AudioMusicClicked);
        Btn_AudioEffect.onClick.AddListener(OnBtn_AudioEffectClicked);
        Btn_Vibrate.onClick.AddListener(OnBtn_VibrateClicked);
        Btn_Leave.onClick.AddListener(OnBtn_LeaveClicked);
    }

    private void UpdateStatus()
    {
        Obj_AudioMusicOn.SetActive(m_AudioMusicFlag);
        Obj_AudioMusicOff.SetActive(!m_AudioMusicFlag);

        Obj_AudioEffectOn.SetActive(m_AudioEffectFlag);
        Obj_AudioEffectOff.SetActive(!m_AudioEffectFlag);

        Obj_VibrateOn.SetActive(m_VibrateFlag);
        Obj_VibrateOff.SetActive(!m_VibrateFlag);

        AudioManager.Instance.BackgroundMusicEnable = m_AudioMusicFlag;
        AudioManager.Instance.EffectMusicEnable = m_AudioEffectFlag;
        J.Minigame.EnableVibrate = m_VibrateFlag;

        AudioManager.Instance.SaveAudioConfig();
    }


    public void OnBtn_CloseClicked()
    {
        UIManager.Instance.CloseUI(UIViewID.UISetting);
    }

    public void OnBtn_AudioMusicClicked()
    {
        m_AudioMusicFlag = !m_AudioMusicFlag;
        UpdateStatus();
    }

    public void OnBtn_AudioEffectClicked()
    {
        m_AudioEffectFlag = !m_AudioEffectFlag;
        UpdateStatus();
    }

    public void OnBtn_VibrateClicked()
    {
        m_VibrateFlag = !m_VibrateFlag;
        UpdateStatus();
    }

    public void OnBtn_LeaveClicked()
    {

    }

    public void UnregistEvent()
    {
        Btn_Close.onClick.RemoveListener(OnBtn_CloseClicked);
        Btn_AudioMusic.onClick.RemoveListener(OnBtn_AudioMusicClicked);
        Btn_AudioEffect.onClick.RemoveListener(OnBtn_AudioEffectClicked);
        Btn_Vibrate.onClick.RemoveListener(OnBtn_VibrateClicked);
        Btn_Leave.onClick.RemoveListener(OnBtn_LeaveClicked);
    }


}
