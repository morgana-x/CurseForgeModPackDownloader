using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForgeModPackDownloader
{
    public class packMetaData
    {
        public minecraftEntry minecraft { get; set; }
        public string manifestType { get; set; }
        public int manifestVersion { get; set; }
        public string name { get; set; }
        public string author { get; set; }
        public List<modEntry> files { get; set; }
    }
}
