using HarmonyLib;
using I2.Loc;

namespace MatingTentMod.Patches;

[HarmonyPatch]
internal class TranslationsPatch
{
    [HarmonyPatch(typeof(LocalizationManager), "GetTranslation")]
    [HarmonyPrefix]
    private static bool LocalizationManager_GetTranslation(ref string __result, string Term)
    {
        if (Term == "FollowerInteractions/NoTents")
        {
            __result = "Great Leader, there are no tents available for me to mate in!";
            return false;
        }

        if (Term == "FollowerInteractions/NoFollowers")
        {
            __result = "Great Leader, there are no followers available for me to mate with!";
        }

        return true;
    }
}