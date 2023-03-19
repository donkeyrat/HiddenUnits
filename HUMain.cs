using System;
using Landfall.TABS;
using UnityEngine;
using Landfall.TABS.UnitEditor;
using Landfall.TABS.Workshop;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using System.Reflection;
using BitCode.Debug.Commands;
using Object = UnityEngine.Object;
using DM;
using Landfall.TABS.GameMode;
using TGCore;
using UnityEngine.Audio;

namespace HiddenUnits 
{

	public class HUMain 
    {

        public HUMain()
        {
            //AssetBundle.LoadFromMemory(Properties.Resources.egyptmap);
            //AssetBundle.LoadFromMemory(Properties.Resources.egyptmap2);
            
            var newMapList = new List<MapAsset>();
            var newMapDict = new Dictionary<DatabaseID, int>();
            
            var maps = ((MapAsset[])typeof(LandfallContentDatabase).GetField("m_orderedMapAssets", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(TGMain.landfallDb)).ToList();

            for (int i = 0; i < 29; i++)
            {
                newMapList.Add(maps[i]);
            }
            newMapList.Add(huMaps.LoadAsset<MapAsset>("Egypt1"));
            newMapList.Add(huMaps.LoadAsset<MapAsset>("Egypt2"));
            maps.RemoveRange(0, 29);
            newMapList.AddRange(maps);
            
            foreach (var map in huMaps.LoadAllAssets<MapAsset>()) 
            {
                if (!map.name.Contains("Egypt")) {
                    newMapList.Add(map);
                }
            }

            foreach (var map in newMapList)
            {
                newMapDict.Add(map.Entity.GUID, newMapList.IndexOf(map));
            }

            typeof(LandfallContentDatabase).GetField("m_orderedMapAssets", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(TGMain.landfallDb, newMapList.ToArray());
            typeof(LandfallContentDatabase).GetField("m_mapAssetIndexLookup", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(TGMain.landfallDb, newMapDict);

            new Harmony("HiddenUnis").PatchAll();

            var allConditions = new List<SecretUnlockCondition>(Resources.FindObjectsOfTypeAll<SecretUnlockConditions>()[0].m_unlockConditions);
            foreach (var condition in hiddenUnits.LoadAsset<SecretUnlockConditions>("HUUnlockConditions").m_unlockConditions) 
            {
                allConditions.Add(condition);
            }
            Resources.FindObjectsOfTypeAll<SecretUnlockConditions>()[0].m_unlockConditions = allConditions.ToArray();

            foreach (var mat in hiddenUnits.LoadAllAssets<Material>()) if (Shader.Find(mat.shader.name)) mat.shader = Shader.Find(mat.shader.name);

            foreach (var unit in hiddenUnits.LoadAllAssets<UnitBlueprint>().Where(x => x.UnitBase != null))
            {
                foreach (var unitBase in TGMain.landfallDb.GetUnitBases().ToList())
                {
                    if (unitBase.name == unit.UnitBase.name) unit.UnitBase = unitBase;
                }
                foreach (var weapon in TGMain.landfallDb.GetWeapons().ToList())
                {
                    if (unit.RightWeapon && weapon.name == unit.RightWeapon.name) unit.RightWeapon = weapon;
                    if (unit.LeftWeapon && weapon.name == unit.LeftWeapon.name) unit.LeftWeapon = weapon;
                }
            }

            foreach (var fac in hiddenUnits.LoadAllAssets<Faction>())
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
                var egyptFaction = hiddenUnits.LoadAllAssets<Faction>().ToList().Find(x => x.name.Contains("Egypt"));
                var secretFaction = TGMain.landfallDb.GetFactions().ToList().Find(x => x.name.Contains("Secret"));
                
                var allowedU = new List<UnitBlueprint>();
                var allowed = new List<Faction>();
                
                if (lvl.name.Contains("EgyptLevel"))
                { 
                    allowed.AddRange(TGMain.landfallDb.GetFactions().ToList().Where(x => x.m_displayFaction));
                    allowed.Remove(secretFaction);
                }
                else if (lvl.name.Contains("EgyptMiscLevel"))
                {
                    allowed.Add(egyptFaction);
                    allowed.Add(secretFaction);
                    
                    allowedU.AddRange(egyptFaction.Units);
                    
                    allowedU.Add(secretFaction.Units.ToList().Find(x => x.name.Contains("BoomerangThrower")));
                    allowedU.Add(secretFaction.Units.ToList().Find(x => x.name.Contains("PotThrower")));
                    allowedU.Add(secretFaction.Units.ToList().Find(x => x.name.Contains("Sarcophagus")));
                    allowedU.Add(secretFaction.Units.ToList().Find(x => x.name.Contains("Selket")));
                    allowedU.Add(secretFaction.Units.ToList().Find(x => x.name.Contains("RaWarrior")));
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
                    if (unit.m_unitBlueprint.name.Contains("_VANILLA"))
                    {
                        var vanillaVersion = TGMain.landfallDb.GetUnitBlueprints().ToList().Find(x => x.name == unit.m_unitBlueprint.name.Replace("_VANILLA", ""));
                        if (vanillaVersion) unit.m_unitBlueprint = vanillaVersion;
                    }
                }
                
                lvl.AllowedFactions = allowed.ToArray();
                lvl.AllowedUnits = allowedU.ToArray();
            }

            foreach (var prop in hiddenUnits.LoadAllAssets<PropItem>())
            {
                var totalSubmeshes = prop.GetComponentsInChildren<MeshFilter>().Where(rend => rend.gameObject.activeSelf && rend.gameObject.activeInHierarchy && rend.mesh.subMeshCount > 0 && rend.GetComponent<MeshRenderer>() && rend.GetComponent<MeshRenderer>().enabled).Sum(rend => rend.mesh.subMeshCount) + prop.GetComponentsInChildren<SkinnedMeshRenderer>().Where(rend => rend.gameObject.activeSelf && rend.sharedMesh.subMeshCount > 0 && rend.enabled).Sum(rend => rend.sharedMesh.subMeshCount);
                if (totalSubmeshes > 0) 
                {
                    float average = 1f / totalSubmeshes;
                    var averageList = new List<float>();
                    for (var i = 0; i < totalSubmeshes - 1; i++) averageList.Add(average);
                    
                    prop.SubmeshArea = averageList.ToArray();
                }
            }
            
            foreach (var weapon in hiddenUnits.LoadAllAssets<WeaponItem>())
            {
                var totalSubmeshes = weapon.GetComponentsInChildren<MeshFilter>().Where(rend => rend.gameObject.activeSelf && rend.gameObject.activeInHierarchy && rend.mesh.subMeshCount > 0 && rend.GetComponent<MeshRenderer>() && rend.GetComponent<MeshRenderer>().enabled).Sum(rend => rend.mesh.subMeshCount) + weapon.GetComponentsInChildren<SkinnedMeshRenderer>().Where(rend => rend.gameObject.activeSelf && rend.sharedMesh.subMeshCount > 0 && rend.enabled).Sum(rend => rend.sharedMesh.subMeshCount);
                if (totalSubmeshes > 0) 
                {
                    float average = 1f / totalSubmeshes;
                    var averageList = new List<float>();
                    for (var i = 0; i < totalSubmeshes - 1; i++) averageList.Add(average);
                    
                    weapon.SubmeshArea = averageList.ToArray();
                }
            }

            foreach (var audio in hiddenUnits.LoadAllAssets<AudioSource>()) 
            {
                audio.outputAudioMixerGroup = ServiceLocator.GetService<GameModeService>().AudioSettings.AudioMixer.outputAudioMixerGroup;
            }
            
            foreach (var lvl in hiddenUnits.LoadAllAssets<TABSCampaignLevelAsset>())
            {
                var allowedU = new List<UnitBlueprint>();
                var allowed = new List<Faction>();
                allowed.AddRange(ContentDatabase.Instance().LandfallContentDatabase.GetFactions().ToList());
                allowed.Remove(allowed.Find(x => x.name.Contains("Secret")));
                var sub = allowed.Find(x => x.name.Contains("Subunits"));
                if (sub) allowed.Remove(sub);
                if (lvl.name.Contains("Egypt") && !lvl.name.Contains("Misc")) { 
                    allowed.Remove(allowed.Find(x => x.name.Contains("Egypt"))); 
                }
                else if (lvl.name.Contains("Egypt") && lvl.name.Contains("Misc")) { 
                    var egypt = ContentDatabase.Instance().LandfallContentDatabase.GetFactions().ToList().Find(x => x.name.Contains("Egypt"));
                    var legacy = ContentDatabase.Instance().LandfallContentDatabase.GetFactions().ToList().Find(x => x.name.Contains("Legacy"));
                    var secret = ContentDatabase.Instance().LandfallContentDatabase.GetFactions().ToList().Find(x => x.name.Contains("Secret"));
                    allowed.Clear();
                    allowed.Add(egypt);
                    allowed.Add(legacy);
                    allowed.Add(secret);
                    foreach (var unit in egypt.Units) { allowedU.Add(unit); }
                    allowedU.Add(ContentDatabase.Instance().LandfallContentDatabase.GetFactions().ToList().Find(x => x.name.Contains("Secret")).Units.ToList().Find(x => x.name.Contains("Pot")));
                    allowedU.Add(ContentDatabase.Instance().LandfallContentDatabase.GetFactions().ToList().Find(x => x.name.Contains("Secret")).Units.ToList().Find(x => x.name.Contains("Boomerang")));
                    allowedU.Add(ContentDatabase.Instance().LandfallContentDatabase.GetFactions().ToList().Find(x => x.name.Contains("Secret")).Units.ToList().Find(x => x.name.Contains("Warrior")));
                    allowedU.Add(ContentDatabase.Instance().LandfallContentDatabase.GetFactions().ToList().Find(x => x.name.Contains("Secret")).Units.ToList().Find(x => x.name.Contains("Sarcophagus")));
                    allowedU.Add(ContentDatabase.Instance().LandfallContentDatabase.GetFactions().ToList().Find(x => x.name.Contains("Secret")).Units.ToList().Find(x => x.name.Contains("Selket")));
                    allowedU.Add(ContentDatabase.Instance().LandfallContentDatabase.GetFactions().ToList().Find(x => x.name.Contains("Legacy")).Units.ToList().Find(x => x.name.Contains("Pharaoh")));
                }
                if (lvl.name.Contains("MapEquals"))
                {
                    var find = ContentDatabase.Instance().LandfallContentDatabase.GetMapAssetsOrdered().ToList().Find(x => x.name.Contains(lvl.name.Split(new string[] { "MapEquals_" }, StringSplitOptions.RemoveEmptyEntries).Last()));
                    if (find) lvl.MapAsset = find;
                }
                lvl.AllowedFactions = allowed.ToArray();
                lvl.AllowedUnits = allowedU.ToArray();
            }
            
            TGAddons.AddItems(hiddenUnits.LoadAllAssets<UnitBlueprint>(), hiddenUnits.LoadAllAssets<Faction>(),
                hiddenUnits.LoadAllAssets<TABSCampaignAsset>(), hiddenUnits.LoadAllAssets<TABSCampaignLevelAsset>(),
                hiddenUnits.LoadAllAssets<VoiceBundle>(), hiddenUnits.LoadAllAssets<FactionIcon>(),
                hiddenUnits.LoadAllAssets<Unit>(), hiddenUnits.LoadAllAssets<PropItem>(),
                hiddenUnits.LoadAllAssets<SpecialAbility>(), hiddenUnits.LoadAllAssets<WeaponItem>(),
                hiddenUnits.LoadAllAssets<ProjectileEntity>());
            TGMain.newSounds.AddRange(hiddenUnits.LoadAllAssets<SoundBank>());
        }

        public static bool InfiniteScalingEnabled => HULauncher.configInfiniteScalingEnabled.Value;

        public static AssetBundle hiddenUnits;// = AssetBundle.LoadFromMemory(Properties.Resources.hiddenunits);

        public static AssetBundle huMaps;// = AssetBundle.LoadFromMemory(Properties.Resources.humaps);
    }
}
