using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using ServerSync;
using SkillManager;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Exploration;

[BepInPlugin(ModGUID, ModName, ModVersion)]
[BepInIncompatibility("org.bepinex.plugins.valheim_plus")]
public class ExplorationPlugin : BaseUnityPlugin
{
    private const string ModName = "Exploration";
    private const string ModVersion = "1.1.0";
    private const string ModGUID = "org.bepinex.plugins.exploration";

    private static readonly ConfigSync configSync = new(ModName) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

    private static ConfigEntry<Toggle> serverConfigLocked = null!;
    private static ConfigEntry<int> explorationRadiusIncrease = null!;
    private static ConfigEntry<int> movementSpeedIncrease = null!;
    private static ConfigEntry<int> wishboneRadiusIncrease = null!;
    private static ConfigEntry<int> requiredLevelWrite = null!;
    private static ConfigEntry<int> requiredLevelRead = null!;
    private static ConfigEntry<int> treasureMultiplyLevel = null!;
    private static ConfigEntry<int> treasureMultiplyChance = null!;
    private static ConfigEntry<float> experienceGainedFactor = null!;
    private static ConfigEntry<int> experienceLoss = null!;

    // Tracker configs
    public static ConfigEntry<float> DetectionRadius = null!;
    public static ConfigEntry<int> RadiusIncreasePerLevel = null!;
    public static ConfigEntry<float> LoreStoneExp = null!;
    public static ConfigEntry<float> LocationExp = null!;
    public static ConfigEntry<float> NewBiomeExp = null!;
    public static ConfigEntry<Toggle> IconsOnly = null!;
    public static ConfigEntry<int> MaxPin = null!;
    public static ConfigEntry<KeyboardShortcut> HidePinKey = null!;
    public static bool _hidePins;
    public static ConfigEntry<Toggle> DisplayExpGain = null!;

    public static ConfigEntry<string> MeadowsResourcesT1 = null!;
    public static ConfigEntry<int> MeadowsResourcesT1Level = null!;
    public static ConfigEntry<string> BlackForestResourcesT2 = null!;
    public static ConfigEntry<int> BlackForestResourcesT2Level = null!;
    public static ConfigEntry<string> SwampResourcesT3 = null!;
    public static ConfigEntry<int> SwampResourcesT3Level = null!;
    public static ConfigEntry<string> MountainResourcesT4 = null!;
    public static ConfigEntry<int> MountainResourcesT4Level = null!;
    public static ConfigEntry<string> PlainsResourcesT5 = null!;
    public static ConfigEntry<int> PlainsResourcesT5Level = null!;
    public static ConfigEntry<string> MistlandsResourcesT6 = null!;
    public static ConfigEntry<int> MistlandsResourcesT6Level = null!;
    public static ConfigEntry<string> AshlandsResourcesT7 = null!;
    public static ConfigEntry<int> AshlandsResourcesT7Level = null!;
    public static ConfigEntry<int> DungeonsLevel = null!;

