using System;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using ThinRL.Core.Pool;
using UnityEngine;
using YooAsset;

namespace Joyland.GamePlay
{
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
        }
        private PrefabPool m_AudioNodePool = null;

        private AudioSource m_BackgroundAudioSource;

        private Dictionary<string, List<AudioInfo>> m_AudioClipCacheDic;
        private Dictionary<string, AudioInfo> m_BGAudioClipCacheDic;

        private float m_BackgroundMusicVolmue = 1.0f;
        private float m_EffectMusicVolume = 1.0f;
        public float BackgroundMusicVolume
        {
            get { return m_BackgroundMusicVolmue; }
            set
            {
                m_BackgroundMusicVolmue = value;
                m_BackgroundAudioSource.volume = m_BackgroundMusicVolmue;
            }
        }

        public float EffectMusicVolmue
        {
            get {return  m_EffectMusicVolume; }
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
        private void Awake()
        {
            m_AudioClipCacheDic = new Dictionary<string, List<AudioInfo>>();
            m_BGAudioClipCacheDic = new Dictionary<string, AudioInfo>();

            m_BackgroundAudioSource = new GameObject("AudioSource(Clone)", new System.Type[] { typeof(AudioSource) }).GetComponent<AudioSource>();
            m_BackgroundAudioSource.transform.SetParent(this.transform);

            m_AudioNodePool = GameObjectPool.Instance.CreatePool(m_BackgroundAudioSource.gameObject, 3);

            AddOrCreateAudioSource(m_BackgroundAudioSource.gameObject, true);
        }

        private AudioSource AddOrCreateAudioSource(GameObject obj, bool loop)
        {
            var audioSource = obj.GetComponent<AudioSource>() ?? obj.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 1.0f;
            audioSource.spatialBlend = 0;
            audioSource.loop = loop;
            return audioSource;
        }

        public void PlayBackgroundMusic(string url, bool isLoop = true, Action finishCallback = null)
        {
            if (m_BGAudioClipCacheDic.TryGetValue(url, out var infoCache))
            {
                var info = infoCache;
                info.refAudioSourceNode = m_BackgroundAudioSource;
                info.timer = 0;
                info.finishCallback = isLoop ? null : finishCallback;
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
                        m_BGAudioClipCacheDic.Add(url, info);
                        PlayBackgroundMusic_Internal(loadedClip, isLoop);
                    }
                };
            }
        }

        public void PauseBackgroundMusic()
        {
            if (m_BackgroundAudioSource)
            {
                m_BackgroundAudioSource.Pause();
            }
        }
        public void ResumeBackgroundMusic()
        {
            if (m_BackgroundAudioSource)
            {
                m_BackgroundAudioSource.UnPause();
            }
        }
        public void StopBackgroundMusic()
        {
            if (m_BackgroundAudioSource)
            {
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

            var assetHandle = YooAssets.LoadAssetAsync<AudioClip>(url);
            assetHandle.Completed += (handle) =>
            {
                if (handle.Status == EOperationStatus.Succeed)
                {
                    var audioSource = AddOrCreateAudioSource(m_AudioNodePool.GetGameObject(this.transform), false);


                    var loadedClip = handle.AssetObject as AudioClip;

                    AudioInfo info = new AudioInfo();
                    info.clip = loadedClip;
                    info.refAudioSourceNode = audioSource;
                    info.timer = 0;
                    info.finishCallback = finishCallback;

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
            if(m_AudioClipCacheDic.TryGetValue(url, out var elements))
            {
                var len = elements.Count;
                for (var i = 0; i < len; i++)
                {
                    var item = elements[i];
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

        List<ValueTuple<string,AudioInfo>> m_SafeDeleteList = new List<ValueTuple<string, AudioInfo>>(1 << 2);
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
                            m_SafeDeleteList.Add(new (element.Key, item));
                            item.finishCallback?.Invoke();
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
