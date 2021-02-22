# Crafting with Containers

![demo](https://i.imgur.com/o2P8aT2.png)

Are you tired of rummaging through chests to find that one missing ingredient that you know
is somewhere? Tired of having to split stacks, craft and then put it all back again? 

Say no more!

With this mod when you are using a crafting table it will not only use your inventory, 
but also containers in its range.  

Limited to using crafting tables only at the moment. 

[Jump to changelog by clicking here](#changelog)

#### What containers are checked?
- within range of the crafting station (configurable)
- accessible by player using the crafting bench
- created by a player (generated chests are ignored)
- cart and ship containers (configurable)

#### Current limitations:
- if a container is placed after the player started using the crafting station, 
  it will not be considered until the player exits the stations and triggers it again
  
## Manual installation

Drop the `.dll` into `<GameLocation>\BepInEx\plugins`

## Encountered an issue?

Please create an issue in the GitHub repo here:

https://github.com/abearcodes/Valheim/issues/new

## Configuration

The plugin supports multiple ways of configuring it's behaviour. 
For specific details on how to set them, please check the config file
via R2Modman or manually under 

`BepInEx/config/com.github.abearcodes.valheim.craftingwithcontainers.cfg` 

The available settings are as follows:

- **TakeFromPlayerInventoryFirst**

  Prioritize taking items from the players inventory when crafting. 
  By default is disabled and will use resources from containers before
  taking items from the player's inventory. 

- **ContainerLookupRangeMultiplier**

  Multiplier for the range in which the mod searches for containers.
  Base range is equal to the range of the crafting table in use.
  Will not take from containers that are not currently loaded into memory.
     
- **ShowStationExtensionEffect**

  Adds a station extension effect to chests. This effect is the one that
  the game uses by default for chopping blocks, tanning decks, etc.
  
  Shouldn't influence performance too much unless you have hundreds of containers.
  
  The effect only appears when you are using a crafting table. Affects ships and carts!
  
  ![demo](https://i.imgur.com/O8AGTgH.png)
      
 
 - **ShouldFilterByContainerPieceNames** and **AllowedContainerLookupPieceNames**
 
    These settings allows you to enable and filter the containers that are checked.
    For example, you might want to exclude ships, carts or personal chests.
    
    The value for this setting is a comma separated list of identifiers.     
     
    By default it's disabled and all containers are considered when crafting. 

 ## Changelog
 
 ### 1.0.3
  
 - Partial multiplayer workaround for containers not updating. Does not fix: ships, moving carts,
 dragging resources at the last possible second.    
 
 ### 1.0.2
 
 - Fixed a bug of not removing items when the amount of items is the exact
 one needed.
 
 - Fixed a bug where tracker would attempt to remove effects from non existing entries 
 
 ### 1.0.1
 
 Changed GitHub URL in README
 
 ### 1.0.0 
 
 Initial release