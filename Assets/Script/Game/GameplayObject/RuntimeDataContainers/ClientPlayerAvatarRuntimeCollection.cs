using Script.Game.GameplayObject.Character;
using Script.Utils;
using UnityEngine;

namespace Script.Game.GameplayObject.RuntimeDataContainers
{
    /// <summary>
    /// A runtime list of <see cref="PersistentPlayer"/> objects that is populated both on clients and server.
    /// </summary>
    [CreateAssetMenu]
    public class ClientPlayerAvatarRuntimeCollection : RuntimeCollection<ClientPlayerAvatar>
    {
    }
}
