using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Exploration.Functions;

public static class Helper
{
    public static readonly IDictionary<string, Sprite> ResourceIconsT1 = new Dictionary<string, Sprite>();
    public static readonly IDictionary<string, Sprite> ResourceIconsT2 = new Dictionary<string, Sprite>();
    public static readonly IDictionary<string, Sprite> ResourceIconsT3 = new Dictionary<string, Sprite>();
    public static readonly IDictionary<string, Sprite> ResourceIconsT4 = new Dictionary<string, Sprite>();
    public static readonly IDictionary<string, Sprite> ResourceIconsT5 = new Dictionary<string, Sprite>();
    public static readonly IDictionary<string, Sprite> ResourceIconsT6 = new Dictionary<string, Sprite>();
    public static readonly IDictionary<string, Sprite> ResourceIconsT7 = new Dictionary<string, Sprite>();

    private static readonly IDictionary<string, Sprite> DungeonSpritesDict = new Dictionary<string, Sprite>();
    public static readonly IDictionary<string, Sprite> DungeonPinIcons = new Dictionary<string, Sprite>();

    public static readonly HashSet<string> DungeonList = new()
    {
        "Crypt2", "Crypt3", "Crypt4", "TrollCave02", "SunkenCrypt4",
        "MountainCave02", "Mistlands_DvergrTownEntrance1", "Mistlands_DvergrTownEntrance2",
        "MorgenHole1", "MorgenHole2", "MorgenHole3",
        "BOM_FlintMine01", "BOM_FlintMine02", "BOM_CopperMine01", "BOM_CopperMine02",
        "BOM_TinMine01", "BOM_TinMine02", "BOM_CoalMine01", "BOM_CoalMine02",
        "BOM_IronMine01", "BOM_IronMine02", "BOM_SilverMine01", "BOM_SilverMine02"
    };

    public static float tFloat(this float value, int digits)
    {
        double num = Math.Pow(10.0, digits);
        return (float)(Math.Truncate(num * value) / num);
    }

    public static void GetDungeonSprites()
    {
        DungeonSpritesDict.Clear();
        foreach (string dungeon in DungeonList)
        {
            Sprite sprite = dungeon switch
            {
                "Crypt2" or "Crypt3" or "Crypt4" => LoadSprite("burial.png", 64, 64),
                "TrollCave02" => LoadSprite("troll_cave.png", 64, 64),
                "MountainCave02" => LoadSprite("mountain_cave.png", 64, 64),
                "Mistlands_DvergrTownEntrance1" => LoadSprite("mistland_dungeon.png", 64, 64),
                "Mistlands_DvergrTownEntrance2" => LoadSprite("mistland_dungeon_rock.png", 64, 64),
                "MorgenHole1" or "MorgenHole2" or "MorgenHole3" => LoadSprite("morgen_hole.png", 64, 64),
                "BOM_CoalMine01" or "BOM_CoalMine02" or "BOM_IronMine01" or "BOM_IronMine02"
                    or "BOM_FlintMine01" or "BOM_FlintMine02" or "BOM_CopperMine01" or "BOM_CopperMine02"
                    or "BOM_SilverMine01" or "BOM_SilverMine02" or "BOM_TinMine01" or "BOM_TinMine02" => LoadSprite("oremine.png", 64, 64),
                _ => null!
            };
            DungeonSpritesDict.Add(dungeon, sprite);
        }
    }

    public static void GetDungeonPinIcons(string dungeon)
    {
        if (DungeonPinIcons.ContainsKey(dungeon)) return;
        foreach (string key in DungeonSpritesDict.Keys)
        {
            if (dungeon == key)
            {
                DungeonPinIcons.Add(dungeon, DungeonSpritesDict[key]);
            }
        }
    }

    public static void GetTier1PinIcons(string resource)
    {
        if (ResourceIconsT1.ContainsKey(resource)) return;
        foreach (string key in Patches.ObjectDBPatch.ResourceSpritesDictT1.Keys)
        {
            if (resource == key) ResourceIconsT1.Add(resource, Patches.ObjectDBPatch.ResourceSpritesDictT1[key]);
        }
    }

