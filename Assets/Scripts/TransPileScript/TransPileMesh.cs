using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manager;
using Prefab;

namespace TransPileScript
{
    public class TransPileMesh : MonoBehaviour
    {
        [SerializeField] private GameObject coinStackPrefab;
        
        // 预制体
        [SerializeField] private GameObject currentCoinStockPlaceholder;
        [SerializeField] private GameObject nextCoinStockPlaceholder;
        [SerializeField] private GameObject lastCoinStockPlaceholder;
        private Transform currentCoinStockTransform;
        private Transform nextCoinStockTransform;
        private Transform lastCoinStockTransform;

        private GameObject currentTransGameObject;
        private GameObject nextTransGameObject;
        private GameObject lastTransGameObject;
    
        public void Init(TransCoinStackItem currentTransPile, TransCoinStackItem nextTransPile, TransCoinStackItem lastTransPile)
        {
            // 初始化位置
            currentCoinStockTransform = currentCoinStockPlaceholder.transform;
            currentCoinStockPlaceholder.SetActive(false);
            nextCoinStockTransform = nextCoinStockPlaceholder.transform;
            nextCoinStockPlaceholder.SetActive(false);
            lastCoinStockTransform = lastCoinStockPlaceholder.transform;
            lastCoinStockPlaceholder.SetActive(false);
            
            // 初始化运钱区预制体
            SetCurrentTransGameObject(currentTransPile);
            SetNextTransGameObject(nextTransPile);
            SetLastTransGameObject(lastTransPile);
        }
        
        public GameObject GetCurrentTransGameObject()
        {
            return currentTransGameObject;
        }
        
        public GameObject GetNextTransGameObject()
        {
            return nextTransGameObject;
        }
        
        public GameObject GetLastTransGameObject()
        {
            return lastTransGameObject;
        }

        public void SetCurrentTransGameObject(TransCoinStackItem transPile)
        {
            if (currentTransGameObject != null)
            {
                Destroy(currentTransGameObject);
                currentTransGameObject = null;
            }
            currentTransGameObject = transPile != null ? Instantiate(coinStackPrefab, currentCoinStockTransform.position, currentCoinStockTransform.rotation) : null;
            if (currentTransGameObject != null)
            {
                var colorMaterial = MaterialManager.Instance.GetMaterialByColor(transPile.Color);
                currentTransGameObject.GetComponent<CoinStackPrefab>().ChangeColor(colorMaterial);
            }
        }
        
        public void SetNextTransGameObject(TransCoinStackItem transPile)
        {
            if (nextTransGameObject != null)
            {
                Destroy(nextTransGameObject);
                nextTransGameObject = null;
            }
            nextTransGameObject = transPile != null ? Instantiate(coinStackPrefab, nextCoinStockTransform.position, nextCoinStockTransform.rotation) : null;
            if (nextTransGameObject != null)
            {
                var colorMaterial = MaterialManager.Instance.GetMaterialByColor(transPile.Color);
                nextTransGameObject.GetComponent<CoinStackPrefab>().ChangeColor(colorMaterial);
            }
        }
        
        public void SetLastTransGameObject(TransCoinStackItem transPile)
        {
            if (lastTransGameObject != null)
            {
                Destroy(lastTransGameObject);
                lastTransGameObject = null;
            }
            lastTransGameObject =  transPile != null ? Instantiate(coinStackPrefab, lastCoinStockTransform.position, lastCoinStockTransform.rotation) : null;
            if (lastTransGameObject != null)
            {
                var colorMaterial = MaterialManager.Instance.GetMaterialByColor(transPile.Color);
                lastTransGameObject.GetComponent<CoinStackPrefab>().ChangeColor(colorMaterial);
            }
        }

        public void ExChangeCurrentAndNextGameObject()
        {
            currentTransGameObject = nextTransGameObject;
            nextTransGameObject = lastTransGameObject;
            lastTransGameObject = null;
        }
    }
}
