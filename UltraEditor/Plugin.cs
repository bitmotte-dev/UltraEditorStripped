namespace UltraEditorStripped;

using BepInEx;
using HarmonyLib;
using System;
using System.Globalization;
using System.Threading;
using UltraEditorStripped.Classes;
using UltraEditorStripped.Libraries;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;


[BepInPlugin(GUID, Name, Version)]
public class Plugin : BaseUnityPlugin
{
    public const string GUID = "duviz.ultrakill.ultraeditor";
    public const string Name = "UltraEditor";
    public const string Version = "0.1.0";
    public const string ExpectedBuildGUID = "cd2a9e08be6e44379c465e9b5d8b4fc2"; // 17d4

    public static Plugin Instance;
    public plog.Logger Log;

    public static KeyCode EditorOpenKey = KeyCode.F1;
    public static KeyCode SelectCursorKey = KeyCode.F2;
    public static KeyCode SelectMoveKey = KeyCode.F3;
    public static KeyCode SelectScaleKey = KeyCode.F4;
    public static KeyCode SelectRotationKey = KeyCode.F5;
    public static KeyCode ToggleEditorCanvasKey = KeyCode.F9;
    public static KeyCode DeleteObjectKey = KeyCode.Delete;
    public static KeyCode CreateCubeKey = KeyCode.KeypadPlus;
    public static KeyCode CtrlKey = KeyCode.LeftControl;
    public static KeyCode ShiftKey = KeyCode.LeftShift;
    public static KeyCode AltKey = KeyCode.LeftAlt;

    const string LastPlayedVersionPref = "UltraEditor_LastPlayedVersion";

    static bool SeenWelcomeMessage = false;

    public static bool IsToggleEnabledKeyPressed() =>
        Input.GetKey(AltKey) && Input.GetKey(ShiftKey) && Input.GetKeyDown(KeyCode.A);

    public static bool IsDuplicateKeyPressed() =>
        Input.GetKey(CtrlKey) && Input.GetKeyDown(KeyCode.D);

    public static bool IsUndoPressed() =>
        Input.GetKey(CtrlKey) && !Input.GetKey(ShiftKey) && Input.GetKeyDown(KeyCode.Z);

    public static bool IsRedoPressed() =>
        Input.GetKey(CtrlKey) && 
            (
                (Input.GetKey(ShiftKey) && Input.GetKeyDown(KeyCode.Z))  // ctrl+shift+z
                || Input.GetKeyDown(KeyCode.Y) // ctrl+y
            );

    public static bool IsSelectPressed() =>
        Input.GetKey(AltKey) && Input.GetKeyDown(KeyCode.S);

    public static bool CanMove() =>
        !Input.GetKey(CtrlKey) && !Input.GetKey(AltKey);

    public void Awake()
    {
        if (Application.buildGUID != ExpectedBuildGUID)
        {
            Logger.LogFatal("Incompatible game version!" +
                $"\nGUID: {Application.buildGUID} EXPECTED: {ExpectedBuildGUID}");

            Destroy(this);
            return;
        }

        Instance = this;
        LogInfo("Hi :3");
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

        BundlesManager.Load();

        var harmony = new Harmony("duviz.ultrakill.ultraeditor");
        harmony.PatchAll();

        SceneManager.sceneLoaded += SceneUtility.OnSceneLoad;

        gameObject.hideFlags = HideFlags.DontSaveInEditor;
    }

    public void Start()
    {
        // load the assets window
        EmptySceneLoader.Load();
    }

    public void Update()
    {
        if (SceneHelper.CurrentScene == "Main Menu" && SceneHelper.PendingScene == null && !SeenWelcomeMessage)
        {
            if (Preferences.GetString(LastPlayedVersionPref) != GetVersion().ToString())
            {
                Instantiate(BundlesManager.welcomeCanvas);
                Preferences.SetString(LastPlayedVersionPref, GetVersion().ToString());
            }
            SeenWelcomeMessage = true;
        }
    }

    [Obsolete("Use AssHelper.Ass")]
    public static T Ass<T>(string path) { return Addressables.LoadAssetAsync<T>((object)path).WaitForCompletion(); }

    [Obsolete("Use AssHelper.ResAss")]
    public static T Ast<T>(string path) where T : UnityObject
    {
        T obj = Resources.Load<T>(path);
        if (obj == null)
            LogError($"Resources.Load failed for \"{path}\"");

        return obj;
    }

    #region Logging
    public static void LogInfo(object data, string stackTrace = null)
    {
        Instance.Logger?.LogInfo(stackTrace != null
            ? $"{data}\n{stackTrace}"
            : data);

        (Instance.Log ??= new("ULTRAEDITOR"))?.Info(data.ToString(), stackTrace: stackTrace);
    }
    public static void LogError(object data, string stackTrace = null)
    {
        Instance.Logger?.LogError(stackTrace != null
            ? $"{data}\n{stackTrace}"
            : data);

        (Instance.Log ??= new("ULTRAEDITOR"))?.Error(data.ToString(), stackTrace: stackTrace);
    }
    #endregion

    public static Version GetVersion()
    {
        return Instance.Info.Metadata.Version;
    }
}