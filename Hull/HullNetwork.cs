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
        Plugin.Mls.LogInfo("Radiation level: " + radiationLevel + " mSv");
        ShowRadiationLevelServerRPC(radiationLevel, radiationMultiplier);
    }

    [ServerRpc]
    private void ShowRadiationLevelServerRPC(float radiationLevel, float radiationMultiplier)
    {
        Plugin.Mls.LogInfo("Radiation level: " + radiationLevel + " mSv");
        ShowRadiationLevelClientRPC(radiationLevel, radiationMultiplier);
    }

    [ClientRpc]
    private void ShowRadiationLevelClientRPC(float radiationLevel, float radiationMultiplier)
    {
        Plugin.Mls.LogInfo("Radiation level: " + radiationLevel + " mSv");
        HUDManager.Instance.DisplayTip("<color=white>MOON INQUIRY</color>",  "<color=white>Radiation level: " + radiationLevel + " mSv </color>" + "<color=white>   Radiation Multiplier: " + radiationMultiplier + "% </color>", true);
    }
}