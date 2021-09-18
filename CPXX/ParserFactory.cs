using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        }

        public IParser GetParser(string parser_name)
        {
            IParser parser = parseFiles.Find(x => x.ParserName == parser_name);
            
            if (parser != null)
            {
                return parser;
            }

            Assembly assembly = Assembly.GetExecutingAssembly(); // 获取当前程序集 
            dynamic obj = assembly.CreateInstance("CPXX.Parser" + parser_name); // 创建类的实例，返回为 object 类型，需要强制类型转换
            if (obj == null)
            {
                return null;
            }
            parseFiles.Add((IParser)obj);
            return (IParser)obj;
        }
    }
}
