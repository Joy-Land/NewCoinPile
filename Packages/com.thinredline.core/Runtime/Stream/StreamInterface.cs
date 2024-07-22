/*
功能描述：
    本模块用于维护Stream相关接口与委托定义
*/

using System;
using System.IO;

namespace ThinRL.Core.FileSystem
{
    public delegate Stream DelegateOpenRead(OpenReadParameter parameter);  // PS：从重载角度考虑不应设计成DelegateOpenReadByParameter

    public delegate IStreamConverter DelegateStreamConverterProvider(Stream stream, OpenReadParameter parameter);

    /// <summary>
    /// 流逐位转换器
    /// </summary>
    public interface IStreamConverter : IDisposable
    {
        /// <summary>
        /// 逐位转换
        /// </summary>
        /// <param name="buffer">数据源，上层需保证offset+count <= buffer.size()</param>
        /// <param name="offset">起始位置偏移</param>
        /// <param name="length">数量</param>
        /// <param name="position">stream当前位置</param>
        void Convert(byte[] buffer, int offset, int length, long position);
    }

    /// <summary>
    /// 流提供者
    /// </summary>
    public interface IAttachableStreamProvider : IDisposable
    {
        /// <summary>
        /// 打开stream时候的参数
        /// </summary>
        OpenReadParameter parameter { get; set; }

        /// <summary>
        /// 打开方式
        /// </summary>
        DelegateOpenRead delegateOpenRead { get; set; }

        /// <summary>
        /// 提供Stream
        /// </summary>
        /// <param name="tryCreateIfNotExists"></param>
        /// <returns></returns>
        Stream Provide();

        /// <summary>
        /// 是否有效
        /// </summary>
        /// <returns></returns>
        bool Valid();

        /// <summary>
        /// Stream是否有效
        /// </summary>
        /// <returns></returns>
        bool ValidStream();

        /// <summary>
        /// Provider是否有效
        /// </summary>
        /// <returns></returns>
        bool ValidProvider();

        /// <summary>
        /// 连接文件
        /// </summary>
        void Attach();

        /// <summary>
        /// 断开文件
        /// </summary>
        void Detach();

        /// <summary>
        /// 释放过时的文件
        /// </summary>
        void DetachOutOfTime();

        /// <summary>
        /// 最后一次访问到现在的ticks间隔，由于性能考虑，底层会用最高效的维护方式，而不是System.DateTime.Now
        /// update1：现在设计为seek时候的时间，防止不必要的时间消耗，以10w次计
        /// </summary>
        void UpdateLastEnvironmentTickCount();

        /// <summary>
        /// 获取最后一次访问时间
        /// </summary>
        /// <returns></returns>
        long GetLastEnvironmentTickCount();
    }
}
