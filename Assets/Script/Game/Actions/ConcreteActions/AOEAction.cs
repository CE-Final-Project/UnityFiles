using System;
using Script.Game.GameplayObject.Character;
using Script.Game.GameplayObject.RuntimeDataContainers;
using UnityEngine;

namespace Script.Game.Actions.ConcreteActions
{
    /// <summary>
    /// Area-of-effect attack Action. The attack is centered on a point provided by the client.
    /// </summary>
    [CreateAssetMenu(menuName = "TheSurvivor/Actions/AOE Action")]
    public class AOEAction : Action
    {
        /// <summary>
        /// Cheat prevention: to ensure that players don't perform AoEs outside of their attack range,
        /// we ensure that the target is less than Range meters away from the player, plus this "fudge
        /// factor" to accomodate miscellaneous minor movement.
        /// </summary>
        const float k_MaxDistanceDivergence = 1;

        bool m_DidAoE;


        public override bool OnStart(ServerCharacter serverCharacter)
        {
            float distanceAway = Vector2.Distance(serverCharacter.PhysicsWrapper.Transform.position, Data.Position);
            if (distanceAway > Config.Range + k_MaxDistanceDivergence)
            {
                // Due to latency, it's possible for the client side click check to be out of date with the server driven position. Doing a final check server side to make sure.
                return ActionConclusion.Stop;
            }

            // broadcasting to all players including myself.
            // We don't know our actual targets for this attack until it triggers, so the client can't use the TargetIds list (and we clear it out for clarity).
            // This means we are responsible for triggering reaction-anims ourselves, which we do in PerformAoe()
            Data.TargetIDs = Array.Empty<ulong>();
            serverCharacter.serverAnimationHandler.NetworkAnimator.SetTrigger(Config.Anim);
            serverCharacter.ClientCharacter.RecvDoActionClientRPC(Data);
            return ActionConclusion.Continue;
        }

        public override void Reset()
        {
            base.Reset();
            m_DidAoE = false;
        }

        public override bool OnUpdate(ServerCharacter clientCharacter)
        {
            if (TimeRunning >= Config.ExecTimeSeconds && !m_DidAoE)
            {
                // actually perform the AoE attack
                m_DidAoE = true;
                PerformAoE(clientCharacter);
            }

            return ActionConclusion.Continue;
        }

        private void PerformAoE(ServerCharacter parent)
        {
            // Note: could have a non alloc version of this overlap sphere where we statically store our collider array, but since this is a self
            // destroyed object, the complexity added to have a static pool of colliders that could be called by multiplayer players at the same time
            // doesn't seem worth it for now.
            // var colliders = Physics.OverlapSphere(m_Data.Position, Config.Radius, LayerMask.GetMask("NPCs"));
            // for (var i = 0; i < colliders.Length; i++)
            // {
            //     var enemy = colliders[i].GetComponent<IDamageable>();
            //     if (enemy != null)
            //     {
            //         // actually deal the damage
            //         enemy.ReceiveHP(parent, -Config.Amount);
            //     }
            // }
            // change it to 2d;
            
            int layerMask = LayerMask.GetMask("NPCs");

            if (Config.IsFriendly)
            {
                layerMask = LayerMask.GetMask("PCs");
            }
            
            var colliders = Physics2D.OverlapCircleAll(Data.Position, Config.Radius, layerMask);
            foreach (var collider in colliders)
            {
                var enemy = collider.GetComponent<IDamageable>();
                if (enemy != null)
                {
                    if (!Config.IsFriendly)
                        GameStats.Instance.PlayersStats.AddDamageDealt(parent.NetworkObjectId, Config.Amount);
                    // actually deal the damage
                    enemy.ReceiveHP(parent, -Config.Amount);
                }
            }
            AudioManager.Instance.SFXSource.PlayOneShot(Config.SoundEffect);
        }

        public override bool OnStartClient(ClientCharacter clientCharacter)
        {
            base.OnStartClient(clientCharacter);
            GameObject.Instantiate(Config.Spawns[0], Data.Position, Quaternion.identity);
            return ActionConclusion.Stop;
        }

        public override bool OnUpdateClient(ClientCharacter clientCharacter)
        {
            throw new Exception("This should not execute");
        }
    }
}
