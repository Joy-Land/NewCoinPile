using System;
using System.Collections.Generic;

namespace ThinRL.Core
{
    public interface IReference
    {
    }
    public sealed class ReferencePool
    {
        private static Dictionary<Type, ReferencePool> _dictionary = new Dictionary<Type, ReferencePool>();
        private Queue<IReference> _queue = new Queue<IReference>();

        public T GetAReference<T>() where T : IReference , new()
        {
            if (_queue.Count > 0)
            {
                return (T)_queue.Dequeue();
            }
            return new T();
        }

        public void CollectAReference(IReference t)
        {
            _queue.Enqueue(t);
        }

        public static ReferencePool GetReferencePool<T>()
        {
            var type = typeof(T);
            if (_dictionary.ContainsKey(type) == false)
            {
                _dictionary[type] = new ReferencePool();
            }

            return _dictionary[type];
        }

        public static T GetReference<T>() where T : IReference, new()
        {
            return GetReferencePool<T>().GetAReference<T>();
        }

        public static void CollectReference(IReference t)
        {
            var type = t.GetType();
            if (_dictionary.ContainsKey(type) == false)
            {
                _dictionary[type] = new ReferencePool();
            }

            _dictionary[type].CollectAReference(t);
        }
    }
}