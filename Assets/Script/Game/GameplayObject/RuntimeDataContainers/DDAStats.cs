using UnityEngine;

namespace Script.Game.GameplayObject.RuntimeDataContainers
{
    public class DynamicDiffStat
    {
        public float KillPerMin { get; private set; }
        public float DamageDonePerMin { get; private set; }
        public float DamageTakenPerMin { get; private set; }
        public float HealingTakenPerMin { get; private set; }
        public float KkpmAvgValue { get; private set; }
        public float KDmdAvgValue { get; private set; }
        public float KDmtAvgValue { get; private set; }
        public float KHtAvgValue { get; private set; }
        
        public DynamicDiffStat()
        {
            KillPerMin = 0;
            DamageDonePerMin = 0;
            DamageTakenPerMin = 0;
            HealingTakenPerMin = 0;
            KkpmAvgValue = 0;
            KDmdAvgValue = 0;
            KDmtAvgValue = 0;
            KHtAvgValue = 0;
        }
        
        public DynamicDiffStat(
            float killPerMin,
            float damageDonePerMin,
            float damageTakenPerMin,
            float healingTakenPerMin,
            float kkpmAvgValue,
            float kDmdAvgValue,
            float kDmtAvgValue,
            float kHtAvgValue)
        {
            KillPerMin = killPerMin;
            DamageDonePerMin = damageDonePerMin;
            DamageTakenPerMin = damageTakenPerMin;
            HealingTakenPerMin = healingTakenPerMin;
            KkpmAvgValue = kkpmAvgValue;
            KDmdAvgValue = kDmdAvgValue;
            KDmtAvgValue = kDmtAvgValue;
            KHtAvgValue = kHtAvgValue;
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
        
        public void SetHealingTakenPerMin(float healingTakenPerMin)
        {
            HealingTakenPerMin = healingTakenPerMin;
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
        
        public void SetKHtAvgValue(float kHtAvgValue)
        {
            KHtAvgValue = kHtAvgValue;
        }
        
        public string GetDynamicDiffStat()
        {
            return $"KPM: {KillPerMin}\nDDPM: {DamageDonePerMin}\nDTPM: {DamageTakenPerMin}\nHTPM: {HealingTakenPerMin}\nKKPM: {KkpmAvgValue}\nKDMD: {KDmdAvgValue}\nKDMT: {KDmtAvgValue}\nKHT: {KHtAvgValue}";
        }
    }
}