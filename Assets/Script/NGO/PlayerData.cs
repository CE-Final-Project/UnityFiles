using Script.Game.GameplayObject.Character;
using Unity.Netcode;

namespace Script.NGO
{
    public class PlayerData : INetworkSerializable
    {
        public ulong Id;
        public string Name;
        public float Health;
        public CharacterTypeEnum CharacterTypeEnum;
        public int Score;

        public PlayerData(string name, ulong id, float health, CharacterTypeEnum characterTypeEnum, int score = 0)
        {
            Id = id;
            Name = name;
            Health = health;
            CharacterTypeEnum = characterTypeEnum;
            Score = score;
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Id);
            serializer.SerializeValue(ref Name);
            serializer.SerializeValue(ref Health);
            serializer.SerializeValue(ref CharacterTypeEnum);
            serializer.SerializeValue(ref Score);   
        }
    }
}