using BepInEx;
using BepInEx.Logging;
using COTL_API.CustomFollowerCommand;
using COTL_API.CustomStructures;
using HarmonyLib;
using MatingTentMod.FollowerCommands;
using MatingTentMod.Structures;
using System.IO;

namespace MatingTentMod;

[BepInPlugin(PluginGuid, PluginName, PluginVer)]
[BepInDependency("io.github.xhayper.COTL_API")]
[HarmonyPatch]
public class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "IngoH.cotl.MatingTentMod";
    public const string PluginName = "MatingTentMod";
    public const string PluginVer = "1.0.0";

    internal static ManualLogSource Log;
    internal static readonly Harmony Harmony = new(PluginGuid);

    internal static string PluginPath;

    internal static MatingTent MatingTent;
    internal static MateFollower MateCommand;
    internal static MateLamb MateLamb;

    private void Awake()
    {
        this.Logger.LogInfo($"Loaded {PluginName}!");
        Log = this.Logger;

        PluginPath = Path.GetDirectoryName(this.Info.Location);

        MatingTent = new MatingTent();
        CustomStructureManager.Add(new MatingTent());
        MateCommand = new MateFollower();
        CustomFollowerCommandManager.Add(MateCommand);
        MateLamb = new MateLamb();
        CustomFollowerCommandManager.Add(MateLamb);
    }

    private void OnEnable()
    {
        Harmony.PatchAll();
        this.Logger.LogInfo($"Loaded {PluginName}!");
    }

    private void OnDisable()
    {
        Harmony.UnpatchSelf();
        this.Logger.LogInfo($"Unloaded {PluginName}!");
    }
}