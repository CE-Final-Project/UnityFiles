using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Script.UI
{
    public class RelayHostingUI : MonoBehaviour
    {
        [SerializeField] InputField lobbyNameInputField;
        [SerializeField] GameObject loadingIndicatorObject;
        [SerializeField] Toggle isPrivate;
        [SerializeField] CanvasGroup canvasGroup;
        [Inject] private RelayUIMediator _relayUIMediator;
        
        void Awake()
        {
            EnableUnityRelayUI();
        }
        
        void EnableUnityRelayUI()
        {
            loadingIndicatorObject.SetActive(false);
        }
        
        public void OnCreateClick()
        {
            _relayUIMediator.CreateLobbyRequest(lobbyNameInputField.text, isPrivate.isOn);
        }
        
        public void Show()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
        
        public void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }
}