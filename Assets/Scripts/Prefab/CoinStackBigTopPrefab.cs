using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefab
{
    public class CoinStackTopPrefab : MonoBehaviour
    {
        [SerializeField] private new Renderer renderer;

        public void ChangeColor(Material material)
        {
            if (renderer != null)
            {
                renderer.material = material;
            }
        }
    }
}
