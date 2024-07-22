using System;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace ThinRL.Core
{
    public abstract class GameLogicLooper
    {
        public delegate void UpdateDelegate();
        /// <summary>
        /// 当前帧数
        /// </summary>
        public uint FrameCount { get; private set; }

        /// <summary>
        /// 开启后会补帧
        /// 默认关闭
        /// </summary>
        public bool SpeedMode = false;
        protected abstract float DeltaTime { get; }
        private float lastUpdateTime = 0;
        public bool Enable = true;
        public UpdateDelegate PreUpdateQueue = delegate {  };
        public UpdateDelegate AfterUpdateQueue = delegate {  };
        
        public void Update()
        {
            if (SpeedMode == false)
            {
                if (lastUpdateTime + DeltaTime > Time.time)
                {
                    return;
                }


                lastUpdateTime += DeltaTime;
                if (Enable == false) return;
                FrameCount++;
                PreUpdateQueue();
                LogicUpdate();
                AfterUpdateQueue();
            }
            else
            {
                while (lastUpdateTime + DeltaTime <= Time.time)
                {
                    lastUpdateTime += DeltaTime;
                    if (Enable == false) return;
                    FrameCount++;
                    PreUpdateQueue();
                    LogicUpdate();
                    AfterUpdateQueue();
                }
            }
        }
        
        public void ForceUpdate()
        {
            lastUpdateTime = Time.time;
            FrameCount++;
            PreUpdateQueue();
            LogicUpdate();
            AfterUpdateQueue();
        }

        protected abstract void LogicUpdate();
    }
}