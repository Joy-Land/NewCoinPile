namespace ThinRL.Core
{
    public abstract class Singleton<T> where T : new()
    {
        private static object lockObj = new object();

        protected static T s_instance;
        public static T Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (lockObj)
                    {
                        if (s_instance == null)
                            s_instance = new T();
                    }
                }
                return s_instance;
            }
        }

    }

}