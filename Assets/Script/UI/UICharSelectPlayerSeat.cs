using Script.Game.GameplayObject.Character;
using Script.GameState;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Script.UI
{
    public class UICharSelectPlayerSeat : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerNameHolder;
        
        [SerializeField] private Image playerNumberHolder; 
        
        [SerializeField] private Sprite playerNumberHolderDefaultSprite;

        [SerializeField] private CharacterTypeEnum characterClass;
        
        private int _seatIndex;
        private int _playerNumber;
        private NetworkCharSelection.SeatState _state;
        private bool _isDisabled;
        
        public void Initialize(int seatIndex)
        {
            _seatIndex = seatIndex;
            _state = NetworkCharSelection.SeatState.Inactive;
            _playerNumber = -1;
            ConfigureStateGraphics();
        }
        
        public void SetState(NetworkCharSelection.SeatState state, int playerIndex, string playerName)
        {
            if (state == _state && playerIndex == _playerNumber)
            {
                return;
            }
            
            _state = state;
            _playerNumber = playerIndex;
            playerNameHolder.text = playerName;
            
            if (state == NetworkCharSelection.SeatState.Inactive)
            {
                _playerNumber = -1;
            }
            ConfigureStateGraphics();
        }
        
        public void SetDisableInteraction(bool disable)
        {
            _isDisabled = disable;

            if (!disable)
            {
                PlayUnlockAnim();
            }
        }

        public bool IsLocked()
        {
            return _state == NetworkCharSelection.SeatState.LockedIn;
        }
        
        public void SetDisableInteractions(bool disable)
        {
            _isDisabled = disable;
            
            if (!disable)
            {
                PlayUnlockAnim();
            }
        }
        
        private void PlayLockAnim()
        {
            
        }
        
        private void PlayUnlockAnim()
        {
            
        }

        private void ConfigureStateGraphics()
        {
            if (_state == NetworkCharSelection.SeatState.Inactive)
            {
                playerNameHolder.gameObject.SetActive(false);
                playerNumberHolder.sprite = playerNumberHolderDefaultSprite;
                PlayUnlockAnim();
            }
            else
            {
                playerNumberHolder.sprite =
                    ClientCharSelectState.Instance.identifiersForEachPlayerNumber[_playerNumber].Indicator;
                playerNameHolder.gameObject.SetActive(true);
                // playerNameHolder.color =
                //     ClientCharSelectState.Instance.identifiersForEachPlayerNumber[_playerNumber].Color;

                if (_state == NetworkCharSelection.SeatState.LockedIn)
                {
                    PlayLockAnim();
                }
                else
                {
                    PlayUnlockAnim();
                }
            }
        }

        public void OnClicked()
        {
            ClientCharSelectState.Instance.OnPlayerClickedSeat(_seatIndex);
        }
    }
}