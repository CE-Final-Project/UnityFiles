using System.Collections;
using System.Collections.Generic;
using Script.Game.GameplayObject;
using Script.Game.GameplayObject.Character;
using UnityEngine;

namespace Script.Game.Action
{
    [CreateAssetMenu(menuName = "Actions/Attack Action")]
    public partial class AttackAction : Action
    {
        private bool m_ExecutionFired;
        private ulong m_ProvisionalTarget;

        public override bool OnStart(ServerCharacter serverCharacter)
        {
            ulong target = (Data.TargetIDs != null & Data.TargetIDs.Length > 0) ? Data.TargetIDs[0] : serverCharacter.TargetId.Value;
            IDamageable foe = DetectFoe(serverCharacter, target);
            if (foe != null)
            {
                m_ProvisionalTarget = foe.NetworkObjectId;
                Data.TargetIDs = new ulong[] { foe.NetworkObjectId };
            }

            if (Data.Direction != Vector3.zero)
            {
                serverCharacter.physicsWrapper.Transform.forward = Data.Direction;
            }

            serverCharacter.serverAnimationHandler.NetworkAnimator.SetTrigger(Config.Anim);
            serverCharacter.ClientCharacter.RecvDoActionClientRPC(Data);
            return true;
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

        private IDamageable DetectFoe(ServerCharacter parent, ulong foeHint = 0)
        {
            return GetIdealMeleeFoe(Config.IsFriendly ^ parent.IsNpc, parent.physicsWrapper.DamageCollider, Config.Range, foeHint);
        }

        public static IDamageable GetIdealMeleeFoe(bool isNPC, Collider2D ourCollider, float meleeRange, ulong preferredTargetNetworkId)
        {
            RaycastHit[] results;
            int numResults = ActionUtils.DetectMeleeFoe(isNPC, ourCollider, meleeRange, out results);

            IDamageable foundFoe = null;
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