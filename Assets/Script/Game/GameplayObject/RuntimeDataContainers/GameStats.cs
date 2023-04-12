using System;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Game.GameplayObject.RuntimeDataContainers
{
    public class GameStats : MonoBehaviour
    {
        public static GameStats Instance { get; private set; }
    }
}