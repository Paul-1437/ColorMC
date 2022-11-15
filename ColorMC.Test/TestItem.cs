﻿using ColorMC.Core.Game;
using ColorMC.Core.Http;
using ColorMC.Core.Http.Download;
using ColorMC.Core.Http.Downloader;
using ColorMC.Core.Http.Apis;
using ColorMC.Core.Login;
using ColorMC.Core.Objs.Game;
using ColorMC.Core.Objs.Pack;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Objs;

namespace ColorMC.Test;

public static class TestItem
{
    public static void Item1() 
    {
        VersionPath.CheckUpdate("1.12.2").Wait();
        AssetsPath.CheckUpdate("1.12.2").Wait();
    }

    public static void Item2()
    {
        var version = VersionPath.Versions;
        if (version == null)
        {
            Console.WriteLine("版本信息为空");
        }
        else
        {
            //GameDownload.Download(version.versions.First()).Wait();
            var list = GameDownload.Download(version.versions.Where(a => a.id == "1.12.2").First()).Result;
            if (list.State != DownloadState.End)
            {
                Console.WriteLine("下载列表获取失败");
                return;
            }
            DownloadManager.FillAll(list.List!);
            DownloadManager.Start();
        }
    }

    public static void Item3()
    {
        var list = PackDownload.DownloadCurseForge("H:\\ColonyVenture-1.13.zip").Result;
        if (list.State != DownloadState.End)
        {
            Console.WriteLine("下载列表获取失败");
            return;
        }
        DownloadManager.FillAll(list.List!);
        DownloadManager.Start().Wait();
    }

    public static void Item4()
    {
        var res = Get.GetFabricMeta().Result;
        if (res == null)
        {
            Console.WriteLine("Fabric信息为空");
        }
        else
        {
            var item = res.loader.First();
            var list = GameDownload.DownloadFabric("1.19.2", item.version).Result;
            if (list.State != DownloadState.End)
            {
                Console.WriteLine("下载列表获取失败");
                return;
            }
            DownloadManager.FillAll(list.List!);
            DownloadManager.Start();
        }
    }

