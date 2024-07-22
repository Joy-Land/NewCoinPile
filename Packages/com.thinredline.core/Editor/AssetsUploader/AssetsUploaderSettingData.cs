using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThinRL.Editor
{
    public class AssetsUploaderSettingData
    {
        /// <summary>
        /// 配置数据是否被修改
        /// </summary>
        public static bool IsDirty { private set; get; } = false;

        private static AssetsUploaderSetting _setting = null;
        public static AssetsUploaderSetting Setting
        {
            get
            {
                if (_setting == null)
                    _setting = TRLSettingLoader.LoadSettingData<AssetsUploaderSetting>();
                return _setting;
            }
        }


        public static void SaveFile()
        {
            if (Setting != null)
            {
                IsDirty = false;
                EditorUtility.SetDirty(Setting);
                AssetDatabase.SaveAssets();
                Debug.Log($"{nameof(AssetsUploaderSetting)}.asset is saved!");
            }
        }


        public static void CreateUploader(AssetsUploader uploader)
        {
            Setting.Uploaders.Add(uploader);
            IsDirty = true;
        }

        public static void RemoveUploader(AssetsUploader uploader)
        {
            if (Setting.Uploaders.Remove(uploader))
            {
                IsDirty = true;
            }
            else
            {
                Debug.LogWarning($"Failed remove collector : {uploader}");
            }
        }

        public static void ModifyUploader(AssetsUploader uploader)
        {
            if (uploader != null)
            {
                IsDirty = true;
            }
        }

    }

     

    public class TRLSettingLoader
    {
        /// <summary>
        /// 加载相关的配置文件
        /// </summary>
        public static TSetting LoadSettingData<TSetting>() where TSetting : ScriptableObject
        {
            var settingType = typeof(TSetting);
            var guids = AssetDatabase.FindAssets($"t:{settingType.Name}");
            if (guids.Length == 0)
            {
                Debug.LogWarning($"Create new {settingType.Name}.asset");
                var setting = ScriptableObject.CreateInstance<TSetting>();
                string filePath = $"Assets/{settingType.Name}.asset";
                AssetDatabase.CreateAsset(setting, filePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return setting;
            }
            else
            {
                if (guids.Length != 1)
                {
                    foreach (var guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        Debug.LogWarning($"Found multiple file : {path}");
                    }
                    throw new System.Exception($"Found multiple {settingType.Name} files !");
                }

                string filePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                var setting = AssetDatabase.LoadAssetAtPath<TSetting>(filePath);
                return setting;
            }
        }
    }

}
