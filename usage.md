# BandoriBot使用说明
## 说明

- 为了保证机器人的泛用性，其中一部分功能限制群白名单，群白名单可以通过`/whitelist add 群号`和`/whitelist del 群号`指令进行开启/关闭（需要管理员/群主）
- 拉群自动同意似乎有微妙的bug，所以拉群可以联系我（qq1176321897），登录机器人申请加群。

## 指令
- ycm: 查询两分钟内所有车牌（是bandori还是sekai取决于群车牌类型）
- 车牌号 + 描述: 上传车牌，如 `12345 15w melt` ，本机器人同时支持bandori和sekai的车牌，私聊机器人默认为sekai的车牌，新进的群默认是bandori的车牌，如果想更改群车牌类型，可以使用指令`/cartype`（需要管理员/群主），`/cartype 12345 Bandori`可以把群12345的车牌类型改为bandori，同样`/cartype 12345 Sekai`可以把群12345类型改成sekai
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
    - `/reply add数字/del数字 规则 回复的内容`数字为2的时候配置的是机器人被@时的回复，数字为3代表没有被@时的回复，收到的内容和回复的内容支持正则表达式替换, add代表增加某个回复，del代表删除某个回复，如
        - `/reply add3 苹果 鸭梨` 机器人会在收到苹果的时候回复鸭梨
        - `/reply add3 .*爬 $0` 机器人会在收到爬的时候进行复读
        - `/reply add2 苹果 鸭梨` 机器人会在收到`@机器人 苹果`的时候回复鸭梨
    - `/reply list数字 内容` 列出某个指定规则的所有回复
    - `/reply search数字 内容` 列出该内容可能匹配的所有规则
- sekai预测线(使用[sekai viewer](https://sekai.best)的api)
    - `predsekai` 查询所有预测线
    - `predsekai排名` 查询特定排名的预测线，只支持分档排名，如`predsekai1000`，注意没有空格。
## 功能

- 复读/打断，需开启群白名单
- 关键词回复（有很多，可能会很烦人/diss管理，慎用），需开启群白名单
- 防撤回，开启关闭`/antirevoke add 群号`和`/antirevoke del 群号`，不需要开启群白名单
- 车牌号订阅（测试），收到消息后立即会在指定群/好友进行推送，`/subscribe 车牌类型`可以设置具体类型/开关，群内消息设置的为群内订阅（需要管理员/群主），私聊的为好友订阅（必须加好友才能发出去消息），支持的车牌类型如下：
    - Sekai: sekai车牌
    - Bandori: bandori车牌
    - None: 关闭订阅
