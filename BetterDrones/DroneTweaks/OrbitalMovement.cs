using BepInEx;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Reflection;
using System;
using BetterDrones;
using BepInEx.Configuration;
using RoR2.CharacterAI;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;

namespace BetterDrones.DroneTweaks {
    public class OrbitalMovement {
        public static void EnableOrbitalMovement() {
            On.RoR2.CharacterAI.BaseAI.FixedUpdate += OverrideInputs;
            On.RoR2.CharacterBody.Start += EnableOrbit;
            On.RoR2.SummonMasterBehavior.OpenSummonReturnMaster += ForceRecheck;
        }

        private static CharacterMaster ForceRecheck(On.RoR2.SummonMasterBehavior.orig_OpenSummonReturnMaster orig, SummonMasterBehavior self, Interactor interactor) {
            CharacterMaster master = orig(self, interactor);
            OrbitController controller = interactor.GetComponent<OrbitController>();
            if (controller) {
                controller.Invoke(nameof(OrbitController.FetchAllies), 0.2f);
            }
            return master;
        }

        private static void OverrideInputs(On.RoR2.CharacterAI.BaseAI.orig_FixedUpdate orig, BaseAI self) {
            orig(self);
            if (NetworkServer.active) {
                if (self.body && self.body.gameObject && self.body.gameObject.GetComponent<OrbitController>()) {
                    self.bodyInputBank.moveVector = Vector3.zero;
                }
            }
        }

        private static void EnableOrbit(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self) {
            orig(self);
            if (NetworkServer.active) {
                // IEnumerable<CharacterMaster> minions = CharacterMaster.readOnlyInstancesList.Where(x => x.minionOwnership && x.minionOwnership.ownerMaster == self.master);
                if (self.isPlayerControlled) {
                    self.gameObject.AddComponent<OrbitController>();
                }
            }
        }
        class OrbitController : MonoBehaviour {
            private float distance => Main.MechanicalAllyOrbitDistance + (0.1f * (allyCount - 1));
            private Vector3 offset => new Vector3(0, Main.MechanicalAllyOrbitOffset, 0);
            private float speed => Main.MechanicalAllyOrbitSpeed;
            private float allyCount = 1f;
            private float fullAngle = 360f;
            private List<CharacterBody> orbiters = new();
            private float stopwatch = 0f;
            private float updateDelay = 0.4f;
            private CharacterBody body;
            private CharacterMaster master;
            private float initialTime;

            private void Start() {
                body = GetComponent<CharacterBody>();
                master = body.master;
                initialTime = Run.instance.GetRunStopwatch();
            }

            private void FixedUpdate() {
                stopwatch += Time.fixedDeltaTime;
                if (stopwatch >= updateDelay) {
                    stopwatch = 0f;
                    FetchAllies();
                }

                for (int i = 0; i < orbiters.Count; i++) {
                    CharacterBody orbiter = orbiters[i];
                    if (!orbiter) continue;
                    Rigidbody rb = orbiter.GetComponent<Rigidbody>();

                    float elapsed = (Run.instance.GetRunStopwatch() - initialTime);

                    Vector3 plane1 = Vector3.up;
                    Vector3 plane2 = body.transform.forward;
                    
                    Vector3 targetPosition = (body.footPosition + GetOffset(orbiter)) + Quaternion.AngleAxis(fullAngle / allyCount * i + elapsed / speed * fullAngle, plane1) * plane2 * GetDistance(orbiter);
                    Vector3 currentPos = rb ? rb.position : body.transform.position;
                    float vel = body.isSprinting ? body.moveSpeed * body.sprintingSpeedMultiplier * 1.35f : body.moveSpeed * 1.35f;
                    Vector3 lerpedPosition = Vector3.Lerp(currentPos, targetPosition, vel * Time.fixedDeltaTime);

                    if (rb) {
                        rb.MovePosition(lerpedPosition);
                    }
                    else {
                        orbiter.transform.position = lerpedPosition;
                    }
                }
            }

            internal Vector3 GetOffset(CharacterBody orbiter) {
                switch (BodyCatalog.GetBodyName(orbiter.bodyIndex)) {
                    case "MegaDroneBody":
                        return offset * 2;
                    case "EmergencyDroneBody":
                        return offset * 2;
                    case "BackupDroneBody":
                        return offset * 2;
                    default:
                        return offset;
                }
            }

            internal float GetDistance(CharacterBody orbiter) {
                switch (BodyCatalog.GetBodyName(orbiter.bodyIndex)) {
                    case "MegaDroneBody":
                        return distance * 2;
                    case "EmergencyDroneBody":
                        return distance * 2;
                    case "BackupDroneBody":
                        return distance * 2;
                    default:
                        return distance;
                }
            }

            internal void FetchAllies() {
                IEnumerable<CharacterMaster> minions = CharacterMaster.readOnlyInstancesList.Where(x => x.minionOwnership && x.minionOwnership.ownerMaster == master);
                foreach (CharacterMaster minion in minions) {
                    CharacterBody minionBody = minion.GetBody();
                    if (minionBody && minionBody.isFlying && minionBody.bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical)) {
                        if (Main.MechanicalAllyOrbitBlacklist.Contains(BodyCatalog.GetBodyName(minionBody.bodyIndex))) {
                            continue;
                        }
                        else if (!orbiters.Contains(minionBody)) {
                            orbiters.Add(minionBody);
                        }
                    }
                }

                for (int i = 0; i < orbiters.Count; i++) {
                    CharacterBody body = orbiters[i];
                    if (!body) {
                        orbiters.Remove(body);
                    }

                    body.gameObject.layer = LayerIndex.noCollision.intVal;
                }

                allyCount = orbiters.Count;
            }
        }
    }
}