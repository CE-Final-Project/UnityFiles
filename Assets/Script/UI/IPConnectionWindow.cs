using System;
using System.Collections;
using Script.ConnectionManagement;
using Script.Infrastructure.PubSub;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

namespace Script.UI
{
    public class IPConnectionWindow : MonoBehaviour
    {
         [FormerlySerializedAs("m_CanvasGroup")] [SerializeField]
         private CanvasGroup canvasGroup;

        [FormerlySerializedAs("m_TitleText")] [SerializeField]
        private TextMeshProUGUI titleText;

        [Inject] private IPUIMediator _ipUIMediator;

        private ISubscriber<ConnectStatus> _connectStatusSubscriber;

        [Inject]
        private void InjectDependencies(ISubscriber<ConnectStatus> connectStatusSubscriber)
        {
            _connectStatusSubscriber = connectStatusSubscriber;
            _connectStatusSubscriber.Subscribe(OnConnectStatusMessage);
        }

        private void Awake()
        {
            Hide();
        }

        private void OnDestroy()
        {
            _connectStatusSubscriber?.Unsubscribe(OnConnectStatusMessage);
        }

        private void OnConnectStatusMessage(ConnectStatus connectStatus)
        {
            CancelConnectionWindow();
            _ipUIMediator.DisableSignInSpinner();
        }

        private void Show()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }

        private void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        public void ShowConnectingWindow()
        {
            void OnTimeElapsed()
            {
                Hide();
                _ipUIMediator.DisableSignInSpinner();
            }

            UnityTransport utp = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            int maxConnectAttempts = utp.MaxConnectAttempts;
            int connectTimeoutMS = utp.ConnectTimeoutMS;
            StartCoroutine(DisplayUTPConnectionDuration(maxConnectAttempts, connectTimeoutMS, OnTimeElapsed));

            Show();
        }

        public void CancelConnectionWindow()
        {
            Hide();
            StopAllCoroutines();
        }

        private IEnumerator DisplayUTPConnectionDuration(int maxReconnectAttempts, int connectTimeoutMS, Action endAction)
        {
            float connectionDuration = maxReconnectAttempts * connectTimeoutMS / 1000f;

            int seconds = Mathf.CeilToInt(connectionDuration);

            while (seconds > 0)
            {
                titleText.text = $"Connecting...\n{seconds}";
                yield return new WaitForSeconds(1f);
                seconds--;
            }
            titleText.text = "Connecting...";

            endAction();
        }

        // invoked by UI cancel button
        public void OnCancelJoinButtonPressed()
        {
            CancelConnectionWindow();
            _ipUIMediator.JoiningWindowCancelled();
        }
    }
}