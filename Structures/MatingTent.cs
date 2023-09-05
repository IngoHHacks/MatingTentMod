using COTL_API.CustomStructures;
using COTL_API.Helpers;
using Lamb.UI.BuildMenu;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MatingTentMod.Structures;

public class MatingTent : CustomStructure
{
    public override string InternalName => "MATING_TENT";
    public override string PrefabPath => "Prefabs/Structures/Buildings/Building Mating Tent";

    public override Sprite Sprite =>
        TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/MatingTent.png"));

    public override Vector2Int Bounds => new(2, 2);

    public override int BuildDurationMinutes => 300;

    public override List<StructuresData.ItemCost> Cost => new()
    {
        new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.LOG, 20),
        new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.STONE, 10),
        new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 10)
    };

    public override FollowerCategory.Category Category => FollowerCategory.Category.Misc;
    public override Categories StructureCategories => Categories.FOLLOWERS;

    public override string GetLocalizedName()
    {
        return "Mating Tent";
    }

    public override string GetLocalizedDescription()
    {
        return "A tent for mating.";
    }
}