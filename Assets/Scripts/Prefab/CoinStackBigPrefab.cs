using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefab
{
    public class CoinStackPrefab : MonoBehaviour
    {
        [SerializeField] private Renderer renderer;

        public void ChangeColor(Material material)
        {
            if (renderer != null)
            {
                renderer.material = material;
            }
        }
    }
}