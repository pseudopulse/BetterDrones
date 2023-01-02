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
using EntityStates;

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
        // energy shield
        public static bool GunnerTurretShieldEnabled = Main.config.Bind<bool>("Gunner Turret - Energy Shield", "Enabled", true, "Should the Gunner Turret has a periodic energy shield after dealing enough damage?").Value;
        public static float GunnerTurretShieldDamage = Main.config.Bind<float>("Gunner Turret - Energy Shield", "Damage Required", 1000f, "The damage required to active a Gunner Turret's Energy Shield, in percentage of base damage.").Value;
        public static float GunnerTurretShieldDuration = Main.config.Bind<float>("Gunner Turret - Energy Shield", "Duration", 5f, "The duration of a Gunner Turret's Energy Shield.").Value;
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

            if (GunnerTurretShieldEnabled) {
                EntityStateMachine esm = prefab.AddComponent<EntityStateMachine>();
                SerializableEntityStateType shield = new SerializableEntityStateType(typeof(DroneStates.TurretShield));
                esm.initialStateType = shield;
                esm.mainStateType = shield;
                esm.customName = "Shield";

                NetworkStateMachine nsm = prefab.GetComponent<NetworkStateMachine>();
                nsm.stateMachines = prefab.GetComponents<EntityStateMachine>();
            }
        }

        public static void EnableChanges() {
            if (GunnerTurretEnabled) {
                TweakAI();
                TweakBody();
            }
        }
    }
}