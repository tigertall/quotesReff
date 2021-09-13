using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPXX
{
    interface IParser
    {
        public string ParserName { get; }

        public bool ParseFile(string filename, FileDefine fdefine, Dictionary<BlockDefine, ObservableCollection<ExpandoObject>> blDict);

    }
}
