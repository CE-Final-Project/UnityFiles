using UnityEngine;
using UnityEditor;

public class ExitEditor : MonoBehaviour
{
    void ExitTheEditor()
    {
        if (EditorApplication.isPlaying)
        {
            // Exit play mode if running in the editor.
            EditorApplication.ExitPlaymode();
        }
        else
        {
            // Quit the editor.
            EditorApplication.Exit(0);
        }
    }
}
