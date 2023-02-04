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
    public class MegaDrone {
        // paths
        private static string MegaDroneMasterPath = "RoR2/Base/Drones/MegaDroneMaster.prefab";
        private static string MegaDroneBodyPath = "RoR2/Base/Drones/MegaDroneBody.prefab";
        // misc config vars
        private static bool MegaDroneEnabled = Main.config.Bind<bool>("TC-280", "Enable Changes", true, "Should changes to the TC-280 be enabled?").Value;
        // ai config vars
        private static float MegaDroneMinDistanceFromOwner = Main.config.Bind<float>("TC-280 - AI", "Minimum Distance", 120, "The minimum distance a TC-280 can be from you when it has no targets, vanilla is 180.").Value;
        private static float MegaDroneStrafeDistance = Main.config.Bind<float>("TC-280 - AI", "Strafe Distance", 90, "The maximum distance in which a TC-280 will attempt to strafe enemies, vanilla is 60.").Value;
        private static float MegaDroneChaseDistance = Main.config.Bind<float>("TC-280 - AI", "Chase Distance", 175, "The maximum distance in which a TC-280 will attempt to chase a target, vanilla is 90.").Value;
        private static float MegaDroneTooCloseDistance = Main.config.Bind<float>("TC-280 - AI", "Too Close Distance", 10, "The distance in which the TC-280 deems 'too close', vanilla is 20.").Value;
        // body config vars
        private static float MegaDroneBaseHealth = Main.config.Bind<float>("TC-280 - Stats", "Base Health", 1600, "The base health of a TC-280, vanilla is 900.").Value;
        private static float MegaDroneBaseRegen = Main.config.Bind<float>("TC-280 - Stats", "Base Regen", 10f, "The base regen of a TC-280, vanilla is 5.").Value;
        private static float MegaDroneBaseSpeed = Main.config.Bind<float>("TC-280 - Stats", "Base Speed", 24f, "The base speed of a TC-280, vanilla is 20.").Value;
        private static float MegaDroneBaseAcceleration = Main.config.Bind<float>("TC-280 - Stats", "Base Acceleration", 24f, "The base acceleration of a TC-280, vanilla is 20.").Value;
        private static float MegaDroneBaseCrit = Main.config.Bind<float>("TC-280 - Stats", "Base Critical Chance", 10f, "The base critical chance of a TC-280, vanilla is 0.").Value;
        private static float MegaDroneBaseDamage = Main.config.Bind<float>("TC-280 - Stats", "Base Damage", 7.5f, "The base damage of a TC-280, vanilla is 14.").Value;
        // skill config vars
        private static int MegaDroneBulletCount = Main.config.Bind<int>("TC-280 - Primary", "Bullet Count", 20, "The total bullets fired by a TC-280's primary, vanilla is 15.").Value;
        private static int MegaDroneMaxSpread = Main.config.Bind<int>("TC-280 - Primary", "Maximum Spread", 0, "The maximum spread of a TC-280's primary, vanilla is 1.").Value;
        private static int MegaDroneForce = Main.config.Bind<int>("TC-280 - Primary", "Knockback", 400, "The knockback of a TC-280's primary, vanilla is 400.").Value;
        private static float MegaDroneDamageCoeff = Main.config.Bind<int>("TC-280 - Primary", "Damage Coefficient", 3, "The damage coefficient of a TC-280's primary, vanilla is 2.5.").Value;
        // m2
        private static float MegaDroneRocketDamage = Main.config.Bind<float>("TC-280 - Secondary", "Damage Coefficient", 5, "The damage coefficient of a TC-280's rockets, vanilla is 4").Value;
        private static float MegaDroneRocketForce = Main.config.Bind<float>("TC-280 - Secondary", "Knockback", 1000, "The knockback of a TC-280's rockets, vanilla is 1000").Value;
        // debuff laser
        public static bool MegaDroneLaserEnabled = Main.config.Bind<bool>("TC-280 - Laser", "Use Debuff Laser", false, "Should the TC-280 have a passive laser that debuffs targets?").Value;
        public static float MegaDroneLaserInterval = Main.config.Bind<float>("TC-280 - Laser", "Interval", 5, "How often the laser should switch on.").Value;

        private static void TweakAI() {
            GameObject master = Addressables.LoadAssetAsync<GameObject>(MegaDroneMasterPath).WaitForCompletion();
        

            if (Main.MechanicalAllyOrbitEnabled) {
                // use seperate AI changes if mechanical orbiting is enabled
                foreach (AISkillDriver driver in master.GetComponents<AISkillDriver>()) {
                    switch (driver.customName) {
                        case "StrafeCloseEnemy":
                            driver.movementType = AISkillDriver.MovementType.Stop;
                            break;
                        case "ChaseFarEnemiesRocket":
                            driver.maxDistance = MegaDroneChaseDistance;
                            driver.movementType = AISkillDriver.MovementType.Stop;
                            break;
                        case "ChaseFarEnemiesGun":
                            driver.maxDistance = MegaDroneChaseDistance;
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
                    case "LeashLeaderHard":
                        driver.minDistance = MegaDroneMinDistanceFromOwner;
                        break;
                    case "StopTooCloseTarget":
                        driver.maxDistance = MegaDroneTooCloseDistance;
                        break;
                    case "StrafeCloseEnemy":
                        driver.maxDistance = MegaDroneStrafeDistance;
                        break;
                    case "ChaseFarEnemiesRocket":
                        driver.maxDistance = MegaDroneChaseDistance;
                        break;
                    case "ChaseFarEnemiesGun":
                        driver.maxDistance = MegaDroneChaseDistance;
                        break;
                    default:
                        break;
                }
            }
        }

        private static void TweakBody() {
            GameObject prefab = Addressables.LoadAssetAsync<GameObject>(MegaDroneBodyPath).WaitForCompletion();

            prefab.RemoveComponents<AkEvent>();

            // tweak stats
            CharacterBody body = prefab.GetComponent<CharacterBody>();
            body.baseMaxHealth = MegaDroneBaseHealth;
            body.baseCrit = MegaDroneBaseCrit;
            body.baseMoveSpeed = MegaDroneBaseSpeed;
            body.baseAcceleration = MegaDroneBaseAcceleration;
            body.baseRegen = MegaDroneBaseRegen;
            body.baseDamage = MegaDroneBaseDamage;

            // tweak skills
            On.EntityStates.Drone.DroneWeapon.FireMegaTurret.OnEnter += (orig, self) => {
                self.bulletCount = MegaDroneBulletCount;
                FireMegaTurret.force = MegaDroneForce;
                FireMegaTurret.maxSpread = MegaDroneMaxSpread;
                FireMegaTurret.damageCoefficient = MegaDroneDamageCoeff;
                orig(self);
            };

            On.EntityStates.Drone.DroneWeapon.FireTwinRocket.OnEnter += (orig, self) => {
                FireTwinRocket.damageCoefficient = MegaDroneRocketDamage;
                FireTwinRocket.force = MegaDroneRocketForce;
                orig(self);
            };

            // mega laser
            if (MegaDroneLaserEnabled) {
                EntityStateMachine esm = prefab.AddComponent<EntityStateMachine>();
                SerializableEntityStateType laser = new SerializableEntityStateType(typeof(DroneStates.MegaLaser));
                esm.initialStateType = laser;
                esm.mainStateType = laser;
                esm.customName = "Laser";

                NetworkStateMachine nsm = prefab.GetComponent<NetworkStateMachine>();
                nsm.stateMachines = prefab.GetComponents<EntityStateMachine>();
            }
        }

        public static void EnableChanges() {
            if (MegaDroneEnabled) {
                TweakAI();
                TweakBody();
            }
        }
    }
}