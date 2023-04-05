using System;
using Unity.Netcode;
using UnityEngine;

namespace Script.Game.Actions.Input
{
    public struct ActionRequestData : INetworkSerializable
    {
        public ActionID ActionID;
        public Vector3 Position;
        public Vector3 Direction;
        public ulong[] TargetIDs;
        public float Amount;
        public bool ShouldQueue;
        public bool ShouldClose;
        public bool CancelMovement;

        [Flags]
        public enum PackFlags
        {
            None = 0,
            HasPosition = 1,
            HasDirection = 1 << 1,
            HasTargetIds = 1 << 2,
            HasAmount = 1 << 3,
            ShouldQueue = 1 << 4,
            ShouldClose = 1 << 5,
            CancelMovement = 1 << 6,
            //currently serialized with a byte. Change Read/Write if you add more than 8 fields.
        }
        
        public static ActionRequestData Create(Action action) =>
            new()
            {
                ActionID = action.ActionID
            };
        
        /// <summary>
        /// Returns true if the ActionRequestDatas are "functionally equivalent" (not including their Queueing or Closing properties).
        /// </summary>
        public bool Compare(ref ActionRequestData rhs)
        {
            bool scalarParamsEqual = (ActionID, Position, Direction, Amount) == (rhs.ActionID, rhs.Position, rhs.Direction, rhs.Amount);
            if (!scalarParamsEqual) { return false; }

            if (TargetIDs == rhs.TargetIDs) { return true; } //covers case of both being null.
            if (TargetIDs == null || rhs.TargetIDs == null || TargetIDs.Length != rhs.TargetIDs.Length) { return false; }
            for (int i = 0; i < TargetIDs.Length; i++)
            {
                if (TargetIDs[i] != rhs.TargetIDs[i]) { return false; }
            }

            return true;
        }
        
        private PackFlags GetPackFlags()
        {
            PackFlags flags = PackFlags.None;
            if (Position != Vector3.zero) { flags |= PackFlags.HasPosition; }
            if (Direction != Vector3.zero) { flags |= PackFlags.HasDirection; }
            if (TargetIDs != null) { flags |= PackFlags.HasTargetIds; }
            if (Amount != 0) { flags |= PackFlags.HasAmount; }
            if (ShouldQueue) { flags |= PackFlags.ShouldQueue; }
            if (ShouldClose) { flags |= PackFlags.ShouldClose; }
            if (CancelMovement) { flags |= PackFlags.CancelMovement; }
            
            return flags;
        }
            
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            PackFlags flags = PackFlags.None;
            if (!serializer.IsReader)
            {
                flags = GetPackFlags();
            }
            
            serializer.SerializeValue(ref ActionID);
            serializer.SerializeValue(ref flags);

            if (serializer.IsReader)
            {
                ShouldQueue = (flags & PackFlags.ShouldQueue) != 0;
                ShouldClose = (flags & PackFlags.ShouldClose) != 0;
                CancelMovement = (flags & PackFlags.CancelMovement) != 0;
            }
            
            if ((flags & PackFlags.HasPosition) != 0)
            {
                serializer.SerializeValue(ref Position);
            }
            if ((flags & PackFlags.HasDirection) != 0)
            {
                serializer.SerializeValue(ref Direction);
            }
            if ((flags & PackFlags.HasTargetIds) != 0)
            {
                serializer.SerializeValue(ref TargetIDs);
            }
            if ((flags & PackFlags.HasAmount) != 0)
            {
                serializer.SerializeValue(ref Amount);
            }
        }
    }
}