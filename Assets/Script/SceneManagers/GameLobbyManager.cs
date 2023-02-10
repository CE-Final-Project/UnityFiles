using System.Text.RegularExpressions;
using Script.GameFramework.Manager;
using Script.Networks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

namespace Script.SceneManagers
{
    public class GameLobbyManager : MonoBehaviour
    {
        
        [SerializeField] private Button lobbyButton;

        private string _hostIp;

        private void Start()
        {
            SceneTransitionHandler.Instance.SetSceneState(SceneTransitionHandler.SceneStates.Lobby);

            if (NetworkManager.Singleton.IsHost)
            {
                lobbyButton.onClick.AddListener(StartLocalGame);
            }
            else
            {
                lobbyButton.enabled = false;
            }
            
            
            Debug.Log(
                $"Lobby Id: {LobbyManager.Instance.CurrentLobbyData.LobbyId} name: {LobbyManager.Instance.CurrentLobbyData.LobbyName} code: {LobbyManager.Instance.CurrentLobbyData.LobbyCode}");
        }

        private void StartLocalGame()
        {
            SceneTransitionHandler.Instance.SwitchScene("InGame");
        }
        
        private void StartHostNetwork()
        {
            
           var utpTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
           if (utpTransport) _hostIp = "127.0.0.1";
           if (NetworkManager.Singleton.StartHost())
           {
               SceneTransitionHandler.Instance.RegisterCallbacks();
               SceneTransitionHandler.Instance.SwitchScene("InGame");
               NetworkManager.Singleton.OnServerStarted += () =>
               {
                   Debug.Log($"Server Started at {NetworkManager.Singleton.ConnectedHostname}");
               };
           }
           else
           {
               Debug.LogError("Failed to start host.");
           }
        }

        private void StartClientNetwork()
        {
            if (_hostIp == "Hostname") return;
            
            var utpTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            if (utpTransport)
            {
                utpTransport.SetConnectionData(Sanitize(_hostIp), 7777);
                utpTransport.SetConnectionData("127.0.0.1", 7777);
            }
            
            if (!NetworkManager.Singleton.StartClient())
            {
                SceneTransitionHandler.Instance.SwitchScene("InGame");
                Debug.LogError("Failed to start client.");
            }
        }
        
        private static string Sanitize(string dirtyString)
        {
            // sanitize the input for the ip address
            return Regex.Replace(dirtyString, "[^A-Za-z0-9.]", "");
        }
    }
}