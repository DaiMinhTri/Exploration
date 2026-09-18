using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace Exploration.Patches;

[HarmonyPatch(typeof(Leviathan), "Awake")]
public class LeviathanPatch
{
    [HarmonyPostfix]
    private static void Awake_Postfix(Leviathan __instance)
    {
        if (!__instance) return;
        MineRock component = __instance.GetComponent<MineRock>();
        if (!component) return;

        IEnumerable<DropTable.DropData> drops = Enumerable.Empty<DropTable.DropData>();
        if (component?.m_dropItems?.m_drops != null) drops = drops.Concat(component.m_dropItems.m_drops);

        foreach (DropTable.DropData item in drops)
        {
            if (!item.m_item) continue;
            ItemDrop component2 = item.m_item.GetComponent<ItemDrop>();
            if (!component2) continue;

            string name = item.m_item.name;
            string localName = Localization.instance != null
                ? Localization.instance.Localize(component2.m_itemData.m_shared.m_name)
                : component2.m_itemData.m_shared.m_name;

            AddTierTracker(__instance, name, localName);
        }
    }

    private static void AddTierTracker(Leviathan leviathan, string name, string localName)
    {
        AddTracker(leviathan, name, localName, Functions.Helper.ResourceIconsT3, Functions.Helper.GetTier3PinIcons, ExplorationPlugin.SwampResourcesT3Level.Value);
        AddTracker(leviathan, name, localName, Functions.Helper.ResourceIconsT7, Functions.Helper.GetTier7PinIcons, ExplorationPlugin.AshlandsResourcesT7Level.Value);
    }

    private static void AddTracker(Leviathan leviathan, string name, string localName, IDictionary<string, Sprite> iconDict, System.Action<string> fetchIcons, int level)
    {
        fetchIcons(name);
        if (iconDict.TryGetValue(name, out Sprite? value) && !leviathan.GetComponents<Tracker>().Any(t => t.name == localName))
        {
            leviathan.gameObject.AddComponent<Tracker>().Create(localName, ExplorationPlugin.DetectionRadius.Value, value, level);
        }
    }
}
