namespace UltraEditorStripped.Classes;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UltraEditorStripped.Libraries;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary> Handles loading and accessing the empty scene. </summary>
public static class EmptySceneLoader
{
    /// <summary> Whether its already loaded. </summary>
    private static bool _loaded = false;

    /// <summary> Forces the editor to open as soon as the scene loads. </summary>
    public static bool forceEditor = false;

    /// <summary> Forces the editor to load a save as soon as the scene loads. </summary>
    public static string forceSave = "";

    /// <summary> Forces the editor to load a data string as soon as the scene loads, only happens if forceSave is "?". </summary>
    public static string forceSaveData = "";

    /// <summary> Forces the editor to set a scene name, only happens if forceSave is "?". </summary>
    public static string forceLevelName = "";

    /// <summary> Forces the editor to set a scene name to SceneHelper </summary>
    public static string forceLevelGUID = "";

    /// <summary> Load the assetbundle containing the scene. </summary>
    public static void Load()
    {
        // istg why does this crash the game when u dont do this
        AssHelper.Ass<GameObject>("FirstRoom");

        // load asset bundle :3 meow rawr
        Stream bundleStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("UltraEditor.Assets.emptyscene.bundle");

        AssetBundleCreateRequest assetRequest = AssetBundle.LoadFromStreamAsync(bundleStream);
        assetRequest.completed += (_) =>
        {
            Plugin.LogInfo("Loaded Empty Scene bundle.");
            _loaded = true;
        };
    }

    /// <summary> Loads the Empty level. </summary>
    public static Coroutine LoadLevel() => 
        Plugin.Instance.StartCoroutine(LoadLevelAsync());

    /// <summary> Asynchronously loads the Empty level. </summary>
    public static IEnumerator LoadLevelAsync()
    {
        Plugin.LogInfo("Loading Empty Scene.");
        SceneHelper.PendingScene = "EditorManager.EditorSceneName";
        SceneHelper.Instance.loadingBlocker.SetActive(true);
        if (!forceEditor && forceSave != "") SceneHelper.SetLoadingSubtext("Loading level...");
        else SceneHelper.SetLoadingSubtext("Loading editor...");
        yield return null;

        if (!_loaded)
        {
            Plugin.LogError("Empty Scene Bundle wasn't loaded before trying to enter the scene.");
            Load();

            // wait til its loaded
            while (!_loaded) yield return null;
        }

        if (SceneHelper.CurrentScene.StartsWith("EditorManager.EditorSceneName")) 
            SceneHelper.LastScene = SceneHelper.CurrentScene;
        
        SceneHelper.CurrentScene = "EditorManager.EditorSceneName";
        if (forceLevelGUID != "" && forceSave == "?" && !forceEditor)
            SceneHelper.CurrentScene = "EditorManager.EditorSceneName"+"."+forceLevelGUID;

        AsyncOperation sceneload = SceneManager.LoadSceneAsync("Assets/ModTechnicalName/Scenes/TestLevel/Empty Editor Scene.unity");

        // wait til its loaded 
        while (!sceneload.isDone) yield return null;

        Plugin.LogInfo("Scene loaded!");
        SceneHelper.SetLoadingSubtext("");
        SceneHelper.Instance.loadingBlocker.SetActive(false);
        SceneHelper.PendingScene = null;

        // duviz why
        yield return LoadEditor();
    }

    public static string pTime = "";
    public static string pKills = "";
    public static string pStyle = "";
    public static bool forceLevelCanOpenEditor = false;
    public static string forceLevelLayer = "CUSTOM LEVEL";
    public static string forceLevelImage = "null";

    /// <summary> Loads the actual editor or level once you enter the scene. </summary>
    /// <remarks>(THIS IS WRITTEN BY DUVIZ NOT BY ME PLEASE THIS WASNT ME)</remarks>
    public static IEnumerator LoadEditor()
    {
        if (forceEditor)
        {
            forceLevelCanOpenEditor = false;
            while (!NewMovement.Instance.activated && SceneHelper.PendingScene == null) { yield return null; }
            OpenEditor();
            forceLevelLayer = "CUSTOM LEVEL";
        }

        else if (forceSave != "")
        {
            string levelName = forceSave.Replace(".uterus", "");
            if (forceSave != "?")
            {
                forceLevelCanOpenEditor = false;
                forceLevelLayer = "CUSTOM LEVEL";
            }
            else
            {
                levelName = forceLevelName;

                int pt = int.Parse(pTime);
                int pk = int.Parse(pKills);
                int ps = int.Parse(pStyle);

                for (int i = 0; i < 4; i++)
                    StatsManager.Instance.timeRanks[3 - i] = (int)(pt * ((i * 0.25f) + 1f));
                for (int i = 0; i < 4; i++)
                    StatsManager.Instance.killRanks[3 - i] = pk - (i * 5);
                for (int i = 0; i < 4; i++)
                    StatsManager.Instance.styleRanks[3 - i] = ps - (i * 1000);

                StockMapInfo.Instance.assets.LargeImage = forceLevelImage;
            }
            List<GameObject> secrets = [];
            int ind = 0;
            foreach (Bonus secret in UnityObject.FindObjectsOfType<Bonus>(true))
            {
                secret.secretNumber = ind;
                secrets.Add(secret.gameObject);
                ind++;
            }
            StatsManager.Instance.secretObjects = secrets.ToArray();
            StockMapInfo.Instance.levelName = levelName.ToUpper();
            StockMapInfo.Instance.layerName = StockMapInfo.Instance.layerName.Replace("EMPTY", forceLevelLayer);
            StockMapInfo.Instance.assets.LargeText = levelName.ToUpper();

            ShopZone[] sz = UnityObject.FindObjectsOfType<ShopZone>(true);
            foreach (var s in sz)
            {
                s.tipOfTheDay?.text = StockMapInfo.Instance.tipOfTheDay.tip;
            }
            StockMapInfo.Instance.assets.LargeText = StockMapInfo.Instance.levelName.ToUpper();
            LevelNamePopup.Instance.Invoke("Start", 0);
            DiscordController.Instance.FetchSceneActivity(SceneHelper.CurrentScene);
            SteamController.Instance.FetchSceneActivity(SceneHelper.CurrentScene);
        }
        
        StatsManager.Instance.gameObject.AddComponent<FogFadeController>();
        StatsManager.Instance.gameObject.AddComponent<TimeTracker>();
        ChallengeManager.Instance.challengePanel.transform.parent.Find("ChallengeText").GetComponent<TMP_Text>().text = "(NO CHALLENGE)";
        MusicManager.Instance.battleTheme.outputAudioMixerGroup = AudioMixerController.Instance.musicGroup;
        MusicManager.Instance.cleanTheme.outputAudioMixerGroup = AudioMixerController.Instance.musicGroup;
        MusicManager.Instance.bossTheme.outputAudioMixerGroup = AudioMixerController.Instance.musicGroup;
        MusicManager.Instance.GetComponent<AudioSource>().outputAudioMixerGroup = AudioMixerController.Instance.musicGroup;
    }

    /// <summary> Tries to open the editor. </summary>
    public static void OpenEditor()
    {
        if (SceneHelper.PendingScene != null) return;
    }
}
