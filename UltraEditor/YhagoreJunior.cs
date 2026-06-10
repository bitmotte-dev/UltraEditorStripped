namespace UltraEditorStripped;

using UltraEditorStripped.Classes;
using UnityEngine;

public class YhagoreJunior : MonoBehaviour
{
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Backslash))
        {
            EmptySceneLoader.forceEditor = true;
            EmptySceneLoader.LoadLevel();
        }
    }
}