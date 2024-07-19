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
    public class DroneCloakPropagation {
        public static void Setup() {
            On.RoR2.CharacterBody.RecalculateStats += (orig, self) => {
                orig(self);

                if (!NetworkServer.active) {
                    return;
                }

                if (!Main.MechanicalAllyOrbitEnabled) {
                    return;
                }

                if (self.isPlayerControlled) {
                    IEnumerable<CharacterMaster> minions = CharacterMaster.readOnlyInstancesList.Where(x => x.minionOwnership && x.minionOwnership.ownerMaster == self.master && x.GetBody() && x.GetBody().bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical));
                    if (self.HasBuff(RoR2Content.Buffs.Cloak)) {
                        foreach (CharacterMaster master in minions) {
                            if (master.GetBody().HasBuff(RoR2Content.Buffs.Cloak)) {
                                continue;
                            }
                            // Debug.Log("propagating cloak");
                            master.GetBody().AddBuff(RoR2Content.Buffs.Cloak);
                        }
                    }
                    else {
                        foreach (CharacterMaster master in minions) {
                            if (!master.GetBody().HasBuff(RoR2Content.Buffs.Cloak)) {
                                continue;
                            }
                            // Debug.Log("unpropagating cloak");
                            master.GetBody().RemoveBuff(RoR2Content.Buffs.Cloak);
                        }
                    }
                }
            };
        }
    }
}