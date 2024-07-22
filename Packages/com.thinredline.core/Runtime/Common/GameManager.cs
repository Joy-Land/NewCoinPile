using System;
using System.Collections.Generic;
using ThinRL.Core.Tools;
using ThinRL.Core;
using UnityEngine;

namespace ThinRL.Core
{
    [DefaultExecutionOrder(-200)]
    public class GameManager : MonoSingleton<GameManager>
    {
        private List<IManager> _managers;
        private Dictionary<Type, IManager> _managerDictionary;
        private GameLogicLooper _gameLogicLooper;
        private BinaryHeap<ActionItem> _actions;
        public void Init()
        {
            _managers = new List<IManager>();
            _managerDictionary = new Dictionary<Type, IManager>();
            _actions = new BinaryHeap<ActionItem>(ActionItem.Compare);
            _gameLogicLooper = null;
        }

        public T AddManager<T>() where T : IManager, new()
        {
            var type = typeof(T);
            if (_managerDictionary.ContainsKey(type))
            {
                Debug.LogWarningFormat("{0}已经被加入到Gamemanager中了", type);
                return (T)_managerDictionary[type];
            }

            var manager = new T();
            manager.Init();
            _managers.Add(manager);
            _managerDictionary.Add(type, manager);
            return manager;
        }

        public IManager AddManager(IManager manager)
        {
            var type = manager.GetType();
            if (_managerDictionary.ContainsKey(type))
            {
                Debug.LogWarningFormat("{0}已经被加入到Gamemanager中了", type);
                return _managerDictionary[type];
            }

            manager.Init();
            _managers.Add(manager);
            _managerDictionary.Add(type, manager);
            return manager;
        }

        public T GetManager<T>() where T : IManager
        {
            var type = typeof(T);
            if (_managerDictionary.ContainsKey(type) == false)
            {
                return default;
            }

            return (T)_managerDictionary[type];
        }

        public IManager GetManager(string typeName)
        {
            var type = Type.GetType(typeName);
            if (_managerDictionary.ContainsKey(type) == false)
            {
                return default;
            }

            return _managerDictionary[type];
        }

        public void SetLooper(GameLogicLooper looper)
        {
            _gameLogicLooper = looper;
            _gameLogicLooper.AfterUpdateQueue += ManagersUpdate;
            _gameLogicLooper.AfterUpdateQueue += UpdateActions;
        }

        public void Pause()
        {
            var managerArr = _managers.ToArray();
            foreach (var manager in managerArr)
            {
                manager.OnPause();
            }
        }

        public void Resume()
        {
            var managerArr = _managers.ToArray();
            foreach (var manager in managerArr)
            {
                manager.OnResume();
            }
        }

        public void RemoveManager<T>() where T : IManager
        {
            var type = typeof(T);
            if (!_managerDictionary.ContainsKey(type)) return;
            var manager = _managerDictionary[type];
            manager.OnDestroy();
            _managers.Remove(manager);
            _managerDictionary.Remove(type);
        }

        private void ManagersUpdate()
        {
            //TODO: 优化实现
            var managerArr = _managers.ToArray();
            foreach (var manager in managerArr)
            {
                manager.Update(_gameLogicLooper.FrameCount);
            }
        }

        private void OnDestroy()
        {
            var managerArr = _managers.ToArray();
            foreach (var manager in managerArr)
            {
                manager.OnDestroy();
            }
        }

        private bool paused = false;
        private void OnApplicationPause(bool pause)
        {
            if (paused == pause)
                return;
            paused = pause;

            if (pause)
            {
                Pause();
            }
            else
            {
                Resume();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (paused != hasFocus)
                return;
            paused = !hasFocus;

            if (hasFocus)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        private void Update()
        {
            _gameLogicLooper?.Update();
        }

        private void UpdateActions()
        {
            while (_actions.Count > 0)
            {
                Action action = null;
                lock (_actions)
                {
                    var item = _actions.Peek();
                    if (item.TriggerTime > _gameLogicLooper.FrameCount) break;
                    action = _actions.Pop().Action;
                }
                action?.Invoke();
            }
        }
        private static long _queueCount = 0;
        public void PushActionInMainThread(Action action, long delay = 0)
        {
            lock (_actions)
            {
                _queueCount++;
                _actions.Push(new ActionItem(delay + _gameLogicLooper.FrameCount, _queueCount, action));
            }
        }


        private struct ActionItem
        {
            public long TriggerTime;
            public long Count;
            public Action Action;
            public ActionItem(long triggerTime, long count, Action action)
            {
                TriggerTime = triggerTime;
                Count = count;
                Action = action;
            }
            public static bool Compare(ActionItem a, ActionItem b)
            {
                return a.TriggerTime == b.TriggerTime ? a.Count < b.Count : a.TriggerTime < b.TriggerTime;
            }
        }
    }
}