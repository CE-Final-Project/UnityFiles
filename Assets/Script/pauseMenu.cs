using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pauseMenu : MonoBehaviour
{
    public static  bool gameIsPaused = false;
    public GameObject pauseMenuUI;
    public GameObject pauseButtonUI;
    public GameObject gameHUD;
    public GameObject playerInstance;

    // Update is called once per frame
    void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            if(gameIsPaused) {
                Resume();
            } else {
                Pause();
            }
        }
        if (SystemInfo.deviceType == DeviceType.Handheld && gameIsPaused == false)
        {
            print("on mobile and not paused");
            pauseButtonUI.SetActive(true);
        }
    }

    public void Resume() {
        pauseMenuUI.SetActive(false);
        gameHUD.SetActive(true);
        playerInstance.SetActive(true);
        if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            pauseButtonUI.SetActive(true);
        }
        else if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            pauseButtonUI.SetActive(false);
        }
        Time.timeScale = 1f;
        gameIsPaused = false;
    }
    
    public void Pause() {
        pauseMenuUI.SetActive(true);
        gameHUD.SetActive(false);
        playerInstance.SetActive(false);
        if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            pauseButtonUI.SetActive(false);
        }
        else if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            pauseButtonUI.SetActive(false);
        }
        Time.timeScale = 0f;
        gameIsPaused = true;
    }

    public bool isGamePause()
    {
        return gameIsPaused;
    }
}
