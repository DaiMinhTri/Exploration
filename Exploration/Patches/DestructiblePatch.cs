using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace Exploration.Patches;

[HarmonyPatch(typeof(Destructible), "Awake")]
public class DestructiblePatch
{
    [HarmonyPostfix]
    private static void Awake_Postfix(Destructible __instance)
    {
        if (!__instance) return;

        DropOnDestroyed? component = __instance.GetComponent<DropOnDestroyed>();
        MineRock5? val = null;
        if (__instance.m_spawnWhenDestroyed) val = __instance.m_spawnWhenDestroyed.GetComponent<MineRock5>();

        if (((!component || (object)component.m_dropWhenDestroyed == null) && !val) || __instance.GetComponent<Tracker>()) return;

        IEnumerable<DropTable.DropData> drops = Enumerable.Empty<DropTable.DropData>();
        if (component?.m_dropWhenDestroyed?.m_drops != null) drops = drops.Concat(component.m_dropWhenDestroyed.m_drops);
        if (val?.m_dropItems?.m_drops != null) drops = drops.Concat(val.m_dropItems.m_drops);

        foreach (DropTable.DropData item in drops)
        {
            if (!item.m_item) continue;
            ItemDrop? component2 = item.m_item.GetComponent<ItemDrop>();
            if (!component2) continue;

            string name = item.m_item.name;
            string localName = Localization.instance != null
                ? Localization.instance.Localize(component2.m_itemData.m_shared.m_name)
                : component2.m_itemData.m_shared.m_name;

            AddTierTracker(__instance, name, localName);
        }
    }

    private static void AddTierTracker(Destructible destructible, string name, string localName)
    {
        AddTracker(destructible, name, localName, Functions.Helper.ResourceIconsT2, Functions.Helper.GetTier2PinIcons, ExplorationPlugin.BlackForestResourcesT2Level.Value);
        AddTracker(destructible, name, localName, Functions.Helper.ResourceIconsT3, Functions.Helper.GetTier3PinIcons, ExplorationPlugin.SwampResourcesT3Level.Value);
        AddTracker(destructible, name, localName, Functions.Helper.ResourceIconsT4, Functions.Helper.GetTier4PinIcons, ExplorationPlugin.MountainResourcesT4Level.Value);
        AddTracker(destructible, name, localName, Functions.Helper.ResourceIconsT5, Functions.Helper.GetTier5PinIcons, ExplorationPlugin.PlainsResourcesT5Level.Value);
        AddTracker(destructible, name, localName, Functions.Helper.ResourceIconsT6, Functions.Helper.GetTier6PinIcons, ExplorationPlugin.MistlandsResourcesT6Level.Value);
        AddTracker(destructible, name, localName, Functions.Helper.ResourceIconsT7, Functions.Helper.GetTier7PinIcons, ExplorationPlugin.AshlandsResourcesT7Level.Value);
    }

    private static void AddTracker(Destructible destructible, string name, string localName, IDictionary<string, Sprite> iconDict, System.Action<string> fetchIcons, int level)
    {
        fetchIcons(name);
        if (iconDict.TryGetValue(name, out Sprite? value) && !destructible.GetComponents<Tracker>().Any(t => t.name == localName))
        {
            destructible.gameObject.AddComponent<Tracker>().Create(localName, ExplorationPlugin.DetectionRadius.Value, value, level);
        }
    }
}

[HarmonyPatch(typeof(DropOnDestroyed), "Awake")]
public class DropOnDestroyedPatch
{
    [HarmonyPostfix]
    private static void Awake_Postfix(DropOnDestroyed __instance)
    {
        if (!__instance.name.Contains("Beehive")) return;

        foreach (DropTable.DropData drop in __instance.m_dropWhenDestroyed.m_drops)
        {
            ItemDrop component = drop.m_item.GetComponent<ItemDrop>();
            string name = drop.m_item.name;
            string resourceName = Localization.instance.Localize(component.m_itemData.m_shared.m_name);

            Functions.Helper.GetTier1PinIcons(name);
            if (!Functions.Helper.ResourceIconsT1.TryGetValue(name, out Sprite? value)) break;

            __instance.gameObject.AddComponent<Tracker>().Create(resourceName, ExplorationPlugin.DetectionRadius.Value, value, ExplorationPlugin.MeadowsResourcesT1Level.Value);
        }
    }
}
