using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fullScreenToggle : MonoBehaviour{

    void Start()
    {
        //Screen.SetResolution(1600, 900, false);
        print(Screen.currentResolution);
    }

    public void toggle(bool is_fullScreen) {
        //Screen.fullScreen = is_fullScreen;
        print(Screen.currentResolution);
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            if (is_fullScreen)
            {
                print("full screen");
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                //Screen.SetResolution(2650, 1600, true);
            }
            else
            {
                print("not full screen");
                Screen.fullScreenMode = FullScreenMode.Windowed;
                //Screen.SetResolution(1600, 900, false);
            }
        }
    }

}
