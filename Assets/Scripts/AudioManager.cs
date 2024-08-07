using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using ThinRL.Core.Pool;
using ThinRL.Core.Tools;
using UnityEngine;
using UnityEngine.Scripting;
using YooAsset;

namespace Joyland.GamePlay
{
    [Preserve]
    public class AudioConfig
    {
        public float volumeBg;
        public bool enableBg;
        public float volumeEffect;
        public bool enableEffect;
    }

    [DisallowMultipleComponent]
    public class AudioManager : MonoSingleton<AudioManager>
    {
        private class AudioInfo
        {
            public AudioClip clip;
            public Action finishCallback;
            /// <summary>
            /// 挂载在那个go上
            /// </summary>
            public AudioSource refAudioSourceNode;
            public float timer;
            /// <summary>
            /// 用来辅助判定是否可以回收
            /// </summary>
            public bool needRecycle;
        }
        private PrefabPool m_AudioNodePool = null;

        private AudioSource m_BackgroundAudioSource;

        private Dictionary<string, List<AudioInfo>> m_AudioClipCacheDic;
        private Dictionary<string, AudioInfo> m_BGAudioClipCacheDic;

        private string m_CurrentBgURL = string.Empty;
        private bool m_CurrentBgLoopMode = true;
        private Action m_CurrentBGFinishCallback;

        private float m_BackgroundMusicVolmue = 1.0f;
        public float BackgroundMusicVolume
        {
            get { return m_BackgroundMusicVolmue; }
            set
            {
                m_BackgroundMusicVolmue = value;
                m_BackgroundAudioSource.volume = m_BackgroundMusicVolmue;
            }
        }

        private float m_EffectMusicVolume = 1.0f;
        public float EffectMusicVolmue
        {
            get { return m_EffectMusicVolume; }
            set
            {
                m_EffectMusicVolume = value;
                foreach (var element in m_AudioClipCacheDic)
                {
                    var len = element.Value.Count;
                    for (var i = 0; i < len; i++)
                    {
                        var item = element.Value[i];
                        item.refAudioSourceNode.volume = m_EffectMusicVolume;
                    }
                }
            }
        }

        private bool m_BackgroundMusicEnable = true;
        public bool BackgroundMusicEnable
        {
            get { return m_BackgroundMusicEnable; }
            set
            {
                m_BackgroundMusicEnable = value;
                if (m_BackgroundMusicEnable)
                {
                    PlayBackgroundMusic(m_CurrentBgURL, m_CurrentBgLoopMode, m_CurrentBGFinishCallback);
                }
                else
                {
                    //TODO:干掉正在播放的小曲
                    StopBackgroundMusic();
                }

            }
        }

        private bool m_EffectMusicEnable = true;
        public bool EffectMusicEnable
        {
            get { return m_EffectMusicEnable; }
            set
            {
                m_EffectMusicEnable = value;
                if (m_EffectMusicEnable)
                {

                }
                else
                {
                    //TODO:干掉正在播放的小曲
                    StopAllEffectMusic();
                }
            }
        }

        public void LoadAudioConfig()
        {
            var dataString = PlayerPrefsManager.GetUserString(GamePlayerPrefsKey.AudioSetting, "");
            if (string.IsNullOrEmpty(dataString) == false)
            {
                try
                {
                    var data = JsonConvert.DeserializeObject<AudioConfig>(dataString);
                    m_BackgroundMusicVolmue = data.volumeBg;
                    m_EffectMusicVolume = data.volumeEffect;
                    m_BackgroundMusicEnable = data.enableBg;
                    m_EffectMusicEnable = data.enableEffect;
                }
                catch
                {
                    m_BackgroundMusicVolmue = 1;
                    m_EffectMusicVolume = 1;
                    m_BackgroundMusicEnable = true;
                    m_EffectMusicEnable = true;
                }
            }
        }

