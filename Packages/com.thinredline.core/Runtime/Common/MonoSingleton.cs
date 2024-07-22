using UnityEngine;
using System.Collections;

namespace ThinRL.Core
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance = null;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject();
                    DontDestroyOnLoad(go);
                    go.name = "Mono Singleton:" + typeof(T).ToString();
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localEulerAngles = Vector3.zero;
                    go.transform.localScale = Vector3.one;
                    _instance = go.AddComponent<T>();
                }

                return _instance;
            }
        }
    }
}
