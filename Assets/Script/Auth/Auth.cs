using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Script.Auth
{
    public enum AuthState
    {
        Initialized,
        Authenticating,
        Authenticated,
        Error,
        TimedOut
    }
    
    public static class Auth
    {
        private static AuthState AuthenticationState { get; set; } = AuthState.Initialized;
        
        public static async Task<AuthState> Authenticate(string profile,int tries = 5)
        {
            //If we are already authenticated, just return Auth
            if (AuthenticationState == AuthState.Authenticated)
            {
                return AuthenticationState;
            }
            
            if (AuthenticationState == AuthState.Authenticating)
            {
                Debug.LogWarning("Cant Authenticate if we are authenticating or authenticated");
                await Authenticating();
                return AuthenticationState;
            }
            
            InitializationOptions profileOptions = new InitializationOptions();
            profileOptions.SetProfile(profile);
            await UnityServices.InitializeAsync(profileOptions);
            await SignInAnonymouslyAsync(tries);
            
            return AuthenticationState;
        }
        
        private static async Task Authenticating()
        {
            while (AuthenticationState is AuthState.Authenticating or AuthState.Initialized)
            {
                await Task.Delay(200);
            }
        }
        
        private static async Task SignInAnonymouslyAsync(int maxRetries)
        {
            AuthenticationState = AuthState.Authenticating;
            int tries = 0;
            while (AuthenticationState == AuthState.Authenticating && tries < maxRetries)
            {
                try
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync().ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            Debug.LogError("SignInAnonymouslyAsync failed: " + task.Exception);
                            AuthenticationState = AuthState.Error;
                        }
                        else
                        {
                            Debug.Log("SignInAnonymouslyAsync succeeded");
                            AuthenticationState = AuthState.Authenticated;
                        }
                    });
                }
                catch (System.Exception e)
                {
                    Debug.LogError("SignInAnonymouslyAsync failed: " + e);
                    AuthenticationState = AuthState.Error;
                }
                tries++;
                await Task.Delay(1000);
            }
            
            if (AuthenticationState != AuthState.Authenticated)
            {
                Debug.LogWarning($"Player failed to authenticate after {tries} tries");
                AuthenticationState = AuthState.TimedOut;
            }
        }
        
        public static void SignOut()
        {
            AuthenticationService.Instance.SignOut();
            AuthenticationState = AuthState.Initialized;
        }
    }
}