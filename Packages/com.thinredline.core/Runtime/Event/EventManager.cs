using System;
using System.Collections.Generic;

namespace ThinRL.Core
{
    public class EventArgsPack
    {
        private object[] m_Args;

        public object[] args
        {
            get
            {
                return m_Args;
            }
        }

        public EventArgsPack(params object[] args)
        {
            m_Args = args;
        }

        public object GetData(int index)
        {
            if (index < m_Args.Length)
            {
                return m_Args[index];
            }
            return null;
        }

        public T GetData<T>(int index)
        {
            T result = default(T);
            if (index < m_Args.Length)
            {
                var arg = m_Args[index];
                if (arg is T)
                {
                    result = (T)arg;
                }
            }
            return result;
        }

        public object[] GetData()
        {
            return m_Args;
        }
    }

    public class EventManager : Singleton<EventManager>
    {
        public void Dispose()
        {
            m_Events.Clear();
        }

        public delegate void EventHandler(object sender, EventArgsPack e);

        private Dictionary<string, List<EventHandler>> m_Events = new Dictionary<string, List<EventHandler>>();



        public void DispatchEvent(string evt, object sender, EventArgsPack e)
        {
            if (m_Events.ContainsKey(evt))
            {
                List<EventHandler> newList = new List<EventHandler>(m_Events[evt]);
                foreach (var act in newList)
                {
                    act(sender, e);
                }
            }
        }

        public void AddEvent(string evt, EventHandler hander)
        {
            if (!m_Events.ContainsKey(evt))
            {
                m_Events.Add(evt, new List<EventHandler>());
            }
            m_Events[evt].Add(hander);
        }

        public void RemoveEvent(string evt, EventHandler hander)
        {
            if (m_Events.ContainsKey(evt))
            {
                m_Events[evt].Remove(hander);
            }
        }
    }
}
