﻿using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System.Text;

namespace ColorMC.Core.Net.Download;

public static class PackDownload
{
    public static int Size { get; private set; }
    public static int Now { get; private set; }
    /// <summary>
    /// 下载整合包
    /// </summary>
    /// <param name="zip">压缩包路径</param>
    public static async Task<(GetDownloadState State, List<DownloadItem>? List, GameSettingObj? Game)> DownloadCurseForgeModPack(string zip, string? name, string? group)
    {
        var list = new List<DownloadItem>();

        ColorMCCore.PackState?.Invoke(CoreRunState.Init);
        using ZipFile zFile = new(zip);
        using MemoryStream stream1 = new();
        bool find = false;
        //获取主信息
        foreach (ZipEntry e in zFile)
        {
            if (e.IsFile && e.Name == "manifest.json")
            {
                using var stream = zFile.GetInputStream(e);
                await stream.CopyToAsync(stream1);
                find = true;
                break;
            }
        }

        if (!find)
        {
            return (GetDownloadState.Init, null, null);
        }
        CurseForgePackObj info;
        byte[] array1 = stream1.ToArray();
        try
        {
            var data = Encoding.UTF8.GetString(array1);
            info = JsonConvert.DeserializeObject<CurseForgePackObj>(data)!;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.GetName("Core.Pack.Error1"), e);
            return (GetDownloadState.Init, null, null);
        }
        if (info == null)
            return (GetDownloadState.Init, null, null);

        //获取版本数据
        Loaders loaders = Loaders.Normal;
        string loaderversion = "";
        foreach (var item in info.minecraft.modLoaders)
        {
            if (item.id.StartsWith("forge"))
            {
                loaders = Loaders.Forge;
                loaderversion = item.id.Replace("forge-", "");
            }
            else if (item.id.StartsWith("fabric"))
            {
                loaders = Loaders.Fabric;
                loaderversion = item.id.Replace("fabric-", "");
            }
        }
        name ??= $"{info.name}-{info.version}";

        //创建游戏实例
        var game = await InstancesPath.CreateVersion(new()
        {
            GroupName = group,
            Name = name,
            Version = info.minecraft.version,
            ModPack = true,
            Loader = loaders,
            LoaderVersion = loaderversion,
            Mods = new()
        });

        if (game == null)
        {
            return (GetDownloadState.GetInfo, null, game);
        }

        //解压文件
        foreach (ZipEntry e in zFile)
        {
            if (e.IsFile && e.Name.StartsWith(info.overrides + "/"))
            {
                using var stream = zFile.GetInputStream(e);
                string file = Path.GetFullPath(game.GetGamePath() +
                    e.Name[info.overrides.Length..]);
                FileInfo info2 = new(file);
                info2.Directory?.Create();
                using FileStream stream2 = new(file, FileMode.Create,
                    FileAccess.ReadWrite, FileShare.ReadWrite);
                await stream.CopyToAsync(stream2);
            }
        }

        File.WriteAllBytes(game.GetModJsonFile(), array1);

        ColorMCCore.PackState?.Invoke(CoreRunState.GetInfo);

        //获取Mod信息
        Size = info.files.Count;
        Now = 0;
        var res = await CurseForge.GetMods(info.files);
        if (res != null)
        {
            var res1 = res.Distinct(CurseDataComparer.Instance);
            foreach (var item in res1)
            {
                item.downloadUrl ??= $"https://edge.forgecdn.net/files/{item.id / 1000}/{item.id % 1000}/{item.fileName}";

                var item11 = new DownloadItem()
                {
                    Url = item.downloadUrl,
                    Name = item.fileName,
                    Local = game.GetGamePath() + "/mods/" + item.fileName,
                    SHA1 = item.hashes.Where(a => a.algo == 1)
                            .Select(a => a.value).FirstOrDefault()
                };

                list.Add(item11);

                game.Mods.Add(item.modId.ToString(), new()
                {
                    Path = "mods",
                    Name = item.displayName,
                    File = item.fileName,
                    SHA1 = item11.SHA1!,
                    ModId = item.modId.ToString(),
                    FileId = item.id.ToString(),
                    Url = item.downloadUrl
                });

                Now++;

                ColorMCCore.PackUpdate?.Invoke(Size, Now);
            }
        }
        else
        {
            bool done = true;
            await Parallel.ForEachAsync(info.files, async (item, token) =>
            {
                var res = await CurseForge.GetMod(item);
                if (res == null)
                {
                    done = false;
                    return;
                }

                res.data.downloadUrl ??= $"https://edge.forgecdn.net/files/{res.data.id / 1000}/{res.data.id % 1000}/{res.data.fileName}";

                var item11 = new DownloadItem()
                {
                    Url = res.data.downloadUrl,
                    Name = res.data.displayName,
                    Local = InstancesPath.GetGamePath(game) + "/mods/" + res.data.fileName,
                    SHA1 = res.data.hashes.Where(a => a.algo == 1)
                        .Select(a => a.value).FirstOrDefault()
                };

                list.Add(item11);

                game.Mods.Add(res.data.modId.ToString(), new()
                {
                    Path = "mods",
                    Name = res.data.displayName,
                    File = res.data.fileName,
                    SHA1 = item11.SHA1!,
                    ModId = res.data.modId.ToString(),
                    FileId = res.data.id.ToString(),
                    Url = res.data.downloadUrl
                });

                Now++;

                ColorMCCore.PackUpdate?.Invoke(Size, Now);
            });
            if (!done)
            {
                return (GetDownloadState.GetInfo, list, game);
            }
        }

