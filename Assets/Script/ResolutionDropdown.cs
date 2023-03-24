using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ResolutionDropdown : MonoBehaviour
{
    public Dropdown resolutionDropdown;

    private Resolution[] resolutions;

    void Start()
    {
        resolutionDropdown = transform.GetComponent<Dropdown>();
        // Define the available resolution options
        resolutions = new Resolution[2];
        resolutions[0] = new Resolution { width = 1920, height = 1080 };
        resolutions[1] = new Resolution { width = 2560, height = 1440 };

        // Clear any existing options from the dropdown
        resolutionDropdown.ClearOptions();

        // Create a list of resolution options to add to the dropdown
        List<string> resolutionOptions = new List<string>();
        foreach (Resolution resolution in resolutions)
        {
            string option = resolution.width + " x " + resolution.height;
            if (!resolutionOptions.Contains(option))
            {
                resolutionOptions.Add(option);
            }
        }

        // Add the resolution options to the dropdown
        resolutionDropdown.AddOptions(resolutionOptions);

        // Set the default resolution option based on the current screen resolution
        int defaultOption = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                defaultOption = i;
                break;
            }
        }
        resolutionDropdown.value = defaultOption;
        resolutionDropdown.RefreshShownValue();
    }

    public void OnResolutionDropdownValueChanged(int optionIndex)
    {
        // Get the selected resolution from the dropdown
        Resolution selectedResolution = resolutions[optionIndex];

        // Set the screen resolution to the selected resolution
        Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreen);
    }
}