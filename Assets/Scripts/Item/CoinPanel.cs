using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;
using SOData;

public class CoinPanel : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer panelRenderer;

    public void SetUpBlendShape(CoinPanelBlendShape coinPanelBlendShape)
    {
        panelRenderer.SetBlendShapeWeight(0, coinPanelBlendShape.height);
        panelRenderer.SetBlendShapeWeight(1, coinPanelBlendShape.width);
        panelRenderer.SetBlendShapeWeight(2, coinPanelBlendShape.cross);
        panelRenderer.SetBlendShapeWeight(3, coinPanelBlendShape.triangle);
        panelRenderer.SetBlendShapeWeight(4, coinPanelBlendShape.triangle2);
        panelRenderer.SetBlendShapeWeight(5, coinPanelBlendShape.triangle3);
        panelRenderer.SetBlendShapeWeight(6, coinPanelBlendShape.baklava);
        panelRenderer.SetBlendShapeWeight(7, coinPanelBlendShape.heart);
    }

    public void SetUpColor(CoinColor coinColor)
    {
        var material = MaterialManager.Instance.GetMaterialByColor(coinColor);
        panelRenderer.materials[1] = material;
    }
}
