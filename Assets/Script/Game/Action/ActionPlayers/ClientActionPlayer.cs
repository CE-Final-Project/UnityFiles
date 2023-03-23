using System.Collections.Generic;
using Script.Game.GameplayObject.Character;

namespace Script.Game.Action.ActionPlayers
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
    }
}