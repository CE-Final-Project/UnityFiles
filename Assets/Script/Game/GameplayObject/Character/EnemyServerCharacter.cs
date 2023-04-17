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
    public class EnemyServerCharacter : NetworkBehaviour
    {
        private static readonly List<ServerCharacter> ActiveEnemies = new List<ServerCharacter>();

        [SerializeField] private ServerCharacter cachedServerCharacter;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                ActiveEnemies.Add(cachedServerCharacter);
            }
            else
            {
                enabled = false;
            }
        }

        private void OnDisable()
        {
            ActiveEnemies.Remove(cachedServerCharacter);
        }

        /// <summary>
        /// Returns a list of all active players' ServerCharacters. Treat the list as read-only!
        /// The list will be empty on the client.
        /// </summary>
        public static List<ServerCharacter> GetEnemyServerCharacters()
        {
            return ActiveEnemies;
        }

        /// <summary>
        /// Returns the ServerCharacter owned by a specific client. Always returns null on the client.
        /// </summary>
        /// <param name="ownerClientId"></param>
        /// <returns>The ServerCharacter owned by the client, or null if no ServerCharacter is found</returns>
        public static ServerCharacter GetEnemyServerCharacter(ulong ownerClientId)
        {
            foreach (ServerCharacter playerServerCharacter in ActiveEnemies)
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