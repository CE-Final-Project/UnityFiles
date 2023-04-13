using Script.Game.GameplayObject.Character;
using Script.Game.GameplayObject.RuntimeDataContainers;
using UnityEngine;

namespace Script.Game.Actions.ConcreteActions
{
    [CreateAssetMenu(menuName = "TheSurvivor/Actions/MeleeAction")]
    public partial class MeleeAction : Action
    {
        private bool m_ExecutionFired;
        private ulong m_ProvisionalTarget;

        public override bool OnStart(ServerCharacter serverCharacter)
        {
            ulong target = (Data.TargetIDs != null && Data.TargetIDs.Length > 0) ? Data.TargetIDs[0] : serverCharacter.TargetId.Value;
            IDamageable foe = DetectFoe(serverCharacter, target);
            if (foe != null)
            {
                m_ProvisionalTarget = foe.NetworkObjectId;
                Data.TargetIDs = new ulong[] { foe.NetworkObjectId };
            }

            // snap to face the right direction
            if (Data.Direction != Vector3.zero)
            {
                serverCharacter.PhysicsWrapper.Transform.forward = Data.Direction;
            }

            serverCharacter.serverAnimationHandler.NetworkAnimator.SetTrigger(Config.Anim);
            serverCharacter.ClientCharacter.RecvDoActionClientRPC(Data);
            return true;
        }

        public override void Reset()
        {
            base.Reset();
            m_ExecutionFired = false;
            m_ProvisionalTarget = 0;
            m_ImpactPlayed = false;
            // m_SpawnedGraphics = null;
        }

        public override bool OnUpdate(ServerCharacter clientCharacter)
        {
            if (!m_ExecutionFired && (Time.time - TimeStarted) >= Config.ExecTimeSeconds)
            {
                m_ExecutionFired = true;
                var foe = DetectFoe(clientCharacter, m_ProvisionalTarget);
                if (foe != null)
                {
                    foe.ReceiveHP(clientCharacter, -Config.Amount);
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the ServerCharacter of the foe we hit, or null if none found.
        /// </summary>
        /// <returns></returns>
        private IDamageable DetectFoe(ServerCharacter parent, ulong foeHint = 0)
        {
            return GetIdealMeleeFoe(Config.IsFriendly ^ parent.IsNpc, parent.PhysicsWrapper.DamageCollider, Config.Range, foeHint);
        }

        /// <summary>
        /// Utility used by Actions to perform Melee attacks. Performs a melee hit-test
        /// and then looks through the results to find an alive target, preferring the provided
        /// enemy.
        /// </summary>
        /// <param name="isNPC">true if the attacker is an NPC (and therefore should hit PCs). False for the reverse.</param>
        /// <param name="ourCollider">The collider of the attacking GameObject.</param>
        /// <param name="meleeRange">The range in meters to check for foes.</param>
        /// <param name="preferredTargetNetworkId">The NetworkObjectId of our preferred foe, or 0 if no preference</param>
        /// <returns>ideal target's IDamageable, or null if no valid target found</returns>
        public static IDamageable GetIdealMeleeFoe(bool isNPC, Collider2D ourCollider, float meleeRange, ulong preferredTargetNetworkId)
        {
            RaycastHit[] results;
            int numResults = ActionUtils.DetectMeleeFoe(isNPC, ourCollider, meleeRange, out results);

            IDamageable foundFoe = null;

            //everything that got hit by the raycast should have an IDamageable component, so we can retrieve that and see if they're appropriate targets.
            //we always prefer the hinted foe. If he's still in range, he should take the damage, because he's who the client visualization
            //system will play the hit-react on (in case there's any ambiguity).
            for (int i = 0; i < numResults; i++)
            {
                var damageable = results[i].collider.GetComponent<IDamageable>();
                if (damageable != null && damageable.IsDamageable() &&
                    (damageable.NetworkObjectId == preferredTargetNetworkId || foundFoe == null))
                {
                    foundFoe = damageable;
                }
            }

            return foundFoe;
        }
    }
}