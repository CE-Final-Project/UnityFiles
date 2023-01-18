using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneManager : MonoBehaviour {
    public void MoveToScene(int sceneID) {
        SceneManager.LoadScene(sceneID);
    }
}
