using System;
using System.Collections.Generic;
using Script.Game.Actions.Input;
using Script.Game.GameplayObject.Character;

namespace Script.Game.Actions.ActionPlayers
{
    public sealed class ClientActionPlayer
    {
        private List<Action> _playingActions = new List<Action>();
        
        private const float AnticipationTimeoutSeconds = 1f;
        
        public ClientCharacter ClientCharacter { get; private set; }
        
        public ClientActionPlayer(ClientCharacter clientCharacter)
        {
            ClientCharacter = clientCharacter;
        }

        private DateTime lastActionSoundFX = DateTime.UtcNow;

        public void OnUpdate()
        {
            for (int i = _playingActions.Count - 1; i >= 0; i--)
            {
                Action action = _playingActions[i];
                bool keepGoing = action.AnticipatedClient || action.OnUpdateClient(ClientCharacter);
                bool expirable = action.Config.DurationSeconds > 0f;
                bool timeExpired = expirable && action.TimeRunning >= action.Config.DurationSeconds;
                bool timedOut = action.AnticipatedClient && action.TimeRunning >= AnticipationTimeoutSeconds;
                if (!keepGoing || timeExpired || timedOut)
                {
                    if (timedOut)
                    {
                        action.CancelClient(ClientCharacter);
                    }
                    else
                    {
                        action.EndClient(ClientCharacter);
                    }
                    _playingActions.RemoveAt(i);
                    ActionFactory.ReturnAction(action);
                }
            }
        }

        private int FindAction(ActionID actionID, bool anticipatedOnly)
        {
            return _playingActions.FindIndex(a => a.ActionID == actionID && (!anticipatedOnly || a.AnticipatedClient));
        }
        
        public void OnAnimEvent(string id)
        {
            foreach (var actionFX in _playingActions)
            {
                actionFX.OnAnimEventClient(ClientCharacter, id);
            }
        }

        public void OnStoppedChargingUp(float finalChargeUpPercentage)
        {
            foreach (var actionFX in _playingActions)
            {
                actionFX.OnStoppedChargingUpClient(ClientCharacter, finalChargeUpPercentage);
            }
        }
        
        public void AnticipateAction(ref ActionRequestData data)
        {
            if (!ClientCharacter.IsAnimating() && Action.ShouldClientAnticipate(ClientCharacter, ref data))
            {
                var actionFX = ActionFactory.CreateActionFromData(ref data);
                actionFX.AnticipateActionClient(ClientCharacter);
                _playingActions.Add(actionFX);
            }
        }

        public void PlayAction(ref ActionRequestData data)
        {
            var anticipatedActionIndex = FindAction(data.ActionID, true);

            var actionFX = anticipatedActionIndex >= 0 ? _playingActions[anticipatedActionIndex] : ActionFactory.CreateActionFromData(ref data);
            if (actionFX.OnStartClient(ClientCharacter))
            {
                if (anticipatedActionIndex < 0)
                {
                    _playingActions.Add(actionFX);
                }
                //otherwise just let the action sit in it's existing slot
                if (actionFX != null && lastActionSoundFX.Subtract(DateTime.UtcNow).TotalSeconds <= 0)
                {
                    AudioManager.Instance.SFXSource.PlayOneShot(actionFX.Config.SoundEffect);
                    lastActionSoundFX = DateTime.UtcNow.AddSeconds(actionFX.Config.ReuseTimeSeconds);
                }
            }
            else if (anticipatedActionIndex >= 0)
            {
                var removedAction = _playingActions[anticipatedActionIndex];
                _playingActions.RemoveAt(anticipatedActionIndex);
                ActionFactory.ReturnAction(removedAction);
            }
        }
        
        /// <summary>
        /// Cancels all playing ActionFX.
        /// </summary>
        public void CancelAllActions()
        {
            foreach (var action in _playingActions)
            {
                action.CancelClient(ClientCharacter);
                ActionFactory.ReturnAction(action);
            }
            _playingActions.Clear();
        }

        public void CancelAllActionsWithSamePrototypeID(ActionID actionID)
        {
            for (int i = _playingActions.Count - 1; i >= 0; --i)
            {
                if (_playingActions[i].ActionID == actionID)
                {
                    var action = _playingActions[i];
                    action.CancelClient(ClientCharacter);
                    _playingActions.RemoveAt(i);
                    ActionFactory.ReturnAction(action);
                }
            }
        }
    }
}