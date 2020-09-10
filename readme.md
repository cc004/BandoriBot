# BandoriBot

A plugin based on Mirai-Csharp and mirai-http-api.

This plugin has a basic message handling framework and permission control. Some functions attached are listed below:

- Bandori station (powered by bandoristation.com)
- Bandori gacha simulation (powered by bestdori.com)
- Priconne clanbattle management
- Priconne period rank query and client api call
- Priconne Schedule query
- Priconne arena query (powered by www.bigfun.cn)
- Terraria Server Management
- Custom Replies with regex or csharp code
- Random Pixiv Images (powered by lolicon.app, pixiv.cat and api.imjad.cn)
- Anti-revoke

## Usage

The plugin is not designed to be on-click-startup. So the authkey is hard-coded as `1234567890` and the http endpoint is `127.0.0.1:8080`. You need to pass the qq id to the program through commandline, for example:

`botclient\bandoribot.exe 2025551588`

To send a picture, you need to make a soft link from `imagecache` to the mirai-http-api images folder. The following command might be helpful:

`mklink /j imagecache mirai\plugins\MiraiAPIHTTP\images`

Some functions such as Gacha and Arena query need static resources, You need to download it from release and extract it into the folder.

