using System;
using Survival.Game.Gameplay.GameplayObjects.Character;
using Survival.Game.Infrastructure;
using UnityEngine;

namespace Survival.Game.Gameplay.GameplayObjects
{
    /// <summary>
    /// A runtime list of <see cref="PersistentPlayer"/> objects that is populated both on clients and server.
    /// </summary>
    [CreateAssetMenu]
    public class ClientPlayerAvatarRuntimeCollection : RuntimeCollection<ClientPlayerAvatar>
    {
    }
}
