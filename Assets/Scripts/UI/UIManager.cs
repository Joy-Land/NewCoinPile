using Cysharp.Threading.Tasks;
using Joyland.GamePlay;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThinRL.Core;
using ThinRL.Core.Pool;
using ThinRL.Core.Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

namespace Joyland.GamePlay
{
    public struct UIViewConfig
    {
        public string packageName;
        public string bundleName;
        public UIViewLayerEnum layer;
    }

    public struct UICompConfig
    {
        public string packageName;
        public string bundleName;
        public int preloadAmount;
    }

    public enum UIViewLayerEnum : int
    {
        Lowest = 0,
        Low = 1,
        Middle = 2,
        Hight = 3,
        Hightest = 4,
        Count = 5,
    }

    /// <summary>
    /// 挂载到Canvas上
    /// </summary>
    public class UIManager : MonoSingleton<UIManager>
    {

        private class UIViewInfo
        {
            public UIViewID id;
            public EventArgsPack args;
            public IUIView view;
            public bool isClose = false;
            public bool isHiding = false;
            public Coroutine openCoroutine;
            public Coroutine closeCoroutine;
            /**用于计时的字段 */
            public float secondTimer = 0;
            public UIViewInfo()
            {
                isClose = false;
                isHiding = false;
                secondTimer = 0;
            }
        }

        private bool m_IsOpening = false;
        private bool m_IsHiding = false;
        private bool m_IsClosing = false;

        public delegate void LoadUIAssetCallback(IUIView uiView);

        //public LoadUIAssetCallback LoadUIAsset;

        private (Vector2 offsetMin, Vector2 offsetMax) m_SafeOffset = new ValueTuple<Vector2, Vector2>();
        public (Vector2 offsetMin, Vector2 offsetMax) SafeOffset
        {
            get
            {
                return m_SafeOffset;
            }
        }
        private (Vector2 offsetMin, Vector2 offsetMax) m_FullOffset = new ValueTuple<Vector2, Vector2>();
        public (Vector2 offsetMin, Vector2 offsetMax) FullOffset
        {
            get
            {
                return m_FullOffset;
            }
        }

        private Queue<UIViewInfo> m_UIOpenQueue;
        private Queue<IUIView> m_UICloseQueue;
        private Queue<IUIView> m_UIHideQueue;
        private Canvas m_RootCanvas;

        private Dictionary<int, PrefabPool> m_UICompPoolDic = new Dictionary<int, PrefabPool>();
        private Dictionary<UIViewLayerEnum, List<UIViewInfo>> m_UIViewLayerListDic;

        private static Dictionary<UIViewLayerEnum, GameObject> s_UIViewLayerRootDic = new Dictionary<UIViewLayerEnum, GameObject>();

        private GameObject m_BackgroundNode;
        private GameObject m_SingleImageNode;
        private GameObject m_MultiImageNode;

        public void Init()
        {
            m_UIViewConfig = new Dictionary<int, UIViewConfig>();
            m_RootCanvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
            if(m_RootCanvas == null)
            {
                console.error("需要先确保UI根节点加载完成");
                return;
            }

            if (!m_RootCanvas.TryGetComponent<CanvasScaler>(out var canvasScaler))
            {
                return;
            }
            if (!m_RootCanvas.TryGetComponent<Canvas>(out var can))
            {
                return;
            }
            float standard_width = canvasScaler.referenceResolution.x;        //初始宽度  
            float standard_height = canvasScaler.referenceResolution.y;       //初始高度  
            console.error("fzy ????", standard_width, standard_height, Screen.width, Screen.height, can.pixelRect.ToString(), can.GetComponent<RectTransform>().sizeDelta);


            m_UIOpenQueue = new Queue<UIViewInfo>();
            m_UICloseQueue = new Queue<IUIView>();
            m_UIHideQueue = new Queue<IUIView>();
            m_UICompPoolDic = new Dictionary<int, PrefabPool>();
            m_UIViewLayerListDic = new Dictionary<UIViewLayerEnum, List<UIViewInfo>>();
            m_UIViewLayerListDic.Add(UIViewLayerEnum.Lowest, new List<UIViewInfo>());
            m_UIViewLayerListDic.Add(UIViewLayerEnum.Low, new List<UIViewInfo>());
            m_UIViewLayerListDic.Add(UIViewLayerEnum.Middle, new List<UIViewInfo>());
            m_UIViewLayerListDic.Add(UIViewLayerEnum.Hight, new List<UIViewInfo>());
            m_UIViewLayerListDic.Add(UIViewLayerEnum.Hightest, new List<UIViewInfo>());

            CreateUINode();
            AdjustUI();
            CreateUINode();
        }

