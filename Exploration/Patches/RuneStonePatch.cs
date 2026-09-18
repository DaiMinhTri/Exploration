using HarmonyLib;
using SkillManager;

namespace Exploration.Patches;

[HarmonyPatch(typeof(RuneStone), "Interact")]
public class RuneStonePatch
{
    [HarmonyPrefix]
    private static void Interact_Prefix(RuneStone __instance)
    {
        if (!Player.m_localPlayer) return;
        RuneStone.RandomRuneText randomText = __instance.GetRandomText();
        string label = __instance.gameObject.name + "_BSE" + Functions.Helper.RandomNumber();
        if (randomText != null && !Player.m_localPlayer.m_customData.ContainsValue(randomText.m_text))
        {
            Functions.Helper.AddCustomData(Player.m_localPlayer, label, randomText.m_text);
            Player.m_localPlayer.RaiseSkill("Exploration", ExplorationPlugin.LoreStoneExp.Value);
        }
    }
}
