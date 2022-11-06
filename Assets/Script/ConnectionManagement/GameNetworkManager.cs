using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;

public class GameNetworkManager : MonoBehaviour
{
    public static GameNetworkManager Instance { get; private set; }
    
    private Lobby? m_currentLobby { get; set; } = null;

    private FacepunchTransport m_transport;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        m_transport = GetComponent<FacepunchTransport>();

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnOnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
    }
    
     private void OnDestroy()
    {
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnOnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated -= OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;

        if (NetworkManager.Singleton == null) return;
        
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallBack;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallBack;
    }

    private void OnApplicationQuit() => Disconnect();

    public async void StartHost(int maxMember = 10)
    {
        
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.StartHost();

        await SteamMatchmaking.CreateLobbyAsync(maxMember);
    }

    private void StartClient(SteamId id)
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallBack;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallBack;

        m_transport.targetSteamId = id;
        
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("Client has joined", this);
        }
    }

    private void Disconnect()
    {
        m_currentLobby?.Leave();
        
        if (NetworkManager.Singleton == null) return;
        
        NetworkManager.Singleton.Shutdown();
    }
 
    #region Network Callbacks

    private void OnServerStarted() => Debug.Log("Server has started!!!", this);

    private void OnClientConnectedCallBack(ulong clientId) => Debug.Log($"Client has connected, clientId={clientId}", this);

    private void OnClientDisconnectCallBack(ulong clientId)
    {
        Debug.Log($"Client has disconnected, clientId={clientId}", this);
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallBack;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallBack;
    }

    #endregion

    #region Steam Callbacks

    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        if (result != Result.OK)
        {
            Debug.Log($"Lobby couldn't be created {result}", this);
            return;
        }

        lobby.SetFriendsOnly();
        lobby.SetData("name", "Cool Lobby");
        lobby.SetJoinable(true);
        
        Debug.Log("Lobby has been created!", this);
    }
    
    private void OnLobbyEntered(Lobby lobby)
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            StartClient(lobby.Id);
        };
        
    }
    
    private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
    }
    
    private void OnOnLobbyMemberLeave(Lobby lobby, Friend friend)
    {
    }
    
    private void OnLobbyInvite(Friend friend, Lobby lobby) => Debug.Log($"You got invite from {friend.Name}", this);

    private void OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId arg4)
    {

    }
    
    private void OnGameLobbyJoinRequested(Lobby lobby, SteamId id) => StartClient(id);

    #endregion
    
}
