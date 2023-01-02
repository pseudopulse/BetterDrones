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
using RoR2.CharacterAI;
using EntityStates.EngiTurret.EngiTurretWeapon;
using UnityEngine.AddressableAssets;
using BetterDrones.Utils;
using RoR2.Projectile;

namespace BetterDrones.DroneStates {
    public class TurretShield : BaseState {
        private GameObject shieldPrefab => Utils.Paths.GameObject.EngiBubbleShield.Load<GameObject>();
        private float shieldDuration => DroneTweaks.GunnerTurret.GunnerTurretShieldDuration;
        private float damageRequired => (DroneTweaks.GunnerTurret.GunnerTurretShieldDamage * 0.01f) * base.damageStat;
        private bool isShieldUp = false;
        private float stopwatch = 0f;
        private float damageDealt = 0f;
        private GameObject shieldInstance;

        public override void OnEnter()
        {
            base.OnEnter();
            GlobalEventManager.onServerDamageDealt += DamageDealt;
        }

        public override void OnExit()
        {
            base.OnExit();
            GlobalEventManager.onServerDamageDealt -= DamageDealt;
        }

        private void DamageDealt(DamageReport report) {
            if (report.attackerBody && report.attackerBody == base.characterBody && !isShieldUp) {
                damageDealt += report.damageDealt;

                if (damageDealt >= damageRequired) {
                    damageDealt = 0f;
                    TriggerShield();
                }
            }
        }

        private void TriggerShield() {
            shieldInstance = GameObject.Instantiate(shieldPrefab, base.characterBody.corePosition, Quaternion.identity);
            shieldInstance.transform.localScale *= 0.5f;
            shieldInstance.layer = LayerIndex.noCollision.intVal;
            shieldInstance.GetComponent<ProjectileStickOnImpact>().enabled = false;
            shieldInstance.GetComponent<EntityStateMachine>().SetNextState(new EntityStates.Engi.EngiBubbleShield.Deployed());
            isShieldUp = true;

            AkSoundEngine.PostEvent(Events.Play_item_use_BFG_explode, base.gameObject);
        }

        private void DisableShield() {
            if (shieldInstance) {
                Destroy(shieldInstance);
                isShieldUp = false;
            }

            AkSoundEngine.PostEvent(Events.Play_mage_shift_wall_explode, base.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isShieldUp) {
                stopwatch += Time.fixedDeltaTime;

                if (stopwatch >= shieldDuration) {
                    DisableShield();
                    stopwatch = 0f;
                }
            }

            if (shieldInstance) {
                ProjectileController controller = shieldInstance.GetComponent<ProjectileController>();
                if (controller.ghost) {
                    controller.ghost.gameObject.SetActive(false);
                }
                shieldInstance.GetComponent<Rigidbody>().useGravity = false;
                shieldInstance.transform.position = base.characterBody.corePosition;
            }
        }
    }
}