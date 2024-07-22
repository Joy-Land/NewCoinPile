using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ThinRL.Core.Tools
{
    /// <summary>
    /// 基于 ProtoDataType <see cref="ThinRL.Core.Tools.ProtoDataType"/> 结构解析lua层传来的json
    /// </summary>
    /// <example>
    /// 例: lua table 
    /// <code>
    ///     {
    ///         {Name = "A", Type = TypeCode.Int32},
    ///         {Name = "B", Type = TypeCode.UInt16},
    ///         {Name = "C", Type = TypeCode.String},
    ///         {Name = "D", Type = TypeCode.Object, Table = {
    ///             {Name = "D1", Type = TypeCode.UInt16},
    ///             {Name = "D2", Type = TypeCode.Int32},
    ///         }}
    ///     }
    /// </code>
    ///     json
    /// <code>
    ///     {
    ///         "A": 1,
    ///         "B": 2,
    ///         "C": "abcedf",
    ///         "D": {
    ///             "D1": 21,
    ///             "D2": 22
    ///         }
    ///     }    
    /// </code>
    /// </example>
    public static class SerializeJsonTools
    {
        public static Encoding UTFEncoding = Encoding.UTF8;
        /// <summary>
        /// 基本类型数组 e: int[] IntList;
        /// </summary>
        /// <example>
        /// 例: lua table
        /// <code>
        ///     {
        ///         Name = "TestBasicArray", Type = TypeCode.BasicArray, Table = {
        ///             {Name = "", Type = TypeCode.Int32}
        ///         }
        ///     }
        /// </code>
        ///     json
        /// <code>
        ///     {   
        ///          "TestBasicArray": [1, 2, 4]
        ///     }    
        /// </code>
        /// </example>
        public const int TypeCodeBasicArray = 101;
        /// <summary>
        /// 自定义类型数组 e: DataClass[] DataList;
        /// </summary>
        /// <example>
        /// 例: lua table 
        /// <code>
        ///     {
        ///         Name = "TestCustomArray", Type = TypeCode.CustomArray, Table = {
        ///             {Name = "C1", Type = TypeCode.UInt16},    
        ///             {Name = "C2", Type = TypeCode.Int32},    
        ///         }  
        ///     }
        /// </code>
        ///     json
        /// <code>
        ///     {
        ///         "TestCustomArray":[
        ///             {"C1": 1, "C2": 3},
        ///             {"C1": 2, "C2": 4}
        ///         ]        
        ///     } 
        /// </code>   
        /// </example>
        public const int TypeCodeCustomArray = 102;
        /// <summary>
        /// 异构自定义类型数组，数组的每一项 根据枚举 选择给定的几种类型中的一种。
        /// 
        /// 数组中的元素从大的类别属于同一类，但小的类别又有不同，
        /// 与服务端交流的时候每个元素根据小类别用最少所需字段
        /// </summary>
        /// <example>
        /// 例: lua table
        ///     // Table 第一行固定填 Enums ，后续按照 Enums 每个标识的值对应的类型
        /// <code>
        ///     {
        ///         Name = "TestVariantArray", Type = TypeCode.VariantArray, Table = {
        ///             {Name = "VType", Type = TypeCode.U, Enums = {3, 5}},    
        ///             {
        ///                 {Name = "C1", Type = TypeCode.UInt16},    
        ///                 {Name = "C3", Type = TypeCode.Int32},    
        ///             },
        ///             {
        ///                 {Name = "C1", Type = TypeCode.UInt32},    
        ///                 {Name = "C4", Type = TypeCode.Int16},    
        ///                 {Name = "C5", Type = TypeCode.Int32},    
        ///             },
        ///         }  
        ///     }
        /// </code>
        ///     // VType 会被添加到每个子类型的第一个字段
        ///     // 根据VType的值在 Enums 中的位置，选择实际类型
        ///     json
        /// <code>
        ///     {
        ///         "TestVariantArray":[
        ///             {"VType":5, "C1": 41, "C4": 41, "C5": 43},
        ///             {"VType":3, "C1": 31, "C3": 32}
        ///             {"VType":3, "C1": 331, "C3": 332}
        ///             {"VType":5, "C1": 441, "C4": 442, "C5": 443},
        ///         ]
        ///     }
        /// </code>
        /// </example>
        public const int TypeCodeVariantArray = 103;


        /// <summary>
        /// BigString
        /// </summary>
        public const int TypeCodeBigString = 104;

        /// <summary>
        /// 根据ProtoDataType和JObject转换为byte数组
        /// </summary>
        /// <param name="data"></param>
        /// <param name="table"></param>
        /// <param name="isRootData"></param>
        /// <returns></returns>
        public static byte[] GetObjectBytes(object data, ProtoDataType table, bool isRootData = true)
        {

            string name = table.Name;
            int type = table.Type;
            object obj = data;
            if (isRootData)
            {
                if ((data as JObject).ContainsKey(name))
                    obj = (data as JObject)[name].Value<object>();
                else
                    UnityEngine.Debug.LogError("field " + name + " not found");
            }
            switch (table.Type)
            {
                case (int)TypeCode.Int32:
                    return BitConverter.GetBytes(Convert.ToInt32(obj));
                case (int)TypeCode.UInt32:
                    return BitConverter.GetBytes(Convert.ToUInt32(obj));
                case (int)TypeCode.UInt16:
                    return BitConverter.GetBytes(Convert.ToUInt16(obj));
                case (int)TypeCode.Int16:
                    return BitConverter.GetBytes(Convert.ToInt16(obj));
                case (int)TypeCode.SByte:
                    return new byte[] { (byte)Convert.ToSByte(obj) };
                case (int)TypeCode.Byte:
                    return new byte[] { Convert.ToByte(obj) };
                case (int)TypeCode.Single:
                    return BitConverter.GetBytes(Convert.ToSingle(obj));
                case (int)TypeCode.Boolean:
                    return BitConverter.GetBytes(Convert.ToBoolean(obj));
                case (int)TypeCode.Int64:
                    return BitConverter.GetBytes(Convert.ToInt64(obj));
                case (int)TypeCode.UInt64:
                    return BitConverter.GetBytes(Convert.ToUInt64(obj));
                case (int)TypeCode.Double:
                    return BitConverter.GetBytes(Convert.ToDouble(obj));
                case (int)TypeCode.String:
                    {
                        var str = Convert.ToString(obj);
                        var temp = UTFEncoding.GetBytes(str);
                        var ret = new byte[2 + temp.Length];
                        Buffer.BlockCopy(BitConverter.GetBytes((ushort)temp.Length), 0, ret, 0, 2);
                        Buffer.BlockCopy(temp, 0, ret, 2, temp.Length);
                        return ret;
                    }
                case TypeCodeBigString:
                    {
                        var str = Convert.ToString(obj);
                        var temp = UTFEncoding.GetBytes(str);
                        var ret = new byte[4 + temp.Length];
                        Buffer.BlockCopy(BitConverter.GetBytes(temp.Length), 0, ret, 0, 4);
                        Buffer.BlockCopy(temp, 0, ret, 4, temp.Length);
                        return ret;
                    }
                case (int)TypeCode.Object:
                    {
                        var bytes = new List<byte>();
                        for (int i = 0; i < table.Table.Length; i++)
                        {
                            bytes.AddRange(GetObjectBytes(obj, table.Table[i]));
                        }
                        return bytes.ToArray();
                    }
                case TypeCodeBasicArray:
                    {
                        var arr = obj as JArray;
                        var bytes = new List<byte>();
                        if (arr == null)
                        {
                            bytes.AddRange(BitConverter.GetBytes((ushort)0));
                        }
                        else
                        {
                            bytes.AddRange(BitConverter.GetBytes((ushort)arr.Count));
                            for (int i = 0; i < arr.Count; i++)
                            {
                                bytes.AddRange(GetObjectBytes(arr[i].Value<object>(), table.Table[0], false));
                            }
                        }
                        return bytes.ToArray();
                    }
                case TypeCodeCustomArray:
                    {
                        var arr = obj as JArray;
                        var bytes = new List<byte>();
                        if (arr == null)
                        {
                            bytes.AddRange(BitConverter.GetBytes((ushort)0));
                        }
                        else
                        {
                            bytes.AddRange(BitConverter.GetBytes((ushort)arr.Count));
                            for (int i = 0; i < arr.Count; i++)
                            {
                                for (int j = 0; j < table.Table.Length; j++)
                                {
                                    bytes.AddRange(GetObjectBytes(arr[i].Value<JObject>(), table.Table[j]));
                                }
                            }
                        }
                        return bytes.ToArray();
                    }
                case TypeCodeVariantArray:
                    {
                        var arr = obj as JArray;
                        var enumTable = table.Table[0];
                        var bytes = new List<byte>();
                        if (arr == null)
                        {
                            bytes.AddRange(BitConverter.GetBytes((ushort)0));
                        }
                        else
                        {
                            bytes.AddRange(BitConverter.GetBytes((ushort)arr.Count));
                            for (int i = 0; i < arr.Count; i++)
                            {
                                var enumValue = arr[i][enumTable.Name].Value<int>();
                                var enumIndex = Array.IndexOf(enumTable.Enums, enumValue);
                                if (enumIndex == -1)
                                {
                                    UnityEngine.Debug.LogError("not support enumValue " + enumValue);
                                    continue;
                                }
                                if (enumTable.Type == (int)TypeCode.Byte)
                                {
                                    bytes.Add((byte)enumValue);
                                }
                                else if (enumTable.Type == (int)TypeCode.Int16)
                                {
                                    bytes.AddRange(BitConverter.GetBytes((short)enumValue));
                                }

                                var table2 = table.Table[enumIndex + 1];
                                var item = arr[i][table2.Name].Value<JObject>();
                                for (int j = 0; j < table2.Table.Length; j++)
                                {
                                    bytes.AddRange(GetObjectBytes(item, table2.Table[j]));
                                }
                            }
                        }
                        return bytes.ToArray();
                    }
                default:
                    {
                        UnityEngine.Debug.LogError("type not found " + type);
                        return null;
                    }
            }
        }

        /// <summary>
        /// 根据 ProtoDataType 从 buffer 构造原始数据JObject
        /// </summary>
        /// <param name="table"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static object GetObject(ProtoDataType table, ByteBuffer buffer)
        {
            switch (table.Type)
            {
                case (int)TypeCode.Int32:
                    return buffer.ReadInt();
                case (int)TypeCode.UInt32:
                    return buffer.ReadUInt();
                case (int)TypeCode.UInt16:
                    return buffer.ReadUShort();
                case (int)TypeCode.Int16:
                    return buffer.ReadShort();
                case (int)TypeCode.SByte:
                    return (sbyte)(buffer.ReadByte());
                case (int)TypeCode.Byte:
                    return buffer.ReadByte();
                case (int)TypeCode.Single:
                    return buffer.ReadFloat();
                case (int)TypeCode.String:
                    return buffer.ReadString();
                case (int)TypeCode.Boolean:
                    return buffer.ReadBoolean();
                case (int)TypeCode.Int64:
                    return buffer.ReadLong();
                case (int)TypeCode.UInt64:
                    return buffer.ReadUlong();
                case (int)TypeCode.Double:
                    return buffer.ReadDouble();
                case (int)TypeCode.Object:
                    {
                        JObject obj = new JObject();
                        for (int i = 0; i < table.Table.Length; i++)
                        {
                            var table1 = table.Table[i];
                            obj.Add(new JProperty(table1.Name, GetObject(table1, buffer)));
                        }
                        return obj;
                    }
                case TypeCodeBasicArray:
                    {
                        var len = buffer.ReadUShort();
                        JArray arr = new JArray();
                        if (len == 0) return arr;
                        if (len > 100) UnityEngine.Debug.LogWarning("array len:" + len);

                        var table1 = table.Table[0];
                        for (int i = 0; i < len; i++)
                        {
                            arr.Add(GetObject(table1, buffer));
                        }
                        return arr;
                    }
                case TypeCodeCustomArray:
                    {
                        var len = buffer.ReadUShort();
                        JArray arr = new JArray();
                        if (len == 0) return arr;
                        if (len > 100) UnityEngine.Debug.LogWarning("array len:" + len);

                        for (int i = 0; i < len; i++)
                        {
                            JObject obj = new JObject();
                            for (int j = 0; j < table.Table.Length; j++)
                            {
                                var table1 = table.Table[j];
                                obj.Add(new JProperty(table1.Name, GetObject(table1, buffer)));
                            }
                            arr.Add(obj);
                        }
                        return arr;
                    }
                case TypeCodeVariantArray:
                    {
                        var len = buffer.ReadUShort();
                        JArray arr = new JArray();
                        if (len == 0) return arr;
                        if (len > 100) UnityEngine.Debug.LogWarning("array len:" + len);

                        for (int i = 0; i < len; i++)
                        {
                            JObject obj = new JObject();
                            var enumTable = table.Table[0];
                            int enumIndex = -1;
                            if (enumTable.Type == (int)TypeCode.Byte)
                            {
                                var enumValue = buffer.ReadByte();
                                obj.Add(new JProperty(enumTable.Name, enumValue));
                                enumIndex = Array.IndexOf(enumTable.Enums, enumValue);
                            }
                            else if (enumTable.Type == (int)TypeCode.Int16)
                            {
                                var enumValue = buffer.ReadShort();
                                obj.Add(new JProperty(enumTable.Name, enumValue));
                                enumIndex = Array.IndexOf(enumTable.Enums, enumValue);
                            }

                            if (enumIndex >= 0)
                            {
                                var table1 = table.Table[enumIndex + 1];
                                JObject obj1 = new JObject();
                                for (int j = 0; j < table1.Table.Length; j++)
                                {
                                    obj1.Add(new JProperty(table1.Table[j].Name, GetObject(table1.Table[j], buffer)));
                                }
                                obj.Add(table1.Name, obj1);
                            }
                            arr.Add(obj);
                        }
                        return arr;
                    }
                case TypeCodeBigString:
                    return buffer.ReadBigString();
                default:
                    {
                        UnityEngine.Debug.LogError("type not found " + table.Type);
                        return null;
                    }
            }
        }

        public static void FillObject(JObject ret, ProtoDataType table, ByteBuffer buffer, int indexOffest = 0)
        {
            for (int i = indexOffest; i < table.Table.Length; i++)
            {
                var prop = table.Table[i];
                ret.Add(new JProperty(prop.Name, GetObject(prop, buffer)));
                if (prop.StopDecodeIfNotEqual)
                {
                    var value = Convert.ToInt32(ret[prop.Name].Value<object>());
                    // UnityEngine.Debug.Log(prop.Name + ":" + value);
                    if (value != prop.CheckValue)
                        break;
                }
            }
        }
    }
}
