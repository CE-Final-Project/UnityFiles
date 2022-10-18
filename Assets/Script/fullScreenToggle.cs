using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fullScreenToggle : MonoBehaviour{

    void Start()
    {
        Screen.SetResolution(1600, 900, false);
        print(Screen.currentResolution);
    }

    public void toggle(bool is_fullScreen) {
        Screen.fullScreen = is_fullScreen;
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            if (is_fullScreen)
            {
                print("full screen");
                Screen.SetResolution(1920, 1080, true);
            }
            else
            {
                print("not full screen");
                Screen.SetResolution(1600, 900, false);
            }
        }
    }
}
