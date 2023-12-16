using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HullBreakerCompany.Hull;
using HullBreakerExpansion.Events;
using HullBreakerExpansion.Hull;
using Unity.Netcode;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;


namespace HullBreakerExpansion
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        Harmony _harmony = new("HULLBREAKER EXPANSION");
        private static bool _loaded = false;
        public static ManualLogSource Mls;

        public static Dictionary<SelectableLevel, float> Radiation = new();

        public static List<HullEvent> EventDictionary = new()
        {
            { new RadiationCleanUpEvent() },
            { new MaskedImposterEvent() },
            { new CompanyGiftEvent() }
        };

        private void Awake()
        {
            Mls = BepInEx.Logging.Logger.CreateLogSource("HULLBREAKER EXPANSION " + PluginInfo.PLUGIN_VERSION);
            Mls.LogInfo("Expansion loaded");
            _harmony.PatchAll(typeof(Plugin));
            _harmony.PatchAll(typeof(HullTerminal));
            NetcodeWeaver();
            
            if (!_loaded) Initialize();
        }
        public void Start()
        {
            if (!_loaded) Initialize();
            
        }
        public void OnDestroy()
        {
            if (!_loaded) Initialize();
        }
        public void Initialize()
        {
            EventDictionary.ForEach(CustomEventLoader.AddEvent);
            
            _loaded = true;
        }
        
        static GameObject networkPrefab;
        
        //Netcode 
        [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), "Start")]
        public static void Init()
        {
            if (networkPrefab != null)
                return;
            
            var mainAssetBundle = AssetBundle.LoadFromMemory(Resource1.hullnetwork);
            networkPrefab = (GameObject)mainAssetBundle.LoadAsset("hullnetwork");
            networkPrefab.AddComponent<HullNetwork>();
        
            NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), "Start")]
        static void SpawnNetworkHandler()
        {
            if(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                var networkHandlerHost = Object.Instantiate(networkPrefab, Vector3.zero, Quaternion.identity);
                networkHandlerHost.GetComponent<NetworkObject>().Spawn();
            }
        }
        //Level load
        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.LoadNewLevel))]
        [HarmonyPrefix]
        private static bool ModifiedLoad(ref SelectableLevel newLevel)
        {
            if (!RoundManager.Instance.IsHost) return true;

            if (newLevel.levelID == 3)
            {
                Radiation.Clear();
                return true;
            }
            
            HullRadiation.ResetDebuff();

            if (Radiation.ContainsKey(newLevel))
            {
                Radiation[newLevel] += Random.Range(10f, 35f);
            }
            else
            {
                Radiation[newLevel] = Random.Range(10f, 25f);;
            }

            HullNetwork.Instance.ShowRadiationLevel(Radiation[newLevel], HullRadiation.CalculateMultiplier(Radiation[newLevel]));
            HullRadiation.IncreaseValuesBasedOnRadiation(newLevel);
            HullNetwork.Instance.SyncMessage(HullBreakerCompany.Plugin.CurrentMessage);
            return true;
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.FirePlayersAfterDeadlineClientRpc))]
        private static void FirePlayer()
        {
            Radiation.Clear();
        }
        
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.StartHost))]
        static void RadsReset()
        {
            Radiation.Clear();
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(LungProp), "DisconnectFromMachinery")]
        private static void RadiationIncrease()
        {
            if (!RoundManager.Instance.IsHost) return;
            if (Radiation.ContainsKey(RoundManager.Instance.currentLevel))
            {
                Radiation[RoundManager.Instance.currentLevel] += 70f;
            }
            else
            {
                Radiation[RoundManager.Instance.currentLevel] = 70f;
            }

            HullNetwork.Instance.ShowRadiationLevel(Radiation[RoundManager.Instance.currentLevel], HullRadiation.CalculateMultiplier(Radiation[RoundManager.Instance.currentLevel]));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TimeOfDay), "OnHourChanged")]
        private static void RadiationOnHour()
        {
            if (!RoundManager.Instance.IsHost) return;
            HullRadiation.SetDebuff(Radiation[RoundManager.Instance.currentLevel]);
            if (Random.Range(0, 2) == 0) return;
            if (Radiation.ContainsKey(RoundManager.Instance.currentLevel))
            {
                Radiation[RoundManager.Instance.currentLevel] += 2f;
            }
            else
            {
                Radiation[RoundManager.Instance.currentLevel] = 2f;
            }
            
            HullNetwork.Instance.ShowRadiationLevel(Radiation[RoundManager.Instance.currentLevel], HullRadiation.CalculateMultiplier(Radiation[RoundManager.Instance.currentLevel]));
        }

        private static void NetcodeWeaver()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }
    }
}