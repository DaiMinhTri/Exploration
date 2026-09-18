using Exploration.Functions;
using HarmonyLib;

namespace Exploration.Patches;

[HarmonyPatch(typeof(Skills), nameof(Skills.RaiseSkill))]
public class SkillsPatch
{
    private static float lastAccumulator;

    private static void Prefix(Skills __instance)
    {
        if (__instance.m_player != Player.m_localPlayer) return;
        Skills.SkillType skillType = (Skills.SkillType)"Exploration".GetStableHashCode();
        if (__instance.m_skillData.TryGetValue(skillType, out Skills.Skill value))
        {
            lastAccumulator = value.m_accumulator;
        }
    }

    private static void Postfix(Skills __instance)
    {
        if (ExplorationPlugin.DisplayExpGain.Value == ExplorationPlugin.Toggle.Off) return;
        if (__instance.m_player != Player.m_localPlayer) return;

        Skills.SkillType skillType = (Skills.SkillType)"Exploration".GetStableHashCode();
        if (!__instance.m_skillData.TryGetValue(skillType, out Skills.Skill value)) return;

        try
        {
            if (value.m_level < 100f && value.m_accumulator != lastAccumulator)
            {
                float progress = value.m_accumulator / (value.GetNextLevelRequirement() / 100f);
                __instance.m_player.Message(MessageHud.MessageType.TopLeft,
                    $"Level {value.m_level.tFloat(0)} Exploration [{value.m_accumulator.tFloat(2)}/{value.GetNextLevelRequirement().tFloat(2)}] ({progress.tFloat(0)}%)",
                    0, value.m_info.m_icon, false);
            }
        }
        catch { }
    }
}
