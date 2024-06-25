using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForgeModPackDownloader
{
    public class minecraftEntry
    {
        public string version { get; set; }
        public List<modLoaderEntry> modLoaders { get; set; }

    }
}
