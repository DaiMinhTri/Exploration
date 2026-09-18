using System.Collections.Generic;
using HarmonyLib;
using SkillManager;
using UnityEngine;

namespace Exploration.Patches;

[HarmonyPatch(typeof(Player), "Awake")]
public class PlayerAwakePatch
{
    [HarmonyPostfix]
    private static void Awake_Postfix(Player __instance)
    {
        __instance.m_nview.Register<int>("Exploration IncreaseSkill", (long _, int factor) =>
        {
            __instance.RaiseSkill("Exploration", factor);
        });
    }
}

[HarmonyPatch(typeof(Player), "Update")]
public class PlayerUpdatePatch
{
    [HarmonyPostfix]
    private static void Update_Postfix(Player __instance)
    {
        if (__instance != Player.m_localPlayer) return;
        __instance.m_nview.GetZDO().Set("Exploration Skill Factor", __instance.GetSkillFactor("Exploration"));
    }
}

[HarmonyPatch(typeof(Player), "AddKnownText")]
public class PlayerAddKnownTextPatch
{
    [HarmonyPrefix]
    private static void AddKnownText_Prefix(Player __instance, ref string label)
    {
        if (__instance != Player.m_localPlayer || label.Length == 0 || !label.StartsWith("$lore_") || __instance.m_knownTexts.ContainsKey(label)) return;
        __instance.RaiseSkill("Exploration", ExplorationPlugin.LoreStoneExp.Value);
    }
}

[HarmonyPatch(typeof(Player), "AddKnownBiome")]
public class PlayerAddKnownBiomePatch
{
    private static readonly HashSet<string> knownBiomes = new();

    [HarmonyPostfix]
    private static void AddKnownBiome_Postfix(Player __instance)
    {
        if (__instance != Player.m_localPlayer) return;
        if (knownBiomes.Count < __instance.m_knownBiome.Count)
        {
            knownBiomes.Clear();
            foreach (string biome in __instance.m_knownBiome) knownBiomes.Add(biome);
            __instance.RaiseSkill("Exploration", ExplorationPlugin.NewBiomeExp.Value);
        }
    }
}

[HarmonyPatch(typeof(Player), "FixedUpdate")]
public class PlayerFixedUpdatePatch
{
    [HarmonyPostfix]
    private static void FixedUpdate_Postfix(Player __instance)
    {
        CheckKnownLocation(__instance);
    }

    private static void CheckKnownLocation(Player player)
    {
        if (player != Player.m_localPlayer || player.InIntro() || player.InCutscene() || !player) return;

        Location location = Location.GetLocation(player.transform.position, false);
        if (!location) return;

        string label = location.gameObject.name.Replace("(Clone)", "") + "_BSE" + Functions.Helper.RandomNumber();
        if (Location.IsInsideLocation(player.transform.position, 2f))
        {
            if (!player.m_customData.ContainsValue(location.transform.position.ToString()))
            {
                Functions.Helper.AddCustomData(player, label, location.transform.position.ToString());
                player.RaiseSkill("Exploration", ExplorationPlugin.LocationExp.Value);
                player.Message(MessageHud.MessageType.TopLeft, "Discovered a new location!", 0, null, false);
            }
        }
    }
}
