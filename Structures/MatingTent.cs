using COTL_API.CustomStructures;
using COTL_API.Helpers;
using I2.Loc;
using Lamb.UI.BuildMenu;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MatingTentMod.Structures;

public class MatingTent : CustomStructure
{
    public override string InternalName => "MATING_TENT";
    public override string PrefabPath => "Prefabs/Structures/Buildings/Building Mating Tent";

    public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/MatingTent.png"));

    public override Vector2Int Bounds => new(2, 2);

    public override int BuildDurationMinutes => 300;

    public override string GetLocalizedName()
    {
        if (LocalizationManager.CurrentLanguage == "Simplified Chinese")
            return "交配用帐篷";
        else
            return "Mating Tent";
    }
    public override string GetLocalizedDescription()
    {
        if (LocalizationManager.CurrentLanguage == "Simplified Chinese")
            return "用于交配的帐篷。";
        else
            return"A tent for mating.";
    }

    public override List<StructuresData.ItemCost> Cost => new()
    {
        new(InventoryItem.ITEM_TYPE.LOG, 20),
        new(InventoryItem.ITEM_TYPE.STONE, 10),
        new(InventoryItem.ITEM_TYPE.BLACK_GOLD, 10),
    };

    public override FollowerCategory.Category Category => FollowerCategory.Category.Misc;
    public override Categories StructureCategories => Categories.FOLLOWERS;
}