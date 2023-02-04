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
using UnityEngine.Audio;

namespace HiddenUnits 
{

	public class HUMain 
    {

        public HUMain()
        {
            var db = ContentDatabase.Instance();

            //AssetBundle.LoadFromMemory(Properties.Resources.egyptmap);
            //AssetBundle.LoadFromMemory(Properties.Resources.egyptmap2);
            var newMapList = new List<MapAsset>();
            var newMapDict = new Dictionary<DatabaseID, int>();
            
            var maps = ((MapAsset[])typeof(LandfallContentDatabase).GetField("m_orderedMapAssets", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db.LandfallContentDatabase)).ToList();

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

            typeof(LandfallContentDatabase).GetField("m_orderedMapAssets", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db.LandfallContentDatabase, newMapList.ToArray());
            typeof(LandfallContentDatabase).GetField("m_mapAssetIndexLookup", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db.LandfallContentDatabase, newMapDict);
            
            
            var factions = db.LandfallContentDatabase.GetFactions().ToList();
            foreach (var fac in hiddenUnits.LoadAllAssets<Faction>()) 
            {
                var veryNewUnits = (
                    from UnitBlueprint unit
                    in fac.Units
                    orderby unit.GetUnitCost()
                    select unit).ToList();
                fac.Units = veryNewUnits.ToArray();
                foreach (var vFac in factions) {

                    if (fac.Entity.Name == vFac.Entity.Name + "_NEW") {

                        var vFacUnits = new List<UnitBlueprint>(vFac.Units);
                        vFacUnits.AddRange(fac.Units);
                        var newUnits = (
                            from UnitBlueprint unit
                            in vFacUnits
                            orderby unit.GetUnitCost()
                            select unit).ToList();
                        vFac.Units = newUnits.ToArray();
                        Object.DestroyImmediate(fac);
                    }
                }
            }

            foreach (var sb in hiddenUnits.LoadAllAssets<SoundBank>()) 
            {
                if (sb.name.Contains("Sound")) {
                    var vsb = ServiceLocator.GetService<SoundPlayer>().soundBank;
                    foreach (var sound in sb.Categories) { sound.categoryMixerGroup = vsb.Categories[0].categoryMixerGroup; }
                    var cat = vsb.Categories.ToList();
                    cat.AddRange(sb.Categories);
                    vsb.Categories = cat.ToArray();
                }
                if (sb.name.Contains("Music")) {
                    var vsb = ServiceLocator.GetService<MusicHandler>().bank;
                    var cat = vsb.Categories.ToList();
                    cat.AddRange(sb.Categories);
                    foreach (var categ in sb.Categories) {
                        foreach (var sound in categ.soundEffects) {
                            var song = new SongInstance();
                            song.clip = sound.clipTypes[0].clips[0];
                            song.soundEffectInstance = sound;
                            song.songRef = categ.categoryName + "/" + sound.soundRef;
                            ServiceLocator.GetService<MusicHandler>().m_songs.Add(song.songRef, song);
                        }
                    }
                    vsb.Categories = cat.ToArray();
                }
            }

            new GameObject {
                name = "Bullshit",
                hideFlags = HideFlags.HideAndDontSave
            }.AddComponent<HUSceneManager>();

            var langReader = hiddenUnits.LoadAsset<GameObject>("Lang").GetComponent<LanguageReader>();
            var cringer = new GameObject  {
                name = "Bullshit: The Trilogy",
                hideFlags = HideFlags.HideAndDontSave
            }.AddComponent<LanguageReader>();
            cringer.localizerCH = langReader.localizerCH;
            cringer.localizerEN = langReader.localizerEN;
            cringer.localizerES = langReader.localizerES;
            cringer.localizerFR = langReader.localizerFR;
            cringer.localizerDE = langReader.localizerDE;
            cringer.localizerIT = langReader.localizerIT;
            cringer.localizerJA = langReader.localizerJA;
            cringer.localizerRU = langReader.localizerRU;
            cringer.localizerPT_BR = langReader.localizerPT_BR;

            new Harmony("SussingtonBaka").PatchAll();

            var allConditions = new List<SecretUnlockCondition>(Resources.FindObjectsOfTypeAll<SecretUnlockConditions>()[0].m_unlockConditions);
            foreach (var condition in hiddenUnits.LoadAsset<SecretUnlockConditions>("HUUnlockConditions").m_unlockConditions) 
            {
                allConditions.Add(condition);
            }
            Resources.FindObjectsOfTypeAll<SecretUnlockConditions>()[0].m_unlockConditions = allConditions.ToArray();

            foreach (var mat in hiddenUnits.LoadAllAssets<Material>()) if (Shader.Find(mat.shader.name)) mat.shader = Shader.Find(mat.shader.name);


            var infiniteScaling = CreateSetting(SettingsInstance.SettingsType.Options, "Make units scale infinitely", "Toggles Mathematician/Philosopher projectiles to be able to infinitely scale unit parts.", "BUG", 0f, new[] { "Disabled", "Enabled" });
            infiniteScaling.OnValueChanged += InfiniteScaling_OnValueChanged;
            
            foreach (var unit in hiddenUnits.LoadAllAssets<UnitBlueprint>())
            {
                newUnits.Add(unit);
                foreach (var b in db.LandfallContentDatabase.GetUnitBases().ToList()) { if (unit.UnitBase != null) { if (b.name == unit.UnitBase.name) { unit.UnitBase = b; } } }
                foreach (var b in db.LandfallContentDatabase.GetWeapons().ToList()) { if (unit.RightWeapon != null && b.name == unit.RightWeapon.name) unit.RightWeapon = b; if (unit.LeftWeapon != null && b.name == unit.LeftWeapon.name) unit.LeftWeapon = b; }
            }

            foreach (var fac in hiddenUnits.LoadAllAssets<Faction>()) newFactions.Add(fac);
            
            foreach (var campaign in hiddenUnits.LoadAllAssets<TABSCampaignAsset>()) newCampaigns.Add(campaign);
            
            foreach (var lvl in hiddenUnits.LoadAllAssets<TABSCampaignLevelAsset>())
            {
                newCampaignLevels.Add(lvl);
                
                var egypt = newFactions.Find(x => x.name.Contains("Egypt"));
                var subunits = db.LandfallContentDatabase.GetFactions().ToList().Find(x => x.name.Contains("Subunits"));
                var secret = db.LandfallContentDatabase.GetFactions().ToList().Find(x => x.name.Contains("Secret"));
                
                var allowedU = new List<UnitBlueprint>();
                var allowed = new List<Faction>();
                
                if (lvl.name.Contains("EgyptLevel"))
                { 
                    allowed.AddRange(db.LandfallContentDatabase.GetFactions().ToList());
                    allowed.Remove(secret);
                    if (subunits) allowed.Remove(subunits);
                }
                else if (lvl.name.Contains("EgyptMiscLevel"))
                {
                    allowed.Add(egypt);
                    allowed.Add(secret);
                    
                    allowedU.AddRange(egypt.Units);
                    
                    allowedU.Add(secret.Units.ToList().Find(x => x.name.Contains("BoomerangThrower")));
                    allowedU.Add(secret.Units.ToList().Find(x => x.name.Contains("PotThrower")));
                    allowedU.Add(secret.Units.ToList().Find(x => x.name.Contains("Sarcophagus")));
                    allowedU.Add(secret.Units.ToList().Find(x => x.name.Contains("Selket")));
                    allowedU.Add(secret.Units.ToList().Find(x => x.name.Contains("RaWarrior")));
                }
                
                if (lvl.name.Contains("MapEquals"))
                {
                    var find = db.LandfallContentDatabase.GetMapAssetsOrdered().ToList().Find(x => x.name.Contains(lvl.name.Split(new[] { "MapEquals_" }, StringSplitOptions.RemoveEmptyEntries).Last()));
                    if (find) lvl.MapAsset = find;
                }
                
                lvl.AllowedFactions = allowed.ToArray();
                lvl.AllowedUnits = allowedU.ToArray();
            }

            foreach (var vb in hiddenUnits.LoadAllAssets<VoiceBundle>()) newVoiceBundles.Add(vb);
            
            foreach (var icon in hiddenUnits.LoadAllAssets<FactionIcon>()) newFactionIcons.Add(icon);

            foreach (var objecting in hiddenUnits.LoadAllAssets<GameObject>()) 
            {
                if (objecting != null) {

                    foreach (var audio in objecting.GetComponentsInChildren<AudioSource>(true))
                    {
                        audio.outputAudioMixerGroup =
                            ServiceLocator.GetService<GameModeService>().AudioSettings.AudioMixer.outputAudioMixerGroup;
                    }
                    
                    if (objecting.GetComponent<Unit>())
                    {
                        if (!objecting.GetComponent<Outline>()) objecting.AddComponent<Outline>().OutlineWidth = 1f;
                        newBases.Add(objecting);
                    }
                    else if (objecting.GetComponent<WeaponItem>()) {
                        newWeapons.Add(objecting);
                        int totalSubmeshes = 0;
                        foreach (var rend in objecting.GetComponentsInChildren<MeshFilter>()) {
                            if (rend.gameObject.activeSelf && rend.gameObject.activeInHierarchy && rend.mesh.subMeshCount > 0 && rend.GetComponent<MeshRenderer>() && rend.GetComponent<MeshRenderer>().enabled == true) {

                                totalSubmeshes += rend.mesh.subMeshCount;
                            }
                        }
                        foreach (var rend in objecting.GetComponentsInChildren<SkinnedMeshRenderer>()) {
                            if (rend.gameObject.activeSelf && rend.sharedMesh.subMeshCount > 0 && rend.enabled) {

                                totalSubmeshes += rend.sharedMesh.subMeshCount;
                            }
                        }
                        if (totalSubmeshes != 0) {
                            float average = 1f / totalSubmeshes;
                            var averageList = new List<float>();
                            for (int i = 0; i < totalSubmeshes; i++) { averageList.Add(average); }
                            objecting.GetComponent<WeaponItem>().SubmeshArea = null;
                            objecting.GetComponent<WeaponItem>().SubmeshArea = averageList.ToArray();
                        }
                    }
                    else if (objecting.GetComponent<ProjectileEntity>()) newProjectiles.Add(objecting);
                    else if (objecting.GetComponent<SpecialAbility>()) newAbilities.Add(objecting);
                    else if (objecting.GetComponent<PropItem>()) {
                        newProps.Add(objecting);
                        int totalSubmeshes = 0;
                        foreach (var rend in objecting.GetComponentsInChildren<MeshFilter>()) {
                            if (rend.gameObject.activeSelf && rend.gameObject.activeInHierarchy && rend.mesh.subMeshCount > 0 && rend.GetComponent<MeshRenderer>() && rend.GetComponent<MeshRenderer>().enabled == true) {

                                totalSubmeshes += rend.mesh.subMeshCount;
                            }
                        }
                        foreach (var rend in objecting.GetComponentsInChildren<SkinnedMeshRenderer>()) {
                            if (rend.gameObject.activeSelf && rend.sharedMesh.subMeshCount > 0 && rend.enabled) {

                                totalSubmeshes += rend.sharedMesh.subMeshCount;
                            }
                        }
                        if (totalSubmeshes != 0) {
                            float average = 1f / totalSubmeshes;
                            var averageList = new List<float>();
                            for (int i = 0; i < totalSubmeshes; i++) { averageList.Add(average); }
                            objecting.GetComponent<PropItem>().SubmeshArea = null;
                            objecting.GetComponent<PropItem>().SubmeshArea = averageList.ToArray();
                        }
                    }
                }
            }
            
            AddContentToDatabase();
            
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
        }

