using UnityEngine;

namespace Script.Game.GameplayObject.RuntimeDataContainers
{
    public class DynamicDiffStat
    {
        public float KillPerMin { get; private set; }
        public float DamageDonePerMin { get; private set; }
        public float DamageTakenPerMin { get; private set; }
        public float KkpmAvgValue { get; private set; }
        public float KDmdAvgValue { get; private set; }
        public float KDmtAvgValue { get; private set; }
        
        
        public int SpawnCount { get; private set; }
        public float SpawnDelay { get; private set; }
        
        public DynamicDiffStat()
        {
            KillPerMin = 0;
            DamageDonePerMin = 0;
            DamageTakenPerMin = 0;
            KkpmAvgValue = 0;
            KDmdAvgValue = 0;
            KDmtAvgValue = 0;
            SpawnCount = 0;
            SpawnDelay = 0;
        }
        
        public DynamicDiffStat(
            float killPerMin,
            float damageDonePerMin,
            float damageTakenPerMin,
            float kkpmAvgValue,
            float kDmdAvgValue,
            float kDmtAvgValue,
            int spawnCount,
            float spawnDelay
            )
        {
            KillPerMin = killPerMin;
            DamageDonePerMin = damageDonePerMin;
            DamageTakenPerMin = damageTakenPerMin;
            KkpmAvgValue = kkpmAvgValue;
            KDmdAvgValue = kDmdAvgValue;
            KDmtAvgValue = kDmtAvgValue;
            SpawnCount = spawnCount;
            SpawnDelay = spawnDelay;
        }
        
        public void SetKillPerMin(float killPerMin)
        {
            KillPerMin = killPerMin;
        }
        
        public void SetDamageDonePerMin(float damageDonePerMin)
        {
            DamageDonePerMin = damageDonePerMin;
        }
        
        public void SetDamageTakenPerMin(float damageTakenPerMin)
        {
            DamageTakenPerMin = damageTakenPerMin;
        }

        public void SetKkpmAvgValue(float kkpmAvgValue)
        {
            KkpmAvgValue = kkpmAvgValue;
        }
        
        public void SetKDmdAvgValue(float kDmdAvgValue)
        {
            KDmdAvgValue = kDmdAvgValue;
        }
        
        public void SetKDmtAvgValue(float kDmtAvgValue)
        {
            KDmtAvgValue = kDmtAvgValue;
        }
        
        public void SetSpawnCount(int spawnCount)
        {
            SpawnCount = spawnCount;
        }
        
        public void SetSpawnDelay(float spawnDelay)
        {
            SpawnDelay = spawnDelay;
        }

        public string GetDynamicDiffStat()
        {
            return $"KPM: {KillPerMin}\n" +
                   $"DDPM: {DamageDonePerMin}\n" +
                   $"DTPM: {DamageTakenPerMin}\n" +
                   $"KKPM: {KkpmAvgValue}\n" +
                   $"KDMD: {KDmdAvgValue}\n" +
                   $"KDMT: {KDmtAvgValue}\n" +
                   $"SpawnCount: {SpawnCount}\n"+
                   $"SpawnDelay: {SpawnDelay}\n";
        }
    }
}