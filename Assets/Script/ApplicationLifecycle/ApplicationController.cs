using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Script.ApplicationLifecycle
{
    /// <summary>
    /// An entry point to the application, where we bind all the common dependencies to the root DI scope.
    /// </summary>
    public class ApplicationController : LifetimeScope
    {


        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            Debug.Log("App Container Configured", this);
        }
    }
}