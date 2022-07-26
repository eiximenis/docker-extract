using ICSharpCode.SharpZipLib.Tar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DockerExtract.Extensions
{
    static class TarInputStreamExtensions
    {
        public static bool SeekToName(this TarInputStream tarStream, string name)
        {
            var entry = (TarEntry?)null;
            while ((entry = tarStream.GetNextEntry()) != null)  // Go through all files from archive
            {
                if (!entry.IsDirectory)
                {
                    if (entry.Name == name)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
