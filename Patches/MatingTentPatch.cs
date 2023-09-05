using COTL_API.CustomStructures;
using HarmonyLib;
using MatingTentMod.Structures;
using System;
using System.Collections.Generic;

namespace MatingTentMod.Patches;

[HarmonyPatch]
internal class MatingTentPatch
{
    [HarmonyPatch(typeof(PlacementRegion), "GetHoveredStructure")]
    [HarmonyPrefix]
    private static void PlacementRegion_GetHoveredStructure(PlacementRegion __instance)
    {
        PlacementTile closestTileAtWorldPosition =
            __instance.GetClosestTileAtWorldPosition(__instance.PlacementPosition);
        if (closestTileAtWorldPosition == null)
        {
            return;
        }

        PlacementRegion.TileGridTile tileGridTile =
            __instance.GetTileGridTile(closestTileAtWorldPosition.GridPosition.x,
                closestTileAtWorldPosition.GridPosition.y);
        if (tileGridTile?.ObjectOnTile == CustomStructureManager.GetStructureByType<MatingTent>())
        {
            tileGridTile.ObjectOnTile = StructureBrain.TYPES.MATING_TENT;
        }
    }


    [HarmonyPatch(typeof(StructuresData), "BuildDurationGameMinutes")]
    [HarmonyPrefix]
    private static bool StructuresData_BuildDurationGameMinutes(ref int __result, StructureBrain.TYPES Type)
    {
        if (Type == StructureBrain.TYPES.MATING_TENT)
        {
            __result = Plugin.MatingTent.BuildDurationMinutes;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(StructuresData), "GetCost")]
    [HarmonyPrefix]
    private static bool StructuresData_GetCost(ref List<StructuresData.ItemCost> __result, StructureBrain.TYPES Type)
    {
        if (Type == StructureBrain.TYPES.MATING_TENT)
        {
            __result = Plugin.MatingTent.Cost;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(StructuresData), "GetLocalizedNameStatic", typeof(StructureBrain.TYPES))]
    [HarmonyPrefix]
    public static bool StructuresData_GetLocalizedNameStatic(StructureBrain.TYPES Type, ref string __result)
    {
        if (Type == StructureBrain.TYPES.MATING_TENT)
        {
            __result = Plugin.MatingTent.GetLocalizedName();
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(StructuresData), "LocalizedName", typeof(StructureBrain.TYPES))]
    [HarmonyPrefix]
    public static bool StructuresData_LocalizedName(StructureBrain.TYPES Type, ref string __result)
    {
        if (Type == StructureBrain.TYPES.MATING_TENT)
        {
            __result = Plugin.MatingTent.GetLocalizedName();
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(StructuresData), "LocalizedDescription", typeof(StructureBrain.TYPES))]
    [HarmonyPrefix]
    public static bool StructuresData_LocalizedDescription(StructureBrain.TYPES Type, ref string __result)
    {
        if (Type == StructureBrain.TYPES.MATING_TENT)
        {
            __result = Plugin.MatingTent.GetLocalizedDescription();
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(StructuresData), "LocalizedPros", typeof(StructureBrain.TYPES))]
    [HarmonyPrefix]
    public static bool StructuresData_LocalizedPros(StructureBrain.TYPES Type, ref string __result)
    {
        if (Type == StructureBrain.TYPES.MATING_TENT)
        {
            __result = Plugin.MatingTent.LocalizedPros();
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(StructuresData), "LocalizedCons", typeof(StructureBrain.TYPES))]
    [HarmonyPrefix]
    public static bool StructuresData_LocalizedCons(StructureBrain.TYPES Type, ref string __result)
    {
        if (Type == StructureBrain.TYPES.MATING_TENT)
        {
            __result = Plugin.MatingTent.LocalizedCons();
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(StructuresData), "GetLocalizedName", new Type[] { })]
    [HarmonyPrefix]
    public static bool StructuresData_GetLocalizedName(StructuresData __instance, ref string __result)
    {
        if (__instance.Type == StructureBrain.TYPES.MATING_TENT)
        {
            __result = Plugin.MatingTent.GetLocalizedName();
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(StructuresData), "GetLocalizedDescription", new Type[] { })]
    [HarmonyPrefix]
    public static bool StructuresData_GetLocalizedDescription(StructuresData __instance, ref string __result)
    {
        if (__instance.Type == StructureBrain.TYPES.MATING_TENT)
        {
            __result = Plugin.MatingTent.GetLocalizedDescription();
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(StructuresData), "GetLocalizedLore", new Type[] { })]
    [HarmonyPrefix]
    public static bool StructuresData_GetLocalizedLore(StructuresData __instance, ref string __result)
    {
        if (__instance.Type == StructureBrain.TYPES.MATING_TENT)
        {
            __result = Plugin.MatingTent.GetLocalizedLore();
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(StructuresData), "GetLocalizedName", typeof(bool), typeof(bool), typeof(bool))]
    [HarmonyPrefix]
    public static bool StructuresData_GetLocalizedName(StructuresData __instance, bool plural, bool withArticle,
        bool definite, ref string __result)
    {
        if (__instance.Type == StructureBrain.TYPES.MATING_TENT)
        {
            __result = Plugin.MatingTent.GetLocalizedName(plural, withArticle, definite);
            return false;
        }

        return true;
    }
}