        public void CreateUINode()
        {
            m_BackgroundNode = GameObject.Find("Canvas/Background").gameObject;
            m_SingleImageNode = GameObject.Find("Canvas/Background/SingleImageNode");
            m_MultiImageNode = GameObject.Find("Canvas/Background/MultiImageNode");

            RectTransform lowest = GameObject.Find("Canvas/LowestLayer")?.GetComponent<RectTransform>();
            RectTransform low = GameObject.Find("Canvas/LowLayer")?.GetComponent<RectTransform>();
            RectTransform middle = GameObject.Find("Canvas/MiddleLayer")?.GetComponent<RectTransform>();
            RectTransform hight = GameObject.Find("Canvas/HightLayer")?.GetComponent<RectTransform>();
            RectTransform hightest = GameObject.Find("Canvas/HightestLayer")?.GetComponent<RectTransform>();
            if (lowest == null)
                lowest = new GameObject("LowestLayer", new System.Type[] { typeof(Canvas), typeof(GraphicRaycaster) }).GetComponent<RectTransform>();
            if (low == null)
                low = new GameObject("LowLayer", new System.Type[] { typeof(Canvas), typeof(GraphicRaycaster) }).GetComponent<RectTransform>();
            if (middle == null)
                middle = new GameObject("MiddleLayer", new System.Type[] { typeof(Canvas), typeof(GraphicRaycaster) }).GetComponent<RectTransform>();
            if (hight == null)
                hight = new GameObject("HightLayer", new System.Type[] { typeof(Canvas), typeof(GraphicRaycaster) }).GetComponent<RectTransform>();
            if (hightest == null)
                hightest = new GameObject("HightestLayer", new System.Type[] { typeof(Canvas), typeof(GraphicRaycaster) }).GetComponent<RectTransform>();

            SetUINodeParams(lowest, UIViewLayerEnum.Lowest);
            SetUINodeParams(low, UIViewLayerEnum.Low);
            SetUINodeParams(middle, UIViewLayerEnum.Middle);
            SetUINodeParams(hight, UIViewLayerEnum.Hight);
            SetUINodeParams(hightest, UIViewLayerEnum.Hightest);
        }

        private void Update()
        {
            for (int index = 0; index < (int)UIViewLayerEnum.Count; index++)
            {
                for (int i = 0; i < m_UIViewLayerListDic[(UIViewLayerEnum)index].Count; i++)
                {
                    var element = m_UIViewLayerListDic[(UIViewLayerEnum)index][i];
                    if (element != null && element.view != null)
                    {
                        if (element.view.IsActive)
                        {
                            element.view.OnViewUpdate();
                            if (element.secondTimer >= 1)
                            {
                                element.view.OnViewUpdateBySecond();
                                element.secondTimer = 0;
                            }
                            element.secondTimer += Time.deltaTime;
                        }
                    }
                }
            }

        }


        private void SetUINodeParams(RectTransform node, UIViewLayerEnum layer)
        {
            node.anchorMin = Vector3.zero;
            node.anchorMax = Vector3.one;
            node.transform.SetParent(m_RootCanvas.transform);
            node.offsetMin = m_SafeOffset.offsetMin;
            node.offsetMax = m_SafeOffset.offsetMax;

            var c = node.GetComponent<Canvas>();
            c.overrideSorting = true;
            c.sortingOrder = (int)layer * 100;

            if (s_UIViewLayerRootDic.ContainsKey(layer) == false)
            {
                s_UIViewLayerRootDic.Add(layer, node.gameObject);
            }
        }

