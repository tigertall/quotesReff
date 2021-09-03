# quotesReff
上海行情文本文件解析展示，比如cpxx，fjyYYYYMMDD，reff。

## 使用说明

1.仅支持x64运行平台。依赖.net 5.0 x64运行时，如下地址下载安装。
https://download.visualstudio.microsoft.com/download/pr/8bc41df1-cbb4-4da6-944f-6652378e9196/1014aacedc80bbcc030dabb168d2532f/windowsdesktop-runtime-5.0.9-win-x64.exe

2.目前仅支持 cpxx02 和 fjyYYYYMMDD文件解析。

3.菜单中文件打开，选择文件解析；或者复制路径后点击解析按钮解析。

4.参考FileDefine.xml配置，
frozencolumncount 冻结列头，类似excel效果，配置几就是冻结前几列。可以配置为0，就不会冻结列了。
filter="1" 表示针对该字段启用过滤，可以配置多个字段，会使用这些字段查找。当文件有过滤字段时，过滤的文本框会启用，输入查找内容后回车进行过滤。
hidden="1" 配置表示该字段不展示，cpxx和fjy有些意义不重要的字段，配置上可以减少列宽。
