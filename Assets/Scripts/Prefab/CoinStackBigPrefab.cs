using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefab
{
    public class CoinStackPrefab : MonoBehaviour
    {
        [SerializeField] private Renderer mainRenderer;

        public void ChangeColor(Material material)
        {
            if (mainRenderer != null)
            {
                mainRenderer.material = material;
            }
        }
    }
}