        /// <summary>
        /// 竖屏，暂未考虑横屏适配
        /// </summary>
        public void AdjustUI()
        {
            if (!m_RootCanvas.TryGetComponent<CanvasScaler>(out var canvasScaler))
            {
                return;
            }
            var bgRectTransform = m_BackgroundNode.GetComponent<RectTransform>();
            m_FullOffset.offsetMin = bgRectTransform.offsetMin;
            m_FullOffset.offsetMax = bgRectTransform.offsetMax;

            float standard_width = canvasScaler.referenceResolution.x;        //初始宽度  
            float standard_height = canvasScaler.referenceResolution.y;       //初始高度  
            float root_width = m_RootCanvas.GetComponent<RectTransform>().sizeDelta.x;
            float root_height = m_RootCanvas.GetComponent<RectTransform>().sizeDelta.y;
            float device_width = Screen.width;
            float device_height = Screen.height;
            float adjustor = 0f;
            float standard_aspect = standard_width / standard_height;
            float device_aspect = device_width / device_height;
            //计算矫正比例  
            if (device_aspect < standard_aspect)
            {
                adjustor = standard_aspect / device_aspect;
            }

            CanvasScaler canvasScalerTemp = canvasScaler;
            if (adjustor == 0)
            {
                canvasScalerTemp.matchWidthOrHeight = 1;
            }
            else
            {
                canvasScalerTemp.matchWidthOrHeight = 0;
            }


            ref readonly var miniGame = ref J.Minigame;

            float tx = 0, bx = 0;
            float ty = 0, by = 0;
            if (miniGame.SystemInfo != null && miniGame.SystemInfo.statusBarHeight > 20) //大于20视作有刘海屏
            {
                if (miniGame.SystemInfo.statusBarHeight > 20)
                {
                    tx = 6.0f / (float)miniGame.SystemInfo.windowWidth;
                    bx = 6.0f / (float)miniGame.SystemInfo.windowWidth;


                    ty = (float)miniGame.SystemInfo.safeArea.top / (float)miniGame.SystemInfo.windowHeight;

                    if (miniGame.SystemInfo.safeArea.bottom < miniGame.SystemInfo.windowHeight) //下方也有安全去
                    {
                        by = ((float)miniGame.SystemInfo.windowHeight - (float)(miniGame.SystemInfo.safeArea.bottom + 8)) / (float)miniGame.SystemInfo.windowHeight;
                    }
                    else
                    {
                        by = ((float)miniGame.SystemInfo.windowHeight - (float)(miniGame.SystemInfo.windowHeight - 8)) / (float)miniGame.SystemInfo.windowHeight;
                    }

                }

            }

            console.error(miniGame.SystemInfo.safeArea, miniGame.SystemInfo.windowWidth, miniGame.SystemInfo.windowHeight);
            m_SafeOffset.offsetMin = new Vector2(0 + (root_width * bx), 0 + (root_height * by));
            m_SafeOffset.offsetMax = new Vector2(0 + -(root_width * tx), 0 - (root_height * ty));

 
            console.error(m_SafeOffset.offsetMin, m_SafeOffset.offsetMax);
            var res = new Vector2(root_width, root_height * (1 - (ty + by)));
            console.error(standard_width + "  " + standard_height + "  " + res.x + "  " + res.y);
            //canvasScaler.referenceResolution = res;

        }


        private Dictionary<int, UIViewConfig> m_UIViewConfig;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uiConf"></param>
        public void InitUIViewConfigWithAddingMode(in Dictionary<int, UIViewConfig> uiConf)
        {
            if (uiConf == null || uiConf.Count == 0)
            {
                console.error("没有UI配置吗？你确定？？");
                return;
            }

            foreach (var kvp in uiConf)
            {
                if (!m_UIViewConfig.ContainsKey(kvp.Key))
                {
                    m_UIViewConfig.Add(kvp.Key, kvp.Value);
                }
            }

            m_UIViewConfig = uiConf;
        }

