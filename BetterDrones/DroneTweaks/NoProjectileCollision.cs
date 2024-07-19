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
using RoR2.Projectile;

namespace BetterDrones.DroneTweaks {
    public static class IgnoreCollision {
        public static void Enable() {
            On.RoR2.Projectile.ProjectileController.IgnoreCollisionsWithOwner += Ignore;
            On.RoR2.CharacterBody.Start += Ignore2;
        }

        private static void Ignore2(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);

            if (self.bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical) && self.master && self.master.minionOwnership.ownerMaster && self.GetComponent<Collider>()) {
                CharacterBody body = self.master.minionOwnership.ownerMaster.GetBody();

                if (body) {
                    Physics.IgnoreCollision(self.GetComponent<Collider>(), body.GetComponent<Collider>(), true);
                }
            }
        }

        private static void Ignore(On.RoR2.Projectile.ProjectileController.orig_IgnoreCollisionsWithOwner orig, ProjectileController self, bool ignore) {
            orig(self, ignore);
            if (!self.owner || !self.owner.GetComponent<CharacterBody>() || !self.owner.GetComponent<CharacterBody>().master) {
                return;
            }

            List<CharacterMaster> minions = CharacterMaster.readOnlyInstancesList.Where(delegate (CharacterMaster x) {
                return x.minionOwnership && x.minionOwnership.ownerMaster && x.minionOwnership.ownerMaster == self.owner.GetComponent<CharacterBody>().master;
            }).ToList();

            foreach (CharacterMaster minion in minions) {
                if (!minion.GetBody()) {
                    continue;
                }

                if (!minion.GetBody().hurtBoxGroup) {
                    continue;
                }

                CharacterBody body = minion.GetBody();
                HurtBoxGroup group = body.hurtBoxGroup;

                foreach (HurtBox box in group.hurtBoxes) {
                    foreach (Collider collider in self.myColliders) {
                        Physics.IgnoreCollision(collider, box.collider, true);
                    }
                }
            }
        }
    }
}