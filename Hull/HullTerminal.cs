using HarmonyLib;
using HullBreakerExpansion.Hull;

[HarmonyPatch(typeof(Terminal))]
internal class HullTerminal
{
    [HarmonyPostfix]
    [HarmonyPatch("ParsePlayerSentence")]
    private static void Events(ref Terminal __instance, ref TerminalNode __result)
    {
        
        string text = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded);
        if (text.ToLower() != "events")
        {
            return;
        }
        
        TerminalNode tNode = new TerminalNode();
        var message = HullNetwork.CurrentMessage;
        tNode.displayText = message;
        tNode.clearPreviousText = true;
        __result = tNode;
    }
}