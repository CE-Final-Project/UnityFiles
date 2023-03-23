using UnityEngine;

public class PauseScript : MonoBehaviour
{
    private bool overlayCheck = false;
    public GameObject overlayObj;

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
    }
}
