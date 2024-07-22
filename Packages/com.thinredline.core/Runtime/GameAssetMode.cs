using UnityEngine;

namespace ThinRL.Core
{
    // 游戏启动模式，开发阶段有不同模式可以使用，编辑器之外只有一种
    public static class GameAssetMode
    {
        public enum EGameAssetMode
        {
            LocalAssetDevLua = 0,     //git lua和asset
            LocalBundleDevLua = 1,    //git lua和编辑器本地打出的bundle
            RemoteBundlePure = 2,     //从网上下载的bundle和lua，编辑器之外只能用这种
            RemoteBundleDevLua = 3,   // 网上下载的bundle和git lua
        }

        // 必须要设置为LocalAssetDevLua，以便在非play时加载localAsset；
        static EGameAssetMode s_AssetMode = EGameAssetMode.LocalAssetDevLua;
        public static EGameAssetMode AssetMode
        {
            get
            {
                return s_AssetMode;
            }
            set
            {
                if (Application.isEditor == false)
                    s_AssetMode = EGameAssetMode.RemoteBundlePure;
                else
                    s_AssetMode = value;
            }
        }

        public static bool IsDownloadNeeded
        {
            get
            {
                return s_AssetMode == EGameAssetMode.RemoteBundleDevLua ||
                    s_AssetMode == EGameAssetMode.RemoteBundlePure;
            }
        }

        public static bool IsLocalBundle
        {
            get
            {
                return s_AssetMode == EGameAssetMode.LocalBundleDevLua;
            }
        }

        public static bool IsBundleUsed
        {
            get
            {
                return s_AssetMode != EGameAssetMode.LocalAssetDevLua;
            }
        }

        public static bool ShouldUseDevLua
        {
            get
            {
                return s_AssetMode != EGameAssetMode.RemoteBundlePure;
            }
        }

        public static bool IsRemoteBundle
        {
            get
            {
                return s_AssetMode == EGameAssetMode.RemoteBundleDevLua ||
                    s_AssetMode == EGameAssetMode.RemoteBundlePure;
            }
        }

        public static bool IsPureMode
        {
            get
            {
                return s_AssetMode == EGameAssetMode.RemoteBundlePure;
            }
        }


    }

}