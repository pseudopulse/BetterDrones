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
    public class EmergencyDrone {
        // paths
        private static string EmergencyDroneMasterPath = "RoR2/Base/Drones/EmergencyDroneMaster.prefab";
        private static string EmergencyDroneBodyPath = "RoR2/Base/Drones/EmergencyDroneBody.prefab";
        // misc config vars
        private static bool EmergencyDroneEnabled = Main.config.Bind<bool>("Emergency Drone", "Enable Changes", true, "Should changes to the Emergency Drone be enabled?").Value;
        // ai config vars
        private static float EmergencyDroneMinDistanceFromOwner = Main.config.Bind<float>("Emergency Drone - AI", "Minimum Distance", 20, "The minimum distance a Emergency Drone can be from you when it has no targets, vanilla is 35.").Value;
        private static bool EmergencyDroneUseTargeting = Main.config.Bind<bool>("Emergency Drone - AI", "Use Standard Targeting", false, "Use the weird special targeting that Emergency Drones have? vanilla is true.").Value;
        // body config vars
        private static float EmergencyDroneBaseHealth = Main.config.Bind<float>("Emergency Drone - Stats", "Base Health", 460, "The base health of a Emergency Drone, vanilla is 300.").Value;
        private static float EmergencyDroneBaseRegen = Main.config.Bind<float>("Emergency Drone - Stats", "Base Regen", 7.5f, "The base regen of a Emergency Drone, vanilla is 2.").Value;
        private static float EmergencyDroneBaseSpeed = Main.config.Bind<float>("Emergency Drone - Stats", "Base Speed", 24f, "The base speed of a Emergency Drone, vanilla is 17.").Value;
        private static float EmergencyDroneBaseAcceleration = Main.config.Bind<float>("Emergency Drone - Stats", "Base Acceleration", 24f, "The base acceleration of a Emergency Drone, vanilla is 17.").Value;
        private static float EmergencyDroneBaseDamage = Main.config.Bind<float>("Emergency Drone - Stats", "Base Damage", 5f, "The base damage of a Emergency Drone, vanilla is 10.").Value;
        // skill config vars
        private static int EmergencyDroneDuration = Main.config.Bind<int>("Emergency Drone - Primary", "Duration", 7, "The duration of a Emergency Drone's heal, vanilla is 5.").Value;
        private static int EmergencyDroneRange = Main.config.Bind<int>("Emergency Drone - Primary", "Range", 90, "The maximum targeting range of a Emergency Drone's heal, vanilla is 50.").Value;
        private static float EmergencyDroneHealCoeff = Main.config.Bind<int>("Emergency Drone - Primary", "Heal Coefficient", 1, "The heal coefficient of a Emergency Drone's healing beam, vanilla is 0.5.").Value;
        // heal aura vars
        public static bool EmergencyDroneHealAuraEnabled = Main.config.Bind<bool>("Emergency Drone - Heal Aura", "Use Healing Aura", true, "Should Emergency Drones have a passive healing aura?").Value;
        public static float EmergencyDroneHealAuraFraction = Main.config.Bind<float>("Emergency Drone - Heal Aura", "Heal Fraction", 0.25f, "The healing fraction of an Emergency Drone's aura.").Value;
        public static float EmergencyDroneHealAuraRange = Main.config.Bind<float>("Emergency Drone - Heal Aura", "Range", 25f, "The range of an Emergency Drone's aura.").Value;
        public static float EmergencyDroneHealAuraInterval = Main.config.Bind<float>("Emergency Drone - Heal Aura", "Inteval", 5f, "The frequency in seconds of an Emergency Drone's aura.").Value;
        

        private static void TweakAI() {
            GameObject master = Addressables.LoadAssetAsync<GameObject>(EmergencyDroneMasterPath).WaitForCompletion();
        
            if (Main.MechanicalAllyOrbitEnabled) {
                // use seperate AI changes if mechanical orbiting is enabled
                foreach (AISkillDriver driver in master.GetComponents<AISkillDriver>()) {
                    switch (driver.customName) {
                        case "HealLeader":
                            driver.movementType = AISkillDriver.MovementType.Stop;
                            driver.maxTargetHealthFraction = 0.85f;
                            driver.maxDistance = EmergencyDroneRange;
                            break;
                        case "HealNearAlly":
                            driver.movementType = AISkillDriver.MovementType.Stop;
                            driver.maxTargetHealthFraction = 0.85f;
                            driver.maxDistance = EmergencyDroneRange;
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
                        driver.minDistance = EmergencyDroneMinDistanceFromOwner;
                        break;
                    case "HealLeader":
                        driver.maxTargetHealthFraction = 0.85f;
                        driver.maxDistance = EmergencyDroneRange;
                        break;
                    case "HealNearAlly":
                        driver.maxTargetHealthFraction = 0.85f;
                        driver.maxDistance = EmergencyDroneRange;
                        break;
                    default:
                        break;
                }
            }

            if (!EmergencyDroneUseTargeting) {
                GameObject.Destroy(master.GetComponent<EmergencyDroneCustomTarget>());
            }
        }

        private static void TweakBody() {
            GameObject prefab = Addressables.LoadAssetAsync<GameObject>(EmergencyDroneBodyPath).WaitForCompletion();

            // tweak stats
            CharacterBody body = prefab.GetComponent<CharacterBody>();
            body.baseMaxHealth = EmergencyDroneBaseHealth;
            body.baseMoveSpeed = EmergencyDroneBaseSpeed;
            body.baseAcceleration = EmergencyDroneBaseAcceleration;
            body.baseRegen = EmergencyDroneBaseRegen;
            body.baseDamage = EmergencyDroneBaseDamage;

            // tweak skills
            On.EntityStates.Drone.DroneWeapon.StartHealBeam.OnEnter += (orig, self) => {
                if (self.characterBody.baseNameToken == "EMERGENCYDRONE_BODY_NAME") {
                    self.healRateCoefficient = EmergencyDroneHealCoeff;
                    self.baseDuration = EmergencyDrone.EmergencyDroneDuration;
                    self.targetSelectionRange = EmergencyDrone.EmergencyDroneRange;
                }
                orig(self);
            };

            if (EmergencyDroneHealAuraEnabled) {
                EntityStateMachine esm = prefab.AddComponent<EntityStateMachine>();
                SerializableEntityStateType aura = new SerializableEntityStateType(typeof(DroneStates.HealAura));
                esm.initialStateType = aura;
                esm.mainStateType = aura;
                esm.customName = "HealAura";

                NetworkStateMachine nsm = prefab.GetComponent<NetworkStateMachine>();
                nsm.stateMachines = prefab.GetComponents<EntityStateMachine>();
            }
        }

        public static void EnableChanges() {
            if (EmergencyDroneEnabled) {
                TweakAI();
                TweakBody();
            }
        }
    }
}