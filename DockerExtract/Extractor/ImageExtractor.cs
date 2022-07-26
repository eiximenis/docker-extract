using DockerExtract.Extensions;
using ICSharpCode.SharpZipLib.Tar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DockerExtract.Extractor
{
    class ImageExtractor
    {
        private readonly string _filePath;
        private bool disposedValue;
        private bool _opened;
        private readonly List<LayerExtractor> _layers;

        public Manifest? Manifest { get; private set; }

        public ImageExtractor(string imageFilePath)
        {
            _filePath = imageFilePath;
            _opened = false;
            _layers = new List<LayerExtractor>();
            Manifest = null;
        }

        public async Task Open()
        {
            Manifest = await ReadManifest();
            foreach (var layer in Manifest.Layers)
            {
                var layerExtractor = new LayerExtractor(layer);
                _layers.Add(layerExtractor);
            }
        }

        public void IndexAllLayers()
        {
            foreach (var layer in _layers.Where(l => !l.IsIndexed))
            {
                IndexLayer(layer);
            }
        }

        public LayerExtractor? FindFile(string fullPath, bool ignoreInputPaths)
        {
            var fileLayer = _layers.FirstOrDefault(l => l.HasFile(fullPath, ignoreInputPaths));
            return fileLayer;
        }

        public async Task<bool> ExtractFile(string fullPath, string destPath, bool ignoreInputPaths)
        {
            var fileLayer = FindLayerWithFile(fullPath, ignoreInputPaths);

            return fileLayer switch
            {
                null => false,
                _ => await ExtractFileFromLayer(fileLayer, fileLayer.GetLayerEntry(fullPath, ignoreInputPaths)!.Path, destPath)
            };
        }

        private LayerExtractor? FindLayerWithFile(string fullPath, bool ignoreInputPaths)
        {
            foreach (var layer in _layers)
            {
                if (!layer.IsIndexed)
                {
                    IndexLayer(layer);
                }
                if (layer.HasFile(fullPath, ignoreInputPaths))
                {
                    return layer;
                }
            }

            return null;
        }

        private void IndexLayer(LayerExtractor layer)
        {
            using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read);
            using var tarStream = new TarInputStream(fs, Encoding.UTF8);
            tarStream.SeekToName(layer.LayerFile);
            layer.IndexFromStream(tarStream);
        }

        private async Task<bool> ExtractFileFromLayer(LayerExtractor layer, string entryFile, string destPath)
        {
            using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read);
            using var tarStream = new TarInputStream(fs, Encoding.UTF8);
            tarStream.SeekToName(layer.LayerFile);
            return await layer.CopyFileFromStream(tarStream, entryFile, destPath);
        }

        private async Task<Manifest> ReadManifest()
        {
            using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read);
            using var tarStream = new TarInputStream(fs, Encoding.UTF8);

            var found = tarStream.SeekToName("manifest.json");

            if (found)
            {
                using (var stream = new MemoryStream())
                {
                    tarStream.CopyEntryContents(stream);
                    stream.Position = 0;
                    return (await JsonSerializer.DeserializeAsync<IEnumerable<Manifest>>(stream)).First();
                }
            }

            throw new InvalidOperationException("No manifest.json found!");
        }

    }
}
