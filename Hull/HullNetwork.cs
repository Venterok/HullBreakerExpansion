using System;
using System.Collections.Generic;
using HullBreakerCompany.Hull;
using Unity.Netcode;

namespace HullBreakerExpansion.Hull;

public class HullNetwork : NetworkBehaviour
{
    public static HullNetwork Instance { get; private set; }
    
    public static String CurrentMessage = "";

    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    //Show radiation level on client
    public void ShowRadiationLevel(float radiationLevel, float radiationMultiplier)
    {
        ShowRadiationLevelServerRPC(radiationLevel, radiationMultiplier);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShowRadiationLevelServerRPC(float radiationLevel, float radiationMultiplier)
    {
        ShowRadiationLevelClientRPC(radiationLevel, radiationMultiplier);
    }

    [ClientRpc]
    private void ShowRadiationLevelClientRPC(float radiationLevel, float radiationMultiplier)
    {
        Plugin.Mls.LogInfo("Radiation level Client RPC: " + radiationLevel + " mSv");
        HUDManager.Instance.DisplayTip("<color=white>MOON INQUIRY</color>",  "<color=white>Radiation level: " + radiationLevel + " mSv </color>" + "<color=white>   Radiation Multiplier: " + radiationMultiplier + "% </color>", true);
    }
    
    //Sync Events
    public void SyncMessage(string message)
    {
        SyncMessageServerRPC(message);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SyncMessageServerRPC(string message)
    {
        SyncMessageClientRPC(message);
    }

    [ClientRpc]
    private void SyncMessageClientRPC(string message)
    {
        Plugin.Mls.LogInfo("Syncing message");
        CurrentMessage = message;
    }
    
}