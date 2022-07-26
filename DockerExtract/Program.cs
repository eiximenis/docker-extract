using DockerExtract.Extractor;
using ICSharpCode.SharpZipLib.Tar;
using System.Text;
using System.Text.Json;


var options = DockerExtractOptions.FromArgs(args);

if (!options.Validate())
{
    Help();
    return;
}

var reader = new ImageExtractor(options.InputName);
await reader.Open();
var manifest = reader.Manifest;

switch (options.Command)
{
    case DockerExtractCommand.List:
        ListFiles(reader, options);
        break;
    case DockerExtractCommand.Extract:
        await ExtractFiles(reader, options);
        break;
}



async Task ExtractFiles(ImageExtractor reader, DockerExtractOptions options)
{
    foreach (var file in options.FilesToExtract)
    {
        var found = await reader.ExtractFile(file, Path.GetFileName(file), options.IgnorePaths);
        if (!found)
        {
            await Console.Error.WriteLineAsync($"File {file} not found");
        }
    }
}

void ListFiles(ImageExtractor reader, DockerExtractOptions options)
{

    reader.IndexAllLayers();
    foreach (var file in options.FilesToExtract)
    {
        var layer = reader.FindFile(file, options.IgnorePaths);
        if (layer is not null)
        {
            var entry = layer.GetLayerEntry(file, options.IgnorePaths)!;
            Console.WriteLine(layer.LayerFile);
            Console.WriteLine($"\t {entry.Path} ({entry.Bytes})");
        }
    }
}


void Help()
{
    Console.WriteLine("docker-extract: extract or list a file inside a docker image");
    Console.WriteLine("Usage: docker-extract [command] -i tar-input-file -f file-to-extract [flags]");
    Console.WriteLine("\t command is either list or extract (defaults to extract)");
    Console.WriteLine("\t -i image in tar format (result of docker save command)");
    Console.WriteLine("\t -f file to extract. Can use multiple times -f file1 -f file2 ... -f fileN");
    Console.WriteLine("\t Flags:");
    Console.WriteLine("\t\t --ignore-paths: If set, no need to specify the full file path inside the image in -f");
    Console.WriteLine("");
    Console.WriteLine("Example:");
    Console.WriteLine("docker-extract -i image.tar -f foo.txt --ignore-paths  ---> Search file foo.txt (any path) inside image and saves it");
    Console.WriteLine("docker-extract -i image.tar -f /some/path/foo.txt ---> Search file /some/path/foo.txt inside image and saves it");
    Console.WriteLine("docker-extract list -i image.tar -f /some/path/foo.txt ---> List layer that contains file /some/path/foo.txt");
}
