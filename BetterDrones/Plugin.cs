using BepInEx;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Reflection;
using BepInEx.Configuration;
using BetterDrones.DroneTweaks;

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
        public void Awake() {
            // set logger and config
            ModLogger = Logger;
            config = Config;

            MechanicalAllyOrbitEnabled = config.Bind<bool>("Global", "RoR1 Drone Movement", true, "Should aerial mechanical allies orbit you like in Risk of Rain 1? This disables AI movement changes.").Value;
            MechanicalAllyOrbitDistance = config.Bind<float>("Global", "Drone Orbit Distance", 3.5f, "The distance aerial mechanical allies should orbit you from.").Value;
            MechanicalAllyOrbitSpeed = config.Bind<float>("Global", "Drone Orbit Speed", 7f, "The speed in seconds in which it should take for an aerial mechanical ally to make a full rotation around you.").Value;

            PingControlEnabled = config.Bind<bool>("Global", "Ping Control", true, "Should mechanical allies target your most recent ping?").Value;

            if (MechanicalAllyOrbitEnabled) {
                OrbitalMovement.EnableOrbitalMovement();
            }

            if (PingControlEnabled) {
                PingControl.EnablePingControls();
            }

            GunnerDrone.EnableChanges();
            MissileDrone.EnableChanges();
            HealingDrone.EnableChanges();
            IncineratorDrone.EnableChanges();
        }
    }
}