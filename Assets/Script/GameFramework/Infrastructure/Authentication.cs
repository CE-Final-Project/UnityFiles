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

                Debug.Log("Unity Services initialized!");
                
#if UNITY_EDITOR 
                if (ParrelSync.ClonesManager.IsClone())
                {
                    // When using a ParrelSync clone, switch to a different authentication profile to force the clone
                    // to sign in as a different anonymous user account.
                    string customArgument = ParrelSync.ClonesManager.GetArgument();
                    AuthenticationService.Instance.SwitchProfile($"Clone_{customArgument}_Profile");
                }
#endif
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async void Start()
        {
            AuthenticationService.Instance.SignedIn += OnSignIn;
            AuthenticationService.Instance.SignedOut += OnSignOut;
            AuthenticationService.Instance.SignInFailed += OnSignInFailed;
            AuthenticationService.Instance.Expired += OnSessionExpired;

            await SignInAnonymouslyAsync();
        }
        
        private void OnDestroy()
        {
            AuthenticationService.Instance.SignedIn -= OnSignIn;
            AuthenticationService.Instance.SignedOut -= OnSignOut;
            AuthenticationService.Instance.SignInFailed -= OnSignInFailed;
            AuthenticationService.Instance.Expired -= OnSessionExpired;
        }

        private static async Task SignInAnonymouslyAsync()
        {
            try
            {
                
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    Debug.LogError("Unity Services is not initialized!");
                    return;
                }
                
                if (AuthenticationService.Instance.IsAuthorized)
                {
                    Debug.Log("Already signed in!");
                    return;
                }
                
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

        public void OnApplicationQuit()
        {
            Debug.Log("Signing out...");
            
            Instance.SignOut();
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