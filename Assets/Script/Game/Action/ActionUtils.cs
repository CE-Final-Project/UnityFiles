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