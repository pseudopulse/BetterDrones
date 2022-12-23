using BepInEx;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Reflection;
using System;
using BetterDrones;
using BepInEx.Configuration;
using RoR2.CharacterAI;
using EntityStates.Drone.DroneWeapon;

namespace BetterDrones.DroneTweaks {
    public class GunnerTurret {
        // paths
        private static string GunnerTurretMasterPath = "RoR2/Base/Drones/Turret1Master.prefab";
        private static string GunnerTurretBodyPath = "RoR2/Base/Drones/Turret1Body.prefab";
        // misc config vars
        private static bool GunnerTurretEnabled = Main.config.Bind<bool>("Gunner Turret", "Enable Changes", true, "Should changes to the Gunner Turret be enabled?").Value;
        // ai config vars
        private static float GunnerTurretChaseDistance = Main.config.Bind<float>("Gunner Turret - AI", "Maximum Distance", 175, "The maximum distance in which a Gunner Turret will attempt to target, vanilla is 60.").Value;
        // body config vars
        private static float GunnerTurretBaseHealth = Main.config.Bind<float>("Gunner Turret - Stats", "Base Health", 310, "The base health of a Gunner Turret, vanilla is 200.").Value;
        private static float GunnerTurretBaseRegen = Main.config.Bind<float>("Gunner Turret - Stats", "Base Regen", 12f, "The base regen of a Gunner Turret, vanilla is 7.5.").Value;
        private static float GunnerTurretBaseCrit = Main.config.Bind<float>("Gunner Turret - Stats", "Base Critical Chance", 10f, "The base critical chance of a Gunner Turret, vanilla is 0.").Value;
        private static float GunnerTurretBaseDamage = Main.config.Bind<float>("Gunner Turret - Stats", "Base Damage", 18f, "The base damage of a Gunner Turret, vanilla is 18.").Value;

        private static void TweakAI() {
            GameObject master = Addressables.LoadAssetAsync<GameObject>(GunnerTurretMasterPath).WaitForCompletion();

            // tweak skill drivers
            foreach (AISkillDriver driver in master.GetComponents<AISkillDriver>()) {
                if (driver.maxDistance == 60) {
                    driver.maxDistance = GunnerTurretChaseDistance;
                }
            }
        }

        private static void TweakBody() {
            GameObject prefab = Addressables.LoadAssetAsync<GameObject>(GunnerTurretBodyPath).WaitForCompletion();

            // tweak stats
            CharacterBody body = prefab.GetComponent<CharacterBody>();
            body.baseMaxHealth = GunnerTurretBaseHealth;
            body.baseCrit = GunnerTurretBaseCrit;
            body.baseRegen = GunnerTurretBaseRegen;
            body.baseDamage = GunnerTurretBaseDamage;
        }

        public static void EnableChanges() {
            if (GunnerTurretEnabled) {
                TweakAI();
                TweakBody();
            }
        }
    }
}