
# Crafting with Containers

![demo](https://raw.githubusercontent.com/abearcodes/Valheim/development/ABearCodes.Valheim.CraftingWithContainers/ThunderStoreResources/succ.gif)

Are you tired of rummaging through chests to find that one missing ingredient that you know is somewhere? Tired of having to split stacks, craft and then put it all back again? 

Say no more!

With this mod when you are crafting, building or fueling up, it will not only use your inventory, but also containers within your range.  

Limited to using crafting tables only at the moment. 

[Jump to changelog by clicking here](#changelog)

#### What containers are checked?
- within a configurable range
- accessible by the player using the mod
- created by a player (generated chests are ignored)
- cart and ship containers (configurable)
    - (!) there are still some issues with networking that are being worked on  
    
  
## Manual installation

Drop the `.dll` into `<GameLocation>\BepInEx\plugins`

## Encountered an issue?

Please create an issue in the GitHub repo here:

https://github.com/abearcodes/Valheim/issues/new

## Configuration

The plugin supports multiple ways of configuring it's behaviour. For specific details on how to set them, please check the config file via R2Modman or manually under 

`BepInEx/config/com.github.abearcodes.valheim.craftingwithcontainers.cfg` 

The list below only highlights settings with major impact. 

#### General settings

- **ContainerLookupRange**

    Range in which the mod searches for containers. Base range is equal to the range of the crafting table (20). Will not take from containers that are not currently loaded into memory, so setting this value to big numbers might not yield the expected result.

- **AddExtractionEffectWhenCrafting**
  
    _Enabled by default_  

    If enabled, when removing items from containers an fading line effect will appear from affected containers (see demo screenshot).

- **LogItemRemovalsToConsole**

    If enabled, item removal requests will be logged to the ingame console (accessible via F5).

    ![!image](https://i.imgur.com/VQ1Qkpb.png)

#### Interactions settings

- **AllowTakeFuelForKilnAndFurnace** and **AllowTakeFuelForFireplace**

    If enabled, will allow the mod to take fuel from nearby containers when using fuels & kilns or fireplaces accordingly. 
 
#### Filtering settings  
 
- **ShouldFilterByContainerPieceNames** and **AllowedContainerLookupPieceNames**
 
    These settings allows you to enable and filter the containers that are checked.
    For example, you might want to exclude ships, carts or personal chests.
    
    The value for this setting is a comma separated list of identifiers.     
     
    By default it's disabled and all containers are considered when crafting. 

# Changelog

### v1.0.4

Big changes! Pretty much a rewrite!

- _**No longer requires a crafting station**_
- _**Now supports building too!**_
- _**No longer patches Inventory methods directly.**_
    - Instead it reroutes required calls to Inventory to a static class.
    - This should help with mod compatibility as the initial methods are untouched. This will cause mods not to use the "CraftingFromContainers" functionality, but both mods should work properly.   
- Toggleable Smelter, Kiln and Fireplace fuel consumption from nearby containers
- Added indicator to how many resources of a type the player has

  ![image](https://i.imgur.com/hDS5fyL.png)
  
- Swapped previous "crafting station extension" effect for a "sucking" effect when the crafting actually takes place
- Option to take resources from player inventory first removed
- Option to log crafting events to Console window (via `F5`)

  ![!image](https://i.imgur.com/VQ1Qkpb.png)

 
## 1.0.3
  
 - Partial multiplayer workaround for containers not updating. Does not fix: ships, moving carts,
 dragging resources at the last possible second.    
 
## 1.0.2
 
 - Fixed a bug of not removing items when the amount of items is the exact
 one needed.
 
 - Fixed a bug where tracker would attempt to remove effects from non existing entries 
 
## 1.0.1
 
 Changed GitHub URL in README
 
## 1.0.0 
 
 Initial release