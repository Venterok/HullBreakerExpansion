

using UnityEngine;

namespace HullBreakerExpansion.Hull;

public static class HullRadiation
{
    public static bool DamageOnTheFactory = false;
    public static bool BlurPlayerScreenOnTheFactory = false;
    
    public static float GetRadiationLevel(SelectableLevel level)
    {
        if (!Plugin.Radiation.ContainsKey(level)) return 0;
        return Plugin.Radiation[level];
        
    }
    
    public static void SetRadiationLevel(SelectableLevel level, float radiationLevel)
    {
        if (!Plugin.Radiation.ContainsKey(level)) return;
        Plugin.Radiation[level] = radiationLevel;
    }

    public static void IncreaseValuesBasedOnRadiation(SelectableLevel level)
    {
        var Radiation = Plugin.Radiation;
        if (!Radiation.ContainsKey(level)) return;

        float radiation = Radiation[level];
        float multiplier = CalculateMultiplier(radiation);
            
        level.enemySpawnChanceThroughoutDay = MultiplyAnimationCurve(level.enemySpawnChanceThroughoutDay, multiplier);
        level.outsideEnemySpawnChanceThroughDay = MultiplyAnimationCurve(level.outsideEnemySpawnChanceThroughDay, multiplier);
        level.maxEnemyPowerCount = (int)(level.maxEnemyPowerCount * multiplier); 
        
        if (Radiation[level] > 80)
        {
            for (int i = 0; i < Radiation[level]*0.05; i++)
            {
                RoundManager.Instance.SpawnEnemiesOutside();
            }
        }
    }
    
    public static AnimationCurve MultiplyAnimationCurve(AnimationCurve curve, float multiplier)
    {
        for (int i = 0; i < curve.keys.Length; i++)
        {
            Keyframe key = curve.keys[i];
            key.value *= multiplier;
            curve.MoveKey(i, key);
        }

        return curve;
    }

    public static float CalculateMultiplier(float radiation)
    {
        return 1 + radiation / 50;
    }
    
    public static void ResetDebuff()
    {
        DamageOnTheFactory = false;
        BlurPlayerScreenOnTheFactory = false;
    }
    
    public static void SetDebuff(float rad)
    {
        if (rad > 80)
        {
            BlurPlayerScreenOnTheFactory = true;
        }
        else if (rad > 100)
        {
            DamageOnTheFactory = true;
            for (int i = 0; i < 1; i++)
            {
                RoundManager.Instance.SpawnEnemiesOutside();
            }
        }
    }
}