using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Exploration.Patches;

[HarmonyPatch(typeof(Location), "Awake")]
public class LocationPatch
{
    private static readonly IDictionary<string, string> LocalizationDict = new Dictionary<string, string>();

    [HarmonyPostfix]
    private static void Awake_Postfix(Location __instance)
    {
        string objName = gameObject(__instance).name.Replace("(Clone)", "");
        if (!Functions.Helper.DungeonList.Contains(objName)) return;

        GetLocalizedName();

        if (objName == "SunkenCrypt4")
        {
            Sprite pinIcon = Functions.Helper.LoadSprite("crypt.png", 64, 64);
            __instance.gameObject.AddComponent<Tracker>().Create(Localized("SunkenCrypt4"), ExplorationPlugin.DetectionRadius.Value, pinIcon, ExplorationPlugin.DungeonsLevel.Value);
            return;
        }

        Functions.Helper.GetDungeonPinIcons(objName);
        if (Functions.Helper.DungeonPinIcons.TryGetValue(objName, out Sprite? value))
        {
            __instance.gameObject.AddComponent<Tracker>().Create(Localized(objName), ExplorationPlugin.DetectionRadius.Value, value, ExplorationPlugin.DungeonsLevel.Value);
        }
    }

    private static GameObject gameObject(Location loc) => loc.gameObject;

    private static void GetLocalizedName()
    {
        LocalizationDict.Clear();
        foreach (string dungeon in Functions.Helper.DungeonList)
        {
            string text = dungeon switch
            {
                "Crypt2" or "Crypt3" or "Crypt4" => Localization.instance.Localize("$location_forestcrypt"),
                "TrollCave02" => Localization.instance.Localize("$location_forestcave"),
                "SunkenCrypt4" => Localization.instance.Localize("$location_sunkencrypt"),
                "MountainCave02" => Localization.instance.Localize("$location_mountaincave"),
                "Mistlands_DvergrTownEntrance1" or "Mistlands_DvergrTownEntrance2" => Localization.instance.Localize("$location_dvergrtown"),
                "MorgenHole1" or "MorgenHole2" or "MorgenHole3" => Localization.instance.Localize("$location_morgenhole"),
                _ => ""
            };
            LocalizationDict.Add(dungeon, text);
        }
    }

    private static string Localized(string name)
    {
        string result = "";
        foreach (string key in LocalizationDict.Keys)
        {
            if (name == key) result = LocalizationDict[key];
        }
        return result;
    }
}
