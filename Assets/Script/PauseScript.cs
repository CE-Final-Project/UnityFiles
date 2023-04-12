using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PauseScript : MonoBehaviour
{
    private bool overlayCheck = false;
    public GameObject overlayObj;
    public TextMeshProUGUI playerPositionText;
    public TextMeshProUGUI cameraPositionText;
    private GameObject playerAvatar;
    private GameObject playerCamera;

    void Start()
    {
        playerAvatar = GameObject.FindGameObjectWithTag("Player");
        playerCamera = GameObject.FindGameObjectWithTag("VirtualCamera");
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (overlayObj == null)
            {
                Debug.LogWarning("Overlay object is not assigned in inspector.");
                return;
            }

            //Debug.Log("Overlay toggle");
            overlayCheck = !(overlayCheck);

            if (overlayCheck)
            {
                Debug.Log("Overlay ON");
                overlayObj.SetActive(true);
            }
            else
            {
                Debug.Log("Overlay OFF");
                overlayObj.SetActive(false);
            }
        }

        //Update Player Position on HUD
        if (playerAvatar != null)
        {
            // Get the transform position of the player avatar
            Vector2 playerPosition = playerAvatar.transform.position;

            // Update the text UI component with the player position
            playerPositionText.text = "Player position\n " + playerPosition.ToString();
        }

        //Update Camera Position on HUD
        if (playerCamera != null)
        {
            // Get the transform position of the player avatar
            Vector2 cameraPosition = playerCamera.transform.position;

            // Update the text UI component with the player position
            cameraPositionText.text = "Camera position\n " + cameraPosition.ToString();
        }
    }
}
