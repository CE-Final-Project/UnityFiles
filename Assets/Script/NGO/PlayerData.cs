using Script.Game;
using Unity.Netcode;

namespace Script.NGO
{
    public class PlayerData : INetworkSerializable
    {
        public ulong Id;
        public string Name;
        public float Health;
        public CharacterType CharacterType;
        public int Score;
        
        public PlayerData()
        {
        }
        
        public PlayerData(string name, ulong id, float health, CharacterType characterType, int score = 0)
        {
            Id = id;
            Name = name;
            Health = health;
            CharacterType = characterType;
            Score = score;
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Id);
            serializer.SerializeValue(ref Name);
            serializer.SerializeValue(ref Health);
            serializer.SerializeValue(ref CharacterType);
            serializer.SerializeValue(ref Score);   
        }
    }
}