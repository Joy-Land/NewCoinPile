using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Placeholder
{
    public class CoinStackPlaceholder : MonoBehaviour
    {
        [SerializeField] private GameObject[] PlaceholderList;

        public Boolean GetIndexedPlaceholder(int index, out GameObject placeholder)
        {
            if (index < 0 || index >= PlaceholderList.Length)
            {
                placeholder = null;
                return false;
            }

            placeholder = PlaceholderList[index];
            return true;
        }

        public void Clear()
        {
            foreach (var coinPlaceholder in PlaceholderList)
            {
                coinPlaceholder.GetComponent<CoinPlaceholder>().Clear();
            }
        }
    }
}
