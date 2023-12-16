
using System;
using System.Collections.Generic;
using HullBreakerCompany.Hull;

namespace HullBreakerExpansion.Events
{
    public class RadiationCleanUpEvent : HullEvent
    {
        public override string ID() => "RadiationCleanUp";
        public override int GetWeight() => 15;
        public override string GetDescription() => "Radiation levels are reduced by 99%";
        public override string GetMessage() => "<color=white>The company sent a couple of satellites that reduced radiation</color>";
        public override string GetShortMessage() => "<color=white>RAD CLEAN UP</color>";
        
        public override void Execute(SelectableLevel level, Dictionary<Type, int> enemyComponentRarity,
            Dictionary<Type, int> outsideComponentRarity)
        {
            if (!Plugin.Radiation.ContainsKey(level)) return;
            var currentLevel = Plugin.Radiation[level];
            Plugin.Radiation[level] = currentLevel * 0.090f;
        }
    }
}

