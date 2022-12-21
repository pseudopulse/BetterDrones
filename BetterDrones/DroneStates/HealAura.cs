using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity;
using UnityEngine;
using EntityStates;
using RoR2;
using BetterDrones.DroneTweaks;
using UnityEngine.Networking;

namespace BetterDrones.DroneStates
{
    public class HealAura : BaseState 
    {
        private float interval => EmergencyDrone.EmergencyDroneHealAuraInterval;
        private float range => EmergencyDrone.EmergencyDroneHealAuraRange;
        private float fraction => EmergencyDrone.EmergencyDroneHealAuraFraction;
        private float stopwatch = 0f;
        public override void FixedUpdate() {
            if (base.isAuthority) {
                stopwatch += Time.fixedDeltaTime;

                if (stopwatch >= interval) {
                    stopwatch = 0f;

                    SphereSearch search = new();
                    search.radius = range;
                    search.origin = base.characterBody.corePosition;
                    search.mask = LayerIndex.entityPrecise.mask;
                    search.queryTriggerInteraction = QueryTriggerInteraction.Ignore;
                    search.RefreshCandidates();
                    TeamMask mask = new();
                    mask.AddTeam(base.GetTeam());
                    search.FilterCandidatesByHurtBoxTeam(mask);
                    search.OrderCandidatesByDistance();
                    search.FilterCandidatesByDistinctHurtBoxEntities();
                    HurtBox[] allies = search.GetHurtBoxes();

                    foreach (HurtBox box in allies) {
                        if (box.healthComponent) {
                            box.healthComponent.Heal(box.healthComponent.fullCombinedHealth * fraction, new(), true);
                        }
                    }
                    
                    search.ClearCandidates();

                    EffectManager.SpawnEffect(EntityStates.AffixEarthHealer.Heal.effectPrefab, new EffectData() {
                        scale = range,
                        origin = base.characterBody.corePosition,
                    }, true);
                }
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}