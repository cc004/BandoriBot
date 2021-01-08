# BandoriBot

A plugin based on Mirai-Csharp and mirai-http-api.

This plugin has a basic message handling framework and permission control. Some functions attached are listed below:

- Bandori station (powered by bandoristation.com)
- Bandori gacha simulation (powered by bestdori.com)
- Priconne clanbattle management
- Priconne period rank query and client api call
- Priconne Schedule query
- Priconne arena query (powered by www.pcrdfans.com)
- Terraria Server Management
- Custom Replies with regex or csharp code
- Random Pixiv Images (powered by lolicon.app, pixiv.cat and api.acgmx.com)
- Anti-revoke
- Sekai event rank query (realtime)
- Sekai station (memory based database)

## Usage

The plugin is not designed to be one-click-startup. You need to ~~enter the authkey at startup~~ create a text file named `authkey.txt` containing the authkey and the http endpoint is hard coded as `127.0.0.1:8080`. Meanwhile, You need to pass the qq id to the program through commandline, for example:

`botclient\bandoribot.exe 2025551588`

To send a picture, you need to make a directory named `imagecache`.

Some functions such as Gacha and Arena query need static resources, You need to download it from release and extract it into the folder.

please use mirai-http-api v1.9.x