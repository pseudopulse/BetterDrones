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
    public class IncineratorDrone {
        // paths
        private static string IncineratorDroneMasterPath = "RoR2/Base/Drones/FlameDroneMaster.prefab";
        private static string IncineratorDroneBodyPath = "RoR2/Base/Drones/FlameDroneBody.prefab";
        // misc config vars
        private static bool IncineratorDroneEnabled = Main.config.Bind<bool>("Incinerator Drone", "Enable Changes", true, "Should changes to the Incinerator Drone be enabled?").Value;
        // ai config vars
        private static float IncineratorDroneMinDistanceFromOwner = Main.config.Bind<float>("Incinerator Drone - AI", "Minimum Distance", 120, "The minimum distance a Incinerator Drone can be from you when it has no targets, vanilla is 120.").Value;
        private static float IncineratorDroneStrafeDistance = Main.config.Bind<float>("Incinerator Drone - AI", "Strafe Distance", 10, "The maximum distance in which a Incinerator Drone will attempt to strafe enemies, vanilla is 12.").Value;
        private static float IncineratorDroneChaseDistance = Main.config.Bind<float>("Incinerator Drone - AI", "Chase Distance", 100, "The maximum distance in which a Incinerator Drone will attempt to chase a target, vanilla is 100.").Value;
        // body config vars
        private static float IncineratorDroneBaseHealth = Main.config.Bind<float>("Incinerator Drone - Stats", "Base Health", 450, "The base health of a Incinerator Drone, vanilla is 300.").Value;
        private static float IncineratorDroneBaseRegen = Main.config.Bind<float>("Incinerator Drone - Stats", "Base Regen", 7.5f, "The base regen of a Incinerator Drone, vanilla is 5.").Value;
        private static float IncineratorDroneBaseSpeed = Main.config.Bind<float>("Incinerator Drone - Stats", "Base Speed", 24f, "The base speed of a Incinerator Drone, vanilla is 17.").Value;
        private static float IncineratorDroneBaseAcceleration = Main.config.Bind<float>("Incinerator Drone - Stats", "Base Acceleration", 24f, "The base acceleration of a Incinerator Drone, vanilla is 17.").Value;
        private static float IncineratorDroneBaseCrit = Main.config.Bind<float>("Incinerator Drone - Stats", "Base Critical Chance", 15f, "The base critical chance of a Incinerator Drone, vanilla is 0.").Value;
        private static float IncineratorDroneBaseDamage = Main.config.Bind<float>("Incinerator Drone - Stats", "Base Damage", 5f, "The base damage of a Incinerator Drone, vanilla is 10.").Value;
        private static int IncineratorDroneBaseArmor = Main.config.Bind<int>("Incinerator Drone - Stats", "Base Armor", 50, "The base armor of a Incinerator Drone, vanilla is 20.").Value;
        // skill config vars
        private static int IncineratorDroneRange = Main.config.Bind<int>("Incinerator Drone - Primary", "Range", 15, "The range of an Incinerator Drone's primary, vanilla is 12.").Value;
        private static int IncineratorDroneDuration = Main.config.Bind<int>("Incinerator Drone - Primary", "Duration", 5, "The duration of a Incinerator Drone's primary, vanilla is 3.").Value;
        private static int IncineratorDroneBurnChance = Main.config.Bind<int>("Incinerator Drone - Primary", "Ignite Chance", 100, "The ignite chance of a Incinerator Drone's primary, vanilla is 50.").Value;
        private static float IncineratorDroneDamageCoeff = Main.config.Bind<float>("Incinerator Drone - Primary", "Total Damage Coefficient", 25f, "The total damage coefficient of a Incinerator Drone's primary, vanilla is 20.").Value;
        

        private static void TweakAI() {
            GameObject master = Addressables.LoadAssetAsync<GameObject>(IncineratorDroneMasterPath).WaitForCompletion();

            // tweak skill drivers
            foreach (AISkillDriver driver in master.GetComponents<AISkillDriver>()) {
                switch (driver.customName) {
                    case "HardLeashToLeader":
                        driver.minDistance = IncineratorDroneMinDistanceFromOwner;
                        break;
                    case "StrafeNearbyEnemies":
                        driver.maxDistance = IncineratorDroneStrafeDistance;
                        break;
                    case "ChaseFarEnemies":
                        driver.maxDistance = IncineratorDroneChaseDistance;
                        break;
                    default:
                        break;
                }
            }
        }

        private static void TweakBody() {
            GameObject prefab = Addressables.LoadAssetAsync<GameObject>(IncineratorDroneBodyPath).WaitForCompletion();

            // tweak stats
            CharacterBody body = prefab.GetComponent<CharacterBody>();
            body.baseMaxHealth = IncineratorDroneBaseHealth;
            body.baseCrit = IncineratorDroneBaseCrit;
            body.baseMoveSpeed = IncineratorDroneBaseSpeed;
            body.baseAcceleration = IncineratorDroneBaseAcceleration;
            body.baseRegen = IncineratorDroneBaseRegen;
            body.baseDamage = IncineratorDroneBaseDamage;
            body.baseArmor = IncineratorDroneBaseArmor;

            prefab.RemoveComponents<AkEvent>();

            // tweak skills
            On.EntityStates.Mage.Weapon.Flamethrower.OnEnter += (orig, self) => {
                if (self.characterBody.baseNameToken == "FLAMEDRONE_BODY_NAME") {
                    self.maxDistance = IncineratorDroneRange;
                    Flamethrower.baseFlamethrowerDuration = IncineratorDroneDuration;
                    Flamethrower.ignitePercentChance = IncineratorDroneBurnChance;
                    Flamethrower.totalDamageCoefficient = IncineratorDroneDamageCoeff;
                }
                orig(self);
            };
        }

        public static void EnableChanges() {
            if (IncineratorDroneEnabled) {
                TweakAI();
                TweakBody();
            }
        }
    }
}