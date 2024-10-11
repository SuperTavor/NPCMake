using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPCMake.Core.Utils.Tinifan.Tools;

namespace NPCMake.Core.Utils.Tinifan.Archive
{
    public interface IArchive
    {
        string Name { get; }

        VirtualDirectory Directory { get; set; }

        void Save(string path);

        void Close();
    }
}