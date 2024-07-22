using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace ThinRL.Core.FileSystem
{
    public static partial class StreamExtension
    {
        public static string Name(this Stream stream)
        {
            if (stream is FileStream fs)
            {
                return fs.Name;
            }

            return stream.ToString();
        }
    }
}
