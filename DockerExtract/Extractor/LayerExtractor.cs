using DockerExtract.Extensions;
using ICSharpCode.SharpZipLib.Tar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DockerExtract.Extractor
{
    record LayerEntry (string Path, long Bytes);

    internal class LayerExtractor
    {

        public string LayerFile { get; }
        public bool IsIndexed { get; private set; }
        private readonly List<LayerEntry> _contents;

        public LayerExtractor(string layerFile)
        {
            LayerFile = layerFile;
            IsIndexed = false;
            _contents = new List<LayerEntry>();
        }

        public async Task<bool> SearchForFile(string fileName)
        {
            return false;
        }

        public bool HasFile(string fullPath, bool ignorePath)
        {
            return GetLayerEntry(fullPath, ignorePath) is not null;
        }

        public LayerEntry? GetLayerEntry(string file, bool ignorePath)
        {
            if (ignorePath)
            {
                return _contents.FirstOrDefault(c => Path.GetFileName(c.Path) == file);
            }
            return _contents.FirstOrDefault(c => c.Path == file);
        }



        internal void IndexFromStream(Stream layerStream)
        {
            using var tarStream = new TarInputStream(layerStream, Encoding.UTF8);
            var entry = (TarEntry?)null;
            while ((entry = tarStream.GetNextEntry()) != null)  // Go through all files from archive
            {
                _contents.Add(new LayerEntry(entry.Name, entry.Size));
            }
            IsIndexed = true;
        }

        public async Task<bool> CopyFileFromStream(TarInputStream layerStream, string filePath, string destPath)
        {
            using var tarStream = new TarInputStream(layerStream, Encoding.UTF8);
            var found = tarStream.SeekToName(filePath);
            if (!found)
            {
                return false;
            }

            using var outFile = new FileStream(destPath, FileMode.CreateNew, FileAccess.Write);
            tarStream.CopyEntryContents(outFile);
            outFile.Close();
            return true;
        }
    }
}
