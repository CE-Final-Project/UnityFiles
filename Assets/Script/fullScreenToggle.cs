using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fullScreenToggle : MonoBehaviour{
    public void toggle(bool is_fullScreen) {
        Screen.fullScreen = is_fullScreen;
        if(is_fullScreen) {
            Resolution currentResolution = Screen.currentResolution;
            print("full screen");
            print(currentResolution);
            Screen.SetResolution(1920, 1080, true);
        }
        else {
            print("not full screen");
            Screen.SetResolution(1280, 720, false);
        }
    }
}
