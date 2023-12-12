using System;
using System.Collections.Generic;
using HullBreakerCompany.Hull;

namespace HullBreakerExpansion.Events
{
    public class WhereIAmEvent : HullEvent
    {
        public override string ID() => "WhereIAm";
        public override int GetWeight() => 5;
        public override string GetDescription() => "Teleportation upon landing";
        public override string GetMessage() => "<color=orange>Spontaneous teleportation upon landing</color>";
        public override string GetShortMessage() => "<color=orange>WHERE I AM?</color>";

        public override void Execute(SelectableLevel level, Dictionary<Type, int> enemyComponentRarity,
            Dictionary<Type, int> outsideComponentRarity)
        {
            //TODO
            
            HullManager.SendChatEventMessage(this);
        }
    }
}