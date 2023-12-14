using Unity.Netcode;

namespace HullBreakerExpansion.Hull;

public class HullNetwork : NetworkBehaviour
{
    public static HullNetwork Instance { get; private set; }

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

    public void ShowRadiationLevel(float radiationLevel, float radiationMultiplier)
    {
        Plugin.Mls.LogInfo("Radiation level Local: " + radiationLevel + " mSv");
        ShowRadiationLevelServerRPC(radiationLevel, radiationMultiplier);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShowRadiationLevelServerRPC(float radiationLevel, float radiationMultiplier)
    {
        Plugin.Mls.LogInfo("Radiation level Server RPC: " + radiationLevel + " mSv");
        ShowRadiationLevelClientRPC(radiationLevel, radiationMultiplier);
    }

    [ClientRpc]
    private void ShowRadiationLevelClientRPC(float radiationLevel, float radiationMultiplier)
    {
        Plugin.Mls.LogInfo("Radiation level Cleint RPC: " + radiationLevel + " mSv");
        HUDManager.Instance.DisplayTip("<color=white>MOON INQUIRY</color>",  "<color=white>Radiation level: " + radiationLevel + " mSv </color>" + "<color=white>   Radiation Multiplier: " + radiationMultiplier + "% </color>", true);
    }
}