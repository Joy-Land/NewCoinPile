using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefab
{
    public class CoinPrefab : MonoBehaviour
    {
        [SerializeField] private Renderer mainRenderer;
        [SerializeField] private Renderer subRenderer;
        [SerializeField] private Transform ropePoint;
        [SerializeField] private GameObject ropePin;

        public void ChangeColor(Material material)
        {
            if (mainRenderer != null)
            {
                mainRenderer.material = material;
            }
        }
        
        public void Hide()
        {
            if (mainRenderer != null)
            {
                mainRenderer.enabled = false;
                subRenderer.enabled = false;
            }
        }

        public void Show()
        {
            if (mainRenderer != null)
            {
                mainRenderer.enabled = true;
                subRenderer.enabled = true;
            }
        }

        public Transform GetRopePoint()
        {
            return ropePoint;
        }

        public void ShowRopePin()
        {
            if (ropePin != null)
            {
                ropePin.SetActive(true);
            }
        }
        
        public void HideRopePin()
        {
            if (ropePin != null)
            {
                ropePin.SetActive(false);
            }
        }
    }
}
