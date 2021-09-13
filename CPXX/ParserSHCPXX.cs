using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPXX
{
    class ParserSHCPXX: IParser
    {
        public ParserSHCPXX()
        { }

        string IParser.ParserName => "SHCPXX";

        bool IParser.ParseFile(string filename, FileDefine fdefine, Dictionary<BlockDefine, ObservableCollection<ExpandoObject>> blDict)
        {
            // 数据存放定义
            blDict.Clear();

            // 找到最长的头，用于定位数据类型
            // 先按照最长的读取，然后按照字串截取判断跟哪一个block一致
            int headerLength = 0;
            fdefine.blockDefines.ForEach(x => {
                if (x.header.Length > headerLength)
                {
                    headerLength = x.header.Length;
                }
            });

            // 读取文件内容
            byte[] info = File.ReadAllBytes(filename);
            string header;
            int n = 0;
            string colName, colValue;
            BlockDefine bdefine;
            ObservableCollection<ExpandoObject> blockItems;
            while (n < info.Length)
            {
                // 定位合适的头，现在都是英文，都用UTF-8先看下
                header = Encoding.UTF8.GetString(info, n, headerLength);
                bdefine = fdefine.blockDefines.Find(x => {
                    return x.header.StartsWith(header.Substring(0, x.header.Length));
                });

                if (bdefine == null)
                {
                    throw new FileFormatException(string.Format("未识别的文件结构！{0} 位置{1}", header, n));
                }

                // 尝试读取一段数据，一般是一行
                dynamic item = new ExpandoObject();
                foreach (FileCol c in bdefine.file_cols)
                {
                    colName = c.col_name;
                    colValue = c.col_encoding.GetString(info, n, c.col_length); // 获取值
                    n += c.col_length;
                    n += 1;  // |分隔符和行尾换行

                    // 不显示的过滤掉
                    if (c.col_hidden)
                    {
                        continue;
                    }

                    // 如果是数字类型，直接把空格处理掉，减少占用的列宽
                    if (c.col_type == "N")
                    {
                        colValue = colValue.Trim();
                    }

                    (item as IDictionary<string, object>).Add(colName, colValue);
                }

                if (bdefine.drop)  // 配置为丢弃处理的话，直接扔掉
                {
                    continue;
                }

                // 如果是新出现的block，那么构建存放的集合
                if (!blDict.ContainsKey(bdefine))
                {
                    blDict.Add(bdefine, new());
                }
                blockItems = blDict[bdefine];
                blockItems.Add(item);
            }

            return true;
        }
    }
}
