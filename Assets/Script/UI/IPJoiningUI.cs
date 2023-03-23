using UnityEngine;
using UnityEngine.UI;

namespace Script.UI
{
    public class IPJoiningUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private InputField ipInputField;
        [SerializeField] private InputField portInputField;

        [SerializeField] private IPUIMediator ipUIMediator;

        
        private void Awake()
        {
            ipInputField.text = IPUIMediator.DefaultIP;
            portInputField.text = IPUIMediator.DefaultPort.ToString();
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
        
        public void OnJoinButtonPressed()
        {
            ipUIMediator.JoinWithIP(ipInputField.text, portInputField.text);
        }
        
        public void SanitizeIPInputText()
        {
            ipInputField.text = IPUIMediator.Sanitize(ipInputField.text);
        }
        
        public void SanitizePortText()
        {
            string inputFieldText = IPUIMediator.Sanitize(portInputField.text);
            portInputField.text = inputFieldText;
        }
    }
}