    public static void Item5() 
    {
        using FileStream stream2 = new("E:\\code\\ColorMC\\ColorMC.Test\\bin\\Debug\\net7.0\\minecraft\\assets\\objects\\0c\\0cd209ea16b052a2f445a275380046615d20775e", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        stream2.Seek(0, SeekOrigin.Begin);
        string sha1 = Sha1.GenSha1(stream2);
    }

    public static void Item6()
    {
        var list = Get.GetCurseForge("1.16.5").Result;
        if (list == null)
        {
            Console.WriteLine("整合包信息为空");
        }
        else
        {
            var data = list.data[6];
            var list1 = PackDownload.DownloadCurseForge(data).Result;
            if (list1.State != DownloadState.End)
            {
                Console.WriteLine("下载列表获取失败");
                return;
            }
            DownloadManager.FillAll(list1.List!);
            DownloadManager.Start();
        }
    }

    public static void Item7()
    {
        var data = InstancesPath.GetGames().First();
        var list = Launch.CheckGameFile(data).Result;
        if (list == null)
        {
            Console.WriteLine("文件检查失败");
        }
        else
        {
            foreach (var item in list)
            {
                Console.WriteLine($"文件丢失:{item.Name}");
            }
        }
    }

    public static void Item8()
    {
        var data = Auth.LoginWithOAuth().Result;
        if (data == null)
        {
            Console.WriteLine("登录错误");
        }
        else
        {
            var data1 = Minecraft.GetMinecraftProfileAsync(data.Token).Result;
            Console.WriteLine(data);
            Console.WriteLine(JsonConvert.SerializeObject(data1));
        }
    }

    public static void Item9() 
    {
        var game = InstancesPath.GetGames();
        var login = new LoginObj()
        {
            UUID = "UUID",
            Token = "Token",
            AccessToken = "AccessToken",
            UserName = "Test"
        };

        var process = game[0].StartGame(login, null).Result;
        process?.WaitForExit();
    }

    public static void Item10() 
    {
        var login = new LoginObj()
        {
            UUID = "UUID",
            Token = "Token",
            AccessToken = "AccessToken",
            UserName = "Test"
        };
        var game = new GameSettingObj()
        {
            Dir = "E:\\code\\ColorMC\\ColorMC.Test\\bin\\Debug\\net7.0\\/minecraft/instances/test1",
            Name = "test1",
            Version = "1.7.2",
            Loader = Loaders.Forge,
            LoaderInfo = new()
            {
                Name = "forge",
                Version = "10.12.2.1161"
            }
        };
        Process? process;
        //process = game.StartGame(login, null).Result;
        //process?.WaitForExit();

        game.Version = "1.7.10";
        game.LoaderInfo.Version = "10.13.4.1614";
        process = game.StartGame(login, null).Result;
        process?.WaitForExit();

        //game.Version = "1.8";
        //game.LoaderInfo.Version = "11.14.4.1577";
        //process = game.StartGame(login, null).Result;
        //process?.WaitForExit();

        //game.Version = "1.8.8";
        //game.LoaderInfo.Version = "11.15.0.1655";
        //process = game.StartGame(login, null).Result;
        //process?.WaitForExit();

        //game.Version = "1.8.9";
        //game.LoaderInfo.Version = "11.15.1.2318";
        //process = game.StartGame(login, null).Result;
        //process?.WaitForExit();

        //game.Version = "1.9";
        //game.LoaderInfo.Version = "12.16.1.1938";
        //process = game.StartGame(login, null).Result;
        //process?.WaitForExit();

        //game.Version = "1.9.4";
        //game.LoaderInfo.Version = "12.17.0.2317";
        //process = game.StartGame(login, null).Result;
        //process?.WaitForExit();

        //game.Version = "1.10";
        //game.LoaderInfo.Version = "12.18.0.2000";
        //process = game.StartGame(login, null).Result;
        //process?.WaitForExit();

        //game.Version = "1.10.2";
        //game.LoaderInfo.Version = "12.18.3.2511";
        //process = game.StartGame(login, null).Result;
        //process?.WaitForExit();

        //game.Version = "1.11";
        //game.LoaderInfo.Version = "13.19.1.2199";
        //process = game.StartGame(login, null).Result;
        //process?.WaitForExit();

        game.Version = "1.11.2";
        game.LoaderInfo.Version = "13.20.1.2588";
        process = game.StartGame(login, null).Result;
        process?.WaitForExit();

        game.Version = "1.12.2";
        game.LoaderInfo.Version = "14.23.5.2860";
        process = game.StartGame(login, null).Result;
        process?.WaitForExit();

        game.Version = "1.13.2";
        game.LoaderInfo.Version = "25.0.223";
        process = game.StartGame(login, null).Result;
        process?.WaitForExit();

        //game.Version = "1.14.4";
        //game.LoaderInfo.Version = "28.2.26";
        //process = game.StartGame(login, null).Result;
        //process?.WaitForExit();

        //game.Version = "1.15.2";
        //game.LoaderInfo.Version = "31.2.57";
        //process = game.StartGame(login, null).Result;
        //process?.WaitForExit();

        //game.Version = "1.16.5";
        //game.LoaderInfo.Version = "36.2.39";
        //process = game.StartGame(login, null).Result;
        //process?.WaitForExit();

        //game.Version = "1.17.1";
        //game.LoaderInfo.Version = "37.1.1";
        //process = game.StartGame(login, null).Result;
        //process?.WaitForExit();

        //game.Version = "1.18.2";
        //game.LoaderInfo.Version = "40.1.85";
        //process = game.StartGame(login, null).Result;
        //process?.WaitForExit();

        game.Version = "1.19.2";
        game.LoaderInfo.Version = "43.1.52";
        process = game.StartGame(login, null).Result;
        process?.WaitForExit();
    }
}