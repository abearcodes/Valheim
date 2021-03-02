
# Simple Recycler

![demo](https://i.imgur.com/91ILtUh.png)

Adds a button that recycles (uncrafts) items in any container. 

## CRITICAL CHANGES AS OF 0.0.4

### Please read the changelog below. 

### Key points

- Returns 50% of the resources by default. Configurable via settings. 
- You need to know the recipe for the item to "uncraft" it
- You can move the recycling button by pressing the left Ctrl and right click dragging the button
- Has a fail safe to prevent accidental presses
  
    ![](https://i.imgur.com/iAbLzvN.png)

## Manual installation

Drop the `.dll` into `<GameLocation>\BepInEx\plugins`

## Encountered an issue?

Please create an issue in the GitHub repo here:

https://github.com/abearcodes/Valheim/issues/new

## Development plans

- introduce a separate buildable "station" to do the uncrafting
- add option to uncraft even if recipe is unknown

## Configurability

```
## Settings file was created by plugin SimpleRecycling v0.0.6
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

## If enabled and recycling an item would yield 0 of any material,
## instead you will receive 1. If disabled, you get nothing.
# Setting type: Boolean
# Default value: true
PreventZeroResourceYields = true

[Recycling on containers]

## If enabled, the mod will display the container recycling button
# Setting type: Boolean
# Default value: true
ContainerRecyclingEnabled = true

## The last saved recycling button position stored in JSON
# Setting type: String
# Default value: {\"x\":502.42425537109377,\"y\":147.06060791015626,\"z\":-1.0}
ContainerButtonPosition = {\"x\":502.42425537109377,\"y\":147.06060791015626,\"z\":-1.0}

[UI]

## If enabled and recycling a specific item runs into any issues, the mod will print a message
## in the center of the screen (native Valheim notification). At the time of implementation,
## this happens in the following cases:
##  - not enough free slots in the inventory to place the resulting resources
##  - player does not know the recipe for the item
##  - if enabled, cases when `PreventZeroResourceYields` kicks in and prevent the crafting
# Setting type: Boolean
# Default value: true
NotifyOnSalvagingImpediments = true


```


## Changelog

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