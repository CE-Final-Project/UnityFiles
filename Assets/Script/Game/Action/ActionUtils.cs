using System.Collections.Generic;
using UnityEngine;

namespace Script.Game.Action
{
    public static class ActionUtils
    {
        private static RaycastHit[] _raycastHits = new RaycastHit[4];
        private static int _PCLayer = -1;
        private static int _npcLayer = -1;
        private static int _environmentLayer = -1;

        public static int DetectMeleeFoe(bool isNPC, Collider2D attacker, float range, out RaycastHit[] results)
        {
            return DetectNearbyEntities(isNPC, !isNPC, attacker, range, out results);
        }
        public static int DetectNearbyEntities(bool wantPcs, bool wantNpcs, Collider2D attacker, float range, out RaycastHit[] results)
        {

            var myBounds = attacker.bounds;

            if (_PCLayer == -1)
                _PCLayer = LayerMask.NameToLayer("PCs");
            if (_npcLayer == -1)
                _npcLayer = LayerMask.NameToLayer("NPCs");

            int mask = 0;
            if (wantPcs)
                mask |= (1 << _PCLayer);
            if (wantNpcs)
                mask |= (1 << _npcLayer);

            int numResults = Physics.BoxCastNonAlloc(attacker.transform.position, myBounds.extents,
                attacker.transform.forward, _raycastHits, Quaternion.identity, range, mask);

            results = _raycastHits;
            return numResults;
        }
    }

    public static class ActionConclusion
    {
        public const bool Stop = false;
        public const bool Continue = true;
    }
    
    public class RaycastHitComparer : IComparer<RaycastHit>
    {
        public int Compare(RaycastHit x, RaycastHit y)
        {
            return x.distance.CompareTo(y.distance);
        }
    }
}