        public void SaveAudioConfig()
        {
            AudioConfig conf = new AudioConfig();
            conf.enableEffect = m_EffectMusicEnable;
            conf.enableBg = m_BackgroundMusicEnable;
            conf.volumeEffect = m_EffectMusicVolume;
            conf.volumeBg = m_BackgroundMusicVolmue;
            var confString = JsonConvert.SerializeObject(conf);
            PlayerPrefsManager.SetUserString(GamePlayerPrefsKey.AudioSetting, confString);
        }

        private void Awake()
        {
            LoadAudioConfig();

            m_AudioClipCacheDic = new Dictionary<string, List<AudioInfo>>();
            m_BGAudioClipCacheDic = new Dictionary<string, AudioInfo>();

            m_BackgroundAudioSource = new GameObject("AudioSource(Clone)", new System.Type[] { typeof(AudioSource) }).GetComponent<AudioSource>();
            m_BackgroundAudioSource.transform.SetParent(this.transform);

            m_AudioNodePool = GameObjectPool.Instance.CreatePool(m_BackgroundAudioSource.gameObject, 3);


            AddOrCreateAudioSource(m_BackgroundAudioSource.gameObject, true, m_BackgroundMusicVolmue);
        }

        private AudioSource AddOrCreateAudioSource(GameObject obj, bool loop, float volume)
        {
            var audioSource = obj.GetComponent<AudioSource>() ?? obj.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = volume;
            audioSource.spatialBlend = 0;
            audioSource.loop = loop;
            return audioSource;
        }

        public void PlayBackgroundMusic(string url, bool isLoop = true, Action finishCallback = null)
        {
            if (BackgroundMusicEnable == false)
            {
                return;
            }

            //保证正在播放的音频不会被打断
            if (m_CurrentBgURL != url || isLoop == false)
            {
                m_CurrentBgURL = url;
                m_CurrentBgLoopMode = isLoop;
                m_CurrentBGFinishCallback = finishCallback;

                if (m_BGAudioClipCacheDic.TryGetValue(url, out var infoCache))
                {
                    var info = infoCache;
                    info.refAudioSourceNode = m_BackgroundAudioSource;
                    info.timer = 0;
                    info.finishCallback = isLoop ? null : finishCallback;
                    info.needRecycle = false;
                    PlayBackgroundMusic_Internal(info.clip, isLoop);
                }
                else
                {
                    var assetHandle = YooAssets.LoadAssetAsync<AudioClip>(url);
                    assetHandle.Completed += (handle) =>
                    {
                        if (handle.Status == EOperationStatus.Succeed)
                        {
                            var loadedClip = handle.AssetObject as AudioClip;
                            AudioInfo info = new AudioInfo();
                            info.clip = loadedClip;
                            info.refAudioSourceNode = m_BackgroundAudioSource;
                            info.timer = 0;
                            info.finishCallback = isLoop ? null : finishCallback;
                            info.needRecycle = false;
                            m_BGAudioClipCacheDic.Add(url, info);
                            PlayBackgroundMusic_Internal(loadedClip, isLoop);
                        }
                    };
                }
            }
        }

        public void PauseBackgroundMusic()
        {
            if (m_BackgroundAudioSource && m_BackgroundAudioSource.isPlaying)
            {
                m_BackgroundAudioSource.Pause();
            }
        }
        public void ResumeBackgroundMusic()
        {
            if (m_BackgroundAudioSource && m_BackgroundAudioSource.isPlaying)
            {
                m_BackgroundAudioSource.UnPause();
            }
        }
        public void StopBackgroundMusic()
        {
            if (m_BackgroundAudioSource)
            {
                //var url = m_BackgroundAudioSource.name;
                //if(m_BGAudioClipCacheDic.TryGetValue(url, out var info))
                //{
                //    info.needRecycle = true;
                //}
                m_BackgroundAudioSource.Stop();
            }
        }

        private void PlayBackgroundMusic_Internal(AudioClip clip, bool isLoop)
        {
            if (clip == null)
            {
                console.error("clip没有");
                return;
            }
            if (m_BackgroundAudioSource.isPlaying)
            {
                m_BackgroundAudioSource.Stop();
            }
            m_BackgroundAudioSource.clip = clip;
            m_BackgroundAudioSource.loop = isLoop;
            m_BackgroundAudioSource.Play();
        }

