/*
功能描述：
    本模块用于维护OpenReadParameter：打开stream时候的默认参数
*/


namespace ThinRL.Core.FileSystem
{
    public struct OpenReadParameter
    {
        /// <summary>
        /// 文件路径（已经拼接好的可直接openread的）
        /// </summary>
        public string filePath { get; set; }

        /// <summary>
        /// read buffer的大小，单位：byte
        /// </summary>
        public int bufferSize { get; set; }

        /// <summary>
        /// stream的shareDetele行为
        /// </summary>
        public bool shareDelete { get; set; }

        /// <summary>
        /// 起始读取位置，该位置不为0的话，需要包装一层sub stream
        /// </summary>
        public long startPosition { get; set; }

        /// <summary>
        /// 长度，当文件为子文件的时候，需要提供长度，来进行读取截止判定
        /// </summary>
        public long fileSize { get; set; }

        /// <summary>
        /// 使用安卓Jni的方式
        /// </summary>
        public bool useAndroidJni { get; set; }

        public OpenReadParameter(string filePath, int bufferSize = 4096, bool shareDelete = false, long startPosition = 0, long fileSize = 0, bool useAndroidJni = false)
        {
            this.filePath = filePath;
            this.bufferSize = bufferSize;
            this.shareDelete = shareDelete;
            this.startPosition = startPosition;
            this.fileSize = fileSize;
            this.useAndroidJni = useAndroidJni;
        }

        /// <summary>
        /// 是否有效，当前只根据文件名不为空作为判定依据
        /// </summary>
        /// <returns></returns>
        public bool Valid()
        {
            return !string.IsNullOrEmpty(filePath) && !string.IsNullOrWhiteSpace(filePath);
        }
    }
}
