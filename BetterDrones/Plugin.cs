using BepInEx;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Reflection;
using BepInEx.Configuration;
using BetterDrones.DroneTweaks;
using RoR2.CharacterAI;
using UnityEngine.Networking;

namespace BetterDrones {
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "pseudopulse";
        public const string PluginName = "BetterDrones";
        public const string PluginVersion = "1.0.0";
        public static BepInEx.Logging.ManualLogSource ModLogger;
        public static ConfigFile config;

        // global config vars
        public static bool MechanicalAllyOrbitEnabled;
        public static float MechanicalAllyOrbitDistance;
        public static float MechanicalAllyOrbitSpeed;
        public static bool PingControlEnabled;
        public static bool PerfectAimEnabled;
        public void Awake() {
            // set logger and config
            ModLogger = Logger;
            config = Config;

            MechanicalAllyOrbitEnabled = config.Bind<bool>("Global", "RoR1 Drone Movement", true, "Should aerial mechanical allies orbit you like in Risk of Rain 1? This disables AI movement changes.").Value;
            MechanicalAllyOrbitDistance = config.Bind<float>("Global", "Drone Orbit Distance", 4.5f, "The distance aerial mechanical allies should orbit you from.").Value;
            MechanicalAllyOrbitSpeed = config.Bind<float>("Global", "Drone Orbit Speed", 7f, "The speed in seconds in which it should take for an aerial mechanical ally to make a full rotation around you.").Value;

            PingControlEnabled = config.Bind<bool>("Global", "Ping Control", true, "Should mechanical allies target your most recent ping?").Value;

            PerfectAimEnabled = config.Bind<bool>("Global", "Perfect Aim", true, "Should mechanical allies have perfect aim?").Value;

            if (MechanicalAllyOrbitEnabled) {
                OrbitalMovement.EnableOrbitalMovement();
            }

            if (PingControlEnabled) {
                PingControl.EnablePingControls();
            }

            if (PerfectAimEnabled) {
                On.RoR2.CharacterAI.BaseAI.FixedUpdate += OverrideInputsPerfectAim;
            }

            GunnerDrone.EnableChanges();
            MissileDrone.EnableChanges();
            HealingDrone.EnableChanges();
            IncineratorDrone.EnableChanges();
        }

        private static void OverrideInputsPerfectAim(On.RoR2.CharacterAI.BaseAI.orig_FixedUpdate orig, BaseAI self) {
            orig(self);
            if (NetworkServer.active) {
                if (self.body && self.body.gameObject && self.body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical)) {
                    if (self.currentEnemy.GetBullseyePosition(out Vector3 pos)) {
                        self.bodyInputBank.aimDirection = (pos - self.bodyInputBank.aimOrigin).normalized;
                    }
                }
            }
        }
    }
}