        public void PlayEffectMusic(string url, Action finishCallback = null)
        {
            if (EffectMusicEnable == false)
            {
                return;
            }
            var assetHandle = YooAssets.LoadAssetAsync<AudioClip>(url);
            assetHandle.Completed += (handle) =>
            {
                if (handle.Status == EOperationStatus.Succeed)
                {
                    var audioSource = AddOrCreateAudioSource(m_AudioNodePool.GetGameObject(this.transform), false, EffectMusicVolmue);


                    var loadedClip = handle.AssetObject as AudioClip;

                    AudioInfo info = new AudioInfo();
                    info.clip = loadedClip;
                    info.refAudioSourceNode = audioSource;
                    info.timer = 0;
                    info.finishCallback = finishCallback;
                    info.needRecycle = false;

                    if (m_AudioClipCacheDic.ContainsKey(url))
                    {
                        var list = m_AudioClipCacheDic[url];
                        list.Add(info);
                    }
                    else
                    {
                        m_AudioClipCacheDic.Add(url, new List<AudioInfo>() { info });
                    }
                    //console.error(info.clip.name, info.refAudioSourceNode.name, info.timer);
                    PlayEffectMusic_Internal(audioSource, info.clip);
                }
            };
        }


        public void StopEffectMusic(string url)
        {
            if (m_AudioClipCacheDic.TryGetValue(url, out var elements))
            {
                var len = elements.Count;
                for (var i = 0; i < len; i++)
                {
                    var item = elements[i];
                    item.needRecycle = true;
                    item.refAudioSourceNode.Stop();
                }
            }
        }

        public void StopAllEffectMusic()
        {
            foreach (var element in m_AudioClipCacheDic)
            {
                var len = element.Value.Count;
                for (var i = 0; i < len; i++)
                {
                    var item = element.Value[i];
                    item.needRecycle = true;
                    item.refAudioSourceNode.Stop();
                }
            }
        }


        private void PlayEffectMusic_Internal(AudioSource audioSource, AudioClip clip)
        {
            if (clip == null)
            {
                console.error("clip没有");
                return;
            }
            audioSource.clip = clip;
            audioSource.Play();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        List<ValueTuple<string, AudioInfo>> m_SafeDeleteList = new List<ValueTuple<string, AudioInfo>>(1 << 2);
        // Update is called once per frame
        void Update()
        {
            m_SafeDeleteList.Clear();

            foreach (var element in m_AudioClipCacheDic)
            {
                var len = element.Value.Count;
                for (var i = 0; i < len; i++)
                {
                    var item = element.Value[i];
                    if (item.refAudioSourceNode.isPlaying)
                    {
                        item.timer += Time.deltaTime;
                        if (item.timer > item.clip.length)
                        {
                            m_SafeDeleteList.Add(new(element.Key, item));
                            item.finishCallback?.Invoke();
                        }
                    }
                    else
                    {
                        if (item.needRecycle)
                        {
                            m_SafeDeleteList.Add(new(element.Key, item));
                        }
                    }
                }
            }

            foreach (var element in m_BGAudioClipCacheDic)
            {
                var item = element.Value;
                if (item.refAudioSourceNode.isPlaying && item.refAudioSourceNode.loop == false)
                {
                    item.timer += Time.deltaTime;
                    if (item.timer > item.clip.length)
                    {
                        item.finishCallback?.Invoke();
                    }
                }
                else
                {
                    if (item.needRecycle)
                    {
                        //
                    }
                }
            }

            for (var i = 0; i < m_SafeDeleteList.Count; i++)
            {
                var (key, item) = m_SafeDeleteList[i];
                m_AudioClipCacheDic[key].Remove(item);
                m_AudioNodePool.RecycleGameObject(item.refAudioSourceNode.gameObject);
            }
        }
    }

}
