using BepInEx;
using BepInEx.Logging;
using COTL_API.CustomFollowerCommand;
using COTL_API.CustomStructures;
using HarmonyLib;
using MatingTentMod.FollowerCommands;
using MatingTentMod.Structures;
using System.IO;

namespace MatingTentMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("io.github.xhayper.COTL_API")]
    [HarmonyPatch]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "IngoH.cotl.MatingTentMod";
        public const string PluginName = "MatingTentMod";
        public const string PluginVer = "0.1.4";

        internal static ManualLogSource Log;
        internal readonly static Harmony Harmony = new(PluginGuid);

        internal static string PluginPath;

        internal static MatingTent MatingTent;
        internal static MateCommand MateCommand;

        private void Awake()
        {
            Logger.LogInfo($"Loaded {PluginName}!");
            Plugin.Log = base.Logger;

            PluginPath = Path.GetDirectoryName(Info.Location);

            MatingTent = new MatingTent();
            CustomStructureManager.Add(new MatingTent());
            MateCommand = new MateCommand();
            CustomFollowerCommandManager.Add(MateCommand);

        }

        private void OnEnable()
        {
            Harmony.PatchAll();
            Logger.LogInfo($"Loaded {PluginName}!");
        }

        private void OnDisable()
        {
            Harmony.UnpatchSelf();
            Logger.LogInfo($"Unloaded {PluginName}!");
        }
    }
}