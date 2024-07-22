
#if UNITY_ANDROID && false // || true
using System;
using System.IO;
using UnityEngine;
using System.Threading;

namespace ThinRL.Core.FileSystem
{
    public class AndroidJniZipAccessor : FileUtil.IZipAccessor
    {
        private const string ACTIVITY_JAVA_CLASS = "com.unity3d.player.UnityPlayer";

        private static AndroidJavaObject assetManager;

        protected static AndroidJavaObject AssetManager
        {
            get
            {
                if (assetManager != null)
                    return assetManager;

                using (AndroidJavaClass activityClass = new AndroidJavaClass(ACTIVITY_JAVA_CLASS))
                {
                    using (var context = activityClass.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        assetManager = context.Call<AndroidJavaObject>("getAssets");
                    }
                }
                return assetManager;
            }
        }
        /// <summary>
        /// RuntimeInitializeOnLoadMethod执行顺序：
        /// SubsystemRegistration
        /// AfterAssembliesLoaded
        /// BeforeSplashScreen
        /// BeforeSceneLoad
        /// AfterSceneLoad
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void OnInitialized()
        {
            FileUtil.Register(new AndroidJniZipAccessor());

            JvmThreadAttacher.mainThread = Thread.CurrentThread.ManagedThreadId;
        }

        protected static string GetAssetFilePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            int start = path.LastIndexOf("!/assets/");
            if (start < 0)
                return path;

            return path.Substring(start + 9);
        }

        public int Priority { get { return 0; } }

        public bool Exists(string path)
        {
            try
            {
                using (AndroidJavaObject fileDescriptor = AssetManager.Call<AndroidJavaObject>("openFd", GetAssetFilePath(path)))
                {
                    if (fileDescriptor != null)
                        return true;
                }
            }
            catch (Exception) { }

            return false;
        }

        public Stream OpenRead(string path, int bufferSize = 4096)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("the filename is null or empty.");

            return new InputStreamWrapper(AssetManager.Call<AndroidJavaObject>("open", GetAssetFilePath(path)));
        }

        public static Stream OpenReadStatic(string path, int bufferSize = 4096)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("the filename is null or empty.");

            return new InputStreamWrapper(AssetManager.Call<AndroidJavaObject>("open", GetAssetFilePath(path)));
        }

        public IAttachableStreamProvider GetStreamProvider(string path, int bufferSize = 4096)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("the filename is null or empty.");

            var parameter = new OpenReadParameter
            {
                filePath = path,
                bufferSize = bufferSize,
                shareDelete = false,
                useAndroidJni = true
            };

            return new DefaultStreamProvider(parameter);
        }

        public bool Support(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            string fullname = path.ToLower();
            if (fullname.IndexOf(".apk") > 0 && fullname.LastIndexOf("!/assets/") > 0)
                return true;

            return false;
        }

        public class InputStreamWrapper : Stream
        {
            //private object _lock = new object();
            private long length = 0;
            private long position = 0;
            private AndroidJavaObject inputStream;
            private static IntPtr ReadMethod = IntPtr.Zero;

            public InputStreamWrapper(AndroidJavaObject inputStream)
            {
                this.inputStream = inputStream;

                // 如果本身就是主线程，那么也不会有太多消耗
                using (var attacher = JvmThreadAttacher.AttachCurrentThread())
                {
                    this.length = JniAvailable();
                }
            }

            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanSeek
            {
                get { return true; }
            }

            public override bool CanWrite
            {
                get { return false; }
            }

            public override long Length
            {
                get { return this.length; }
            }

            public override long Position
            {
                get { return this.position; }
                set { Seek(value, SeekOrigin.Begin); }
            }

            public override void Flush()
            {
                throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                //lock (_lock)
                using (var attacher = JvmThreadAttacher.AttachCurrentThread())
                {
                    int ret = 0;
                    IntPtr array = IntPtr.Zero;
                    try
                    {
#if UNITY_2019_3_OR_NEWER
                        array = AndroidJNI.NewSByteArray(count);
#else
                        array = AndroidJNI.NewByteArray(count);
#endif
                        if (IntPtr.Zero == ReadMethod)
                        {
                            ReadMethod = AndroidJNIHelper.GetMethodID(inputStream.GetRawClass(), "read", "([BII)I");
                        }

                        ret = AndroidJNI.CallIntMethod(inputStream.GetRawObject(), ReadMethod, new[] {
                            new jvalue { l = array },
                            new jvalue { i = offset },
                            new jvalue { i = count }
                        });

#if UNITY_2019_3_OR_NEWER
                        sbyte[] data = AndroidJNI.FromSByteArray(array);
#else
                        byte[] data = AndroidJNI.FromByteArray(array);
#endif
                        ret = Math.Max(0, ret);

                        position += ret;

                        Buffer.BlockCopy(data, 0, buffer, offset, ret);
                    }
                    finally
                    {
                        if (array != IntPtr.Zero)
                            AndroidJNI.DeleteLocalRef(array);
                    }

                    return ret;

                }
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                //lock (_lock)
                using (var attacher = JvmThreadAttacher.AttachCurrentThread())
                {
                    if (!CanSeek)
                    {
                        throw new InvalidOperationException("The stream is unseekable.");
                    }

                    long pos = position;

                    switch (origin)
                    {
                        case SeekOrigin.Begin:
                            pos = offset;
                            break;
                        case SeekOrigin.Current:
                            pos += offset;
                            break;
                        case SeekOrigin.End:
                            pos = Length + offset;
                            break;
                    }

                    if (pos != position)
                    {
                        if (pos > position)
                        {
                            position += JniSkip(pos - position);
                        }
                        else
                        {
                            JniReset();

                            position = JniSkip(pos);
                        }
                    }

                    return position;
                }
            }
            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            protected override void Dispose(bool disposing)
            {
                //lock (_lock)
                using (var attacher = JvmThreadAttacher.AttachCurrentThread())
                {
                    base.Dispose(disposing);

                    if (inputStream != null)
                    {
                        JniClose();

                        inputStream.Dispose();
                        inputStream = null;
                    }
                }
            }

#region wrappers of java methods
            private int JniAvailable()
            {
                return inputStream.Call<int>("available");
            }

            private bool JniMarkSupported()
            {
                return inputStream.Call<bool>("markSupported");
            }

            private void JniMark(int readlimit)
            {
                if (JniMarkSupported())
                {
                    inputStream.Call("mark", readlimit);
                }
            }

            public void JniReset()
            {
                inputStream.Call("reset");
            }

            private long JniSkip(long n)
            {
                return inputStream.Call<long>("skip", n);
            }

            private void JniClose()
            {
                inputStream.Call("close");
            }
#endregion
        }

        /// <summary>
        /// Stream异步加载bundle报错，需要AndroidJNI.AttachCurrentThread来解决问题
        /// </summary>
        public struct JvmThreadAttacher : IDisposable
        {
            internal static int mainThread;

            private bool attached;

            public JvmThreadAttacher TryAttachCurrentThread()
            {
                if (Thread.CurrentThread.ManagedThreadId != mainThread)
                {
                    attached = AndroidJNI.AttachCurrentThread() == 0;
                }
                else
                {
                    attached = false;
                }

                return this;
            }

            public static JvmThreadAttacher AttachCurrentThread()
            {
                return new JvmThreadAttacher().TryAttachCurrentThread();
            }

            public void Dispose()
            {
                if (attached)
                {
                    attached = false;

                    AndroidJNI.DetachCurrentThread();
                }
            }
        }
    }
}
#endif