        public void AddContentToDatabase()
        {
	        Dictionary<DatabaseID, UnityEngine.Object> nonStreamableAssets = (Dictionary<DatabaseID, UnityEngine.Object>)typeof(AssetLoader).GetField("m_nonStreamableAssets", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ContentDatabase.Instance().AssetLoader);
	        
            var db = ContentDatabase.Instance().LandfallContentDatabase;
            
            Dictionary<DatabaseID, UnitBlueprint> units = (Dictionary<DatabaseID, UnitBlueprint>)typeof(LandfallContentDatabase).GetField("m_unitBlueprints", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var unit in newUnits)
            {
	            if (!units.ContainsKey(unit.Entity.GUID))
	            {
		            units.Add(unit.Entity.GUID, unit);
		            nonStreamableAssets.Add(unit.Entity.GUID, unit);
	            }
            }
            typeof(LandfallContentDatabase).GetField("m_unitBlueprints", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, units);
            
            Dictionary<DatabaseID, Faction> factions = (Dictionary<DatabaseID, Faction>)typeof(LandfallContentDatabase).GetField("m_factions", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            List<DatabaseID> defaultHotbarFactions = (List<DatabaseID>)typeof(LandfallContentDatabase).GetField("m_defaultHotbarFactionIds", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var faction in newFactions)
            {
	            if (!factions.ContainsKey(faction.Entity.GUID))
	            {
		            factions.Add(faction.Entity.GUID, faction);
		            nonStreamableAssets.Add(faction.Entity.GUID, faction);
		            defaultHotbarFactions.Add(faction.Entity.GUID);
	            }
            }
            typeof(LandfallContentDatabase).GetField("m_factions", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, factions);
            typeof(LandfallContentDatabase).GetField("m_defaultHotbarFactionIds", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, defaultHotbarFactions.OrderBy(x => factions[x].index).ToList());

            Dictionary<DatabaseID, TABSCampaignAsset> campaigns = (Dictionary<DatabaseID, TABSCampaignAsset>)typeof(LandfallContentDatabase).GetField("m_campaigns", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var campaign in newCampaigns)
            {
	            if (!campaigns.ContainsKey(campaign.Entity.GUID))
	            {
		            campaigns.Add(campaign.Entity.GUID, campaign);
		            nonStreamableAssets.Add(campaign.Entity.GUID, campaign);
	            }
            }
            typeof(LandfallContentDatabase).GetField("m_campaigns", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, campaigns);
            
            Dictionary<DatabaseID, TABSCampaignLevelAsset> campaignLevels = (Dictionary<DatabaseID, TABSCampaignLevelAsset>)typeof(LandfallContentDatabase).GetField("m_campaignLevels", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var campaignLevel in newCampaignLevels)
            {
	            if (!campaignLevels.ContainsKey(campaignLevel.Entity.GUID))
	            {
		            campaignLevels.Add(campaignLevel.Entity.GUID, campaignLevel);
		            nonStreamableAssets.Add(campaignLevel.Entity.GUID, campaignLevel);
	            }
            }
            typeof(LandfallContentDatabase).GetField("m_campaignLevels", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, campaignLevels);
            
            Dictionary<DatabaseID, VoiceBundle> voiceBundles = (Dictionary<DatabaseID, VoiceBundle>)typeof(LandfallContentDatabase).GetField("m_voiceBundles", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var voiceBundle in newVoiceBundles)
            {
	            if (!voiceBundles.ContainsKey(voiceBundle.Entity.GUID))
	            {
		            voiceBundles.Add(voiceBundle.Entity.GUID, voiceBundle);
		            nonStreamableAssets.Add(voiceBundle.Entity.GUID, voiceBundle);
	            }
            }
            typeof(LandfallContentDatabase).GetField("m_voiceBundles", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, voiceBundles);
            
            List<DatabaseID> factionIcons = (List<DatabaseID>)typeof(LandfallContentDatabase).GetField("m_factionIconIds", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var factionIcon in newFactionIcons)
            {
	            if (!factionIcons.Contains(factionIcon.Entity.GUID))
	            {
		            factionIcons.Add(factionIcon.Entity.GUID);
		            nonStreamableAssets.Add(factionIcon.Entity.GUID, factionIcon);
	            }
            }
            typeof(LandfallContentDatabase).GetField("m_factionIconIds", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, factionIcons);
            
            Dictionary<DatabaseID, GameObject> unitBases = (Dictionary<DatabaseID, GameObject>)typeof(LandfallContentDatabase).GetField("m_unitBases", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var unitBase in newBases)
            {
	            if (!unitBases.ContainsKey(unitBase.GetComponent<Unit>().Entity.GUID))
	            {
		            unitBases.Add(unitBase.GetComponent<Unit>().Entity.GUID, unitBase);
		            nonStreamableAssets.Add(unitBase.GetComponent<Unit>().Entity.GUID, unitBase);
	            }
            }
            typeof(LandfallContentDatabase).GetField("m_unitBases", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, unitBases);
            
            Dictionary<DatabaseID, GameObject> props = (Dictionary<DatabaseID, GameObject>)typeof(LandfallContentDatabase).GetField("m_characterProps", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var prop in newProps)
            {
	            if (!props.ContainsKey(prop.GetComponent<PropItem>().Entity.GUID))
	            {
		            props.Add(prop.GetComponent<PropItem>().Entity.GUID, prop);
		            nonStreamableAssets.Add(prop.GetComponent<PropItem>().Entity.GUID, prop);
	            }
            }
            typeof(LandfallContentDatabase).GetField("m_characterProps", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, props);
            
            Dictionary<DatabaseID, GameObject> abilities = (Dictionary<DatabaseID, GameObject>)typeof(LandfallContentDatabase).GetField("m_combatMoves", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var ability in newAbilities)
            {
	            if (!abilities.ContainsKey(ability.GetComponent<SpecialAbility>().Entity.GUID))
	            {
		            abilities.Add(ability.GetComponent<SpecialAbility>().Entity.GUID, ability);
		            nonStreamableAssets.Add(ability.GetComponent<SpecialAbility>().Entity.GUID, ability);
	            }
            }
            typeof(LandfallContentDatabase).GetField("m_combatMoves", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, abilities);
            
            Dictionary<DatabaseID, GameObject> weapons = (Dictionary<DatabaseID, GameObject>)typeof(LandfallContentDatabase).GetField("m_weapons", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var weapon in newWeapons)
            {
	            if (!weapons.ContainsKey(weapon.GetComponent<WeaponItem>().Entity.GUID))
	            {
		            weapons.Add(weapon.GetComponent<WeaponItem>().Entity.GUID, weapon);
		            nonStreamableAssets.Add(weapon.GetComponent<WeaponItem>().Entity.GUID, weapon);
	            }
            }
            typeof(LandfallContentDatabase).GetField("m_weapons", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, weapons);
            
            Dictionary<DatabaseID, GameObject> projectiles = (Dictionary<DatabaseID, GameObject>)typeof(LandfallContentDatabase).GetField("m_projectiles", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var proj in newProjectiles)
            {
	            if (!projectiles.ContainsKey(proj.GetComponent<ProjectileEntity>().Entity.GUID))
	            {
		            projectiles.Add(proj.GetComponent<ProjectileEntity>().Entity.GUID, proj);
		            nonStreamableAssets.Add(proj.GetComponent<ProjectileEntity>().Entity.GUID, proj);
	            }
            }
            typeof(LandfallContentDatabase).GetField("m_projectiles", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, projectiles);

            typeof(AssetLoader).GetField("m_nonStreamableAssets", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(ContentDatabase.Instance().AssetLoader, nonStreamableAssets);
            
            ServiceLocator.GetService<CustomContentLoaderModIO>().QuickRefresh(WorkshopContentType.Unit, null);
        }
        
        private void InfiniteScaling_OnValueChanged(int value)
        {
            InfiniteScaling = value != 0;
        }
        
        private SettingsInstance CreateSetting(SettingsInstance.SettingsType settingsType, string settingName, string toolTip, string settingListToAddTo, float defaultValue, string[] options = null, float min = 0f, float max = 1f) 
        {
            var setting = new SettingsInstance
            {
                settingName = settingName,
                toolTip = toolTip,
                m_settingsKey = settingName,
                settingsType = settingsType,
                options = options,
                min = min,
                max = max,
                defaultValue = (int)defaultValue,
                currentValue = (int)defaultValue,
                defaultSliderValue = defaultValue,
                currentSliderValue = defaultValue
            };

            var global = ServiceLocator.GetService<GlobalSettingsHandler>();
            SettingsInstance[] listToAdd;
            if (settingListToAddTo == "BUG") listToAdd = global.BugsSettings;
            else if (settingListToAddTo == "VIDEO") listToAdd = global.VideoSettings;
            else if (settingListToAddTo == "AUDIO") listToAdd = global.AudioSettings;
            else if (settingListToAddTo == "CONTROLS") listToAdd = global.ControlSettings;
            else listToAdd = global.GameplaySettings;

            var list = listToAdd.ToList();
            list.Add(setting);

            switch (settingListToAddTo)
            {
                case "BUG":
                    typeof(GlobalSettingsHandler).GetField("m_bugsSettings", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(global, list.ToArray());
                    break;
                case "VIDEO":
                    typeof(GlobalSettingsHandler).GetField("m_videoSettings", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(global, list.ToArray());
                    break;
                case "AUDIO":
                    typeof(GlobalSettingsHandler).GetField("m_audioSettings", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(global, list.ToArray());
                    break;
                case "CONTROLS":
                    typeof(GlobalSettingsHandler).GetField("m_controlSettings", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(global, list.ToArray());
                    break;
                default:
                    typeof(GlobalSettingsHandler).GetField("m_gameplaySettings", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(global, list.ToArray());
                    break;
            }

            return setting;
        }
        
        public List<UnitBlueprint> newUnits = new List<UnitBlueprint>();

        public List<Faction> newFactions = new List<Faction>();
        
        public List<TABSCampaignAsset> newCampaigns = new List<TABSCampaignAsset>();
        
        public List<TABSCampaignLevelAsset> newCampaignLevels = new List<TABSCampaignLevelAsset>();
        
        public List<VoiceBundle> newVoiceBundles = new List<VoiceBundle>();
        
        public List<FactionIcon> newFactionIcons = new List<FactionIcon>();
        
        public List<GameObject> newBases = new List<GameObject>();

        public List<GameObject> newProps = new List<GameObject>();
        
        public List<GameObject> newAbilities = new List<GameObject>();

        public List<GameObject> newWeapons = new List<GameObject>();

        public List<GameObject> newProjectiles = new List<GameObject>();

        public static bool InfiniteScaling = false;

        public static AssetBundle hiddenUnits;// = AssetBundle.LoadFromMemory(Properties.Resources.hiddenunits);

        public static AssetBundle huMaps;// = AssetBundle.LoadFromMemory(Properties.Resources.humaps);
    }
}
