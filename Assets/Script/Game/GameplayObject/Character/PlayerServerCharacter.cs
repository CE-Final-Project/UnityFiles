using System.Collections.Generic;
using Script.ConnectionManagement;
using Unity.Multiplayer.Samples.BossRoom;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Vector3 = UnityEngine.Vector3;

namespace Script.Game.GameplayObject.Character
{
    [RequireComponent(typeof(ServerCharacter))]
    public class PlayerServerCharacter : NetworkBehaviour
    {
        private static readonly List<ServerCharacter> ActivePlayers = new List<ServerCharacter>();

        [SerializeField] private ServerCharacter cachedServerCharacter;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                ActivePlayers.Add(cachedServerCharacter);
            }
            else
            {
                enabled = false;
            }
        }

        private void OnDisable()
        {
            ActivePlayers.Remove(cachedServerCharacter);
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                Transform movementTransform = cachedServerCharacter.transform;
                SessionPlayerData? sessionPlayerData =
                    SessionManager<SessionPlayerData>.Instance.GetPlayerData(OwnerClientId);
                if (sessionPlayerData.HasValue)
                {
                    SessionPlayerData playerData = sessionPlayerData.Value;
                    Vector3 position = movementTransform.position;
                    playerData.PlayerPosition = new Vector2(position.x, position.y);
                    playerData.CurrentHitPoints = cachedServerCharacter.HitPoints;
                    playerData.HasCharacterSpawned = true;
                    SessionManager<SessionPlayerData>.Instance.SetPlayerData(OwnerClientId, playerData);
                }
            }
        }

        /// <summary>
        /// Returns a list of all active players' ServerCharacters. Treat the list as read-only!
        /// The list will be empty on the client.
        /// </summary>
        public static List<ServerCharacter> GetPlayerServerCharacters()
        {
            return ActivePlayers;
        }

        /// <summary>
        /// Returns the ServerCharacter owned by a specific client. Always returns null on the client.
        /// </summary>
        /// <param name="ownerClientId"></param>
        /// <returns>The ServerCharacter owned by the client, or null if no ServerCharacter is found</returns>
        public static ServerCharacter GetPlayerServerCharacter(ulong ownerClientId)
        {
            foreach (ServerCharacter playerServerCharacter in ActivePlayers)
            {
                if (playerServerCharacter.OwnerClientId == ownerClientId)
                {
                    return playerServerCharacter;
                }
            }
            return null;
        }
    }
}