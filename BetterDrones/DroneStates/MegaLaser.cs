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

namespace BetterDrones.DroneStates {
    public class MegaLaser : BaseState {
        // stats
        private float interval => MegaDrone.MegaDroneLaserInterval;
        // vars
        private float stopwatch = 0f;
        private bool isActive = false;
        private GameObject currentTarget;
        // vfx
        private GameObject laserInstance;
        private Transform laserInstanceEnd;
        private GameObject laserInstance2;
        private Transform laserInstanceEnd2;

        public override void OnEnter()
        {
            base.OnEnter();
            GameObject prefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/LaserEngiTurret.prefab").WaitForCompletion();
            Transform transform = base.GetModelChildLocator().FindChild("GatRight");
            if (transform) {
                laserInstance = GameObject.Instantiate(prefab, transform.position, transform.rotation);
                laserInstance.transform.parent = transform.parent;
                laserInstanceEnd = laserInstance.GetComponent<ChildLocator>().FindChild("LaserEnd");
            }

            Transform transform2 = base.GetModelChildLocator().FindChild("GatLeft");
            if (transform2) {
                laserInstance2 = GameObject.Instantiate(prefab, transform2.position, transform2.rotation);
                laserInstance2.transform.parent = transform.parent;
                laserInstanceEnd2 = laserInstance.GetComponent<ChildLocator>().FindChild("LaserEnd");
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority) {
                stopwatch += Time.fixedDeltaTime;
                if (stopwatch >= interval) {
                    isActive = !isActive;
                    stopwatch = 0f;
                }

                if (ShouldFireLaser() && currentTarget) {
                    laserInstanceEnd.position = currentTarget.transform.position;
                    laserInstanceEnd2.position = currentTarget.transform.position;
                    CharacterBody body = currentTarget.GetComponent<CharacterBody>();
                    List<BuffDef> buffs = new() {
                        RoR2Content.Buffs.Cripple,
                        RoR2Content.Buffs.HealingDisabled,
                        RoR2Content.Buffs.Slow80,
                        RoR2Content.Buffs.Weak
                    };

                    foreach (BuffDef buff in buffs) {
                        body.AddTimedBuff(buff, Time.fixedDeltaTime * 5, 1);
                    }
                }
            }

            if (isActive && currentTarget) {
                if (laserInstance) laserInstance.SetActive(true);
                if (laserInstance2) laserInstance2.SetActive(true);
            }
            else {
                if (laserInstance) laserInstance.SetActive(false);
                if (laserInstance2) laserInstance2.SetActive(false);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (laserInstance) {
                Destroy(laserInstance);
            }
        }

        protected bool ShouldFireLaser() {
            if (!isActive) {
                return false;
            }

            if (base.characterBody?.master?.GetComponent<BaseAI>()) {
                BaseAI ai = base.characterBody.master.GetComponent<BaseAI>();
                if (ai.currentEnemy.gameObject) {
                    currentTarget = ai.currentEnemy.gameObject;
                    return true;
                }
            }

            return false;
        }
    }
}