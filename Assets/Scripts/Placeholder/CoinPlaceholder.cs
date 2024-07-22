using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Placeholder
{
    public class CoinPlaceholder : MonoBehaviour
    {
        public GameObject coin;

        public void Clear()
        {
            Destroy(coin);
            coin = null;
        }
    }
}