        game.SaveCurseForgeMod();

        return (GetDownloadState.End, list, game);
    }

    public static async Task<(GetDownloadState State, List<DownloadItem>? List, GameSettingObj? Game)> DownloadModrinthModPack(string zip, string? name, string? group)
    {
        var list = new List<DownloadItem>();

        ColorMCCore.PackState?.Invoke(CoreRunState.Init);
        using ZipFile zFile = new(zip);
        using MemoryStream stream1 = new();
        bool find = false;
        //获取主信息
        foreach (ZipEntry e in zFile)
        {
            if (e.IsFile && e.Name == "modrinth.index.json")
            {
                using var stream = zFile.GetInputStream(e);
                await stream.CopyToAsync(stream1);
                find = true;
                break;
            }
        }

        if (!find)
        {
            return (GetDownloadState.Init, null, null);
        }
        ModrinthPackObj info;
        byte[] array1 = stream1.ToArray();
        try
        {
            var data = Encoding.UTF8.GetString(array1);
            info = JsonConvert.DeserializeObject<ModrinthPackObj>(data)!;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.GetName("Core.Pack.Error1"), e);
            return (GetDownloadState.Init, null, null);
        }
        if (info == null)
            return (GetDownloadState.Init, null, null);

        //获取版本数据
        Loaders loaders = Loaders.Normal;
        string loaderversion = "";
        if (!string.IsNullOrWhiteSpace(info.dependencies.forge))
        {
            loaders = Loaders.Forge;
            loaderversion = info.dependencies.forge;
        }
        else if (!string.IsNullOrWhiteSpace(info.dependencies.fabric_loader))
        {
            loaders = Loaders.Fabric;
            loaderversion = info.dependencies.fabric_loader;
        }
        else if (!string.IsNullOrWhiteSpace(info.dependencies.quilt_loader))
        {
            loaders = Loaders.Quilt;
            loaderversion = info.dependencies.quilt_loader;
        }
        name ??= $"{info.name}-{info.versionId}";

        //创建游戏实例
        var game = await InstancesPath.CreateVersion(new()
        {
            GroupName = group,
            Name = name,
            Version = info.dependencies.minecraft,
            ModPack = true,
            Loader = loaders,
            LoaderVersion = loaderversion,
            Mods = new()
        });

        if (game == null)
        {
            return (GetDownloadState.GetInfo, null, game);
        }

        int length = "overrides".Length;

        //解压文件
        foreach (ZipEntry e in zFile)
        {
            if (e.IsFile && e.Name.StartsWith("overrides/"))
            {
                using var stream = zFile.GetInputStream(e);
                string file = Path.GetFullPath(game.GetGamePath() +
                    e.Name[length..]);
                FileInfo info2 = new(file);
                info2.Directory?.Create();
                using FileStream stream2 = new(file, FileMode.Create,
                    FileAccess.ReadWrite, FileShare.ReadWrite);
                await stream.CopyToAsync(stream2);
            }
        }

        File.WriteAllBytes(game.GetModJsonFile(), array1);

        ColorMCCore.PackState?.Invoke(CoreRunState.GetInfo);

        //获取Mod信息
        Size = info.files.Count;
        Now = 0;
        foreach (var item in info.files)
        {
            string? modid, fileid;
            var url = item.downloads.FirstOrDefault(a => a.StartsWith("https://cdn.modrinth.com/data/"));
            if (url == null)
            {
                url = item.downloads[0];
                modid = item.path;
                fileid = "";
            }
            else
            {
                modid = Funtcions.GetString(url, "data/", "/ver");
                fileid = Funtcions.GetString(url, "versions/", "/");
            }

            var item11 = new DownloadItem()
            {
                Url = item.downloads[0],
                Name = item.path,
                Local = game.GetGamePath() + "/" + item.path,
                SHA1 = item.hashes.sha1
            };

            list.Add(item11);

            game.Mods.Add(modid, new()
            {
                Path = item.path[..item.path.IndexOf('/')],
                Name = item.path,
                File = item.path,
                SHA1 = item11.SHA1!,
                ModId = modid,
                FileId = fileid,
                Url = url
            });

            Now++;

            ColorMCCore.PackUpdate?.Invoke(Size, Now);
        }

        game.SaveCurseForgeMod();

        return (GetDownloadState.End, list, game);
    }
}
