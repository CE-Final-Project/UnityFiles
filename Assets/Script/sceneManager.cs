using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneManager : MonoBehaviour {
    public void MoveToScene(int sceneID) {
        GameNetworkManager.Instance.StartHost();
        SceneManager.LoadScene(sceneID);
    }
}