    private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
    {
        ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);
        SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
        syncedConfigEntry.SynchronizedConfig = synchronizedSetting;
        return configEntry;
    }

    private ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => config(group, name, value, new ConfigDescription(description), synchronizedSetting);

    private ConfigEntry<T> configTextBox<T>(string group, string name, T value, string desc, bool synchronizedConfig = true)
    {
        return config(group, name, value, new ConfigDescription(desc, null, new ConfigurationManagerAttributes { CustomDrawer = TextBox }));
    }

    public enum Toggle
    {
        On = 1,
        Off = 0,
    }

    private class ConfigurationManagerAttributes
    {
        [UsedImplicitly] public int? Order;
        [UsedImplicitly] public bool? ShowRangeAsPercent;
        [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer;
    }

    private static Skill exploration = null!;

    public void Awake()
    {
        Config.SaveOnConfigSet = false;

        exploration = new Skill("Exploration", "exploration.png");
        exploration.Description.English("Increases movement speed, exploration radius, and pins resources on the map.");
        exploration.Name.German("Erkundung");
        exploration.Description.German("Erhöht die Bewegungsgeschwindigkeit, den Erkundungsradius und markiert Ressourcen auf der Karte.");
        exploration.Configurable = false;

        int order = 0;

        // General
        serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On, "If on, the configuration is locked and can be changed by server admins only.");
        configSync.AddLockingConfigEntry(serverConfigLocked);

        // Exploration (existing features)
        explorationRadiusIncrease = config("2 - Exploration", "Exploration Radius Increase", 250, new ConfigDescription("Exploration radius increase at skill level 100.", new AcceptableValueRange<int>(0, 1000), new ConfigurationManagerAttributes { Order = --order }));
        movementSpeedIncrease = config("2 - Exploration", "Movement Speed Increase", 15, new ConfigDescription("Movement speed increase at skill level 100.", new AcceptableValueRange<int>(0, 30), new ConfigurationManagerAttributes { Order = --order }));
        wishboneRadiusIncrease = config("2 - Exploration", "Wishbone Radius Increase", 50, new ConfigDescription("Wishbone radius increase at skill level 100.", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = --order }));
        requiredLevelWrite = config("2 - Exploration", "Cartography Write Level", 20, new ConfigDescription("Exploration skill level required to write your map to the cartography table. Set to 0 to disable.", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = --order, ShowRangeAsPercent = false }));
        requiredLevelRead = config("2 - Exploration", "Cartography Read Level", 40, new ConfigDescription("Exploration skill level required to read the map from the cartography table. Set to 0 to disable.", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = --order, ShowRangeAsPercent = false }));
        treasureMultiplyLevel = config("2 - Exploration", "Treasure Multiplication Level", 50, new ConfigDescription("Exploration skill level required to have a chance to multiply treasure chest content. Set to 0 to disable.", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = --order, ShowRangeAsPercent = false }));
        treasureMultiplyChance = config("2 - Exploration", "Treasure Multiplication Chance", 25, new ConfigDescription("Chance to multiply the content of treasure chests.", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = --order }));

        // Tracker features
        DetectionRadius = config("2 - Exploration", "Detection Radius", 64f, new ConfigDescription("Detection radius for tracking down resources.", new AcceptableValueRange<float>(0f, 100f)));
        RadiusIncreasePerLevel = config("2 - Exploration", "Radius Increase Per Level", 2, new ConfigDescription("Exploration radius increase per level.", new AcceptableValueRange<int>(0, 10), new ConfigurationManagerAttributes { Order = --order, ShowRangeAsPercent = false }));
        LoreStoneExp = config("2 - Exploration", "Lore Stone Exp Gain", 3f, new ConfigDescription("Exp factor gain when interacting with a runestone.", new AcceptableValueRange<float>(1f, 10f)));
        LocationExp = config("2 - Exploration", "Discovered Location Exp Gain", 2f, new ConfigDescription("Exp factor gain when discovering a new location.", new AcceptableValueRange<float>(1f, 10f)));
        NewBiomeExp = config("2 - Exploration", "New Biome Exp Gain", 10f, new ConfigDescription("Exp factor gain when discovering a new biome.", new AcceptableValueRange<float>(1f, 25f)));
        IconsOnly = config("2 - Exploration", "Icons Only", Toggle.Off, new ConfigDescription("If On, only icons will be visible on the world map."));
        MaxPin = config("2 - Exploration", "Max Pin", 3, new ConfigDescription("Max number of pins of the same type on the map.", new AcceptableValueRange<int>(0, 50)));
        HidePinKey = config("2 - Exploration", "Hide Pin Hotkey", new KeyboardShortcut(KeyCode.E, KeyCode.LeftControl), new ConfigDescription("Hotkey to hide/show pins on the map."), false);
        DisplayExpGain = config("3 - Skills Exp", "Display Exp Gain", Toggle.On, new ConfigDescription("Enable/Disable exp gain notification."));

        // Resources
        MeadowsResourcesT1 = configTextBox("4 - Resources", "Tier1 Meadows", "Flint,Mushroom,Raspberry,Honey", "Prefab names of resources in meadows biome. Logout to apply changes.");
        MeadowsResourcesT1Level = config("4 - Resources", "Tier1 Meadows Level", 10, new ConfigDescription("Skill level required to track meadows resources.", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = --order, ShowRangeAsPercent = false }));
        BlackForestResourcesT2 = configTextBox("4 - Resources", "Tier2 BlackForest", "Blueberries,CarrotSeeds,Thistle,CopperOre,TinOre", "Prefab names of resources in black forest biome. Logout to apply changes.");
        BlackForestResourcesT2Level = config("4 - Resources", "Tier2 BlackForest Level", 22, new ConfigDescription("Skill level required to track black forest resources.", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = --order, ShowRangeAsPercent = false }));
        SwampResourcesT3 = configTextBox("4 - Resources", "Tier3 Swamp", "TurnipSeeds,Guck,Chitin", "Prefab names of resources in swamp biome. Logout to apply changes.");
        SwampResourcesT3Level = config("4 - Resources", "Tier3 Swamp Level", 34, new ConfigDescription("Skill level required to track swamp resources.", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = --order, ShowRangeAsPercent = false }));
        MountainResourcesT4 = configTextBox("4 - Resources", "Tier4 Mountain", "DragonEgg,OnionSeeds,SilverOre", "Prefab names of resources in mountain biome. Logout to apply changes.");
        MountainResourcesT4Level = config("4 - Resources", "Tier4 Mountain Level", 46, new ConfigDescription("Skill level required to track mountain resources.", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = --order, ShowRangeAsPercent = false }));
        PlainsResourcesT5 = configTextBox("4 - Resources", "Tier5 Plains", "Barley,Cloudberry,Flax,Tar", "Prefab names of resources in plains biome. Logout to apply changes.");
        PlainsResourcesT5Level = config("4 - Resources", "Tier5 Plains Level", 58, new ConfigDescription("Skill level required to track plains resources.", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = --order, ShowRangeAsPercent = false }));
        MistlandsResourcesT6 = configTextBox("4 - Resources", "Tier6 Mistlands", "MushroomJotunPuffs,MushroomMagecap,Softtissue,BlackMarble,IronScrap,CopperScrap", "Prefab names of resources in mistlands biome. Logout to apply changes.");
        MistlandsResourcesT6Level = config("4 - Resources", "Tier6 Mistlands Level", 70, new ConfigDescription("Skill level required to track mistlands resources.", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = --order, ShowRangeAsPercent = false }));
        AshlandsResourcesT7 = configTextBox("4 - Resources", "Tier7 Ashlands", "Fiddleheadfern,MushroomSmokePuff,FlametalOreNew,ProustitePowder,SulfurStone", "Prefab names of resources in ashlands biome. Logout to apply changes.");
        AshlandsResourcesT7Level = config("4 - Resources", "Tier7 Ashlands Level", 82, new ConfigDescription("Skill level required to track ashlands resources.", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = --order, ShowRangeAsPercent = false }));

        // Dungeons
        DungeonsLevel = config("5 - Dungeons", "Dungeons Level", 28, new ConfigDescription("Skill level required to track dungeons and caves.", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = --order, ShowRangeAsPercent = false }));

        Functions.Helper.GetDungeonSprites();

        experienceGainedFactor = config("3 - Skills Exp", "Skill Experience Gain Factor", 1f, new ConfigDescription("Factor for experience gained for the exploration skill.", new AcceptableValueRange<float>(0.01f, 5f)));
        experienceGainedFactor.SettingChanged += (_, _) => exploration.SkillGainFactor = experienceGainedFactor.Value;
        exploration.SkillGainFactor = experienceGainedFactor.Value;
        experienceLoss = config("3 - Skills Exp", "Skill Experience Loss", 0, new ConfigDescription("How much experience to lose in the exploration skill on death.", new AcceptableValueRange<int>(0, 100)));
        experienceLoss.SettingChanged += (_, _) => exploration.SkillLoss = experienceLoss.Value;
        exploration.SkillLoss = experienceLoss.Value;

        Config.SaveOnConfigSet = true;
        Config.Save();

        Assembly assembly = Assembly.GetExecutingAssembly();
        Harmony harmony = new(ModGUID);
        harmony.PatchAll(assembly);
    }

    public void Update()
    {
        Player? localPlayer = Player.m_localPlayer;
        if (!localPlayer) return;

        KeyboardShortcut key = HidePinKey.Value;
        if (key.IsDown() && localPlayer.TakeInput() && !_hidePins)
        {
            _hidePins = true;
            localPlayer.Message(MessageHud.MessageType.TopLeft, "Explorer pins are turned Off.", 0, null, false);
            return;
        }
        key = HidePinKey.Value;
        if (key.IsDown() && localPlayer.TakeInput() && _hidePins)
        {
            localPlayer.Message(MessageHud.MessageType.TopLeft, "Explorer pins are turned On.", 0, null, false);
            _hidePins = false;
        }
    }

    private void OnDestroy() => Config.Save();

    private static void TextBox(ConfigEntryBase entryBase)
    {
        entryBase.BoxedValue = GUILayout.TextArea((string)entryBase.BoxedValue, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
    }

    // Existing patches
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.Explore), typeof(Vector3), typeof(float))]
    private class IncreaseExplorationRadius
    {
        public static int exploredPixels = 0;

        [UsedImplicitly]
        private static void Prefix(Minimap __instance, ref float radius)
        {
            exploredPixels = 0;
            radius *= 1 + Player.m_localPlayer.GetSkillFactor("Exploration") * (explorationRadiusIncrease.Value / 100f);
        }

        private static void Postfix()
        {
            if (exploredPixels > 0 && Player.m_localPlayer)
            {
                Player.m_localPlayer.RaiseSkill("Exploration", 0.075f * exploredPixels);
            }
        }
    }

    [HarmonyPatch(typeof(Minimap), nameof(Minimap.Explore), typeof(int), typeof(int))]
    private static class IncreaseExplorationSkill
    {
        private static void Postfix(bool __result)
        {
            if (__result) ++IncreaseExplorationRadius.exploredPixels;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.GetJogSpeedFactor))]
    private class IncreaseJogSpeed
    {
        private static void Postfix(Player __instance, ref float __result)
        {
            __result += __instance.GetSkillFactor("Exploration") * (movementSpeedIncrease.Value / 100f);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.GetRunSpeedFactor))]
    private class IncreaseRunSpeed
    {
        private static void Postfix(Player __instance, ref float __result)
        {
            __result += __instance.GetSkillFactor("Exploration") * (movementSpeedIncrease.Value / 100f);
        }
    }

    [HarmonyPatch(typeof(MapTable), nameof(MapTable.OnWrite))]
    private static class PreventMapTableUsageWrite
    {
        private static bool Prefix() => requiredLevelWrite.Value <= 0 || !(Player.m_localPlayer.GetSkillFactor("Exploration") < requiredLevelWrite.Value / 100f);
    }

    [HarmonyPatch(typeof(MapTable), nameof(MapTable.OnRead), typeof(Switch), typeof(Humanoid), typeof(ItemDrop.ItemData), typeof(bool))]
    private static class PreventMapTableUsageRead
    {
        private static bool Prefix() => requiredLevelRead.Value <= 0 || !(Player.m_localPlayer.GetSkillFactor("Exploration") < requiredLevelRead.Value / 100f);
    }

    [HarmonyPatch(typeof(MapTable), nameof(MapTable.GetWriteHoverText))]
    private static class UpdateHoverTextWrite
    {
        private static void Postfix(MapTable __instance, ref string __result)
        {
            if (requiredLevelWrite.Value > 0 && Player.m_localPlayer.GetSkillFactor("Exploration") < requiredLevelWrite.Value / 100f)
            {
                __result = Localization.instance.Localize(__instance.m_name + $"\nRequires Exploration level {requiredLevelWrite.Value}");
            }
        }
    }

    [HarmonyPatch(typeof(MapTable), nameof(MapTable.GetReadHoverText))]
    private static class UpdateHoverTextRead
    {
        private static void Postfix(MapTable __instance, ref string __result)
        {
            if (requiredLevelRead.Value > 0 && Player.m_localPlayer.GetSkillFactor("Exploration") < requiredLevelRead.Value / 100f)
            {
                __result = Localization.instance.Localize(__instance.m_name + $"\nRequires Exploration level {requiredLevelRead.Value}");
            }
        }
    }

    [HarmonyPatch(typeof(Container), nameof(Container.RPC_OpenResponse))]
    private static class MultiplyTreasure
    {
        private static void Prefix(Container __instance, bool granted)
        {
            if (!Player.m_localPlayer || !granted || !__instance.name.StartsWith("TreasureChest_", StringComparison.Ordinal)) return;

            if (__instance.m_nview.GetZDO().GetBool("Exploration Treasure Looted")) return;
            __instance.m_nview.GetZDO().Set("Exploration Treasure Looted", true);

            if (treasureMultiplyLevel.Value / 100f >= Player.m_localPlayer.GetSkillFactor("Exploration") && Random.value < treasureMultiplyChance.Value / 100f)
            {
                Inventory inventory = __instance.GetInventory();
                foreach (ItemDrop.ItemData item in inventory.GetAllItems().ToArray())
                {
                    __instance.m_inventory.AddItem(item.m_dropPrefab, item.m_stack);
                }
            }

            Player.m_localPlayer.RaiseSkill("Exploration", 35f);
        }
    }

    [HarmonyPatch]
    private static class AlterBeaconRange
    {
        private static IEnumerable<MethodInfo> TargetMethods() => new[]
        {
            AccessTools.DeclaredMethod(typeof(SE_Finder), nameof(SE_Finder.UpdateStatusEffect)),
            AccessTools.DeclaredMethod(typeof(Beacon), nameof(Beacon.FindBeaconsInRange)),
            AccessTools.DeclaredMethod(typeof(Beacon), nameof(Beacon.FindClosestBeaconInRange)),
        };

        private static readonly FieldInfo range = AccessTools.DeclaredField(typeof(Beacon), nameof(Beacon.m_range));

        private static float ModifyBeaconRange(float range) => range * (1 + wishboneRadiusIncrease.Value / 100f * Player.m_localPlayer.GetSkillFactor("Exploration"));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (instruction.LoadsField(range))
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(AlterBeaconRange), nameof(ModifyBeaconRange)));
                }
            }
        }
    }
}
