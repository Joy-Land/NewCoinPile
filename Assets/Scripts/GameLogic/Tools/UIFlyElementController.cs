using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI中，生成金币飞行效果：一串金币从起点飞到终点
/// </summary>

public class UIFlyElementController : MonoBehaviour
{
    [SerializeField]
    private GameObject _coinTemplateGo;
    [SerializeField]
    private RectTransform _endPosTrans;
    public Vector3 EndPos { get { return _endPosTrans.transform.position; } }

    private List<GameObject> _coinGos;
    [SerializeField]
    private Canvas _canvas;
    public Canvas Canvas { get { return _canvas; } }
    [SerializeField]
    private Camera _cam;
    public Camera Camera { get { return _cam; } }
    public void Init()
    {
        _coinTemplateGo.SetActive(false);
        _coinGos = new List<GameObject>();
        gameObject.SetActive(true);
    }
    private void OnDestroy()
    {
        _coinGos.Clear();
        _coinGos = null;
    }

    public GameObject GetCoinGo()
    {
        if (_coinGos.Count == 0)
        {
            _coinGos.Add(Instantiate(_coinTemplateGo));
        }

        var go = _coinGos[0];
        _coinGos.Remove(go);
        return go;
    }

    public void RecyleCoinGo(GameObject coinGo)
    {
        coinGo.SetActive(false);
        coinGo.transform.SetParent(GetCoinGoParent());
        coinGo.transform.localPosition = Vector3.zero;
        coinGo.transform.localRotation = Quaternion.identity;
        coinGo.transform.localScale = Vector3.one;
        _coinGos.Add(coinGo);
    }

    public Transform GetCoinGoParent()
    {
        return _coinTemplateGo.transform.parent;
    }
}

public class UIFlyElementSys
{
    private static UIFlyElementSys _instance;
    public static UIFlyElementSys Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new UIFlyElementSys();
            }

            return _instance;
        }
    }

    private UIFlyElementController _uiFlyElementController;
    private UIFlyElementController UIFlyElementController
    {
        get
        {
            if (_uiFlyElementController == null)
            {
                _uiFlyElementController = GameObject.FindObjectOfType<UIFlyElementController>();
                _uiFlyElementController.Init();
            }

            return _uiFlyElementController;
        }
    }

    private UIFlyElementSys()
    {

    }

    public void Destroy()
    { }

    private Vector2 WorldPosToUiLocalPos(Vector3 worldPos)
    {
        var screenPos = UIFlyElementController.Camera.WorldToScreenPoint(worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(UIFlyElementController.Canvas.transform as RectTransform, screenPos, UIFlyElementController.Camera, out Vector2 localPos);
        return localPos;
    }

    // 起始点：世界坐标
    public float PlayFlyCoinEffect(Vector3 beginPos)
    {
        return PlayFlyCoinEffect(beginPos, UIFlyElementController.EndPos);
    }
    public float PlayFlyCoinEffect(Vector3 beginPos, Vector3 endPos, int cnt = 10)
    {
        return PlayFlyCoinEffect(WorldPosToUiLocalPos(beginPos), WorldPosToUiLocalPos(endPos), cnt);
    }

    // 播放一个金币飞行效果。返回值：效果持续时间
    private float PlayFlyCoinEffect(Vector2 beginPos, Vector2 endPos, int cnt = 10)
    {
        // TODO:可以直接在此处调用播放音效：金币飞行

        var seq = GetFlyCoinEffectSeq(beginPos, endPos, cnt);
        seq.Play();
        return seq.Duration(false);
    }

    private Sequence GetFlyCoinEffectSeq(Vector2 beginPos, Vector2 endPos, int cnt)
    {
        var step = Mathf.Clamp(cnt, 10, 30);
        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < step; i++)
        {
            GameObject cell = UIFlyElementController.GetCoinGo();
            Transform flyObjTf = cell.transform;
            flyObjTf.SetParent(UIFlyElementController.GetCoinGoParent(), false);
            flyObjTf.localPosition = endPos;
            var endPos3d = new Vector3(flyObjTf.localPosition.x, flyObjTf.localPosition.y, -100f);

            flyObjTf.localPosition = beginPos;
            flyObjTf.localPosition = new Vector3(flyObjTf.localPosition.x, flyObjTf.localPosition.y, -100f);
            flyObjTf.gameObject.SetActive(true);

            var path = GetPath(flyObjTf.localPosition, endPos3d);

            var tempSeq = DOTween.Sequence();

            var randomStartPos = new Vector2(flyObjTf.localPosition.x, flyObjTf.localPosition.y) + Vector2.up * 50 + UnityEngine.Random.insideUnitCircle * new Vector2(500, 200);
            Tween startMove = flyObjTf.DOLocalMove(new Vector3(randomStartPos.x, randomStartPos.y, -100f), 0.1f);
            tempSeq.Append(startMove);
            tempSeq.AppendInterval(0.1f);

            Tween moveT = flyObjTf.DOLocalPath(path, 0.6f, PathType.CatmullRom).SetEase(Ease.InSine);
            Tween scaleT = flyObjTf.DOScale(new Vector3(0.7f, 0.7f, 0.7f), 0.6f).SetEase(Ease.InBack);
            Tween rotateT = flyObjTf.DOLocalRotate(new Vector3(0, UnityEngine.Random.Range(-45,45), 360), 0.25f, RotateMode.FastBeyond360).SetLoops(2, LoopType.Restart).SetEase(Ease.Linear);
            tempSeq.Append(moveT);
            tempSeq.Join(scaleT);
            tempSeq.Join(rotateT);

            tempSeq.SetDelay(UnityEngine.Random.Range(0.005f, 0.025f) * i);
            tempSeq.SetUpdate(true);
            tempSeq.onComplete = () =>
            {
                GameObject ttObj = cell;
                UIFlyElementController.RecyleCoinGo(ttObj);
            };
            //加入总序列
            seq.Join(tempSeq);
        }
        //seq.SetUpdate(true);
        return seq;
    }
    private Vector3[] GetPath(Vector3 start, Vector3 end)
    {
        var middle = (start + end) / 2;
        middle += UnityEngine.Random.Range(-0.2f, 0.2f) * 5.4f * Vector3.right;
        return new Vector3[] { middle, end };
    }
}