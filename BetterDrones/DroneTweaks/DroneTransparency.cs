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
    public class DroneTransparency {
        internal static void EnableChanges() {
            On.RoR2.CharacterBody.Start += (orig, self) => {
                orig(self);
                if (self.gameObject.GetComponent<VectorPID>() && self.bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical)) {
                    if (self.teamComponent.teamIndex == TeamIndex.Player) {
                        self.gameObject.AddComponent<DroneAlphaController>();
                    }
                }
            };
        }
    }

    public class DroneAlphaController : MonoBehaviour {
        private float Alpha => Main.Alpha;
        private List<Renderer> renderers = new();
        private MaterialPropertyBlock propertyBlock = new();

        private void Start() {
            CharacterBody body = GetComponent<CharacterBody>();
            if (!body) return;
            ModelLocator locator = body.modelLocator;
            if (!locator) return;
            if (!locator.modelTransform || !locator.modelTransform.GetComponent<CharacterModel>()) return;
            CharacterModel model = locator.modelTransform.GetComponent<CharacterModel>();
            
            foreach (CharacterModel.RendererInfo info in model.baseRendererInfos) {
                renderers.Add(info.renderer);
            }

            SceneCamera.onSceneCameraPreRender += UpdateTransparency;
        }

        private void UpdateTransparency(SceneCamera sceneCamera) {
            for (int i = 0; i < renderers.Count; i++) {
                Renderer renderer = renderers[i];
                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat("_Fade", Alpha);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void OnDestroy() {
            SceneCamera.onSceneCameraPreRender -= UpdateTransparency;
        }
    }
}