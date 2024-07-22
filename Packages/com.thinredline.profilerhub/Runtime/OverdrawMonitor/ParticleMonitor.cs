using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThinRL.ProfilerHub.OverdrawMonitor
{
    [ExecuteAlways]
    public class ParticleMonitor : MonoBehaviour
    {
        CDParticleMonitor m_Config = null;

        //播放中的粒子总数
        private int m_ParticleCount = 0;
        public int particleCount { get { return m_ParticleCount; } }
        //播放中的粒子总数峰值
        private int m_ParticlePeakValue = 0;
        public int particlePeakValue { get { return m_ParticlePeakValue; } }
        //播放中的ParticleSystem对象数量
        private int m_ParticleSystemCountPlaying = 0;
        //没有播放的ParticleSystem对象数量
        private int m_ParticleSystemCountNotPlaying = 0;
        //Prefab中ParticleSystem对象数量
        private int m_ParticleSystemCountPrefab = 0;
        public int particleSystemCountPlaying { get { return m_ParticleSystemCountPlaying; } }
        public int particleSystemCountNotPlaying { get { return m_ParticleSystemCountNotPlaying; } }
        public int particleSystemCountPrefab { get { return m_ParticleSystemCountPrefab; } }

        private float m_IntervalTimeAcc = 0.0f;

        public static ParticleMonitor GetMonitor(Transform parent, CDParticleMonitor config)
        {
            string name = "ParticleMonitor";
            GameObject go;
            if (parent == null)
            {
                go = GameObject.Find(name);
                if (go == null)
                {
                    go = new GameObject(name, new System.Type[] { typeof(ParticleMonitor) });
                }
            }
            else
            {
                Transform t = parent.Find(name);
                if (t == null)
                {
                    go = new GameObject(name, new System.Type[]{ typeof(ParticleMonitor) });
                    go.transform.SetParent(parent, true);
                }
                else
                {
                    go = t.gameObject;
                }
            }

            ParticleMonitor pm = go.GetComponent<ParticleMonitor>();
            if (pm == null)
            {
                pm = go.AddComponent<ParticleMonitor>();
            }

            pm.m_Config = config;

            return pm;
        }
        public void Awake()
        {
            if (Application.isPlaying) DontDestroyOnLoad(this.gameObject);
            if (!Application.isPlaying)
            {
                gameObject.hideFlags = HideFlags.DontSave;
            }
        }
        void LateUpdate()
        {
            DoCount();
        }
        public void Dispose()
        {
            if (this != null)
            {
                GameObject.DestroyImmediate(this.gameObject);
            }
        }
        void DoCount()
        {
            m_IntervalTimeAcc += Time.deltaTime;

            float sampleInterval = (m_Config == null) ? 0.5f : m_Config.sampleInterval;
            if (m_IntervalTimeAcc > sampleInterval)
            {
                m_IntervalTimeAcc -= sampleInterval;

#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPaused)
#else
                if (true)
#endif
                {
                    int n = 0;
                    int npi = 0;
                    int nnpi = 0;
                    int npb = 0;

                    UnityEngine.Profiling.Profiler.BeginSample("Editor find all ParticleSystem");
                    ParticleSystem[] pss = Resources.FindObjectsOfTypeAll<ParticleSystem>();
                    UnityEngine.Profiling.Profiler.EndSample();

                    if (pss != null)
                    {
                        foreach (ParticleSystem ps in pss)
                        {
                            bool isPrefab = false;
#if UNITY_EDITOR
                            UnityEngine.Profiling.Profiler.BeginSample("Editor each ParticleSystem");
                            {
                                //方法1，开销(504次2.1ms)。
                                //isPrefab = UnityEditor.PrefabUtility.GetPrefabAssetType(ps.gameObject) != UnityEditor.PrefabAssetType.NotAPrefab;               

                                //方法2，开销(504次0.34ms)。
                                isPrefab = UnityEditor.PrefabUtility.IsPartOfPrefabAsset(ps.gameObject);

                                //方法3，开销(504次0.14ms)。根据结果分析出来的方法，没有官方资料证实
                                //isPrefab = ps.gameObject.GetInstanceID() > 0;
                            }
                            UnityEngine.Profiling.Profiler.EndSample();

#endif
                            if (isPrefab)
                            {
                                npb++;
                            }
                            else if (ps.gameObject.activeInHierarchy)
                            {
                                if (ps.isPlaying)
                                {
                                    n += ps.particleCount;
                                    npi++;
                                }
                                else
                                {
                                    nnpi++;
                                }
                            }
                            else
                            {
                                nnpi++;
                            }
                        }
                    }

                    m_ParticleCount = n;

                    if (m_ParticleCount > m_ParticlePeakValue)
                    {
                        m_ParticlePeakValue = m_ParticleCount;
                    }


                    m_ParticleSystemCountPlaying = npi;
                    m_ParticleSystemCountNotPlaying = nnpi;
                    m_ParticleSystemCountPrefab = npb;
                    //Debug.LogError("ParticleSystem count:" + pss.Length);
                }
            }
        }
    }
}
