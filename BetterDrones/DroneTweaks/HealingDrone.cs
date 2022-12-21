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
    public class HealingDrone {
        // paths
        private static string HealingDroneMasterPath = "RoR2/Base/Drones/Drone2Master.prefab";
        private static string HealingDroneBodyPath = "RoR2/Base/Drones/Drone2Body.prefab";
        // misc config vars
        private static bool HealingDroneEnabled = Main.config.Bind<bool>("Healing Drone", "Enable Changes", true, "Should changes to the Healing Drone be enabled?").Value;
        // ai config vars
        private static float HealingDroneMinDistanceFromOwner = Main.config.Bind<float>("Healing Drone - AI", "Minimum Distance", 20, "The minimum distance a Healing Drone can be from you when it has no targets, vanilla is 35.").Value;
        // body config vars
        private static float HealingDroneBaseHealth = Main.config.Bind<float>("Healing Drone - Stats", "Base Health", 210, "The base health of a Healing Drone, vanilla is 150.").Value;
        private static float HealingDroneBaseRegen = Main.config.Bind<float>("Healing Drone - Stats", "Base Regen", 7.5f, "The base regen of a Healing Drone, vanilla is 5.").Value;
        private static float HealingDroneBaseSpeed = Main.config.Bind<float>("Healing Drone - Stats", "Base Speed", 24f, "The base speed of a Healing Drone, vanilla is 17.").Value;
        private static float HealingDroneBaseAcceleration = Main.config.Bind<float>("Healing Drone - Stats", "Base Acceleration", 24f, "The base acceleration of a Healing Drone, vanilla is 17.").Value;
        private static float HealingDroneBaseCrit = Main.config.Bind<float>("Healing Drone - Stats", "Base Critical Chance", 15f, "The base critical chance of a Healing Drone, vanilla is 0.").Value;
        private static float HealingDroneBaseDamage = Main.config.Bind<float>("Healing Drone - Stats", "Base Damage", 5f, "The base damage of a Healing Drone, vanilla is 10.").Value;
        // skill config vars
        private static int HealingDroneDuration = Main.config.Bind<int>("Healing Drone - Primary", "Duration", 7, "The duration of a Healing Drone's heal, vanilla is 5.").Value;
        private static int HealingDroneRange = Main.config.Bind<int>("Healing Drone - Primary", "Range", 90, "The maximum targeting range of a Healing Drone's heal, vanilla is 50.").Value;
        private static float HealingDroneHealCoeff = Main.config.Bind<int>("Healing Drone - Primary", "Heal Coefficient", 1, "The heal coefficient of a Healing Drone's healing beam, vanilla is 0.5.").Value;
        

        private static void TweakAI() {
            GameObject master = Addressables.LoadAssetAsync<GameObject>(HealingDroneMasterPath).WaitForCompletion();
        
            if (Main.MechanicalAllyOrbitEnabled) {
                // use seperate AI changes if mechanical orbiting is enabled
                foreach (AISkillDriver driver in master.GetComponents<AISkillDriver>()) {
                    switch (driver.customName) {
                        case "HealLeader":
                            driver.movementType = AISkillDriver.MovementType.Stop;
                            driver.maxTargetHealthFraction = 0.85f;
                            break;
                        case "HealNearAlly":
                            driver.movementType = AISkillDriver.MovementType.Stop;
                            driver.maxTargetHealthFraction = 0.85f;
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
                        driver.minDistance = HealingDroneMinDistanceFromOwner;
                        break;
                    case "HealLeader":
                        driver.maxTargetHealthFraction = 0.85f;
                        break;
                    case "HealNearAlly":
                        driver.maxTargetHealthFraction = 0.85f;
                        break;
                    default:
                        break;
                }
            }
        }

        private static void TweakBody() {
            GameObject prefab = Addressables.LoadAssetAsync<GameObject>(HealingDroneBodyPath).WaitForCompletion();

            // tweak stats
            CharacterBody body = prefab.GetComponent<CharacterBody>();
            body.baseMaxHealth = HealingDroneBaseHealth;
            body.baseCrit = HealingDroneBaseCrit;
            body.baseMoveSpeed = HealingDroneBaseSpeed;
            body.baseAcceleration = HealingDroneBaseAcceleration;
            body.baseRegen = HealingDroneBaseRegen;
            body.baseDamage = HealingDroneBaseDamage;

            // tweak skills
            On.EntityStates.Drone.DroneWeapon.StartHealBeam.OnEnter += (orig, self) => {
                if (self.characterBody.baseNameToken == "DRONE_HEALING_BODY_NAME") {
                    self.healRateCoefficient = HealingDroneHealCoeff;
                    self.baseDuration = HealingDrone.HealingDroneDuration;
                    self.targetSelectionRange = HealingDrone.HealingDroneRange;
                }
                orig(self);
            };
        }

        public static void EnableChanges() {
            if (HealingDroneEnabled) {
                TweakAI();
                TweakBody();
            }
        }
    }
}