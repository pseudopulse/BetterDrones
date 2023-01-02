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
    public class GunnerDrone {
        // paths
        private static string GunnerDroneMasterPath = "RoR2/Base/Drones/Drone1Master.prefab";
        private static string GunnerDroneBodyPath = "RoR2/Base/Drones/Drone1Body.prefab";
        // misc config vars
        private static bool GunnerDroneEnabled = Main.config.Bind<bool>("Gunner Drone", "Enable Changes", true, "Should changes to the Gunner Drone be enabled?").Value;
        // ai config vars
        private static float GunnerDroneMinDistanceFromOwner = Main.config.Bind<float>("Gunner Drone - AI", "Minimum Distance", 50, "The minimum distance a Gunner Drone can be from you when it has no targets, vanilla is 60.").Value;
        private static float GunnerDroneStrafeDistance = Main.config.Bind<float>("Gunner Drone - AI", "Strafe Distance", 30, "The maximum distance in which a Gunner Drone will attempt to strafe enemies, vanilla is 15.").Value;
        private static float GunnerDroneChaseDistance = Main.config.Bind<float>("Gunner Drone - AI", "Chase Distance", 175, "The maximum distance in which a Gunner Drone will attempt to chase a target, vanilla is 45.").Value;
        // body config vars
        private static float GunnerDroneBaseHealth = Main.config.Bind<float>("Gunner Drone - Stats", "Base Health", 210, "The base health of a Gunner Drone, vanilla is 150.").Value;
        private static float GunnerDroneBaseRegen = Main.config.Bind<float>("Gunner Drone - Stats", "Base Regen", 7.5f, "The base regen of a Gunner Drone, vanilla is 5.").Value;
        private static float GunnerDroneBaseSpeed = Main.config.Bind<float>("Gunner Drone - Stats", "Base Speed", 24f, "The base speed of a Gunner Drone, vanilla is 17.").Value;
        private static float GunnerDroneBaseAcceleration = Main.config.Bind<float>("Gunner Drone - Stats", "Base Acceleration", 24f, "The base acceleration of a Gunner Drone, vanilla is 17.").Value;
        private static float GunnerDroneBaseCrit = Main.config.Bind<float>("Gunner Drone - Stats", "Base Critical Chance", 10f, "The base critical chance of a Gunner Drone, vanilla is 0.").Value;
        private static float GunnerDroneBaseDamage = Main.config.Bind<float>("Gunner Drone - Stats", "Base Damage", 5f, "The base damage of a Gunner Drone, vanilla is 10.").Value;
        // skill config vars
        private static int GunnerDroneBulletCount = Main.config.Bind<int>("Gunner Drone - Primary", "Bullet Count", 5, "The total bullets fired by a Gunner Drone's primary, vanilla is 4.").Value;
        private static int GunnerDroneMaxSpread = Main.config.Bind<int>("Gunner Drone - Primary", "Maximum Spread", 0, "The maximum spread of a Gunner Drone's primary, vanilla is 1.").Value;
        private static int GunnerDroneForce = Main.config.Bind<int>("Gunner Drone - Primary", "Knockback", 250, "The knockback of a Gunner Drone's primary, vanilla is 200.").Value;
        private static float GunnerDroneDamageCoeff = Main.config.Bind<float>("Gunner Drone - Primary", "Damage Coefficient", 0.5f, "The damage coefficient of a Gunner Drone's primary, vanilla is 0.5.").Value;
        

        private static void TweakAI() {
            GameObject master = Addressables.LoadAssetAsync<GameObject>(GunnerDroneMasterPath).WaitForCompletion();
        

            if (Main.MechanicalAllyOrbitEnabled) {
                // use seperate AI changes if mechanical orbiting is enabled
                foreach (AISkillDriver driver in master.GetComponents<AISkillDriver>()) {
                    switch (driver.customName) {
                        case "StrafeNearbyEnemies":
                            driver.movementType = AISkillDriver.MovementType.Stop;
                            break;
                        case "ChaseFarEnemies":
                            driver.maxDistance = GunnerDroneChaseDistance;
                            driver.movementType = AISkillDriver.MovementType.Stop;
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
                        driver.minDistance = GunnerDroneMinDistanceFromOwner;
                        break;
                    case "SoftLeashAttack":
                        driver.minDistance = GunnerDroneMinDistanceFromOwner;
                        break;
                    case "SoftLeashToLeader":
                        driver.minDistance = GunnerDroneMinDistanceFromOwner * 0.4f;;
                        break;
                    case "StrafeNearbyEnemies":
                        driver.maxDistance = GunnerDroneStrafeDistance;
                        break;
                    case "ChaseFarEnemies":
                        driver.maxDistance = GunnerDroneChaseDistance;
                        break;
                    default:
                        break;
                }
            }
        }

        private static void TweakBody() {
            GameObject prefab = Addressables.LoadAssetAsync<GameObject>(GunnerDroneBodyPath).WaitForCompletion();

            // tweak stats
            CharacterBody body = prefab.GetComponent<CharacterBody>();
            body.baseMaxHealth = GunnerDroneBaseHealth;
            body.baseCrit = GunnerDroneBaseCrit;
            body.baseMoveSpeed = GunnerDroneBaseSpeed;
            body.baseAcceleration = GunnerDroneBaseAcceleration;
            body.baseRegen = GunnerDroneBaseRegen;
            body.baseDamage = GunnerDroneBaseDamage;

            // tweak skills
            On.EntityStates.Drone.DroneWeapon.FireTurret.OnEnter += (orig, self) => {
                FireTurret.bulletCount = GunnerDroneBulletCount;
                FireTurret.force = GunnerDroneForce;
                FireTurret.maxSpread = GunnerDroneMaxSpread;
                FireTurret.damageCoefficient = GunnerDroneDamageCoeff;
                orig(self);
            };
        }

        public static void EnableChanges() {
            if (GunnerDroneEnabled) {
                TweakAI();
                TweakBody();
            }
        }
    }
}