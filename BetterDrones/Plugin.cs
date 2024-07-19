using BepInEx;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Reflection;
using BepInEx.Configuration;
using BetterDrones.DroneTweaks;
using RoR2.CharacterAI;
using UnityEngine.Networking;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Xml;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using EntityStates.Merc;

namespace BetterDrones {
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "pseudopulse";
        public const string PluginName = "BetterDrones";
        public const string PluginVersion = "1.7.0";
        public static BepInEx.Logging.ManualLogSource ModLogger;
        public static ConfigFile config;

        // global config vars
        public static bool MechanicalAllyOrbitEnabled;
        public static float MechanicalAllyOrbitDistance;
        public static float MechanicalAllyOrbitSpeed;
        public static float MechanicalAllyOrbitOffset;
        public static List<string> MechanicalAllyOrbitBlacklist;
        public static bool PingControlEnabled;
        public static bool PerfectAimEnabled;
        public static float Alpha;
        public static bool MechanicalAllyDisableSounds;
        public void Awake() {
            // set logger and config
            ModLogger = Logger;
            config = Config;

            MechanicalAllyOrbitEnabled = config.Bind<bool>("Global", "RoR1 Drone Movement", true, "Should aerial mechanical allies orbit you like in Risk of Rain 1? This disables AI movement changes.").Value;
            MechanicalAllyOrbitDistance = config.Bind<float>("Global", "Drone Orbit Distance", 3f, "The distance aerial mechanical allies should orbit you from.").Value;
            MechanicalAllyOrbitOffset = config.Bind<float>("Global", "Drone Orbit Height", 1.3f, "The height aerial mechanical allies should orbit you from.").Value;
            MechanicalAllyOrbitSpeed = config.Bind<float>("Global", "Drone Orbit Speed", 7f, "The speed in seconds in which it should take for an aerial mechanical ally to make a full rotation around you.").Value;

            MechanicalAllyOrbitBlacklist = config.Bind<string>("Global", "Orbit Blacklist", "RoboBallBossBody SuperRoboBallBossBody SquallBody BackupDroneBody FlameDroneBody MegaDroneBody", "List of body names to blacklist from orbit, seperated by whitespace.").Value.Split(' ').ToList();

            MechanicalAllyOrbitBlacklist.Add("HellDroneBody");
            MechanicalAllyOrbitBlacklist.Add("BoosterDroneBody");
            MechanicalAllyOrbitBlacklist.Add("ShredderDrone");

            MechanicalAllyDisableSounds = config.Bind<bool>("Global", "Disable Drone Idle Sounds", true, "Disables the sounds that drones play when idle. Recommended when using orbitals.").Value;


            PingControlEnabled = config.Bind<bool>("Global", "Ping Control", true, "Should mechanical allies target your most recent ping?").Value;

            PerfectAimEnabled = config.Bind<bool>("Global", "Perfect Aim", true, "Should mechanical allies have perfect aim?").Value;

            Alpha = config.Bind<float>("Global", "Ally Transparency", 0.5f, "The transparency of mechanical allies between 1 (fully visible) and 0 (fully transparent)").Value;

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
            EmergencyDrone.EnableChanges();
            MegaDrone.EnableChanges();
            GunnerTurret.EnableChanges();

            IgnoreCollision.Enable();
            DroneTransparency.EnableChanges();
            DroneCloakPropagation.Setup();

            On.RoR2.BulletAttack.Fire += AlliesDontEatShots;
            //On.EntityStates.Merc.Evis.SearchForTarget += DontTargetAllies;
            // IL.EntityStates.Merc.EvisDash.FixedUpdate += HopooWhat;
        }

        // this doesnt work guh

        private static void HopooWhat(ILContext il) {
            ILCursor c = new(il);
            bool found = c.TryGotoNext(MoveType.Before, 
                x => x.MatchLdloc(4),
                x => x.MatchLdfld<HurtBox>(nameof(HurtBox.healthComponent)),
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<EntityStates.EntityState>("get_healthComponent")
            );

            if (found) {
                c.RemoveRange(4);
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc, 4);
                c.Index += 2;
                c.EmitDelegate<Func<EvisDash, HurtBox, bool>>((evis, hb) => {
                    return hb.teamIndex != evis.GetTeam();
                });
            }
            else {
                ModLogger.LogError("Failed to apply merc dash IL hook");
            }
        }

        private static void OverrideInputsPerfectAim(On.RoR2.CharacterAI.BaseAI.orig_FixedUpdate orig, BaseAI self) {
            orig(self);
            if (NetworkServer.active) {
                if (self.gameObject.name.Contains("Drone2") || self.gameObject.name.Contains("EmergencyDrone")) {
                    return;
                }
                if (self.body && self.body.gameObject && self.body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical) && self.body.teamComponent.teamIndex == TeamIndex.Player) {
                    if (self.currentEnemy.GetBullseyePosition(out Vector3 pos)) {
                        self.bodyInputBank.aimDirection = (pos - self.bodyInputBank.aimOrigin).normalized;
                    }
                }
            }
        }

        private static void AlliesDontEatShots(On.RoR2.BulletAttack.orig_Fire orig, BulletAttack self) {
            if (self.filterCallback == BulletAttack.defaultFilterCallback) {
                self.filterCallback = delegate (BulletAttack attack, ref BulletAttack.BulletHit hit) {
                    if (self.owner && hit.hitHurtBox && hit.hitHurtBox.teamIndex == self.owner.GetComponent<TeamComponent>().teamIndex) {
                        return false;
                    }
                    else {
                        return BulletAttack.DefaultFilterCallbackImplementation(attack, ref hit);
                    }
                };
            }
            orig(self);
        }
    } 
}