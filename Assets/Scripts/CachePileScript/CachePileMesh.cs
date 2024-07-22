using Placeholder;
using UnityEngine;

namespace CachePileScript
{
    public class CachePileMesh : MonoBehaviour
    {
        [SerializeField] private GameObject[] cacheGameObjectList;

        public void Init()
        {
            foreach (var cacheGameObject in cacheGameObjectList)
            {
                cacheGameObject.GetComponent<CoinStackPlaceholder>().Clear();
            }
        }
        
        public void GetCacheGameObjectList(out GameObject[] list)
        {
            list = cacheGameObjectList;
        }

        public GameObject GetCacheGameObjectByIndex(int index)
        {
            return cacheGameObjectList[index];
        }
    }
}
