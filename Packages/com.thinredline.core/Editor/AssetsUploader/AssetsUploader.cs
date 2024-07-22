using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThinRL.Editor
{
    [Serializable]
    public class AssetsUploader
    {
        /// <summary>
        /// 覆盖模式，如果不使用，会尝试将源文件/文件夹删除再上传
        /// </summary>
        public bool coverMode = true; 

        /// <summary>
        /// 要上传到的cdn路径
        /// </summary>
        public string bucketKey = string.Empty;

        /// <summary>
        /// 待上传的资源路径
        /// </summary>
        public List<string> filesPath = new List<string>();
    }

}
