using System;
using Script.Game.GameplayObject.Character;
using UnityEngine;

namespace Script.Game.Actions.Input
{
    public abstract class BaseActionInput : MonoBehaviour
    {
        protected ServerCharacter PlayerOwner;
        protected Vector3 Origin;
        protected ActionID ActionPrototypeID;
        protected Action<ActionRequestData> SendInput;
        private System.Action _onFinished;
        
        public void Initiate(ServerCharacter playerOwner, Vector3 origin, ActionID actionPrototypeID, Action<ActionRequestData> onSendInput, System.Action onFinished)
        {
            PlayerOwner = playerOwner;
            Origin = origin;
            ActionPrototypeID = actionPrototypeID;
            SendInput = onSendInput;
            _onFinished = onFinished;
        }
        
        public void OnDestroy()
        {
            _onFinished();
        }
        
        public virtual void OnReleaseKey() { }
    }
}