        public async UniTask InitUICompListConfig(Dictionary<int, UICompConfig> compConf)
        {
            if (compConf == null || compConf.Count == 0)
            {
                console.error("没有UIComp配置吗？你确定？？");
                return;
            }

            console.info("加载开始--------");
            var taskList = new List<UniTask>(2);
            foreach (var item in compConf)
            {
                var compID = item.Key;
                var conf = item.Value;
                var package = YooAssets.GetPackage(conf.packageName);

                var assetHandle = package.LoadAssetAsync<GameObject>(conf.bundleName);
                assetHandle.Completed += (handle) =>
                {
                    if (handle.Status == EOperationStatus.Succeed)
                    {
                        var go = handle.InstantiateSync();
                        if (go == null)
                        {
                            console.error("未知错误", handle.Status);
                            return;
                        }
                        if (!m_UICompPoolDic.ContainsKey(compID))
                        {
                            var pool = GameObjectPool.Instance.CreatePool(go, conf.preloadAmount);
                            m_UICompPoolDic.Add(compID, pool);
                        }
                        go.SetActive(false);
                    }

                };

                taskList.Add(assetHandle.ToUniTask());
            }

            var task = UniTask.WhenAll(taskList);
            await task;
            console.info("加载完成---------");
        }

        /// <summary>
        /// 设置背景 0单张image，1多张image
        /// </summary>
        /// <param name="type">0单张image，1多张image</param>
        public void SetBackground(int type, Sprite sprite)
        {
            m_SingleImageNode.SetActive(false);
            m_MultiImageNode.SetActive(false);
            if (type == 0)
            {
                m_SingleImageNode.SetActive(true);
                var image = m_SingleImageNode.GetComponent<Image>();
                image.sprite = sprite;
            }
            else if (type == 1)
            {
                m_MultiImageNode.SetActive(true);
                var images = m_MultiImageNode.GetComponentsInChildren<Image>();
                var totalImageCount = images.Length;

                var bgRoot = m_MultiImageNode;

                // 计算Canvas的尺寸 默认多来150，确保铺满屏幕
                float canvasWidth = bgRoot.GetComponent<RectTransform>().rect.width + 200;
                float canvasHeight = bgRoot.GetComponent<RectTransform>().rect.height + 600;
                console.error(canvasWidth, canvasHeight);
                var sourceHeight = 150;
                var sourceWidth = 150;
                var imageHeight = (int)(sourceHeight * images[0].transform.localScale.y);
                var imageWidth = (int)(sourceWidth * images[0].transform.localScale.x);
                // 计算需要铺设的图片行数和列数
                int rows = Mathf.CeilToInt(canvasHeight / imageHeight);
                int cols = Mathf.CeilToInt(canvasWidth / imageWidth);

                var numImages = images.Length;
                // 限制图片数量，防止超过预设数量
                int maxImages = rows * cols;
                numImages = Mathf.Min(numImages, maxImages);

                var idx = 0;
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        var image = images[idx];
                        // 设置Image组件的属性
                        image.sprite = sprite;
                        image.rectTransform.sizeDelta = new Vector2(sourceWidth, sourceHeight);
                        image.rectTransform.anchoredPosition = new Vector2(j * imageWidth - canvasWidth * 0.5f, i * imageHeight - canvasHeight * 0.5f);
                        idx++;
                    }
                }

