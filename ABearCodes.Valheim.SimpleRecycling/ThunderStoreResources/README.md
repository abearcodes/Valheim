
# Simple Recycler

![new ui](https://i.imgur.com/9ZyxBN8.png)

Adds a tab to the crafting station that recycles (uncrafts) items from player's inventory. 

### Key points

- the aim of the mod is to feel as much native to Valheim as possible. In many cases this will mean that the default settings in the mod are less softcore, but configurable to accomodate for all types of experiences
- accounts for resources invested into upgraded items
- returns 50% of the resources by default (configurable)
- only shows items that have a recipe for crafting them, i.e. won't show wood.  
- you need to know the recipe for the item to "uncraft" it (will be configurable)
- requires the right crafting station type and level to recycle an item (configurable)
    - for example, a Silver Sword upgraded to Level 4 will require a Forge of Level 4 to recycle
- if the mod is unable to recycle something, it reports as to why in the "description" text of the selected item


## Manual installation

Drop the `.dll` into `<GameLocation>\BepInEx\plugins`

## Encountered an issue?

Please create an issue in the GitHub repo here:

https://github.com/abearcodes/Valheim/issues/new

## Development plans

- introduce a separate buildable "station" to do the uncrafting

## Configurability

```
## Settings file was created by plugin SimpleRecycling v0.0.11
## Plugin GUID: com.github.abearcodes.valheim.simplerecycling

[General]

## Rate at which the resources are recycled. Value must be between 0 and 1.
## The mod always rolls *down*, so if you were supposed to get 2.5 items, you would only receive 2.
# Setting type: Single
# Default value: 0.5
RecyclingRate = 0.5

## If enabled and recycling a specific _unstackable_ item would yield 0 of a material,
## instead you will receive 1. If disabled, you get nothing.
# Setting type: Boolean
# Default value: true
UnstackableItemsAlwaysReturnAtLeastOneResource = true

## If enabled, recycling will also check for the required crafting station type and level.
## If disabled, will ignore all crafting station requirements altogether.
## Enabled by default, to keep things close to how Valheim operates.
# Setting type: Boolean
# Default value: true
RequireExactCraftingStationForRecycling = true

## If enabled and recycling an item would yield 0 of any material,
## instead you will receive 1. If disabled, you get nothing.
# Setting type: Boolean
# Default value: true
PreventZeroResourceYields = true

## If enabled, it will allow you to recycle items that you do not know the recipe for yet.
## Disabled by default as this can be cheaty, but sometimes required due to people losing progress.
# Setting type: Boolean
# Default value: false
AllowRecyclingUnknownRecipes = false

[Recycling on containers]

## The last saved recycling button position stored in JSON
# Setting type: String
# Default value: {\"x\":502.42425537109377,\"y\":147.06060791015626,\"z\":-1.0}
ContainerButtonPosition = {\"x\":502.42425537109377,\"y\":147.06060791015626,\"z\":-1.0}

[UI]

## If enabled, the mod will display the container recycling button
# Setting type: Boolean
# Default value: false
ContainerRecyclingEnabled = false

## If enabled and recycling a specific item runs into any issues, the mod will print a message
## in the center of the screen (native Valheim notification). At the time of implementation,
## this happens in the following cases:
##  - not enough free slots in the inventory to place the resulting resources
##  - player does not know the recipe for the item
##  - if enabled, cases when `PreventZeroResourceYields` kicks in and prevent the crafting
# Setting type: Boolean
# Default value: true
NotifyOnSalvagingImpediments = true

## If enabled, will display the experimental work in progress crafting tab UI
## Enabled by default.
# Setting type: Boolean
# Default value: true
EnableExperimentalCraftingTabUI = true

## If enabled, it will hide equipped items in the crafting tab.
## This does not make the item recyclable and only influences whether or not it's shown.
## Enabled by default.
# Setting type: Boolean
# Default value: true
HideRecipesForEquippedItems = true

## If enabled, it will hide hotbar items in the crafting tab.
## Enabled by default.
# Setting type: Boolean
# Default value: true
IgnoreItemsOnHotbar = true

## If enabled, will filter all recycling recipes based on the crafting station
## used to produce said item. Main purpose of this is to prevent showing food
## as a recyclable item, but can be extended further if needed.
## Enabled by default
# Setting type: Boolean
# Default value: true
StationFilterEnabled = true

## Comma separated list of crafting stations (by their "piece name")
## recipes from which should be ignored in regards to recycling.
## Main purpose of this is to prevent showing food as a recyclable item,
## but can be extended further if needed.
## 
## Full list of stations used in recipes as of 0.147.3:
## - identifier: `$piece_forge` in game name: Forge
## - identifier: `$piece_workbench` in game name: Workbench
## - identifier: `$piece_cauldron` in game name: Cauldron
## - identifier: `$piece_stonecutter` in game name: Stonecutter
## 
## Use the identifiers, not the in game names (duh!)
# Setting type: String
# Default value: $piece_cauldron
StationFilterList = $piece_cauldron

[zDebug]

## If enabled will dump a complete detailed recycling report every time. This is taxing in terms
## of performance and should only be used when debugging issues. 
# Setting type: Boolean
# Default value: false
DebugAlwaysDumpAnalysisContext = false

## If enabled, will spam recycling checks to the console.
## VERY. VERY. SPAMMY. Influences performance. 
# Setting type: Boolean
# Default value: false
DebugAllowSpammyLogs = false

[zUtil]

## Nexus mod ID for updates
# Setting type: Int32
# Default value: 205
NexusID = 205

```


## Changelog

### 0.0.13

- fixes an issue where moving items with the recycling tab will log errors if it's the last recyclable item in the list
- recycling bronze is now possible, other multireciped items coming _**soon**_

### 0.0.12

- recycling now requires the right crafting station type and level
    - can be disabled in settings via `RequireExactCraftingStationForRecycling`
    - ui is not final, still needs station indicator
    
    ![](https://i.imgur.com/PtCJ730.png)

### 0.0.11

- added option to ignore items on the hotbar (enabled by default)
- added filter based on the crafting station used by recipe. Initially implemented for food, but can be configured to whatever one's needs are.
- fixed some minor compatibility issues with EpicLoot, moved button more to the right for now, needs proper fix. 

### 0.0.10

- setting to disable container recycling button now actually works
- fix a bug where the recycle button would appear in situations when it shouldn't
- main recycler logic now evaluates if an item is equipped and if it is, adds that as a blocking issue for recycling. 
- added setting to hide recipes for equipped items, enabled by default

### 0.0.9

- fixed null reference exceptions when plugin starts up (missed due to plugin being loaded before as a script)

### 0.0.8

- added new experimental UI that addds a new tab to the crafting station. Enabled by default.
- container recycle button no longer enabled by default

### 0.0.7

- fixed a bug where recycling an upgraded item with a rate of 1 that used additional resources than the initial recipe (per level rather than initially), would still return that ingredient back
- fixed some config naming consistency

### 0.0.6

- introduced the option to prevent recycling if item yield after recycling is 0. On by default
- added notifications for when the mod cannot (or decides not to) recycle. On by default 

### 0.0.5

- fixed a bug when uncrafting a single stacked item would yield returns for the whole stack

### 0.0.4

- __**(!) Added "RecyclingRate" setting**__
  
    This sets the rate at which the items are recycled. The mod used to return 100% of the resources, but now the rate is 50% by default.

- added a setting that when enabled, will prevent recycling 0 resources on unstackable items  
- button position is now movable by pressing left ctrl and dragging it with right click. Persists on reload
- refactored code in preparation for containerless recycling

### 0.0.3

- now searches all recipes 
- calculates the amount of resources based on quality level
- will not recycle if not enough slots are available

### 0.0.2

- Fix a null reference exception in main game menu

### 0.0.1
 
Initial release