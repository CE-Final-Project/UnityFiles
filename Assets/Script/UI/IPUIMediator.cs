using System.Text.RegularExpressions;
using Script.Configuration;
using Script.ConnectionManagement;
using Script.Infrastructure.PubSub;
using TMPro;
using UnityEngine;
using VContainer;

namespace Script.UI
{
    public class IPUIMediator : MonoBehaviour
    {
        public const string DefaultIP = "127.0.0.1";
        public const int DefaultPort = 9889;

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI playerNameLabel;
        [SerializeField] private IPJoiningUI ipJoiningUI;
        [SerializeField] private IPHostingUI ipHostingUI;
        [SerializeField] private IPConnectionWindow ipConnectionWindow;
        [SerializeField] private GameObject signInSpinner;
        
        [Inject] private NameGenerationData _nameGenerationData;
        [Inject] private ConnectionManager _connectionManager;
        
        public IPHostingUI IpHostingUI => ipHostingUI;
        
        private ISubscriber<ConnectStatus> _connectStatusSubscriber;

        [Inject]
        private void InjectionDependencies(ISubscriber<ConnectStatus> connectStatusSubscriber)
        {
            _connectStatusSubscriber = connectStatusSubscriber;
            _connectStatusSubscriber.Subscribe(OnConnectStatusMessage);
        }
        
        private void Awake()
        {
            Hide();
        }

        private void Start()
        {
            ToggleHostIPUI();
            RegenerateName();
        }
        
        private void OnDestroy()
        {
            _connectStatusSubscriber?.Unsubscribe(OnConnectStatusMessage);
        }

        private void OnConnectStatusMessage(ConnectStatus connectStatus)
        {
            DisableSignInSpinner();
        }

        public void HostIPRequest(string ip, string port)
        {
            int.TryParse(port, out int portInt);
            if (portInt <= 0)
            {
                portInt = DefaultPort;
            }
            
            ip = string.IsNullOrEmpty(ip) ? DefaultIP : ip;
            
            signInSpinner.SetActive(true);
            _connectionManager.StartHostIp(playerNameLabel.text, ip, portInt);
        }

        public void JoinWithIP(string ip, string port)
        {
            int.TryParse(port, out int portInt);
            if (portInt <= 0)
            {
                portInt = DefaultPort;
            }

            ip = string.IsNullOrEmpty(ip) ? DefaultIP : ip;

            signInSpinner.SetActive(true);
  
            _connectionManager.StartClientIp(playerNameLabel.text, ip, portInt);
            
            // ipConnectionWindow.ShowConnectingWindow();
        }
        
        public void JoiningWindowCancelled()
        {
            DisableSignInSpinner();
            RequestShutdown();
        }
        
        public void DisableSignInSpinner()
        {
            signInSpinner.SetActive(false);
        }

        private void RequestShutdown()
        {
            if (_connectionManager && _connectionManager.NetworkManager)
            {
                _connectionManager.RequestShutdown();
            }
        }

        private void RegenerateName()
        {
            playerNameLabel.text = _nameGenerationData.GenerateName();
        }

        public void ToggleJoinIPUI()
        {
            ipJoiningUI.Show();
            ipHostingUI.Hide();
        }
        
        public void ToggleHostIPUI()
        {
            ipHostingUI.Show();
            ipJoiningUI.Hide();
        }

        public void Show()
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        public void Hide()
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        /// <summary>
        /// Sanitize user port InputField box allowing only alphanumerics and '.'
        /// </summary>
        /// <param name="dirtyString"> string to sanitize. </param>
        /// <returns> Sanitized text string. </returns>
        public static string Sanitize(string dirtyString)
        {
            return Regex.Replace(dirtyString, "[^A-Za-z0-9.]", "");
        }
    }
}