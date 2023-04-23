using System;
using System.Collections.Generic;
using Script.ConnectionManagement;
using Script.Lobby;
using Script.UI;
using TMPro;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Script.GameState
{
    [RequireComponent(typeof(NetcodeHooks))]
    public class ClientCharSelectState : GameStateBehaviour
    {
        public static ClientCharSelectState Instance { get; private set; }
        
        [SerializeField] private NetcodeHooks netCodeHooks;
        protected override GameState ActiveState => GameState.CharSelect;
        
        [SerializeField] private NetworkCharSelection networkCharSelection;

        [SerializeField] private TextMeshProUGUI numberOfPlayersText;
        [SerializeField] private TextMeshProUGUI lobbyCodeText;
        [SerializeField] private TextMeshProUGUI readyButtonText;
        
        [SerializeField] private TextMeshProUGUI LobbyNameAndRegionText;
        
        [SerializeField] private List<UICharSelectPlayerSeat> playerSeats;

        [SerializeField] private PlayerNameListUI playerNameListUI;

        [Serializable]
        public class ColorAndIndicator
        {
            public Sprite Indicator;
            public Color Color;
        }
        
        [SerializeField] public ColorAndIndicator[] identifiersForEachPlayerNumber;
        
        private int _lastSelectedCharacterIndex;
        private bool _hasLocalPlayerLockedIn;

        private enum LobbyMode
        {
            ChooseCharacter, // Waiting for local player to choose
            CharacterChosen, // Waiting for other players to choose
            LobbyEnding, // Game is about to start
            FatalError
        }

        private ConnectionManager _connectionManager;
        private LocalLobby _localLobby;
        
        [Inject]
        private void InjectDependencies(ConnectionManager connectionManager, LocalLobby localLobby)
        {
            _connectionManager = connectionManager;
            _localLobby = localLobby;
            _localLobby.Changed += UpdateLobbyCode;
            UpdateLobbyCode(localLobby);
        }
        
        private void UpdateLobbyCode(LocalLobby localLobby)
        {
            if (!string.IsNullOrEmpty(localLobby.LobbyCode) && lobbyCodeText != null)
            {
                lobbyCodeText.enabled = true;
                lobbyCodeText.text = $"Lobby Code: {localLobby.LobbyCode}";
            }
            
            if (!string.IsNullOrEmpty(localLobby.LobbyName))
            {
                // set lobby name and relay region
                LobbyNameAndRegionText.text = $"{localLobby.LobbyName} ({localLobby.RelayRegion})";
            }
            else
            {
                // set ip address and port from connection
                UnityTransport utp = (UnityTransport)_connectionManager.NetworkManager.NetworkConfig.NetworkTransport;
                LobbyNameAndRegionText.text = $"{utp.ConnectionData.Address}:{utp.ConnectionData.Port}";
            }
        }
        
        protected override void Awake()
        {
            base.Awake();
            Instance = this;
            lobbyCodeText.enabled = false;
            
            netCodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
            netCodeHooks.OnNetworkDespawnHook += OnNetworkDespawn;
            
            AudioManager.Instance.StartLobbyMusic();
        }

        protected override void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            
            base.OnDestroy();
        }
        
        protected override void Start()
        {
            base.Start();
            for (int i = 0; i < playerSeats.Count; i++)
            {
                playerSeats[i].Initialize(i);
            }
        }

        private void OnNetworkDespawn()
        {
            if (networkCharSelection)
            {
                networkCharSelection.IsLobbyClosed.OnValueChanged -= OnLobbyClosedChanged;
                networkCharSelection.LobbyPlayerStates.OnListChanged -= OnLobbyPlayerStatesChanged;
            }
        }

        private void OnNetworkSpawn()
        {
            if (!NetworkManager.Singleton.IsClient)
            {
                enabled = false;
            }
            else
            {
                networkCharSelection.IsLobbyClosed.OnValueChanged += OnLobbyClosedChanged;
                networkCharSelection.LobbyPlayerStates.OnListChanged += OnLobbyPlayerStatesChanged;
            }
        }

        private void OnAssignedPlayerNumber(int playerNum)
        {
            // Set Player number 
        }

        private void UpdatePlayerCount()
        {
            int count = networkCharSelection.LobbyPlayerStates.Count;
            string playerCountText = count == 1 ? "1 Player" : $"{count} Players";
            numberOfPlayersText.text = playerCountText;
        }
        
        private void UpdatePlayerList()
        {

            var playerNameList = new Dictionary<int, string>();
            for (int i = 0; i < networkCharSelection.LobbyPlayerStates.Count; i++)
            {
                string IsReady = networkCharSelection.LobbyPlayerStates[i].SeatState == NetworkCharSelection.SeatState.LockedIn ? " - Ready" : "";
                playerNameList.Add(networkCharSelection.LobbyPlayerStates[i].PlayerNumber, networkCharSelection.LobbyPlayerStates[i].PlayerName + IsReady);
            }
            
            playerNameListUI.UpdatePlayerNameList(playerNameList);
        }

        
        private void OnLobbyPlayerStatesChanged(NetworkListEvent<NetworkCharSelection.LobbyPlayerState> changeEvent)
        {
            UpdateSeat();
            UpdatePlayerList();
            UpdatePlayerCount();

            int localPlayerIdx = -1;
            for (int i = 0; i < networkCharSelection.LobbyPlayerStates.Count; i++)
            {
                if (networkCharSelection.LobbyPlayerStates[i].ClientId == NetworkManager.Singleton.LocalClientId)
                {
                    localPlayerIdx = i;
                    break;
                }
            }
            
            if (localPlayerIdx == -1)
            {
                UpdateCharacterSelection(NetworkCharSelection.SeatState.Inactive);
            } 
            else if (networkCharSelection.LobbyPlayerStates[localPlayerIdx].SeatState ==
                       NetworkCharSelection.SeatState.Inactive)
            {
                UpdateCharacterSelection(NetworkCharSelection.SeatState.Inactive);
                
                OnAssignedPlayerNumber(networkCharSelection.LobbyPlayerStates[localPlayerIdx].PlayerNumber);
            }
            else
            {
                UpdateCharacterSelection(networkCharSelection.LobbyPlayerStates[localPlayerIdx].SeatState, networkCharSelection.LobbyPlayerStates[localPlayerIdx].SeatIdx);
            }
        }
        
        private void UpdateCharacterSelection(NetworkCharSelection.SeatState state, int seatIdx = -1)
        {
            bool isNewSeat = _lastSelectedCharacterIndex != seatIdx;

            _lastSelectedCharacterIndex = seatIdx;
            
            if (state == NetworkCharSelection.SeatState.Inactive)
            {
                // Set character selection to inactive
            }
            else
            {
                if (seatIdx != -1)
                {
                    // Change character preview when selecting a new character
                    if (isNewSeat)
                    {
                        //TODO: Update UI
                        ConfigureUIForLobbyMode(LobbyMode.ChooseCharacter);
                    }
                }

                if (state == NetworkCharSelection.SeatState.LockedIn && !_hasLocalPlayerLockedIn)
                {
                    //  the local player has locked in their seat choice!
                    ConfigureUIForLobbyMode(networkCharSelection.IsLobbyClosed.Value ? LobbyMode.LobbyEnding : LobbyMode.CharacterChosen);
                    _hasLocalPlayerLockedIn = true;
                }
                else if (_hasLocalPlayerLockedIn && state == NetworkCharSelection.SeatState.Active)
                {
                    // reset character seats if locked in choice was unselected
                    if (_hasLocalPlayerLockedIn)
                    {
                        ConfigureUIForLobbyMode(LobbyMode.ChooseCharacter);
                        _hasLocalPlayerLockedIn = false;
                    }
                }
                else if (state == NetworkCharSelection.SeatState.Active && isNewSeat)
                {
                    // Set character selection to active
                    
                }
            }
        }

        private void UpdateSeat()
        {
            // Players can hop between seats -- and can even SHARE seats -- while they're choosing a class.
            // Once they have chosen their class (by "locking in" their seat), other players in that seat are kicked out.
            // But until a seat is locked in, we need to display each seat as being used by the latest player to choose it.
            // So we go through all players and figure out who should visually be shown as sitting in that seat.
            var
                curSeats = new NetworkCharSelection.LobbyPlayerState[playerSeats.Count];
            foreach (NetworkCharSelection.LobbyPlayerState playerState in networkCharSelection.LobbyPlayerStates)
            {
                if (playerState.SeatIdx == -1 || playerState.SeatState == NetworkCharSelection.SeatState.Inactive)
                    continue; // this player isn't seated at all!
                if (curSeats[playerState.SeatIdx].SeatState == NetworkCharSelection.SeatState.Inactive 
                    || (curSeats[playerState.SeatIdx].SeatState == NetworkCharSelection.SeatState.Active && curSeats[playerState.SeatIdx].LastChangeTime < playerState.LastChangeTime))
                {
                    // this is the best candidate to be displayed in this seat (so far)
                    curSeats[playerState.SeatIdx] = playerState;
                }
            }
            
            // now actually update the seats in the UI
            for (int i = 0; i < playerSeats.Count; i++)
            {
                playerSeats[i].SetState(curSeats[i].SeatState, curSeats[i].PlayerNumber, curSeats[i].PlayerName);
            }
        }
        
        private void OnLobbyClosedChanged(bool wasLobbyClosed, bool isLobbyClosed)
        {
            if (isLobbyClosed)
            {
                ConfigureUIForLobbyMode(LobbyMode.LobbyEnding);
            }
            else
            {
                if (_lastSelectedCharacterIndex == -1)
                {
                    ConfigureUIForLobbyMode(LobbyMode.ChooseCharacter);
                }
                else
                {
                    ConfigureUIForLobbyMode(LobbyMode.CharacterChosen);
                    // TODO: Update UI
                }
            }
        }

        private void ConfigureUIForLobbyMode(LobbyMode lobbyMode)
        {
            // first the easy bit: turn off all the inappropriate ui elements, and turn the appropriate ones on!
            
            // foreach (var list in m_LobbyUIElementsByMode.Values)
            // {
            //     foreach (var uiElement in list)
            //     {
            //         uiElement.SetActive(false);
            //     }
            // }
            //
            // foreach (var uiElement in m_LobbyUIElementsByMode[mode])
            // {
            //     uiElement.SetActive(true);
            // }
            
            // that finishes the easy bit. Next, each lobby mode might also need to configure the lobby seats and class-info box.
            bool isSeatsDisabledInThisMode = false;
            switch (lobbyMode)
            {
                case LobbyMode.ChooseCharacter:
                    if (_lastSelectedCharacterIndex == -1)
                    {
                        // if (currentCharacterGraphics)
                        // {
                        //     currentCharacterGraphics.gameObject.SetActive(false);
                        // }
                        //TODO: Update UI
                    }
                    readyButtonText.text = "READY!";
                    break;
                case LobbyMode.CharacterChosen:
                    isSeatsDisabledInThisMode = true;
                    //TODO: Update UI
                    readyButtonText.text = "UNREADY";
                    break;
                case LobbyMode.LobbyEnding:
                    isSeatsDisabledInThisMode = true;
                    //TODO: Update UI
                    break;
                case LobbyMode.FatalError:
                    isSeatsDisabledInThisMode = true;
                    //TODO: Update UI
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lobbyMode), lobbyMode, null);
            }
            
            // // go through all our seats and enable or disable buttons
            foreach (UICharSelectPlayerSeat seat in playerSeats)
            {
                // disable interaction if seat is already locked or all seats disabled
                seat.SetDisableInteraction(seat.IsLocked() || isSeatsDisabledInThisMode);
            }
        }

        public void OnPlayerClickedSeat(int seatIdx)
        {
            if (networkCharSelection.IsSpawned)
            {
                networkCharSelection.ChangeSeatServerRpc(NetworkManager.Singleton.LocalClientId, seatIdx, false);
            }
        }
        
        public void OnPlayerClickedReady()
        {
            if (networkCharSelection.IsSpawned)
            {
                // request to lock in or unlock if already locked in
                networkCharSelection.ChangeSeatServerRpc(NetworkManager.Singleton.LocalClientId, _lastSelectedCharacterIndex, !_hasLocalPlayerLockedIn);
            }
        }

        public void OnQuitLobbyClicked()
        {
            _connectionManager.RequestShutdown();
        }

        // private GameObject GetCharacterGraphics(Avatar avatar)
        // {
        //     if (!m_SpawnedCharacterGraphics.TryGetValue(avatar.Guid, out GameObject characterGraphics))
        //     {
        //         characterGraphics = Instantiate(avatar.GraphicsCharacterSelect, m_CharacterGraphicsParent);
        //         m_SpawnedCharacterGraphics.Add(avatar.Guid, characterGraphics);
        //     }
        //
        //     return characterGraphics;
        // }
    }
}