    public static void GetTier2PinIcons(string resource)
    {
        if (ResourceIconsT2.ContainsKey(resource)) return;
        foreach (string key in Patches.ObjectDBPatch.ResourceSpritesDictT2.Keys)
        {
            if (resource == key) ResourceIconsT2.Add(resource, Patches.ObjectDBPatch.ResourceSpritesDictT2[key]);
        }
    }

    public static void GetTier3PinIcons(string resource)
    {
        if (ResourceIconsT3.ContainsKey(resource)) return;
        foreach (string key in Patches.ObjectDBPatch.ResourceSpritesDictT3.Keys)
        {
            if (resource == key) ResourceIconsT3.Add(resource, Patches.ObjectDBPatch.ResourceSpritesDictT3[key]);
        }
    }

    public static void GetTier4PinIcons(string resource)
    {
        if (ResourceIconsT4.ContainsKey(resource)) return;
        foreach (string key in Patches.ObjectDBPatch.ResourceSpritesDictT4.Keys)
        {
            if (resource == key) ResourceIconsT4.Add(resource, Patches.ObjectDBPatch.ResourceSpritesDictT4[key]);
        }
    }

    public static void GetTier5PinIcons(string resource)
    {
        if (ResourceIconsT5.ContainsKey(resource)) return;
        foreach (string key in Patches.ObjectDBPatch.ResourceSpritesDictT5.Keys)
        {
            if (resource == key) ResourceIconsT5.Add(resource, Patches.ObjectDBPatch.ResourceSpritesDictT5[key]);
        }
    }

    public static void GetTier6PinIcons(string resource)
    {
        if (ResourceIconsT6.ContainsKey(resource)) return;
        foreach (string key in Patches.ObjectDBPatch.ResourceSpritesDictT6.Keys)
        {
            if (resource == key) ResourceIconsT6.Add(resource, Patches.ObjectDBPatch.ResourceSpritesDictT6[key]);
        }
    }

    public static void GetTier7PinIcons(string resource)
    {
        if (ResourceIconsT7.ContainsKey(resource)) return;
        foreach (string key in Patches.ObjectDBPatch.ResourceSpritesDictT7.Keys)
        {
            if (resource == key) ResourceIconsT7.Add(resource, Patches.ObjectDBPatch.ResourceSpritesDictT7[key]);
        }
    }

    public static void AddCustomData(Player player, string label, string text)
    {
        if (player.m_customData.ContainsKey(label))
            player.m_customData[label] = text;
        else
            player.m_customData.Add(label, text);
    }

    public static string RandomNumber()
    {
        int n1 = UnityEngine.Random.Range(0, 9);
        int n2 = UnityEngine.Random.Range(0, 9);
        int n3 = UnityEngine.Random.Range(0, 9);
        int n4 = UnityEngine.Random.Range(0, 9);
        int n5 = UnityEngine.Random.Range(0, 9);
        return $"{n1}{n2}{n3}{n4}{n5}";
    }

    private static byte[] ReadEmbeddedFileBytes(string name)
    {
        using MemoryStream memoryStream = new MemoryStream();
        Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name + "." + name)!.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    private static MethodInfo? _loadImageMethod;

    private static MethodInfo LoadImageMethod
    {
        get
        {
            if (_loadImageMethod == null)
            {
                Type? imageConversionType = Type.GetType("UnityEngine.ImageConversion, UnityEngine.ImageConversionModule");
                _loadImageMethod = imageConversionType?.GetMethod("LoadImage", new[] { typeof(Texture2D), typeof(byte[]) });
            }
            return _loadImageMethod!;
        }
    }

    private static Texture2D LoadTexture(string name)
    {
        Texture2D tex = new Texture2D(0, 0);
        LoadImageMethod?.Invoke(null, new object[] { tex, ReadEmbeddedFileBytes("icons." + name) });
        return tex;
    }

    public static Sprite LoadSprite(string name, int width, int height)
    {
        return Sprite.Create(LoadTexture(name), new Rect(0f, 0f, width, height), Vector2.zero);
    }
}
