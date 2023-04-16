using UnityEngine;
using UnityEngine.UI;

public class FullScreenToggle : MonoBehaviour
{
    public Toggle toggle;

    public void Toggle()
    {
        if(toggle.GetComponent<Toggle>().isOn)
        {
            Screen.fullScreen = true;
        }
        else
        {
            Screen.fullScreen = false;
        }
    }

}
