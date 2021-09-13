using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPXX
{
    internal class ParserFactory
    {
        List<IParser> parseFiles = new();
        
        // 添加解析类
        public ParserFactory()
        {
            parseFiles.Add(new ParserOFD());
            parseFiles.Add(new ParserSHCPXX());
        }

        public IParser GetParser(string parser_name)
        {
            return parseFiles.Find(x => x.ParserName == parser_name);
        }
    }
}
