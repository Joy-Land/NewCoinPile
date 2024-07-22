using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThinRL.Core.Tools
{
    /// <summary>
    /// lua层协议结构
    /// </summary>
    [Serializable]
    public class ProtoDataType
    {
        /// <summary>
        /// 字段名称
        /// </summary>
        /// <value></value>
        [SerializeField]
        public string Name;
        /// <summary>
        /// 字段类型，跟 <see cref="System.TypeCode"/>对应，
        /// 另外添加三种自定义类型，参见 <see cref="ThinRL.Core.Tools.SerializeJsonTools"/> 里的详细描述
        /// </summary>
        /// <value></value>
        [SerializeField]
        public int Type;
        /// <summary>
        /// 自定义类型使用
        /// </summary>
        /// <value></value>
        [SerializeField]
        public int[] Enums;
        /// <summary>
        /// 相同结构嵌套
        /// </summary>
        /// <value></value>
        [SerializeField]
        public ProtoDataType[] Table;
        /// <summary>
        /// 如果值和StopValue值不相等，停止解析后续字段
        /// </summary>
        [SerializeField]
        public bool StopDecodeIfNotEqual;
        /// <summary>
        /// 判定是否停止解析的值
        /// </summary>
        [SerializeField]
        public int CheckValue;
    }

}