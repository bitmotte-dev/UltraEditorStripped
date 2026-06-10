namespace UltraEditorStripped;

using UltraEditorStripped.Classes;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneUtility
{
    public static void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        GameObject yhagoreJunior = new("Yhagore Junior");
        yhagoreJunior.AddComponent<YhagoreJunior>();
    }
}