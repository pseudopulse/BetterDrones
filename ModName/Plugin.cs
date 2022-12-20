using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Reflection;

namespace ModName {
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI))]
    
    public class ModName : BaseUnityPlugin {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "ModAuthorName";
        public const string PluginName = "ModName";
        public const string PluginVersion = "1.0.0";

        public static AssetBundle bundle;
        public static BepInEx.Logging.ManualLogSource ModLogger;

        public void Awake() {
            // assetbundle loading 
            AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("dll name", "bundle name"));

            // set logger
            ModLogger = Logger;
        }
    }
}