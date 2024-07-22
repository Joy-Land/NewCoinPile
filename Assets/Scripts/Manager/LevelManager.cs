using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using SOData;
using UnityEngine;

namespace Manager
{
    public class LevelManager : SingletonBaseMono<LevelManager>
    {
        [SerializeField] private List<LevelData> levelList;

        public int GetLevelListCount()
        {
            return levelList.Count;
        }

        public Boolean GetLevelDataByIndex(int index, out LevelData levelData)
        {
            if (index < levelList.Count && index >= 0)
            {
                levelData = levelList[index];
                return true;
            }

            levelData = null;
            return false;
        }
    }
}
