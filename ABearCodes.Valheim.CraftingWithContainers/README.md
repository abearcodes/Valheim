# Crafting with Containers

A mod that will search and use resources in nearby containers when
using any crafting station. 

## How does it work?

There are mainly two components to it:

- a tracker that keeps track when a player uses a crafting station 
  and once he does it scans all containers in it's vicinity

- changes to the `CountItems` and `RemoveItem` inventory methods that 
  check if the current inventory is known to the tracker and if it is,
  it enhances those calls

## Harmony patches

Reverse patches:
- `[HarmonyPatch(typeof(Inventory), "CountItems")]`
 