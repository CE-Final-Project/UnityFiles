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
        
        public DynamicDiffStat(float killPerMin, float damageDonePerMin, float damageTakenPerMin, float kkpmAvgValue, float kDmdAvgValue)
        {
            KillPerMin = killPerMin;
            DamageDonePerMin = damageDonePerMin;
            DamageTakenPerMin = damageTakenPerMin;
            KkpmAvgValue = kkpmAvgValue;
            KDmdAvgValue = kDmdAvgValue;
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
        
        public string GetDynamicDiffStat()
        {
            return $"KPM: {KillPerMin}\nDDPM: {DamageDonePerMin}\nDTPM: {DamageTakenPerMin}\nKKPM: {KkpmAvgValue}\nKDMD: {KDmdAvgValue}";
        }
    }
}