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
            // { new RadiationCleanUpEvent() },
            { new MaskedImposterEvent() }
        };

        private void Awake()
        {
            Mls = BepInEx.Logging.Logger.CreateLogSource("HULLBREAKER EXPANSION " + PluginInfo.PLUGIN_VERSION);
            Mls.LogInfo("Expansion loaded");
            _harmony.PatchAll(typeof(Plugin));

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
            var hullNetwork = new GameObject("HullNetwork");
            Mls.LogInfo("HullNetwork created");
            hullNetwork.AddComponent<HullNetwork>();
            hullNetwork.AddComponent<NetworkObject>();
            
            _loaded = true;
        }
        
        static GameObject networkPrefab;
        
        //Netcode 
        [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.StartHost))]
        public static void Init()
        {
            if (networkPrefab != null)
                return;
            
            var mainAssetBundle = AssetBundle.LoadFromMemory(Resource1.hullnetwork);
            networkPrefab = (GameObject)mainAssetBundle.LoadAsset("hullnetwork");
            networkPrefab.AddComponent<HullNetwork>();
        
            NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.StartGame))]
        static void SpawnNetworkHandler()
        {
            Mls.LogInfo("cringe moment");
            if(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                Mls.LogInfo("bruh");
                var networkHandlerHost = Object.Instantiate(networkPrefab, Vector3.zero, Quaternion.identity);
                networkHandlerHost.GetComponent<NetworkObject>().Spawn();
            }
        }
        
        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.LoadNewLevel))]
        [HarmonyPrefix]
        private static bool ModifiedLoad(ref SelectableLevel newLevel)
        {
            Mls.LogInfo("lvel loaded");
            if (!RoundManager.Instance.IsHost) return true;

            if (newLevel.levelID == 3)
            {
                Radiation.Clear();
                return true;
            }

            if (Radiation.ContainsKey(newLevel))
            {
                Radiation[newLevel] += Random.Range(10f, 35f);
            }
            else
            {
                Mls.LogInfo("radiation added to " + newLevel);
                Radiation[newLevel] = Random.Range(10f, 25f);;
            }

            HullNetwork.Instance.ShowRadiationLevel(Radiation[newLevel], CalculateMultiplier(Radiation[newLevel]));
            IncreaseValuesBasedOnRadiation(newLevel);

            return true;
        }
        
        private static void IncreaseValuesBasedOnRadiation(SelectableLevel newLevel)
        {
            if (!Radiation.ContainsKey(newLevel)) return;

            float radiation = Radiation[newLevel];
            float multiplier = CalculateMultiplier(radiation);

            Mls.LogInfo("Old enemySpawnChanceThroughoutDay: " + newLevel.enemySpawnChanceThroughoutDay);
            newLevel.enemySpawnChanceThroughoutDay = MultiplyAnimationCurve(newLevel.enemySpawnChanceThroughoutDay, multiplier);
            Mls.LogInfo("New enemySpawnChanceThroughoutDay: " + newLevel.enemySpawnChanceThroughoutDay);

            Mls.LogInfo("Old outsideEnemySpawnChanceThroughDay: " + newLevel.outsideEnemySpawnChanceThroughDay);
            newLevel.outsideEnemySpawnChanceThroughDay = MultiplyAnimationCurve(newLevel.outsideEnemySpawnChanceThroughDay, multiplier);
            Mls.LogInfo("New outsideEnemySpawnChanceThroughDay: " + newLevel.outsideEnemySpawnChanceThroughDay);

            Mls.LogInfo("Old maxEnemyPowerCount: " + newLevel.maxEnemyPowerCount);
            newLevel.maxEnemyPowerCount = (int)(newLevel.maxEnemyPowerCount * multiplier);
            Mls.LogInfo("New maxEnemyPowerCount: " + newLevel.maxEnemyPowerCount);
        }

        private static AnimationCurve MultiplyAnimationCurve(AnimationCurve curve, float multiplier)
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

            HullNetwork.Instance.ShowRadiationLevel(Radiation[RoundManager.Instance.currentLevel], CalculateMultiplier(Radiation[RoundManager.Instance.currentLevel]));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TimeOfDay), "OnHourChanged")]
        private static void RadiationOnHour()
        {
            if (!RoundManager.Instance.IsHost) return;
            if (Radiation.ContainsKey(RoundManager.Instance.currentLevel))
            {
                Radiation[RoundManager.Instance.currentLevel] += 0.5f;
            }
            else
            {
                Radiation[RoundManager.Instance.currentLevel] = 0.5f;
            }

            HullNetwork.Instance.ShowRadiationLevel(Radiation[RoundManager.Instance.currentLevel], CalculateMultiplier(Radiation[RoundManager.Instance.currentLevel]));
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