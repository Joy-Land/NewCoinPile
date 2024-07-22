using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThinRL.Core
{

    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> m_Keys;
        [SerializeField]
        private List<TValue> m_Values;

        public void OnAfterDeserialize()
        {
            var count = Mathf.Min(m_Keys.Count, m_Values.Count);
            this.Clear();
            for (int i = 0; i < count; i++)
            {
                this.Add(m_Keys[i], m_Values[i]);
            }
        }

        public void OnBeforeSerialize()
        {
            m_Keys = new List<TKey>(this.Keys);
            m_Values = new List<TValue>(this.Values);
        }
    }
}
