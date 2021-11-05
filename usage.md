# BandoriBot使用说明
## 说明

- 为了保证机器人的泛用性，其中一部分功能限制群白名单，群白名单可以通过`/whitelist add 群号`和`/whitelist del 群号`指令进行开启/关闭（需要管理员/群主）
- 拉群自动同意似乎有微妙的bug，所以拉群可以联系我，登录机器人申请加群。
- 目前实现了多qq的负载均衡，即使两个机器人同时拉入一个群也只会有一个处理事件，但是考虑到群太多会导致风控问题，尽量不要拉两个。。。
- 
## 指令
- pjsk抽卡：pjsk抽卡“模拟”，为啥加引号试试就知道了
- ycm: 查询两分钟内所有车牌（是bandori还是sekai取决于群车牌类型）
- 车牌号 + 描述: 上传车牌，如 `12345 15w melt` ，本机器人同时支持bandori和sekai的车牌，私聊机器人默认为sekai的车牌，新进的群默认是bandori的车牌，如果想更改群车牌类型，可以使用指令`/cartype`（需要管理员/群主），`/cartype 群号 Bandori`可以把群车牌类型改为bandori，同样`/cartype 群号 Sekai`可以把群12345类型改成sekai，区分大小写！
- 来点颜色: 随机涩图，图库:lolicon.app，此功能可以通过`/normal add 群号`和`/normal del 群号`指令进行开启/关闭（需要管理员/群主）
- 来点颜色 + 关键字，如 `来点颜色 可可萝` 返回pixiv内关键字相关的随机图片，此功能可以通过`/normal add 群号`和`/normal del 群号`指令进行开启/关闭（需要管理员/群主）
- sekai + 排名/uid，如 `sekai 1000` 查询sekai活动排行榜某个名次/玩家的排行
- 日程：返回pcr b服两周内的日程
- 查排名 + 排名，如 `查排名2000` 查询pcr目前公会战该排名工会的进度+分数
- 抽卡模拟/抽卡模拟+卡池id，模拟邦邦十连（卡池id可以使用指令 `抽卡列表` 获得）
- 抽卡列表/抽卡列表+页码，返回抽卡卡池的列表（包括必3必4和fes）
- /query: 查询群内成员发言情况，该功能只在开启群白名单情况下可用。
- 怎么拆+防守队伍：查询pcr的jjc解法，支持别名，如 `怎么拆 狼 狗 深月 xcw 黑骑`
- 部分功能关闭：可以通过指令`/blacklist add 群号.功能名`和`/blacklist del 群号.功能名`进行设置（需要管理员/群主），如`/blacklist add 12345.SekaiCommand`关闭群内查排名的功能，以下是一些常用的功能名：
    - GachaCommand: 抽卡模拟
    - QueryCommand: /query
    - RepeatHandler: 复读
    - ReplyHandler: 关键词回复
    - ZMCCommand: 怎么拆
    - SetuCommand: 涩图
    - MessageStatistic: 发言统计
    - CarHandler: 发车（包括sekai和bandori）
- 关键词回复的配置  
    **关键词回复全群共享，请不要加一些奇怪言论**
    - `/reply add数字/del数字 规则 回复的内容`数字为2的时候配置的是机器人被@时的回复，数字为3代表没有被@时的回复，收到的内容和回复的内容支持正则表达式替换, add代表增加某个回复，del代表删除某个回复，如
        - `/reply add3 苹果 鸭梨` 机器人会在收到苹果的时候回复鸭梨
        - `/reply add3 .*爬 $0` 机器人会在收到爬的时候进行复读
        - `/reply add2 苹果 鸭梨` 机器人会在收到`@机器人 苹果`的时候回复鸭梨
    - `/reply list数字 内容` 列出某个指定规则的所有回复
    - `/reply search数字 内容` 列出该内容可能匹配的所有规则
- sekai预测线(使用[sekai viewer](https://sekai.best)的api)
    - `predsekai` 查询所有预测线
    - `predsekai排名` 查询特定排名的预测线，只支持分档排名，如`predsekai1000`，注意没有空格。
- 给成员头衔（必须群主才可以）： `/title 头衔`
## 功能

- 复读/打断，需开启群白名单
- 关键词回复（有很多，可能会很烦人/diss管理，慎用），需开启群白名单
- 防撤回，开启关闭`/antirevoke add 群号`和`/antirevoke del 群号`，不需要开启群白名单
- 车牌号订阅（测试），收到消息后立即会在指定群/好友进行推送，`/subscribe 车牌类型`可以设置具体类型/开关，群内消息设置的为群内订阅（需要管理员/群主），私聊的为好友订阅（必须加好友才能发出去消息），支持的车牌类型如下：
    - Sekai: sekai车牌
    - Bandori: bandori车牌
    - None: 关闭订阅

## 权限系统

增加了权限系统后，可以对群权限进行细分，权限一般为`群号.权限名`或者`*.权限名`的形式，如果具有`*`的权限，则可以使用当前群号下所有权限，群管理和群主有该群的`*`权限，通过`/perm add/del qq号 群号.权限名`可以给普通成员某些权限，以下为一些常用权限名：
- `management.antirevoke`: 开启/关闭群的反撤回
- `management.blacklist`: 开启/关闭群内功能的黑名单
- `management.cartype`: 更改群内车牌类型
- `management.subscribe`: 更改群内订阅类型
- `management.perm`: 更改其他人群内权限（不能用来删除管理员的权限）
- `management.whitelist`: 开启/关闭群白名单
- `ignore.cooldown`: 忽略冷却指令的冷却
- `pic.normal`: 开启/关闭涩图

## 网页API

网页API统一使用GET方法，根url为`http://150.138.72.83:4/`，参数uid填执行者的qq，token填qq对应的token，token需要执行者使用指令`/token <token>`自行设置（具体值可以自己选），token错误或者无api权限返回400，目前公开的api（权限找我申请）：
- GET /execute 用于模拟私聊，返回执行结果 参数：message为消息内容，需求`*.rest.execute`权限 如`http://150.138.72.83:4/execute?uid=114514&token=1919810&message=ycm`
