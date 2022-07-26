public enum DockerExtractCommand
{
    List,
    Extract
}

internal class DockerExtractOptions
{
    private readonly List<string> _filesToExtract;
    public string? InputName { get; private set; }
    public IEnumerable<string> FilesToExtract { get => _filesToExtract; }
    public bool IgnorePaths { get; private set; }
    public bool UseRegex { get; private set; }
    public DockerExtractCommand Command { get; private set; }

    public DockerExtractOptions()
    {
        _filesToExtract = new List<string>();
        Command = DockerExtractCommand.Extract;
    }

    public static DockerExtractOptions FromArgs(string[] args)
    {
        var options = new DockerExtractOptions();
        for (var idx = 0; idx < args.Length; idx++)
        {
            if (args[idx] == "-i") idx += ParseInputName(args, idx, options);
            else if (args[idx] == "-f") idx += ParseFileToExtract(args, idx, options);
            else if (args[idx] == "--ignore-paths") options.IgnorePaths = true;
            else if (args[idx] == "--regex") options.UseRegex = true;
            else if (args[idx] == "list") options.Command = DockerExtractCommand.List;
            else if (args[idx] == "extract") options.Command = DockerExtractCommand.Extract;
        }
        return options;
    }

    public bool Validate()
    {
        return !String.IsNullOrEmpty(InputName) && _filesToExtract.Any();
    }

    private static int ParseFileToExtract(string[] args, int idx, DockerExtractOptions options)
    {
        var fileToExtract = args[idx + 1];
        options._filesToExtract.Add(fileToExtract);
        return 1;
    }

    private static int ParseInputName(string[] args, int idx, DockerExtractOptions options)
    {
        options.InputName = args[idx + 1];
        return 1;
    }



    
}