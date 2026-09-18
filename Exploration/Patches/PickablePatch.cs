using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace Exploration.Patches;

[HarmonyPatch(typeof(Pickable), "Awake")]
public class PickableAwakePatch
{
    [HarmonyPostfix]
    private static void Awake_Postfix(Pickable __instance)
    {
        if (!__instance || !__instance.m_itemPrefab) return;
        ItemDrop component = __instance.m_itemPrefab.GetComponent<ItemDrop>();
        if (!component) return;

        string name = __instance.m_itemPrefab.name;
        string localName = Localization.instance != null
            ? Localization.instance.Localize(component.m_itemData.m_shared.m_name)
            : component.m_itemData.m_shared.m_name;

        AddTierTracker(__instance, name, localName);
    }

    private static void AddTierTracker(Pickable pickable, string name, string localName)
    {
        AddTracker(pickable, name, localName, Functions.Helper.ResourceIconsT1, Functions.Helper.GetTier1PinIcons, ExplorationPlugin.MeadowsResourcesT1Level.Value);
        AddTracker(pickable, name, localName, Functions.Helper.ResourceIconsT2, Functions.Helper.GetTier2PinIcons, ExplorationPlugin.BlackForestResourcesT2Level.Value);
        AddTracker(pickable, name, localName, Functions.Helper.ResourceIconsT3, Functions.Helper.GetTier3PinIcons, ExplorationPlugin.SwampResourcesT3Level.Value);
        AddTracker(pickable, name, localName, Functions.Helper.ResourceIconsT4, Functions.Helper.GetTier4PinIcons, ExplorationPlugin.MountainResourcesT4Level.Value);
        AddTracker(pickable, name, localName, Functions.Helper.ResourceIconsT5, Functions.Helper.GetTier5PinIcons, ExplorationPlugin.PlainsResourcesT5Level.Value);
        AddTracker(pickable, name, localName, Functions.Helper.ResourceIconsT6, Functions.Helper.GetTier6PinIcons, ExplorationPlugin.MistlandsResourcesT6Level.Value);
        AddTracker(pickable, name, localName, Functions.Helper.ResourceIconsT7, Functions.Helper.GetTier7PinIcons, ExplorationPlugin.AshlandsResourcesT7Level.Value);
    }

    private static void AddTracker(Pickable pickable, string name, string localName, IDictionary<string, Sprite> iconDict, System.Action<string> fetchIcons, int level)
    {
        fetchIcons(name);
        if (iconDict.TryGetValue(name, out Sprite? value) && !pickable.GetComponents<Tracker>().Any(t => t.name == localName))
        {
            pickable.gameObject.AddComponent<Tracker>().Create(localName, ExplorationPlugin.DetectionRadius.Value, value, level);
        }
    }
}

[HarmonyPatch(typeof(Pickable), "SetPicked")]
public class PickableSetPickedPatch
{
    [HarmonyPostfix]
    private static void SetPicked_Postfix(Pickable __instance, ref bool picked)
    {
        if (__instance || !picked) return;
        Tracker? component = __instance.gameObject.GetComponent<Tracker>();
        if (component) Object.Destroy(component);
    }
}
