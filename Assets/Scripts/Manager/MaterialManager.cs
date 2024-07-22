using System.ComponentModel;
using Framework;
using UnityEngine;
using Manager;

namespace Manager
{
    public class MaterialManager : SingletonBaseMono<MaterialManager>
    {
        [SerializeField] private Material blackMaterial;
        [SerializeField] private Material blueMaterial;
        [SerializeField] private Material blueDarkMaterial;
        [SerializeField] private Material brownMaterial;
        [SerializeField] private Material greenMaterial;
        [SerializeField] private Material greenDarkMaterial;
        [SerializeField] private Material orangeMaterial;
        [SerializeField] private Material pinkMaterial;
        [SerializeField] private Material purpleMaterial;
        [SerializeField] private Material redMaterial;
        [SerializeField] private Material tealMaterial;
        [SerializeField] private Material yellowMaterial;
        
        public Material GetMaterialByColor(CoinColor color)
        {
            switch (color)
            {
                case CoinColor.Black:
                    return blackMaterial;
                case CoinColor.Blue:
                    return blueMaterial;
                case CoinColor.BlueDark:
                    return blueDarkMaterial;
                case CoinColor.Brown:
                    return brownMaterial;
                case CoinColor.Green:
                    return greenMaterial;
                case CoinColor.GreenDark:
                    return greenDarkMaterial;
                case CoinColor.Orange:
                    return orangeMaterial;
                case CoinColor.Pink:
                    return pinkMaterial;
                case CoinColor.Purple:
                    return purpleMaterial;
                case CoinColor.Red:
                    return redMaterial;
                case CoinColor.Teal:
                    return tealMaterial;
                case CoinColor.Yellow:
                    return yellowMaterial;
            }
    
            return null;
        }
    }
}
