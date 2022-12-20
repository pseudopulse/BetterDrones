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
    public class MissileDrone {
        // paths
        private static string MissileDroneMasterPath = "RoR2/Base/Drones/DroneMissileMaster.prefab";
        private static string MissileDroneBodyPath = "RoR2/Base/Drones/MissileDroneBody.prefab";
        // misc config vars
        private static bool MissileDroneEnabled = Main.config.Bind<bool>("Missile Drone", "Enable Changes", true, "Should changes to the Missile Drone be enabled?").Value;
        // ai config vars
        private static float MissileDroneMinDistanceFromOwner = Main.config.Bind<float>("Missile Drone - AI", "Minimum Distance", 20, "The minimum distance a Missile Drone can be from you when it has no targets, vanilla is 60.").Value;
        private static float MissileDroneStrafeDistance = Main.config.Bind<float>("Missile Drone - AI", "Strafe Distance", 15, "The maximum distance in which a Missile Drone will attempt to strafe enemies, vanilla is 15.").Value;
        private static float MissileDroneChaseDistance = Main.config.Bind<float>("Missile Drone - AI", "Chase Distance", 175, "The maximum distance in which a Missile Drone will attempt to chase a target, vanilla is 45.").Value;
        // body config vars
        private static float MissileDroneBaseHealth = Main.config.Bind<float>("Missile Drone - Stats", "Base Health", 300, "The base health of a Missile Drone, vanilla is 225.").Value;
        private static float MissileDroneBaseRegen = Main.config.Bind<float>("Missile Drone - Stats", "Base Regen", 7.5f, "The base regen of a Missile Drone, vanilla is 5.").Value;
        private static float MissileDroneBaseSpeed = Main.config.Bind<float>("Missile Drone - Stats", "Base Speed", 24f, "The base speed of a Missile Drone, vanilla is 17.").Value;
        private static float MissileDroneBaseAcceleration = Main.config.Bind<float>("Missile Drone - Stats", "Base Acceleration", 24f, "The base acceleration of a Missile Drone, vanilla is 17.").Value;
        private static float MissileDroneBaseCrit = Main.config.Bind<float>("Missile Drone - Stats", "Base Critical Chance", 15f, "The base critical chance of a Missile Drone, vanilla is 0.").Value;
        private static float MissileDroneBaseDamage = Main.config.Bind<float>("Missile Drone - Stats", "Base Damage", 7f, "The base damage of a Missile Drone, vanilla is 14.").Value;
        // skill config vars
        private static int MissileDroneMissileCount = Main.config.Bind<int>("Missile Drone - Primary", "Missile Count", 6, "The total missiles fired by a Missile Drone's primary, vanilla is 4.").Value;
        private static float MissileDroneDamageCoeff = Main.config.Bind<int>("Missile Drone - Primary", "Damage Coefficient", 2, "The damage coefficient of a Missile Drone's primary, vanilla is 1.").Value;
        

        private static void TweakAI() {
            GameObject master = Addressables.LoadAssetAsync<GameObject>(MissileDroneMasterPath).WaitForCompletion();

            if (Main.MechanicalAllyOrbitEnabled) {
                // use seperate AI changes if mechanical orbiting is enabled
                foreach (AISkillDriver driver in master.GetComponents<AISkillDriver>()) {
                    switch (driver.customName) {
                        case "StrafeNearbyEnemies":
                            driver.movementType = AISkillDriver.MovementType.Stop;
                            driver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
                            break;
                        case "ChaseFarEnemies":
                            driver.maxDistance = MissileDroneChaseDistance;
                            driver.movementType = AISkillDriver.MovementType.Stop;
                            driver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
                            break;
                        default:
                            GameObject.Destroy(driver);
                            break;
                    }
                }
                return;
            }

            // tweak skill drivers
            foreach (AISkillDriver driver in master.GetComponents<AISkillDriver>()) {
                switch (driver.customName) {
                    case "HardLeashToLeader":
                        driver.minDistance = MissileDroneMinDistanceFromOwner;
                        break;
                    case "SoftLeashAttack":
                        driver.minDistance = MissileDroneMinDistanceFromOwner;
                        break;
                    case "SoftLeashToLeader":
                        driver.minDistance = MissileDroneMinDistanceFromOwner * 0.4f;
                        break;
                    case "StrafeNearbyEnemies":
                        driver.maxDistance = MissileDroneStrafeDistance;
                        break;
                    case "ChaseFarEnemies":
                        driver.maxDistance = MissileDroneChaseDistance;
                        break;
                    default:
                        break;
                }
            }
        }

        private static void TweakBody() {
            GameObject prefab = Addressables.LoadAssetAsync<GameObject>(MissileDroneBodyPath).WaitForCompletion();

            // tweak stats
            CharacterBody body = prefab.GetComponent<CharacterBody>();
            body.baseMaxHealth = MissileDroneBaseHealth;
            body.baseCrit = MissileDroneBaseCrit;
            body.baseMoveSpeed = MissileDroneBaseSpeed;
            body.baseAcceleration = MissileDroneBaseAcceleration;
            body.baseRegen = MissileDroneBaseRegen;
            body.baseDamage = MissileDroneBaseDamage;

            // tweak skills
            On.EntityStates.Drone.DroneWeapon.FireMissileBarrage.OnEnter += (orig, self) => {
                FireMissileBarrage.damageCoefficient = MissileDroneDamageCoeff;
                FireMissileBarrage.maxMissileCount = MissileDroneMissileCount;
                orig(self);
            };
        }

        public static void EnableChanges() {
            if (MissileDroneEnabled) {
                TweakAI();
                TweakBody();
            }
        }
    }
}