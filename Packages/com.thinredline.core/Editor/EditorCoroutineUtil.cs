using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ThinRL.Core.Editor
{
    public class EditorCoroutineUtil
    {
    }

    // 在editor非play状态下强制执行udpate，使coroutine能顺利执行
    // 因为需要是mono脚本，所以还得放在运行时目录里
    [ExecuteAlways]
    public class CoroutineRunnerInEditMode : MonoBehaviour
    {
        static int m_UpdateLeftCount;
        static System.Action m_FinishCallback;
        static List<IEnumerator> m_Enumerators;

        // 开始强制非play时的更新
        // @updateCount 更新次数
        // @destroySelfGoOnFinish 完成更新后是否销毁go
        public static void ForceQueueUpdateInEditMode(int updateCount, System.Action finishCallback)
        {
            m_UpdateLeftCount = updateCount;
            m_FinishCallback = finishCallback;
            EditorApplication.update += EditorUpdate;
        }

        public static CoroutineRunnerInEditMode CreateUpdateObject(int updateCount, System.Action finishCallback = null)
        {
            var go = new GameObject("editorCoroutineRunner");
            var mono = go.AddComponent<CoroutineRunnerInEditMode>();
            ForceQueueUpdateInEditMode(updateCount, () =>
            {
                DestroyImmediate(go);
                finishCallback?.Invoke();
            });
            return mono;
        }

        public static void StopQueueUpdate()
        {
            Debug.LogWarning("remove EditorUpdate");
            EditorApplication.update -= EditorUpdate;
            m_FinishCallback?.Invoke();
        }

        static void EditorUpdate()
        {
            m_UpdateLeftCount--;
            // Debug.LogWarning($"EditorUpdate {m_UpdateLeftCount}");

            // 在EditorApplication.update期间执行QueuePlayerLoopUpdate，可以在非play状态下场景没有变化的时候也执行更新，
            // 脚本的update、协程都能执行到
            // 因为QueuePlayerLoopUpdate在编辑器没有焦点时无法触发playerLoop，所以手动驱动协程
            if (true)
                UpdateEditorCoroutineManually();
            else
                EditorApplication.QueuePlayerLoopUpdate();

            if (m_UpdateLeftCount <= 0)
            {
                StopQueueUpdate();
                m_Enumerators?.Clear();
            }
        }


        static void UpdateEditorCoroutineManually()
        {
            if (m_Enumerators == null) return;

            foreach (var v in m_Enumerators)
            {
                if (v != null)
                {
                    // 只简单支持了部分yield类型
                    AsyncOperation ap = v.Current as AsyncOperation;
                    if (ap == null || ap.isDone)
                        v.MoveNext();
                }
            }
        }

        public Coroutine StartEditorCoroutine(IEnumerator ie)
        {
            if (m_Enumerators == null)
                m_Enumerators = new List<IEnumerator>();
            m_Enumerators.Add(ie);
            return StartCoroutine(ie);
        }

        public IEnumerator TestCoroutine()
        {
            try
            {
                while (m_UpdateLeftCount > 0)
                {
                    Debug.LogError(m_UpdateLeftCount);
                    yield return null;
                }

            }
            finally
            {
                Debug.LogError("done");
                GameObject.DestroyImmediate(gameObject);
                // EditorApplication.update -= EditorUpdate;

            }
        }

    }
}