using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Exploration.Patches;

[HarmonyPatch(typeof(ObjectDB), "UpdateRegisters")]
public class ObjectDBPatch
{
    public static readonly IDictionary<string, Sprite> ResourceSpritesDictT1 = new Dictionary<string, Sprite>();
    public static readonly IDictionary<string, Sprite> ResourceSpritesDictT2 = new Dictionary<string, Sprite>();
    public static readonly IDictionary<string, Sprite> ResourceSpritesDictT3 = new Dictionary<string, Sprite>();
    public static readonly IDictionary<string, Sprite> ResourceSpritesDictT4 = new Dictionary<string, Sprite>();
    public static readonly IDictionary<string, Sprite> ResourceSpritesDictT5 = new Dictionary<string, Sprite>();
    public static readonly IDictionary<string, Sprite> ResourceSpritesDictT6 = new Dictionary<string, Sprite>();
    public static readonly IDictionary<string, Sprite> ResourceSpritesDictT7 = new Dictionary<string, Sprite>();

    [HarmonyPostfix]
    private static void UpdateRegisters_Postfix(ObjectDB __instance)
    {
        GetResourceSprites(__instance, ExplorationPlugin.MeadowsResourcesT1, ResourceSpritesDictT1);
        GetResourceSprites(__instance, ExplorationPlugin.BlackForestResourcesT2, ResourceSpritesDictT2);
        GetResourceSprites(__instance, ExplorationPlugin.SwampResourcesT3, ResourceSpritesDictT3);
        GetResourceSprites(__instance, ExplorationPlugin.MountainResourcesT4, ResourceSpritesDictT4);
        GetResourceSprites(__instance, ExplorationPlugin.PlainsResourcesT5, ResourceSpritesDictT5);
        GetResourceSprites(__instance, ExplorationPlugin.MistlandsResourcesT6, ResourceSpritesDictT6);
        GetResourceSprites(__instance, ExplorationPlugin.AshlandsResourcesT7, ResourceSpritesDictT7);
    }

    private static void GetResourceSprites(ObjectDB objectDB, BepInEx.Configuration.ConfigEntry<string> configEntry, IDictionary<string, Sprite> dict)
    {
        dict.Clear();
        foreach (string item in new HashSet<string>(configEntry.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)))
        {
            GameObject itemPrefab = objectDB.GetItemPrefab(item.GetStableHashCode());
            if (!itemPrefab) continue;
            ItemDrop component = itemPrefab.GetComponent<ItemDrop>();
            if (component) dict.Add(item, component.m_itemData.GetIcon());
        }
    }
}
