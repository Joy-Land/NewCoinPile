using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ThinRL.Core.Tools
{
    public static class SerializeTools
    {
        private static readonly object[] ConstructorParams = new object[0];
        public static Encoding UtfEncoding = Encoding.UTF8;
        public static BindingFlags FieldBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField | BindingFlags.GetField;
        public static BindingFlags PropertyBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.GetProperty;
        public static readonly Dictionary<Type, PropertyInfo[]> ValueTypeProperties = new Dictionary<Type, PropertyInfo[]> {
            {typeof(Rect), new PropertyInfo[] {typeof(Rect).GetProperty("x"), typeof(Rect).GetProperty("y"), typeof(Rect).GetProperty("width"), typeof(Rect).GetProperty("height") } },
            {typeof(RectInt), new PropertyInfo[] {typeof(RectInt).GetProperty("x"), typeof(RectInt).GetProperty("y"), typeof(RectInt).GetProperty("width"), typeof(RectInt).GetProperty("height") } },
        };
        public static readonly Dictionary<Type, FieldInfo[]> ValueTypeFileds = new Dictionary<Type, FieldInfo[]>();

        public static byte[] GetObjectBytes(object obj, Type propType = null)
        {
            switch (obj)
            {
                case int i:
                    return BitConverter.GetBytes(i);
                case Enum @enum:
                    {
                        var typeCode = @enum.GetTypeCode();
                        if (typeCode == TypeCode.Byte)
                        {
                            return BitConverter.GetBytes((byte)Convert.ChangeType(@enum, typeof(byte)));
                        }
                        else if (typeCode == TypeCode.Int16)
                        {
                            return BitConverter.GetBytes((short)Convert.ChangeType(@enum, typeof(short)));
                        }
                        else
                        {
                            return BitConverter.GetBytes((int)Convert.ChangeType(@enum, typeof(int)));
                        }
                    }
                case uint u:
                    return BitConverter.GetBytes(u);
                case ushort @ushort:
                    return BitConverter.GetBytes(@ushort);
                case short s:
                    return BitConverter.GetBytes(s);
                case byte b:
                    return new byte[] { b };
                case float f:
                    return BitConverter.GetBytes(f);
                case bool b:
                    return BitConverter.GetBytes(b);
                case long l:
                    return BitConverter.GetBytes(l);
                case ulong @ulong:
                    return BitConverter.GetBytes(@ulong);
                case double d:
                    return BitConverter.GetBytes(d);
                case string s:
                    {
                        var str = s;
                        var temp = UtfEncoding.GetBytes(str);
                        var ret = new byte[2 + temp.Length];
                        Buffer.BlockCopy(BitConverter.GetBytes((ushort)temp.Length), 0, ret, 0, 2);
                        Buffer.BlockCopy(temp, 0, ret, 2, temp.Length);
                        return ret;
                    }
                case BigString bigString:
                    {
                        var str = bigString.Value;
                        var temp = UtfEncoding.GetBytes(str);
                        var ret = new byte[4 + temp.Length];
                        Buffer.BlockCopy(BitConverter.GetBytes(temp.Length), 0, ret, 0, 4);
                        Buffer.BlockCopy(temp, 0, ret, 4, temp.Length);
                        return ret;
                    }
                case BigBytes bigBytes:
                    {
                        var bytes = bigBytes.Value;
                        var ret = new byte[4 + bytes.Length];
                        Buffer.BlockCopy(BitConverter.GetBytes(bytes.Length), 0, ret, 0, 4);
                        Buffer.BlockCopy(bytes, 0, ret, 4, bytes.Length);
                        return ret;
                    }
                default:
                    {
                        if (propType != null && propType.IsAbstract)
                        {
                            var bytes = new List<byte>();
                            bytes.AddRange(GetObjectBytes(obj.GetType().FullName));
                            bytes.AddRange(GetObjectBytes(obj.GetType().Assembly.GetName().Name));
                            bytes.AddRange(GetObjectBytes(obj));
                            return bytes.ToArray();
                        }
                        else if (obj.GetType().IsValueType)
                        {
                            var tType = obj.GetType();
                            var bytes = new List<byte>() { };
                            if (ValueTypeProperties.TryGetValue(tType, out var tProps))
                            {
                                for (var i = 0; i < tProps.Length; i++)
                                {
                                    var prop = tProps[i].GetValue(obj);
                                    bytes.AddRange(GetObjectBytes(prop, tProps[i].PropertyType));
                                }
                            }
                            else
                            {
                                if (ValueTypeFileds.TryGetValue(tType, out var tFields) == false)
                                {
                                    tFields = tType.GetFields(FieldBindingFlags);
                                    ValueTypeFileds[tType] = tFields;
                                }
                                for (int i = 0; i < tFields.Length; i++)
                                {
                                    var field = tFields[i].GetValue(obj);
                                    bytes.AddRange(GetObjectBytes(field, tFields[i].FieldType));
                                }
                            }
                            return bytes.ToArray();
                        }
                        else switch (obj)
                            {
                                case Array array:
                                    {
                                        var arr = array;
                                        var bytes = new List<byte>();
                                        bytes.AddRange(BitConverter.GetBytes((ushort)arr.Length));
                                        var tType = array.GetType().GetElementType();
                                        for (var i = 0; i < arr.Length; i++)
                                        {
                                            bytes.AddRange(GetObjectBytes(arr.GetValue(i), tType));
                                        }
                                        return bytes.ToArray();
                                    }
                                case IList list:
                                    {
                                        var arr = list;
                                        var bytes = new List<byte>();
                                        bytes.AddRange(BitConverter.GetBytes((ushort)arr.Count));
                                        var tType = list.GetType().GetGenericArguments()[0];
                                        for (var i = 0; i < arr.Count; i++)
                                        {
                                            bytes.AddRange(GetObjectBytes(arr[i], tType));
                                        }
                                        return bytes.ToArray();
                                    }
                                default:
                                    {
                                        var tProps = obj.GetType().GetProperties(PropertyBindingFlags);
                                        var bytes = new List<byte>() { };
                                        for (var i = 0; i < tProps.Length; i++)
                                        {
                                            var prop = tProps[i].GetValue(obj);
                                            bytes.AddRange(GetObjectBytes(prop, tProps[i].PropertyType));
                                        }
                                        return bytes.ToArray();
                                    }
                            }
                    }
            }
        }

        public static object GetObject(Type type, ByteBuffer buffer)
        {
            if (type.IsEnum)
            {
                var typeCode = Type.GetTypeCode(type.GetEnumUnderlyingType());
                switch (typeCode)
                {
                    case TypeCode.Byte:
                        {
                            var e = buffer.ReadByte();
                            return Enum.ToObject(type, e);
                        }
                    case TypeCode.Int16:
                        {
                            var e = buffer.ReadShort();
                            return Enum.ToObject(type, e);
                        }
                    default:
                        {
                            var e = buffer.ReadInt();
                            return Enum.ToObject(type, e);
                        }
                }
            }
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int32:
                    {
                        var tInt = buffer.ReadInt();
                        return tInt;
                    }
                case TypeCode.UInt32:
                    return buffer.ReadUInt();
                case TypeCode.UInt16:
                    return buffer.ReadUShort();
                case TypeCode.Int16:
                    return buffer.ReadShort();
                case TypeCode.Byte:
                    return buffer.ReadByte();
                case TypeCode.Single:
                    return buffer.ReadFloat();
                case TypeCode.String:
                    return buffer.ReadString();
                case TypeCode.Boolean:
                    return buffer.ReadBoolean();
                case TypeCode.Int64:
                    return buffer.ReadLong();
                case TypeCode.UInt64:
                    return buffer.ReadUlong();
                case TypeCode.Double:
                    return buffer.ReadDouble();
                case TypeCode.Object when type.IsEquivalentTo(typeof(BigString)):
                    {
                        var str = new BigString { Value = buffer.ReadBigString() };
                        return str;
                    }
                case TypeCode.Object when type.IsEquivalentTo(typeof(BigBytes)):
                    {
                        var len = buffer.ReadInt();
                        var bytes = new BigBytes { Value = new byte[len] };
                        if (len == 0) return bytes;
                        for (var j = 0; j < len; j++)
                        {
                            bytes.Value[j] = buffer.ReadByte();
                        }
                        return bytes;
                    }
                case TypeCode.Object when type.IsAbstract:
                    {
                        var typeName = buffer.ReadString();
                        var assemblyName = buffer.ReadString();
                        return GetObject(Type.GetType(typeName + "," + assemblyName), buffer);
                    }
                case TypeCode.Object when type.IsGenericType:
                    {
                        // UnityEngine.Debug.Log("arr:" + type.FullName);
                        //TODO:这里默认generic类型就是List类型!需要扩展的时候要进行修改！
                        if (type.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            var len = buffer.ReadUShort();
                            var constructor = type.GetConstructors()[0];
                            var tType = type.GetGenericArguments()[0];
                            var list = constructor.Invoke(new object[0]) as System.Collections.IList;
                            for (int j = 0; j < len; j++)
                            {
                                list.Add(GetObject(tType, buffer));
                            }
                            return list;
                        }
                        else
                        {
                            var data = type.GetConstructors()[0].Invoke(ConstructorParams);
                            var tProps = type.GetProperties(PropertyBindingFlags);
                            FillObject(data, tProps, buffer);
                            return data;
                        }
                    }
                case TypeCode.Object when type.IsArray:
                    {
                        // UnityEngine.Debug.Log("arr:" + type.Name);
                        var len = buffer.ReadUShort();
                        var constructor = type.GetConstructors()[0];
                        var arr = (Array)constructor.Invoke(new object[] { len });
                        if (len == 0) return arr;
                        var tType = type.GetElementType();
                        for (int j = 0; j < len; j++)
                        {
                            arr.SetValue(GetObject(tType, buffer), j);
                        }
                        return arr;
                    }
                case TypeCode.Object when type.IsValueType:
                    {
                        // UnityEngine.Debug.Log("value type:" + type.FullName);
                        var data = Activator.CreateInstance(type);
                        if (ValueTypeProperties.TryGetValue(type, out var tProps))
                        {
                            FillObject(data, tProps, buffer);
                        }
                        else
                        {
                            if (ValueTypeFileds.TryGetValue(type, out var tFileds) == false)
                            {
                                tFileds = type.GetFields(FieldBindingFlags);
                                ValueTypeFileds[type] = tFileds;
                            }
                            FillObjectField(data, tFileds, buffer);
                        };
                        return data;
                    }
                case TypeCode.Object:
                    {
                        var data = type.GetConstructors()[0].Invoke(ConstructorParams);
                        var tProps = type.GetProperties(PropertyBindingFlags);
                        FillObject(data, tProps, buffer);
                        return data;
                    }
            }
            return null;
        }

        public static void FillObject(object ret, PropertyInfo[] props, ByteBuffer buffer, int indexOffest = 0)
        {
            for (var i = indexOffest; i < props.Length; i++)
            {
                var prop = props[i];
                prop.SetValue(ret, GetObject(prop.PropertyType, buffer));
                if (!prop.IsDefined(typeof(StopDecodeIfNotEqualAttribute))) continue;
                var value = prop.GetCustomAttribute<StopDecodeIfNotEqualAttribute>().CheckValue;
                if (prop.GetValue(ret).Equals(value) == false)
                    break;
            }
        }
        public static void FillObjectField(object ret, FieldInfo[] tFields, ByteBuffer buffer, int indexOffest = 0)
        {
            for (var i = indexOffest; i < tFields.Length; i++)
            {
                var field = tFields[i];
                field.SetValue(ret, GetObject(field.FieldType, buffer));
                if (!field.IsDefined(typeof(StopDecodeIfNotEqualAttribute))) continue;
                var value = field.GetCustomAttribute<StopDecodeIfNotEqualAttribute>().CheckValue;
                if (field.GetValue(ret).Equals(value) == false)
                    break;
            }
        }
    }
}
