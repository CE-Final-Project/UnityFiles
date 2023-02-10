using System;
using System.Threading.Tasks;
using Script.GameFramework.Core;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Script.GameFramework.Infrastructure
{
    public class Authentication : Singleton<Authentication>
    {
        private async void Awake()
        {
            try
            {
                await UnityServices.InitializeAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        private void Start()
        {
            AuthenticationService.Instance.SignedIn += OnSignIn;
            AuthenticationService.Instance.SignedOut += OnSignOut;
            AuthenticationService.Instance.SignInFailed += OnSignInFailed;
            AuthenticationService.Instance.Expired += OnSessionExpired;
        }
        
        private void OnDestroy()
        {
            AuthenticationService.Instance.SignedIn -= OnSignIn;
            AuthenticationService.Instance.SignedOut -= OnSignOut;
            AuthenticationService.Instance.SignInFailed -= OnSignInFailed;
            AuthenticationService.Instance.Expired -= OnSessionExpired;
        }
        
        public async Task SignInAnonymouslyAsync()
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Sign in anonymously succeeded!");
            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
        }

        public void SignOut()
        {
            AuthenticationService.Instance.SignOut();
        }

        private static void OnSignIn()
        {
            Debug.Log($"Signed Player ID: {AuthenticationService.Instance.PlayerId}\nAccess Token: {AuthenticationService.Instance.AccessToken}");
        }

        private static void OnSignOut()
        {
            Debug.Log("OnSignOut");
        }

        private static void OnSignInFailed(RequestFailedException requestFailedException)
        {
            Debug.LogError(requestFailedException);
        }
        private static void OnSessionExpired()
        {
            Debug.Log("OnSessionExpired");
        }
    }
}