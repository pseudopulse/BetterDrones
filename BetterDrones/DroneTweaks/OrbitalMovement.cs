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
                if (self.gameObject.GetComponent<VectorPID>() && self.bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical)) {
                    CharacterMaster master = self.master;
                    if (master) {
                        MinionOwnership owner = master.minionOwnership;
                        if (owner && owner.ownerMaster) {
                            GameObject bodyObject = owner.ownerMaster.GetBodyObject();
                            if (bodyObject) {
                                OrbitController controller = self.gameObject.AddComponent<OrbitController>();
                                controller.owner = master;
                                controller.target = bodyObject.transform;
                                controller.FetchAllyCount();
                            }
                        }
                    }
                }
            }
        }
        class OrbitController : MonoBehaviour {
            public Transform target;
            public float distance = Main.MechanicalAllyOrbitDistance;
            public float speed => Main.MechanicalAllyOrbitSpeed;
            private float mainOrbitSpeed;
            private float updateStopwatch = 0f;
            private float updateDelay = 1f;
            private float mainDistance;
            private float offset = 2.5f;
            private int allyCount;
            private float initialTime = Run.instance.GetRunStopwatch();
            private Vector3 initialRadial;
            private float initialDegrees = UnityEngine.Random.Range(0, 360);
            public CharacterMaster owner;

            private void Start() {
                gameObject.layer = LayerIndex.debris.intVal;

                switch (base.GetComponent<CharacterBody>().baseNameToken) {
                    case "ROBOBALLGREENBUDDY_BODY_NAME":
                        offset *= 4;
                        distance *= 4f;
                        break;
                    case "ROBOBALLREDBUDDY_BODY_NAME":
                        offset *= 4;
                        distance *= 4f;
                        break;
                    default:
                        break;
                }

                mainOrbitSpeed = (360 / speed) * UnityEngine.Random.Range(0.8f, 1.2f);
            }

            private void FixedUpdate() {
                if (target && NetworkServer.active) {
                    float angle = (Run.instance.GetRunStopwatch() - initialTime) * mainOrbitSpeed;
                    Vector3 pos = target.position + new Vector3(0, offset, 0) + Quaternion.AngleAxis(angle, Vector3.up) * initialRadial * (distance);
                    base.GetComponent<Rigidbody>().MovePosition(pos);
                }

                if (NetworkServer.active) {
                    updateStopwatch += Time.fixedDeltaTime;

                    if (updateStopwatch >= updateDelay) {
                        updateStopwatch = 0f;
                        FetchAllyCount();
                    }
                }
            }

            public void FetchAllyCount() {
                IEnumerable<CharacterMaster> minions = CharacterMaster.readOnlyInstancesList.Where(x => x.minionOwnership && x.minionOwnership.ownerMaster == owner && x.GetBody() && x.GetBody().gameObject.GetComponent<OrbitController>());
                allyCount = minions.Count();
                mainDistance = (distance) + (allyCount * 0.5f);
                initialRadial = Quaternion.AngleAxis(initialDegrees, Vector3.up) * target.forward;
            }
        }
    }
}