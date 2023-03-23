using Script.Game.GameplayObject.Character;
using Script.Utils;
using UnityEngine;

namespace Script.Configuration
{
    [CreateAssetMenu(menuName = "GameData/CharacterClass", order = 1)]
    public class CharacterClass : ScriptableObject
    {
        public CharacterTypeEnum CharacterType;
        public IntVariable BaseHP;
        public int BaseMana;
        public float Speed;
        public bool IsNpc;
        public string DisplayedName;
        public Sprite ClassBannerLit;
        public Sprite ClassBannerUnlit;
    }
}