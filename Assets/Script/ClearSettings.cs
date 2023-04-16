using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearSettings : MonoBehaviour
{
    [SerializeField] private GameObject clearButton;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            clearButton.SetActive(!clearButton.activeSelf);
        }
    }

    public void ClearAllSettings()
    {
        PlayerPrefs.DeleteAll();
    }
}
