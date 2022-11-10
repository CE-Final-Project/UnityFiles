using System;
using System.Collections;
using Unity.Game.ApplicationLifecycle.Messages;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Unity.Game.ApplicationLifecycle
{
    /// <summary>
    /// An entry point to the application, where we bind all the common dependencies to the root DI scope.
    /// </summary>
    public class ApplicationController : LifetimeScope
    {
        
        
        IDisposable m_Subscriptions;
        
        
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            
        }

        private void QuitGame(QuitApplicationMessage msg)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}