namespace UltraEditorStripped.Patches;

using HarmonyLib;
using UltraEditorStripped.Classes;

[HarmonyPatch(typeof(LeaderboardController), "SubmitLevelScore")]
public static class LeaderboardControllerPatch
{
    public static bool Prefix() =>
        EditorManager.Instance == null;
}