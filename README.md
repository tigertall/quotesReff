# quotesReff
上海行情文本文件解析展示，比如cpxx02，fjyYYYYMMDD，reff04，mktdt04，OFD_C1等。

![image](https://user-images.githubusercontent.com/1582931/132092000-59582c96-e745-4dd3-bc85-07f098a2a714.png)

## 使用说明
1. 仅支持x64运行平台。依赖.net 5.0 x64运行时，从微软的官网可以下载安装。[windowsdesktop-runtime-5.0.9-win-x64](https://download.visualstudio.microsoft.com/download/pr/8bc41df1-cbb4-4da6-944f-6652378e9196/1014aacedc80bbcc030dabb168d2532f/windowsdesktop-runtime-5.0.9-win-x64.exe)

2. 打开文件的方式：
* 菜单【文件】-【打开】，选择文件路径解析；
* 复制路径后到文件路径文本栏，点击解析按钮解析。
* 直接拖拽文件到展示窗格中。

3. 目前支持的文件，其他有需要的也可以自己配置或者联系我配置 
* cpxx02-产品信息第二版
* fjyYYYYMMDD-非交易信息
* reff04-港股基础信息
* mktdt04-沪港通实时行情
* OFD_C1-基金基础参数

4. 左下角会展示解析出来的文件记录数有多少，不统计配置为丢弃的block内容。对于多类型数据的文件，选择对应标签页的时候，还会显示这个类型的数据有多少条。

5. 如果展示的注释内容较多，蓝色的条条分割线可以上下拖动。

## FileDefine.xml配置
文件结构的定义说明，只能打开这个里面定义了的文件。

每个文件定义是一个file节点，由于类似mktdt04里有多个内容，因此每个内容块体现在file节点下的block中，每一块内容结构的行列字段定义在fields中。

### file节点
一个文件数据结构定义
* type：一个描述性的文件分类描述，必须存在，可以空值
* description: 文件的中文描述，必须存在，可以空值
* pattern: 一个正则表达式，会用来根据要打开的文件名（去掉路径前缀后的文件名部分），来匹配是哪个文件类型，必须存在，不可为空值
* parser: 使用的解析方式，需要修改程序才能支持。必须存在，不可空值。目前就 SHCPXX 和 OFD 这两类。如果有新的文件结构，需要提需求增加。

### block节点
一类数据类型的定义，比如mktdt04的MD401，MD404等，就是属于一个block定义。
* header: 用来识别一个数据类型的字符串头，用来做数据开始的匹配，同一个文件的所有block中不能重复。如果只有一个block，可以不配置；对于有多个block的文件，那么需要设置。
* name: 这个段的内容中文描述，可以不存在。
* frozencolumncount: 冻结列头的列数，类似excel效果，配置几就是冻结前几列。可以配置为0，就不会冻结列了。可以不存在。
* drop: 解析后是否丢弃这个内容。比如HEADER和TRAILER，不需要展示就配置为丢弃。

### fields 节点
field描述字段结构定义：
* no: 字段编号，不可为空。
* name: 字段名，展示时候的列头，不可为空。
* name_eng：字段英文名，目前OFD_C1类型文件用来过滤非必填字段使用，其他类型文件不需配置。
* type: 字段长度是最重要的，要按照长度解析文件结构。
* filter: "1" 表示针对该字段启用过滤，可以配置多个字段，会使用这些字段查找。当文件有过滤字段时，过滤的文本框会启用，输入查找内容后回车进行过滤。
* hidden: "1" 配置表示该字段不展示，cpxx和fjy有些意义不重要的字段，配置上可以减少列宽。
* encoding: 现在只有mktdt04才有用，配置为UTF-16LE。可以不配置，不配置默认按照GBK解析。

comments描述定义，选中单元格时，会在界面下的文本框中展示注释信息。不需要每个字段都存在，可以是field 字段的子集，需要提供注释的列配置上就可以
* no: 和 field中定义的一样。
* comments: 用来做一些备注说明，字典或者交易所行情文件中的说明什么的。

## 参考文件
行情文件版本：
* [IS101_上海证券交易所竞价撮合平台市场参与者接口规格说明书1.53版_20210607](http://www.sse.com.cn/services/tradingservice/tradingtech/technical/data/c/IS101_PartTradInterface_CV1.53_20210607.pdf)
* [IS117_上海证券交易所港股通市场参与者接口规格说明书(港股交易)1.06版_20200915](http://www.sse.com.cn/services/tradingservice/tradingtech/technical/data/c/IS117_SSE_HKSE_ITPInterface_CV106_20200915.pdf)
* [中央数据交换平台开放式基金业务数据交换协议_20180205](http://www.chinaclear.cn/zdjs/sjjhtz/201802/eb15bd4820af483ea111d284a55ed1b0.shtml)

## BUG反馈
Mail: `echo "dGlnZXJ0YWxsQDEyNi5jb20K" |base64 -d`

QQ  : `echo "MzQ2MzQzOTcxCg==" |base64 -d`
