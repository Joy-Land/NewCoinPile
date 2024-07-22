/*
功能描述：
    本模块用于维护异常模板类
*/

using System;
using System.Runtime.Serialization;
using System.Security;

namespace ThinRL
{
    internal static class ExceptionValue
    {
        public static readonly string Format = "{0} Exception: {1}\nFull Name: {2}";
    }

    [Serializable]
    public class Exception<T> : Exception, ISerializable
    {
        public Exception() : base(string.Format(ExceptionValue.Format, typeof(T).Name, string.Empty, typeof(T).FullName))
        {
        }

        public Exception(string message) : base(string.Format(ExceptionValue.Format, typeof(T).Name, message, typeof(T).FullName))
        {
        }

        public Exception(string message, Exception innerException) : base(string.Format(ExceptionValue.Format, typeof(T).Name, message, typeof(T).FullName), innerException)
        {
        }

        protected Exception(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
