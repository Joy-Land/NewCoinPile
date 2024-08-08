using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using SOData;

public class CoinPilePanelAnim : MonoBehaviour
{
    [SerializeField] private GameObject vfxShatterPanel;
    private readonly float vfxDuration = 1.8f;
    public void ShatterPanel(List<CoinPanelItem> coinPanelItemList, List<GameObject> coinPileGameObjectList, GameObject coinPanelGameObject)
    {
        if (vfxShatterPanel != null)
        {
            // 计算特效的位置
            var centerPos = new Vector3();
            foreach (var coinPanelItem in coinPanelItemList)
            {
                centerPos += coinPileGameObjectList[coinPanelItem.coinPileIndex].transform.position;
            }
            centerPos /= coinPanelItemList.Count;
            centerPos.z = coinPanelGameObject.transform.position.z;
        
            // 生成特效
            var vfxGameObject = Instantiate(vfxShatterPanel);
            vfxGameObject.transform.position = centerPos;
        
            // 延迟销毁特效
            Destroy(vfxGameObject, vfxDuration);
        }
    }
}