                var remainImagesCount = totalImageCount - numImages;
                if (remainImagesCount > 0)
                {
                    for (int i = numImages; i < totalImageCount; i++)
                    {
                        images[i].gameObject.SetActive(false);
                    }
                }

            }
            else
            {

            }
        }

        public void OpenUI(UIViewID id, EventArgsPack args = null)
        {
            var uiInfo = new UIViewInfo();
            uiInfo.id = id;
            uiInfo.args = args;
            uiInfo.view = null;

            var res = GetUIIndex(id);
            //如果已经打开且活跃，return
            if (res.uiIndex != -1) //在ui列表中
            {
                var info = m_UIViewLayerListDic[(UIViewLayerEnum)res.layer][res.uiIndex];
                if (info.view != null)
                {
                    if (info.view.IsActive)
                    {
                        console.error("UI已经打开了");
                    }
                    else
                    {
                        (info.view as UIViewBase).gameObject.SetActive(true);
                        OnUIOpen(id, info, info.args);
                    }
                }
                else
                {
                    console.error("异常，uiview不存在");
                }
                return;
            }

            if (m_IsOpening || m_IsClosing)
            {
                //如果在准备打开队列中，return
                foreach (var item in m_UIOpenQueue)
                {
                    if (item.id == uiInfo.id)
                    {
                        return;
                    }
                }
                //添加到打开队列，这次不打开,等当前在打开的ui打开/加载完毕再打开
                m_UIOpenQueue.Enqueue(uiInfo);
                return;
            }

            m_IsOpening = true;

            var conf = m_UIViewConfig[(int)id];
            GetOrCreateUI(id, (uiView) =>
            {
                if (uiInfo.isClose || uiInfo.isHiding || uiView == null)
                {
                    this.m_IsOpening = false;
                    return;
                }

                uiInfo.view = uiView;
                m_UIViewLayerListDic[conf.layer].Add(uiInfo);

                OnUIOpen(id, uiInfo, args);
                this.m_IsOpening = false;
                //执行之后在队列中的ui打开逻辑
                AutoExecNextUI();
            }, args);

        }

        public T GetAndOpenUIViewOnNode<T>(GameObject rootNode, UIViewID id, in UIViewConfig uiConf, EventArgsPack args) where T : UIViewBase
        {
            InitUIViewConfigWithAddingMode(new Dictionary<int, UIViewConfig>() { { (int)id,uiConf} });

            var uiView = rootNode.GetComponent<T>();
            var uiInfo = new UIViewInfo();
            uiInfo.id = id;
            uiInfo.args = args;
            console.error(rootNode.name, id, uiConf,args,uiInfo.args);
            uiInfo.view = uiView;
            uiView.OnViewAwake(uiInfo.args);
            uiView.SetAnimatorNode();
            AdjuestUIOrder(uiConf, uiView);
            if (uiInfo.view != null)
            {
                (uiInfo.view as UIViewBase).gameObject.SetActive(true);
                m_UIViewLayerListDic[uiConf.layer].Add(uiInfo);
                OnUIOpen(id, uiInfo, uiInfo.args);
            }



            return uiView;
        }

        private void OnUIOpen(UIViewID id, UIViewInfo uiInfo, EventArgsPack args)
        {
            var uiView = uiInfo.view as UIViewBase;
            if (uiView == null)
            {
                return;
            }

            if (uiView.animator != null)
            {
                PlayEffect(uiInfo, uiView.animator, "view_open", () =>
                {
                    //console.error("fzy 66");
                    CoroutineManager.Instance.StopCoroutine(uiInfo.openCoroutine);
                    //console.error("fzy 77");
                    //console.error("fzy 88");
                });
                uiView.OnViewShow(args);
            }
            else
            {
                uiView.OnViewShow(args);
            }
        }

        private void AutoExecNextUI()
        {
            if (this.m_UICloseQueue.Count > 0)
            {
                var uiView = m_UICloseQueue.Dequeue();
                CloseUI(uiView);
            }
            else if (m_UIHideQueue.Count > 0)
            {
                var uiView = m_UIHideQueue.Dequeue();
                HideUI(uiView);
            }
            else if (m_UIOpenQueue.Count > 0)
            {
                var queueInfo = m_UIOpenQueue.Dequeue();
                OpenUI(queueInfo.id, queueInfo.args);
            }
        }

        private void GetOrCreateUI(UIViewID id, LoadUIAssetCallback callback, EventArgsPack args)
        {
            var conf = m_UIViewConfig[(int)id];
            var package = YooAssets.GetPackage(conf.packageName);

            var assetHandle = package.LoadAssetAsync<GameObject>(conf.bundleName);
            assetHandle.Completed += (handle) =>
            {
                if (handle.Status == EOperationStatus.Succeed)
                {
                    var go = handle.InstantiateSync(s_UIViewLayerRootDic[conf.layer].transform);
                    if (go == null)
                    {
                        callback?.Invoke(null);
                        return;
                    }

                    var uiView = go.GetComponent<UIViewBase>();
                    if (uiView == null)
                    {
                        callback?.Invoke(null);
                        GameObject.Destroy(go);
                        return;
                    }

                    uiView.OnViewAwake(args);
                    uiView.SetAnimatorNode();
                    //TODO：动画
                    callback?.Invoke(uiView);

                    AdjuestUIOrder(conf, uiView);
                }
                else
                {
                    callback?.Invoke(null);
                }

            };
        }

        private void AdjuestUIOrder(UIViewConfig conf, UIViewBase uiView)
        {
            //调整ui顺序 .. 获取conf.layer下所有的儿子，找到uiview组件，牌序设置层级
            var viewList = new List<UIViewBase>(4);
            var len = m_UIViewLayerListDic[conf.layer].Count;
            for (int i = 0; i < len; i++)
            {
                viewList.Add(m_UIViewLayerListDic[conf.layer][i].view as UIViewBase);
            }
            viewList.Add(uiView);
            viewList.Sort((a, b) => a.UIOrder.CompareTo(b.UIOrder));
            len = viewList.Count;
            for (int i = 0; i < len; i++)
            {
                viewList[i].transform.SetSiblingIndex(i);
            }
        }

        public void HideUI(IUIView uiView)
        {
            var res = GetUIIndex(uiView);
            if (res.uiIndex == -1)
            {
                console.error("无隐藏目标ui", uiView);
                return;
            }
            HideUI_Internal(res);
        }

        public void HideUI(UIViewID id)
        {
            var res = GetUIIndex(id);
            if (res.uiIndex == -1)
            {
                console.error("无隐藏目标ui", id);
                return;
            }
            HideUI_Internal(res);
        }

        public void HideUI_Internal((int uiIndex, int layer) res)
        {
            var info = m_UIViewLayerListDic[(UIViewLayerEnum)res.layer][res.uiIndex];
            if (info == null)
            {
                console.error("无隐藏目标uiInfo");
                return;
            }
            var uiIndex = res.uiIndex;
            var uiLayer = (UIViewLayerEnum)res.layer;
            if (m_IsClosing || m_IsOpening || m_IsHiding)
            {
                this.m_UIHideQueue.Enqueue(info.view);
                return;
            }
            info.isHiding = true;
            m_IsHiding = true;

            var uiView = info.view as UIViewBase;
            if (uiView.animator)
            {
                //有动画，执行完再hide
                PlayEffect(info, uiView.animator, "view_close", () =>
                {
                    CoroutineManager.Instance.StopCoroutine(info.closeCoroutine);
                    OnHide(uiView);
                });
            }
            else
            {
                OnHide(uiView);
            }
        }

        private void OnHide(UIViewBase uiView)
        {
            m_IsHiding = false;

            uiView.gameObject.SetActive(false);
            uiView.OnViewHide();

            this.AutoExecNextUI();
        }

        public void CloseUI(IUIView uiView)
        {
            var res = GetUIIndex(uiView);
            if (res.uiIndex == -1)
            {
                console.error("无关闭目标ui", uiView);
                return;
            }
            CloseUI_Internal(res);
        }

        public void CloseUI(UIViewID id)
        {
            var res = GetUIIndex(id);
            if (res.uiIndex == -1)
            {
                console.error("无关闭目标ui", id);
                return;
            }

            CloseUI_Internal(res);
        }

        private void CloseUI_Internal((int uiIndex, int layer) res)
        {
            var info = m_UIViewLayerListDic[(UIViewLayerEnum)res.layer][res.uiIndex];
            if (info == null)
            {
                console.error("无关闭目标uiInfo");
                return;
            }

            var uiIndex = res.uiIndex;
            var uiLayer = (UIViewLayerEnum)res.layer;
            if (m_IsClosing || m_IsOpening || m_IsHiding)
            {
                this.m_UICloseQueue.Enqueue(info.view);
                return;
            }

            info.isClose = true;
            m_IsClosing = true;

            var uiView = info.view as UIViewBase;
            if (uiView.animator)
            {
                PlayEffect(info, uiView.animator, "view_close", () =>
                {
                    CoroutineManager.Instance.StopCoroutine(info.closeCoroutine);
                    OnClose(uiView);
                });
            }
            else
            {
                OnClose(uiView);
            }
            m_UIViewLayerListDic[uiLayer].RemoveAt(uiIndex);

        }

        private int PlayEffect(UIViewInfo info, Animator animator, string stateName, Action callback = null)
        {
            var ctr = Resources.Load<RuntimeAnimatorController>("Animation/Container_Open");
            //console.error("fzy 11", ctr.name);
            animator.runtimeAnimatorController = ctr;
            animator.Update(0f);
            animator.Play(stateName, -1);

            if (callback == null) return 0;
            //console.error("fzy 22", animator.name);

            var c = CoroutineManager.Instance.StartCoroutine(DelayRunEffectCallback(animator, stateName, callback));
            if (stateName == "view_close")
            {
                info.closeCoroutine = c;
            }
            else if (stateName == "view_open")
            {
                info.openCoroutine = c;
            }

            return 0;
        }

        private IEnumerator DelayRunEffectCallback(Animator animator, string stateName, Action callback)
        {
            yield return new WaitForEndOfFrame();

            var info = animator.GetCurrentAnimatorStateInfo(0);
            //console.error("fzy 23", info.ToString());
            if (!info.IsName(stateName)) yield return null;

            yield return new WaitForSeconds(info.length);
            callback();
        }

        private void OnClose(UIViewBase uiView)
        {
            m_IsClosing = false;

            uiView.gameObject.SetActive(false);
            uiView.OnViewHide();

            uiView.OnViewDestroy();
            GameObject.Destroy(uiView.gameObject);
            this.AutoExecNextUI();
        }

        public void CloseAll()
        {
            for (int index = 0; index < (int)UIViewLayerEnum.Count; index++)
            {
                for (int i = 0; i < m_UIViewLayerListDic[(UIViewLayerEnum)index].Count; i++)
                {
                    var element = m_UIViewLayerListDic[(UIViewLayerEnum)index][i];
                    element.isClose = true;
                    if (element.view != null)
                    {
                        var viewBase = element.view as UIViewBase;
                        viewBase.gameObject.SetActive(false);
                        element.view.OnViewHide();
                        element.view.OnViewDestroy();
                        GameObject.Destroy(viewBase.gameObject);
                    }
                }
            }
        }

        public (int uiIndex, int layer) GetUIIndex(UIViewID id)
        {
            int layer = -1;
            int uiIndex = -1;

            for (int index = 0; index < (int)UIViewLayerEnum.Count; index++)
            {
                for (int i = 0; i < m_UIViewLayerListDic[(UIViewLayerEnum)index].Count; i++)
                {
                    var element = m_UIViewLayerListDic[(UIViewLayerEnum)index][i];
                    if (id == element.id)
                    {
                        uiIndex = i;
                        layer = index;
                        return (uiIndex, layer);
                    }
                }
            }
            return (-1, -1);
        }

        public (int uiIndex, int layer) GetUIIndex(IUIView uiView)
        {
            int layer = -1;
            int uiIndex = -1;

            for (int index = 0; index < (int)UIViewLayerEnum.Count; index++)
            {
                for (int i = 0; i < m_UIViewLayerListDic[(UIViewLayerEnum)index].Count; i++)
                {
                    var element = m_UIViewLayerListDic[(UIViewLayerEnum)index][i];
                    if (uiView == element.view)
                    {
                        uiIndex = i;
                        layer = index;
                        return (uiIndex, layer);
                    }
                }
            }
            return (-1, -1);
        }


        //------------------------------------------------------
        //------------------------------component接口------------
        //------------------------------------------------------
        public T CreateUIComponent<T>(UICompID id, Transform parent) where T : UICompBase
        {
            T comp = null;
            if (m_UICompPoolDic.TryGetValue((int)id, out var pool))
            {
                var obj = pool.GetGameObject(parent);
                comp = obj.GetComponent<T>();
                if (comp == null)
                {
                    comp = obj.AddComponent<T>();
                }
                comp.OnComponentInit();
            }
            return comp;
        }

        public T GetUIComponentAtNode<T>(Transform parent) where T : UICompBase
        {
            T comp = parent.GetComponent<T>();
            if (comp == null)
            {
                console.error($"无{nameof(T)}类型组件");
            }
            return comp;
        }

        public void DestroyUIComponent(UICompID id, UICompBase uiComp)
        {
            if (m_UICompPoolDic.TryGetValue((int)id, out var pool))
            {
                uiComp.OnComponentDestroy();
                pool.RecycleGameObject(uiComp.gameObject);
            }
        }

    }



}
