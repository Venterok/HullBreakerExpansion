using System;
using System.Collections.Generic;
using HullBreakerCompany.Hull;

namespace HullBreakerExpansion.Events;

public class CompanyGiftEvent : HullEvent
{
    public override string ID() => "CompanyGift";
    
    public override int GetWeight() => 20;
    
    public override string GetDescription() => "Free delivery";
    
    public override string GetMessage() => "<color=green>For a more enjoyable visit to the moon, equipment will be delivered to you</color>";
    
    public override string GetShortMessage() => "<color=white>COMPANY GIFT</color>";
    
    public override void Execute(SelectableLevel level, Dictionary<Type, int> enemyComponentRarity,
        Dictionary<Type, int> outsideComponentRarity)
    {
        var terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
        var range = UnityEngine.Random.Range(2, 9);

        for (var j = 0; j < range; j++)
        {
            var item = UnityEngine.Random.Range(0, 6);
            terminal.orderedItemsFromTerminal.Add(item);
        }
        
        HullManager.SendChatEventMessage(this);
    }
}