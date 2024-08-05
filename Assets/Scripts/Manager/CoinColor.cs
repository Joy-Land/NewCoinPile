using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manager
{
    public enum CoinColor
    {
        Black,
        Blue,
        BlueDark,
        Brown,
        Green,
        GreenDark,
        Orange,
        Pink,
        Purple,
        Red,
        Teal,
        Yellow,
        GlassBlue,
        GlassGreen,
        GlassPurple,
        GlassRed,
        GlassYellow,
        Empty
    }

    public static class CoinColorHelper
    {
        public static string GetCoinColor(CoinColor coinColor)
        {
            switch (coinColor)
            {
                case CoinColor.Black:
                    return "Black";
                case CoinColor.Blue:
                    return "Blue";
                case CoinColor.BlueDark:
                    return "BlueDark";
                case CoinColor.Brown:
                    return "Brown";
                case CoinColor.Green:
                    return "Green";
                case CoinColor.GreenDark:
                    return "GreenDark";
                case CoinColor.Orange:
                    return "Orange";
                case CoinColor.Pink:
                    return "Pink";
                case CoinColor.Purple:
                    return "Purple";
                case CoinColor.Red:
                    return "Red";
                case CoinColor.Teal:
                    return "Teal";
                case CoinColor.Yellow:
                    return "Yellow";
                case CoinColor.GlassBlue:
                    return "Yellow";
                case CoinColor.GlassGreen:
                    return "Yellow";
                case CoinColor.GlassPurple:
                    return "Yellow";
                case CoinColor.GlassRed:
                    return "Yellow";
                case CoinColor.GlassYellow:
                    return "Yellow";
                case CoinColor.Empty:
                    return "Empty";
            }

            return "";
        }
    }

}
