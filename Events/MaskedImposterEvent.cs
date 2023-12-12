using System;
using System.Collections.Generic;
using HullBreakerCompany.Hull;

namespace HullBreakerExpansion.Events
{
    public class MaskedImposterEvent : HullEvent
    {
        public override string ID() => "MaskedImposter";
        public override int GetWeight() => 20;
        public override string GetDescription() => "Increased chance of Masked Player";
        public override string GetMessage() => "<color=orange>Masked imposters near, caution</color>";
        public override string GetShortMessage() => "<color=white>IMPOSTERS</color>";

        public override void Execute(SelectableLevel level, Dictionary<Type, int> enemyComponentRarity,
            Dictionary<Type, int> outsideComponentRarity)
        {
            foreach (var unit in level.Enemies)
            {
                if (unit.enemyType.enemyPrefab.GetComponent<MaskedPlayerEnemy>() != null)
                {
                    enemyComponentRarity.Add(typeof(MaskedPlayerEnemy), 256);
                    HullManager.SendChatEventMessage(this);
                    break;
                }
            }
        }
    }
}