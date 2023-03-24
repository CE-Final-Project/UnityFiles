using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Script.UI
{
    public class RelayJoiningUI : MonoBehaviour
    {
        [SerializeField] private InputField joinCodeInputField;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button joinButton;

        [Inject] private RelayUIMediator _relayUIMediator;
        
        /// <summary>
        /// Added to the InputField component's OnValueChanged callback for the join code text.
        /// </summary>
        public void OnJoinCodeInputTextChanged()
        {
            joinCodeInputField.text = SanitizeJoinCode(joinCodeInputField.text);
            joinButton.interactable = joinCodeInputField.text.Length > 0;
        }

        private static string SanitizeJoinCode(string dirtyString)
        {
            return Regex.Replace(dirtyString.ToUpper(), "[^A-Z0-9]", "");
        }
        
        public void OnJoinButtonPressed()
        {
            _relayUIMediator.JoinLobbyWithCodeRequest(SanitizeJoinCode(joinCodeInputField.text));
        }
        
        public void Show()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            joinCodeInputField.text = "";
        }

        public void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }
}