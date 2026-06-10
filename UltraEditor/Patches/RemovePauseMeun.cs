namespace UltraEditorStripped.Patches;

using HarmonyLib;
using UltraEditorStripped.Classes;

[HarmonyPatch]
public static class RemovePauseMeun
{
    [HarmonyPrefix] [HarmonyPatch(typeof(OptionsManager), nameof(OptionsManager.Pause))]
    public static bool DontFuckingExistYouFuckAssMenu() =>
        !EditorManager.Instance?.editorOpen ?? true;
}