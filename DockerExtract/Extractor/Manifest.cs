using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DockerExtract.Extractor
{
    internal record Manifest (string Config, IEnumerable<string> RepoTags, IEnumerable<string> Layers);
}
