using System;
using Landfall.TABS;
using UnityEngine;
using Landfall.TABS.UnitEditor;
using Landfall.TABS.Workshop;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using System.Reflection;
using Object = UnityEngine.Object;
using DM;
using Landfall.TABS.GameMode;
using LevelCreator;
using TGCore;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace HiddenUnits 
{
    public class HUMain 
    {
        public HUMain()
        {
            AssetBundle.LoadFromMemory(Properties.Resources.egyptmap);
            AssetBundle.LoadFromMemory(Properties.Resources.egyptmap2); 
            AssetBundle.LoadFromMemory(Properties.Resources.steampunkmap);

            var maps = ((MapAsset[])TGMain.landfallDb.GetField("m_orderedMapAssets")).ToList();
            var mapDict = new Dictionary<DatabaseID, int>();

            maps.AddRange(huMaps.LoadAllAssets<MapAsset>());
            
            maps = maps.OrderBy(x => x.m_mapIndex).ToList();
            for (var i = 0; i < maps.Count; i++)
            {
                maps[i].m_mapIndex = i;
                mapDict.Add(maps[i].Entity.GUID, i);
            }
            
            typeof(LandfallContentDatabase).GetField("m_orderedMapAssets", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(TGMain.landfallDb, maps.ToArray());
            typeof(LandfallContentDatabase).GetField("m_mapAssetIndexLookup", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(TGMain.landfallDb, mapDict);
            
            //for (var i = 0; i < 29; i++)
            //{
            //    newMapList.Add(maps[i]);
            //}
            //newMapList.Add(huMaps.LoadAsset<MapAsset>("Egypt1"));
            //newMapList.Add(huMaps.LoadAsset<MapAsset>("Egypt2"));
            //maps.RemoveRange(0, 29);
            //newMapList.AddRange(maps);

            new Harmony("HiddenUnis").PatchAll();

            foreach (var mat in hiddenUnits.LoadAllAssets<Material>()) if (Shader.Find(mat.shader.name)) mat.shader = Shader.Find(mat.shader.name);
            
            foreach (var unit in hiddenUnits.LoadAllAssets<UnitBlueprint>().Where(x => x.UnitBase != null))
            {
                foreach (var unitBase in TGMain.landfallDb.GetUnitBases().ToList().Where(unitBase => unitBase.name == unit.UnitBase.name))
                {
                    unit.UnitBase = unitBase;
                }

                //foreach (var weapon in TGMain.landfallDb.GetWeapons().ToList())
                //{
                //    if (unit.RightWeapon && weapon.name == unit.RightWeapon.name) unit.RightWeapon = weapon;
                //    if (unit.LeftWeapon && weapon.name == unit.LeftWeapon.name) unit.LeftWeapon = weapon;
                //}
            }

            var factions = hiddenUnits.LoadAllAssets<Faction>().ToList();
            
            foreach (var fac in factions)
            {
                var veryNewUnits = fac.Units.Where(x => x).OrderBy(x => x.GetUnitCost()).ToArray();
                fac.Units = veryNewUnits.ToArray();
                foreach (var vFac in TGMain.landfallDb.GetFactions().ToList()) 
                {
                    if (fac.Entity.Name == vFac.Entity.Name + "_NEW") 
                    {
                        var vFacUnits = new List<UnitBlueprint>(vFac.Units);
                        vFacUnits.AddRange(fac.Units);
                        vFac.Units = vFacUnits.Where(x => x).OrderBy(x => x.GetUnitCost()).ToArray();
                        Object.DestroyImmediate(fac);
                    }
                }
            }
            
            foreach (var lvl in hiddenUnits.LoadAllAssets<TABSCampaignLevelAsset>())
            {
                var egyptFaction = factions.Find(x => x.name.Contains("Egypt"));
                var steampunkFaction = factions.Find(x => x.name.Contains("Steampunk"));
                var huFaction = factions.Find(x => x.name.Contains("HiddenUnits"));
                var secretFaction = TGMain.landfallDb.GetFactions().ToList().Find(x => x.name.Contains("Secret"));
                
                var allowed = new List<Faction>();
                
                if (lvl.name.Contains("EgyptLevel"))
                { 
                    allowed.AddRange(TGMain.landfallDb.GetFactions().ToList().Where(x => x.m_displayFaction));
                    allowed.Remove(huFaction);
                    allowed.Remove(secretFaction);
                    allowed.Remove(egyptFaction);
                }
                else if (lvl.name.Contains("SteampunkLevel"))
                { 
                    allowed.AddRange(TGMain.landfallDb.GetFactions().ToList().Where(x => x.m_displayFaction));
                    allowed.Remove(huFaction);
                    allowed.Remove(secretFaction);
                    allowed.Remove(steampunkFaction);
                }
                else if (lvl.name.Contains("HULevel"))
                { 
                    allowed.AddRange(TGMain.landfallDb.GetFactions().ToList().Where(x => x.m_displayFaction));
                    allowed.Add(steampunkFaction);
                    allowed.Add(egyptFaction);
                }
                
                if (lvl.name.Contains("MapEquals"))
                {
                    var find = TGMain.landfallDb.GetMapAssetsOrdered().ToList().Find(x => x.name.Contains(lvl.name.Split(new[] { "MapEquals_" }, StringSplitOptions.RemoveEmptyEntries).Last()));
                    if (find) lvl.MapAsset = find;
                }

                var unitsToSearch = new List<TABSCampaignLevelAsset.TABSLayoutUnit>();
                unitsToSearch.AddRange(lvl.BlueUnits);
                unitsToSearch.AddRange(lvl.RedUnits);
                foreach (var unit in unitsToSearch)
                {
                    if (unit.m_unitBlueprint && unit.m_unitBlueprint.name.Contains("_VANILLA"))
                    {
                        var vanillaVersion = TGMain.landfallDb.GetUnitBlueprints().ToList().Find(x => x.name == unit.m_unitBlueprint.name.Replace("_VANILLA", ""));
                        if (vanillaVersion) unit.m_unitBlueprint = vanillaVersion;
                    }
                }
                
                if (lvl.AllowedFactions.Length < 1) lvl.AllowedFactions = allowed.ToArray();
            }

            //foreach (var audio in hiddenUnits.LoadAllAssets<AudioSource>())
            //{
            //    audio.outputAudioMixerGroup = ServiceLocator.GetService<GameModeService>().AudioSettings.AudioMixer.outputAudioMixerGroup;
            //}


            AudioMixer = Resources.FindObjectsOfTypeAll<AudioMixerGroup>()[0];
            var allGameObjects = hiddenUnits.LoadAllAssets<GameObject>();
            foreach (var obj in allGameObjects.Where(x => x.GetComponentInChildren<AudioSource>()))
            {
                foreach (var audio in obj.GetComponentsInChildren<AudioSource>())
                {
                    audio.outputAudioMixerGroup = AudioMixer;
                }
            }
            
            TGAddons.AddItems(hiddenUnits.LoadAllAssets<UnitBlueprint>(), factions,
                hiddenUnits.LoadAllAssets<TABSCampaignAsset>(), hiddenUnits.LoadAllAssets<TABSCampaignLevelAsset>(),
                hiddenUnits.LoadAllAssets<VoiceBundle>(), hiddenUnits.LoadAllAssets<FactionIcon>(),
                allGameObjects.Select(x => x.GetComponent<Unit>()), allGameObjects.Select(x => x.GetComponent<PropItem>()),
                allGameObjects.Select(x => x.GetComponent<SpecialAbility>()), allGameObjects.Select(x => x.GetComponent<WeaponItem>()),
                allGameObjects.Select(x => x.GetComponent<ProjectileEntity>()));
            TGMain.newSounds.AddRange(hiddenUnits.LoadAllAssets<SoundBank>());
            TGMain.objectTables.Add(huMaps.LoadAsset<DMEditorObjectTable>("HUEditorObjectTable"));
        }

        public static void CheckUnlockConditions()
        {
            var saveLoader = ServiceLocator.GetService<ISaveLoaderService>();

            foreach (var unlockCondition in moddedConditions.m_unlockConditions)
            {
                if (saveLoader.HasUnlockedSecret(unlockCondition.m_unlock))
                {
                    continue;
                }
            
                var allUnlocked = true;
                foreach (var unlock in unlockCondition.m_conditionUnlocks)
                {
                    if (!saveLoader.HasUnlockedSecret(unlock))
                    {
                        allUnlocked = false;
                    }
                }

                if (allUnlocked)
                {
                    saveLoader.UnlockSecret(unlockCondition.m_unlock);
                    ServiceLocator.GetService<ModalPanel>().OpenUnlockPanel(unlockCondition.m_unlockDescription, unlockCondition.m_unlockImage);
                }
            }
            
        }

        public static bool InfiniteScalingEnabled => HULauncher.configInfiniteScalingEnabled.Value;
        
        public static bool EvilBeesEnabled => HULauncher.configEvilBeesEnabled.Value;
        
        public static bool EvilTrainEnabled => HULauncher.configEvilTrainEnabled.Value;

        public static AssetBundle hiddenUnits = AssetBundle.LoadFromMemory(Properties.Resources.hiddenunits);

        public static AssetBundle huMaps = AssetBundle.LoadFromMemory(Properties.Resources.humaps);

        public static SecretUnlockConditions moddedConditions = hiddenUnits.LoadAsset<SecretUnlockConditions>("HUConditionUnlocker");

        public static AudioMixerGroup AudioMixer;
    }
}
