using System;

namespace ThinRL.Core.Tools
{
    public class ByteBuffer
    {
        protected byte[] bytes;
        protected int rIndex, wIndex;
        // 缓冲区未读取大小
        protected int length;
        protected int size;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="size">需要用到的字节数，构造时会申请四倍的空间,后面一半的空间是前面一半的镜像，方面读取</param>
        public ByteBuffer(int size)
        {
            this.size = size * 2;
            bytes = new byte[size * 4];
            rIndex = wIndex = length = 0;   //r->read, w->write
        }

        public void SetReadIndex(int index)
        {
            rIndex = index % bytes.Length;
        }

        public int GetReadIndex()
        {
            return rIndex;
        }

        public void SetWriteIndex(int index)
        {
            wIndex = index % bytes.Length;
        }

        public void WriteBytes(byte[] bytes, int count)
        {
            if (length + count > size)
            {
                UnityEngine.Debug.LogError("Buffer塞不下了!");
                return;
            }
            if (wIndex + count > size)
            {
                Buffer.BlockCopy(bytes, 0, this.bytes, wIndex, size - wIndex);
                Buffer.BlockCopy(bytes, 0, this.bytes, size + wIndex, size - wIndex);
                Buffer.BlockCopy(bytes, size - wIndex, this.bytes, 0, count - size + wIndex);
                Buffer.BlockCopy(bytes, size - wIndex, this.bytes, size, count - size + wIndex);
                wIndex = wIndex + count - size;
            }
            else
            {
                Buffer.BlockCopy(bytes, 0, this.bytes, wIndex, count);
                Buffer.BlockCopy(bytes, 0, this.bytes, size + wIndex, count);
                wIndex = wIndex + count;
            }
            length += count;
        }

        public int ReadInt()
        {
            rIndex += 4;
            return BitConverter.ToInt32(bytes, rIndex - 4);
        }

        public short ReadShort()
        {
            rIndex += 2;
            return BitConverter.ToInt16(bytes, rIndex - 2);
        }

        public ushort ReadUShort()
        {
            rIndex += 2;
            return BitConverter.ToUInt16(bytes, rIndex - 2);
        }

        public uint ReadUInt()
        {
            rIndex += 4;
            return BitConverter.ToUInt32(bytes, rIndex - 4);
        }

        public float ReadFloat()
        {
            rIndex += 4;
            return BitConverter.ToSingle(bytes, rIndex - 4);
        }

        public string ReadString()
        {
            var len = BitConverter.ToUInt16(bytes, rIndex);
            rIndex += len + 2;
            return len == 0 ? "" : SerializeTools.UtfEncoding.GetString(bytes, rIndex - len, len);
        }
        
        public string ReadBigString()
        {
            var len = BitConverter.ToInt32(bytes, rIndex);
            rIndex += len + 4;
            return len == 0 ? "" : SerializeTools.UtfEncoding.GetString(bytes, rIndex - len, len);
        }

        public byte ReadByte()
        {
            rIndex++;
            return bytes[rIndex - 1];
        }

        public bool ReadBoolean()
        {
            rIndex++;
            return bytes[rIndex - 1] > 0;
        }
        public long ReadLong()
        {
            rIndex += 8;
            return BitConverter.ToInt64(bytes, rIndex - 8);
        }
        public ulong ReadUlong()
        {
            rIndex += 8;
            return BitConverter.ToUInt64(bytes, rIndex - 8);
        }

        public double ReadDouble()
        {
            rIndex += 8;
            return BitConverter.ToDouble(bytes, rIndex - 8);
        }

        public ByteBuffer ReadBuffer(int len)
        {
            rIndex += len;
            ByteBuffer buffer = new ByteBuffer(len);
            Buffer.BlockCopy(bytes, rIndex - len, buffer.bytes, 0, len);
            return buffer;
        }

    }
}