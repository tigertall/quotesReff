﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace CPXX
{
    public class FileDefine
    {
        public string file_type;
        public string file_pattern;
        public string file_description;
        public List<BlockDefine> blockDefines;

        public FileDefine()
        {
            blockDefines = new List<BlockDefine>();
        }
    }

    public class BlockDefine
    {
        public string header;
        public string name;
        public int frozencolumncount;
        public int rows; // 记录行数
        public int row_length;

        public bool drop; // 是否丢弃，无效行列

        public List<FileCol> file_cols;

        public BlockDefine()
        {
            header = string.Empty;
            name = string.Empty;
            frozencolumncount = 0;
            drop = false;
            file_cols = new List<FileCol>();
        }

        public int GetRowLength()
        {
            row_length = 0;
            foreach (FileCol c in file_cols)
            {
                row_length += c.col_length;
                row_length += 1; // 竖线分隔符，最后一个是换行符
            }

            return row_length;
        }
    }

    public class FileCol
    {
        public string col_no;
        public string col_name;
        public string col_type;
        public int col_length;
        public int col_scale;
        public bool col_canfilter;
        public bool col_hidden;
        public string col_comments;
        public Encoding col_encoding;

        public FileCol(string colNo, string colName, string colTypeDesc, bool colFilter=false, bool colHidden=false)
        {
            col_no = colNo;
            col_name = colName;
            col_type = colTypeDesc.Substring(0, 1); //C6 N19(2)
            int indexLeft = colTypeDesc.IndexOf("(");
            int indexRight = colTypeDesc.IndexOf(")");

            if (indexLeft != -1)
            {
                col_length = int.Parse(colTypeDesc[1..indexLeft]);
                col_scale = int.Parse(colTypeDesc[(indexLeft + 1)..indexRight]);
            } 
            else
            {
                col_length = int.Parse(colTypeDesc[1..]);
                col_scale = 0;
            }

            col_canfilter = colFilter;
            col_hidden = colHidden;
            col_comments = string.Empty;
        }

    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //使用CodePagesEncodingProvider去注册扩展编码。
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            // 加载文件格式定义
            LoadDefine();
            dataGrid.ItemsSource = quotesItems;
        }

        readonly List<FileDefine> fileDefines = new();

        // 保存数据条目
        readonly ObservableCollection<ExpandoObject> quotesItems = new();
        readonly Dictionary<BlockDefine, ObservableCollection<ExpandoObject>> blDict = new();

        string filterText = string.Empty;
        private FileDefine fdefine;

        private void LoadDefine()
        {
            XmlDocument doc = new();
            XmlReaderSettings sett = new();
            sett.IgnoreComments = true;
            sett.IgnoreWhitespace = true;
            XmlReader reader = XmlReader.Create("FileDefine.xml", sett);
            doc.Load(reader);

            Encoding encGBK = Encoding.GetEncoding("GBK"); ;
            XmlNode xn = doc.SelectSingleNode("files");
            foreach (XmlNode fl in xn.ChildNodes)  // 遍历file
            {
                FileDefine fdefine = new();
                fdefine.file_type = fl.Attributes["type"].InnerText;
                fdefine.file_pattern = fl.Attributes["pattern"].InnerText;

                fdefine.file_description = fl.Attributes["description"] != null ? fl.Attributes["description"].InnerText: string.Empty;

                foreach (XmlNode x in fl.ChildNodes) // 遍历block
                {
                    BlockDefine bdefine = new();

                    bdefine.header = x.Attributes["header"] !=null ? x.Attributes["header"].InnerText: string.Empty;
                    bdefine.name = x.Attributes["name"] != null ? x.Attributes["name"].InnerText : string.Empty;
                    bdefine.frozencolumncount = x.Attributes["frozencolumncount"] != null ? int.Parse(x.Attributes["frozencolumncount"].InnerText) : 0;
                    bdefine.drop = x.Attributes["drop"] != null && x.Attributes["drop"].InnerText == "1";

                    string colName;
                    XmlNode fn = x.SelectSingleNode("fields");
                    foreach (XmlNode f in fn.ChildNodes) // 遍历field
                    {
                        // 字段名称中不能出现/，datagrid会当成路径查找资源，处理下fjy中 T-1日基金收益/基金净值 这种列名
                        colName = f.Attributes["name"].InnerText.Replace("/", "");

                        FileCol col = new(f.Attributes["no"].InnerText, colName, f.Attributes["type"].InnerText);

                        col.col_hidden = f.Attributes["hidden"] != null && f.Attributes["hidden"].InnerText == "1";
                        col.col_canfilter = f.Attributes["filter"] != null && f.Attributes["filter"].InnerText == "1";
                        col.col_encoding = encGBK;
                        if (f.Attributes["encoding"] != null)
                        {
                            if(f.Attributes["encoding"].InnerText == "UTF16-LE")
                            {
                                col.col_encoding = Encoding.Unicode;
                            }
                        }

                        bdefine.file_cols.Add(col);
                    }
                    
                    fn = x.SelectSingleNode("comments"); // 遍历注释
                    foreach (XmlNode f in fn.ChildNodes)
                    {
                        FileCol col = bdefine.file_cols.Find(col => { return col.col_no == f.Attributes["no"].InnerText; });
                        col.col_comments = f.Attributes["comments"].InnerText;
                    }

                    fdefine.blockDefines.Add(bdefine);
                }
               
                fileDefines.Add(fdefine);
            }
            reader.Close();
        }

        private void LoadFile(string filename)
        {
            if (!File.Exists(filename))
            {
                tbFile.Text = "文件不存在！";
                return;
            }

            // 判断文件类型，正则表达式匹配适配是哪个定义
            string shortname = System.IO.Path.GetFileName(filename);
            fdefine = fileDefines.Find(f => { return Regex.IsMatch(shortname, f.file_pattern, RegexOptions.Compiled); });

            if (fdefine == null)
            {
                tbFile.Text = "不支持的文件类型！";
                return;
            }

            // 读取 cpxx02/reff/fjyYYYMMDD/mktdt04
            bool bRes = ParseFile(filename);
            if (!bRes)
            {
                return;
            }

            // 构建tabcontrol和datagrid视图
            int i = 0;
            foreach(BlockDefine bd in blDict.Keys)
            {
                (tbcBlock.Items[i] as TabItem).Visibility = Visibility.Visible;
                i++;
            }


            BlockDefine bdefine = fdefine.blockDefines[0];

            // 没有列启用了filter属性，不支持过滤
            bool canFilter = bdefine.file_cols.Exists(c => { return c.col_canfilter; });
            tbFilterText.IsEnabled = canFilter;

            btnFile.IsEnabled = false;
            sbError.Content = string.Empty;

            // 定义头
            quotesItems.Clear();
            dataGrid.Columns.Clear();
            dataGrid.FrozenColumnCount = bdefine.frozencolumncount;
            string colName;
            foreach (FileCol c in bdefine.file_cols)
            {
                if (c.col_hidden)
                {
                    continue;
                }

                colName = c.col_name;

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = colName, // 防止 _ 被转义为快捷键
                    Binding = new Binding(colName)
                });
            }

            sbiInfo.Content = "文件记录数：" + quotesItems.Count;
            btnFile.IsEnabled = true;
        }

        private bool ParseFile(string filename)
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
                    sbError.Content = string.Format("未识别的文件结构！{0} 位置{1}", header, n);
                    return false;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadFile(tbFile.Text);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == false)
            {
                return;
            }

            tbFile.Text = openFileDialog.FileName;
            LoadFile(tbFile.Text);
        }

        private void TbFilterText_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            filterText = tbFilterText.Text;
            System.ComponentModel.ICollectionView cvs = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
            if (cvs != null && cvs.CanFilter)
            {
                cvs.Filter = OnFilterApplied;
            }
        }

        private bool OnFilterApplied(object obj)
        {
            BlockDefine bdefine = fdefine.blockDefines[0];
            if (obj is ExpandoObject)
            {
                IDictionary<string, object> item = (IDictionary<string, object>)obj;
                foreach (FileCol c in bdefine.file_cols)
                {
                    if (c.col_canfilter && item[c.col_name].ToString().Contains(filterText))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (e.AddedCells == null || e.AddedCells.Count == 0)
            {
                return;
            }

            BlockDefine bdefine = fdefine.blockDefines[0];
            string header = e.AddedCells[0].Column.Header.ToString();
            FileCol col = bdefine.file_cols.Find(col => { return col.col_name == header; });

            rbInfo.Document.Blocks.Clear();
            if (col.col_comments != null)
            {
                rbInfo.AppendText(col.col_comments);
            }
        }

        private void Grid_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            var fileName = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            tbFile.Text = fileName;
            LoadFile(tbFile.Text);
        }
    }
}
