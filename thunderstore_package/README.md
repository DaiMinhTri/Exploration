# Exploration

A Valheim skill mod that adds the **Exploration** skill. Increases movement speed, exploration radius, and pins resources/dungeons on the map as you explore.

## Features

### Skill
- **Exploration** skill (does not conflict with blacks7ar's "Explorer" skill)
- Configurable experience gain factor and death penalty
- Experience gain notification display

### Movement
- Movement speed increase (jog + run) scaled by skill level
- Exploration radius increase scaled by skill level
- Wishbone/beacon radius increase scaled by skill level

### Resource Tracking
- Automatically pins resources on the map when you enter detection range
- 7 biome tiers with configurable resource lists:
  - Meadows (Tier 1): Flint, Mushroom, Raspberry, Honey
  - Black Forest (Tier 2): Blueberries, Carrot Seeds, Thistle, Copper Ore, Tin Ore
  - Swamp (Tier 3): Turnip Seeds, Guck, Chitin
  - Mountain (Tier 4): Dragon Egg, Onion Seeds, Silver Ore
  - Plains (Tier 5): Barley, Cloudberry, Flax, Tar
  - Mistlands (Tier 6): Mushroom Jotun Puffs, Mushroom Magecap, Soft Tissue, Black Marble, Iron Scrap, Copper Scrap
  - Ashlands (Tier 7): Fiddlehead Fern, Mushroom Smoke Puff, Flametal Ore New, Proustite Powder, Sulfur Stone
- Level-gated unlock per biome tier
- Custom map icons for each resource
- Configurable detection radius
- Max pin limit per resource type
- Icons-only mode (no text labels on map)
- Hide pin hotkey (default: Ctrl+E)

### Dungeon Tracking
- Automatically pins dungeons and caves on the map
- Supported dungeons: Burial Chambers, Troll Caves, Sunken Crypts, Mountain Caves, Dvergr Towns, Morgen Holes, Ore Mines
- Custom icons for each dungeon type
- Configurable level requirement

### Exploration Exp Sources
- Map exploration (new pixels revealed)
- Lore stone interactions
- New biome discovery
- Location discovery

### Other
- Cartography table level requirements (read/write)
- Treasure chest multiplication (configurable level and chance)

## Configuration

All settings are synced with the server when server-side locking is enabled.

| Section | Setting | Default | Description |
|---------|---------|---------|-------------|
| General | Lock Configuration | On | Lock config to server admins only |
| Exploration | Exploration Radius Increase | 250 | Radius increase at skill level 100 |
| Exploration | Movement Speed Increase | 15 | Speed increase at skill level 100 |
| Exploration | Wishbone Radius Increase | 50 | Wishbone radius increase at skill level 100 |
| Exploration | Detection Radius | 64 | Detection radius for resource tracking |
| Exploration | Radius Increase Per Level | 2 | Radius increase per skill level |
| Exploration | Lore Stone Exp Gain | 3 | Exp factor for lore stone interaction |
| Exploration | Discovered Location Exp Gain | 2 | Exp factor for location discovery |
| Exploration | New Biome Exp Gain | 10 | Exp factor for new biome discovery |
| Exploration | Icons Only | Off | Show only icons on map |
| Exploration | Max Pin | 3 | Max pins of same type on map |
| Exploration | Hide Pin Hotkey | Ctrl+E | Toggle pin visibility |
| Skills Exp | Display Exp Gain | On | Show exp gain notification |
| Skills Exp | Skill Experience Gain Factor | 1.0 | Exp gain multiplier |
| Skills Exp | Skill Experience Loss | 0 | Exp lost on death |
| Resources | Tier1 Meadows Level | 10 | Skill level to track meadows resources |
| Resources | Tier2 BlackForest Level | 22 | Skill level to track black forest resources |
| Resources | Tier3 Swamp Level | 34 | Skill level to track swamp resources |
| Resources | Tier4 Mountain Level | 46 | Skill level to track mountain resources |
| Resources | Tier5 Plains Level | 58 | Skill level to track plains resources |
| Resources | Tier6 Mistlands Level | 70 | Skill level to track mistlands resources |
| Resources | Tier7 Ashlands Level | 82 | Skill level to track ashlands resources |
| Dungeons | Dungeons Level | 28 | Skill level to track dungeons |

## Installation

1. Install [BepInEx](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/)
2. Install [SkillManager](https://valheim.thunderstore.io/package/MSchmitt/SkillManager/)
3. Download and extract into `BepInEx/plugins/`

## Compatibility

- Does **not** conflict with blacks7ar's Explorer mod (different skill names)
- Compatible with Valheim version 0.217+
- Requires BepInEx 5.4+

## Credits

- Based on [blaxxun-boop's Exploration](https://github.com/blaxxun-boop/Exploration)
- Resource/dungeon tracking inspired by [blacks7ar's Explorer](https://github.com/blacks7ar/Explorer)

## Buy Me a Coffee

If you enjoy my mods, consider buying me a coffee: https://buymeacoffee.com/daiminhtri

## Changelog

### 1.1.0
- Added resource tracking by biome tier with configurable resource lists
- Added dungeon/cave tracking with custom map icons
- Added level-gated resource unlock system
- Added configurable detection radius and radius increase per level
- Added exp gain notification display
- Added hide pin hotkey (Ctrl+E)
- Added max pin limit per type
- Added icons-only mode
- Added lore stone, location discovery, and new biome exp gain
- Upgraded project to SDK-style csproj
