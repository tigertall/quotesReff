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
    class ParserOFD : IParser
    {
        public ParserOFD()
        { }

        string IParser.ParserName => "OFD";

        bool IParser.ParseFile(string filename, FileDefine fdefine, Dictionary<BlockDefine, ObservableCollection<ExpandoObject>> blDict)
        {
            // 数据存放定义
            blDict.Clear();

            BlockDefine bdefine = fdefine.blockDefines[0];

            string[] info = File.ReadAllLines(filename, Encoding.GetEncoding("GBK"));

            // OFD的特别处理，当作文本读取，跳过头部定义
            // 前10行定义跳过；前9行是10，第10行是多少字段；字段后是多少行数据；最后一个文件结束行跳过

            // 存在非必须字段，文件里可能没有，保存下文件里提供的字段
            int colsCount = int.Parse(info[9]);
            List<string> fileCols = new();
            int index_line = 10, i = 0;
            while (i < colsCount)
            {
                i++;
                fileCols.Add(info[index_line].Trim().ToLower()); // 不规范的大小写头，有些字段全大写，转换下比较
                index_line++;
            }

            // 记录数
            int recdsCount = int.Parse(info[index_line]);
            index_line++;

            string line, colName, colValue;
            i = 0;
            ObservableCollection<ExpandoObject> blockItems;
            while (i < recdsCount)
            {
                i++;
                line = info[index_line];
                index_line++;

                byte[] line_bytes = Encoding.GetEncoding("GBK").GetBytes(line);
                // 尝试读取一段数据，一般是一行
                int n = 0;
                dynamic item = new ExpandoObject();
                foreach (FileCol c in bdefine.file_cols)
                {

                    // 过滤掉文件中不存在的非列
                    if (!fileCols.Contains(c.col_name_eng.ToLower()))
                    {
                        continue;
                    }

                    colName = c.col_name;
                    colValue = c.col_encoding.GetString(line_bytes, n, c.col_length); // 获取值
                    n += c.col_length;

                    // 不显示的过滤掉
                    if (c.col_hidden)
                    {
                        continue;
                    }

                    // 如果是数字类型，直接把空格处理掉，减少占用的列宽
                    // OFD文件展示再处理下格式，加上小数点
                    if (c.col_type == "N")
                    {
                        colValue = colValue.Trim();
                        if (c.col_scale > 0) // OFD的默认展示太难看了，优化下展示
                        {
                            colValue = string.Format("{0:F" + c.col_scale + "}", long.Parse(colValue) / Math.Pow(10, c.col_scale));
                        }
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
