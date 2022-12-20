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
    public class PingControl {
        public static void EnablePingControls() {
            On.RoR2.CharacterAI.BaseAI.UpdateTargets += OverrideTarget;
        }

        public static void OverrideTarget(On.RoR2.CharacterAI.BaseAI.orig_UpdateTargets orig, BaseAI self) {
            orig(self);
            if (NetworkServer.active) {
                if (self.master && self.master.minionOwnership && self.body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical)) {
                    if (self.master.minionOwnership.ownerMaster) {
                        CharacterMaster owner = self.master.minionOwnership.ownerMaster;
                        if (owner.playerCharacterMasterController && owner.playerCharacterMasterController.pingerController) {
                            PingerController controller = owner.playerCharacterMasterController.pingerController;
                            if (controller.currentPing.active && controller.currentPing.targetGameObject) {
                                if (controller.currentPing.targetGameObject.GetComponent<CharacterBody>()) {
                                    self.currentEnemy.gameObject = controller.currentPing.targetGameObject;
                                }
                            }
                        }
                     }
                }
            }
        }
    }
}