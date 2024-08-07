using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPilePanelAnim : MonoBehaviour
{
    public void ShatterPanel(GameObject coinPanelGameObject, Action onComplete)
    {
        if (onComplete != null)
        {
            onComplete();
        }
    }
}
