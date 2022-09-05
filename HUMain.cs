using Landfall.TABS;
using UnityEngine;
using Landfall.TABS.UnitEditor;
using Landfall.TABS.Workshop;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using System.Reflection;

namespace HiddenUnits {

	public class HUMain {

        public HUMain()
        {

            AssetBundle.LoadFromMemory(Properties.Resources.egyptmap);
            AssetBundle.LoadFromMemory(Properties.Resources.egyptmap2);
            AssetBundle huMaps = AssetBundle.LoadFromMemory(Properties.Resources.humaps);
            var list = new List<MapAsset>();
            foreach (var map in huMaps.LoadAllAssets<MapAsset>()) {

                LandfallUnitDatabase.GetDatabase().MapList.AddItem(map);
                if (!map.name.Contains("Egypt")) {
                    list.Add(map);
                }
            }
            List<MapAsset> maps = (List<MapAsset>)typeof(LandfallUnitDatabase).GetField("Maps", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(LandfallUnitDatabase.GetDatabase());
            for (int i = 0; i < 29; i++)
            {
                list.Add(maps[i]);
            }
            list.Add(huMaps.LoadAsset<MapAsset>("Egypt1"));
            list.Add(huMaps.LoadAsset<MapAsset>("Egypt2"));
            maps.RemoveRange(0, 29);
            list.AddRange(maps);

            typeof(LandfallUnitDatabase).GetField("Maps", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(LandfallUnitDatabase.GetDatabase(), list);

            var db = LandfallUnitDatabase.GetDatabase();
            List<Faction> factions = (List<Faction>)typeof(LandfallUnitDatabase).GetField("Factions", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var fac in hiddenUnits.LoadAllAssets<Faction>()) {

                var theNew = new List<UnitBlueprint>(fac.Units);
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

            foreach (var sb in hiddenUnits.LoadAllAssets<SoundBank>()) {
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

            List<Faction> hbFactions = (List<Faction>)typeof(LandfallUnitDatabase).GetField("DefaultHotbarFactions", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var fac in hiddenUnits.LoadAllAssets<Faction>()) {

                db.FactionList.AddItem(fac);
                db.AddFactionWithID(fac);
                hbFactions.Add(fac);

                foreach (var unit in fac.Units) { if (!db.UnitList.Contains(unit)) { db.UnitList.AddItem(unit); db.AddUnitWithID(unit); } }
            }

            typeof(LandfallUnitDatabase).GetField("DefaultHotbarFactions", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, (from Faction fac in hbFactions orderby fac.index select fac).ToList());

            foreach (var unit in hiddenUnits.LoadAllAssets<UnitBlueprint>()) {

                if (!db.UnitList.Contains(unit)) { db.UnitList.AddItem(unit); db.AddUnitWithID(unit); }
                foreach (var b in db.UnitBaseList) { if (unit.UnitBase != null) { if (b.name == unit.UnitBase.name) { unit.UnitBase = b; } } }
                foreach (var b in db.WeaponList) { if (unit.RightWeapon != null && b.name == unit.RightWeapon.name) unit.RightWeapon = b; if (unit.LeftWeapon != null && b.name == unit.LeftWeapon.name) unit.LeftWeapon = b; }
            }

            List<VoiceBundle> vbList = (List<VoiceBundle>)typeof(LandfallUnitDatabase).GetField("VoiceBundles", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var vb in hiddenUnits.LoadAllAssets<VoiceBundle>()) {

                db.VoiceBundlesList.AddItem(vb);
                vbList.Add(vb);
            }
            typeof(LandfallUnitDatabase).GetField("VoiceBundles", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, vbList);

            int startID = 122436;
            List<FactionIcon> iconList = (List<FactionIcon>)typeof(LandfallUnitDatabase).GetField("FactionIcons", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var sprite in hiddenUnits.LoadAllAssets<Sprite>()) {

                if (sprite.name.Contains("Icons_128x128")) {

                    var icon = Object.Instantiate(db.FactionIconsList[0]);
                    icon.name = sprite.name;
                    icon.Entity.SpriteIcon = sprite;
                    icon.Entity.GUID = new DatabaseID(-2, startID);
                    startID++;
                    db.FactionIconsList.AddItem(icon);
                    iconList.Add(icon);
                }
            }
            typeof(LandfallUnitDatabase).GetField("FactionIcons", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, iconList);

            List<GameObject> stuff = (List<GameObject>)typeof(LandfallUnitDatabase).GetField("UnitBases", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            List<GameObject> stuff2 = (List<GameObject>)typeof(LandfallUnitDatabase).GetField("Weapons", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            List<GameObject> stuff3 = (List<GameObject>)typeof(LandfallUnitDatabase).GetField("Projectiles", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            List<GameObject> stuff4 = (List<GameObject>)typeof(LandfallUnitDatabase).GetField("CombatMoves", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            List<GameObject> stuff5 = (List<GameObject>)typeof(LandfallUnitDatabase).GetField("CharacterProps", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var objecting in hiddenUnits.LoadAllAssets<GameObject>()) {

                if (objecting != null) {

                    if (objecting.GetComponent<Unit>()) {
                        stuff.Add(objecting);
                    }
                    else if (objecting.GetComponent<WeaponItem>()) {
                        stuff2.Add(objecting);
                        int totalSubmeshes = 0;
                        foreach (var rend in objecting.GetComponentsInChildren<MeshFilter>()) {
                            if (rend.gameObject.activeSelf && rend.mesh.subMeshCount > 0 && rend.GetComponent<MeshRenderer>() && rend.GetComponent<MeshRenderer>().enabled == true) {

                                totalSubmeshes += rend.mesh.subMeshCount;
                            }
                        }
                        foreach (var rend in objecting.GetComponentsInChildren<SkinnedMeshRenderer>()) {
                            if (rend.gameObject.activeSelf == true && rend.sharedMesh.subMeshCount > 0 && rend.enabled) {

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
                    else if (objecting.GetComponent<ProjectileEntity>()) {
                        stuff3.Add(objecting);
                    }
                    else if (objecting.GetComponent<SpecialAbility>()) {
                        stuff4.Add(objecting);
                    }
                    else if (objecting.GetComponent<PropItem>()) {
                        stuff5.Add(objecting);
                        int totalSubmeshes = 0;
                        foreach (var rend in objecting.GetComponentsInChildren<MeshFilter>()) {
                            if (rend.gameObject.activeSelf == true && rend.mesh.subMeshCount > 0 && rend.GetComponent<MeshRenderer>() && rend.GetComponent<MeshRenderer>().enabled == true) {

                                totalSubmeshes += rend.mesh.subMeshCount;
                            }
                        }
                        foreach (var rend in objecting.GetComponentsInChildren<SkinnedMeshRenderer>()) {
                            if (rend.gameObject.activeSelf == true && rend.sharedMesh.subMeshCount > 0 && rend.enabled) {

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
            typeof(LandfallUnitDatabase).GetField("UnitBases", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, stuff);
            typeof(LandfallUnitDatabase).GetField("Weapons", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, stuff2);
            typeof(LandfallUnitDatabase).GetField("Projectiles", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, stuff3);
            typeof(LandfallUnitDatabase).GetField("CombatMoves", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, stuff4);
            typeof(LandfallUnitDatabase).GetField("CharacterProps", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, stuff5);

            new GameObject() {
                name = "Bullshit",
                hideFlags = HideFlags.HideAndDontSave
            }.AddComponent<HUSecretManager>();
            new GameObject() {
                name = "Bullshit: The Sequel",
                hideFlags = HideFlags.HideAndDontSave
            }.AddComponent<HUUnlockConditionsChecker>();

            var cringer = new GameObject()  {
                name = "Bullshit: The Trilogy",
                hideFlags = HideFlags.HideAndDontSave
            }.AddComponent<LanguageReader>();
            cringer.localizerCH = hiddenUnits.LoadAsset<GameObject>("Lang").GetComponent<LanguageReader>().localizerCH;
            cringer.localizerEN = hiddenUnits.LoadAsset<GameObject>("Lang").GetComponent<LanguageReader>().localizerEN;
            cringer.localizerES = hiddenUnits.LoadAsset<GameObject>("Lang").GetComponent<LanguageReader>().localizerES;
            cringer.localizerFR = hiddenUnits.LoadAsset<GameObject>("Lang").GetComponent<LanguageReader>().localizerFR;
            cringer.localizerDE = hiddenUnits.LoadAsset<GameObject>("Lang").GetComponent<LanguageReader>().localizerDE;
            cringer.localizerIT = hiddenUnits.LoadAsset<GameObject>("Lang").GetComponent<LanguageReader>().localizerIT;
            cringer.localizerJA = hiddenUnits.LoadAsset<GameObject>("Lang").GetComponent<LanguageReader>().localizerJA;
            cringer.localizerRU = hiddenUnits.LoadAsset<GameObject>("Lang").GetComponent<LanguageReader>().localizerRU;
            cringer.localizerPT_BR = hiddenUnits.LoadAsset<GameObject>("Lang").GetComponent<LanguageReader>().localizerPT_BR;


            var harmony = new Harmony("SussingtonBaka");
            harmony.PatchAll();

            foreach (var condition in hiddenUnits.LoadAsset<SecretUnlockConditions>("HUUnlockConditions").m_unlockConditions) {

                var allConditions = new List<SecretUnlockCondition>(Resources.FindObjectsOfTypeAll<SecretUnlockConditions>()[0].m_unlockConditions);
                allConditions.Add(condition);
                Resources.FindObjectsOfTypeAll<SecretUnlockConditions>()[0].m_unlockConditions = allConditions.ToArray();
            }

            var allMats = Resources.FindObjectsOfTypeAll<Material>().ToList();
            foreach (var mat in hiddenUnits.LoadAllAssets<Material>()) {

                if (Shader.Find(mat.shader.name)) { mat.shader = Shader.Find(mat.shader.name); }
                if (mat.name.Contains("Shield")) { mat.shader = Shader.Find("Landfall/Shield"); }
            }

            foreach (var camp in hiddenUnits.LoadAllAssets<TABSCampaignAsset>()) {
                db.AddCampaignWithID(camp);
                db.LandfallCampaignList.Add(camp);
            }

            foreach (var lvl in hiddenUnits.LoadAllAssets<TABSCampaignLevelAsset>()) {

                var allowedU = new List<UnitBlueprint>();
                var allowed = new List<Faction>();
                allowed.AddRange(hbFactions);
                allowed.Remove(hbFactions.Find(x => x.name.Contains("Secret")));
                if (lvl.name.Contains("Egypt") && !lvl.name.Contains("Misc")) { 
                    allowed.Remove(allowed.Find(x => x.name.Contains("Egypt"))); 
                }
                else if (lvl.name.Contains("Egypt") && lvl.name.Contains("Misc")) { 
                    var egypt = hbFactions.Find(x => x.name.Contains("Egypt"));
                    var legacy = hbFactions.Find(x => x.name.Contains("Legacy"));
                    var secret = hbFactions.Find(x => x.name.Contains("Secret"));
                    allowed.Clear();
                    allowed.Add(egypt);
                    allowed.Add(legacy);
                    allowed.Add(secret);
                    foreach (var unit in egypt.Units) { allowedU.Add(unit); }
                    allowedU.Add(hbFactions.Find(x => x.name.Contains("Secret")).Units.ToList().Find(x => x.name.Contains("Pot")));
                    allowedU.Add(hbFactions.Find(x => x.name.Contains("Secret")).Units.ToList().Find(x => x.name.Contains("Boomerang")));
                    allowedU.Add(hbFactions.Find(x => x.name.Contains("Secret")).Units.ToList().Find(x => x.name.Contains("Warrior")));
                    allowedU.Add(hbFactions.Find(x => x.name.Contains("Secret")).Units.ToList().Find(x => x.name.Contains("Sarcophagus")));
                    allowedU.Add(hbFactions.Find(x => x.name.Contains("Secret")).Units.ToList().Find(x => x.name.Contains("Selket")));
                    allowedU.Add(hbFactions.Find(x => x.name.Contains("Legacy")).Units.ToList().Find(x => x.name.Contains("Pharaoh")));
                }
                if (lvl.name.Contains("MapEquals")) {

                    if (lvl.name.Contains("MapEqualsSimulation")) { lvl.MapAsset = db.MapList.Find(x => x.name.Contains("Simulation_Day")); }
                    if (lvl.name.Contains("MapEqualsSimulationArcherTower")) { lvl.MapAsset = db.MapList.Find(x => x.name.Contains("Simulation_11_CastlewithArchertowers")); }
                    if (lvl.name.Contains("MapEqualsViking2")) { lvl.MapAsset = db.MapList.Find(x => x.name.Contains("Viking2")); }
                    if (lvl.name.Contains("MapEqualsAncient2")) { lvl.MapAsset = db.MapList.Find(x => x.name.Contains("Ancient2")); }
                    if (lvl.name.Contains("MapEqualsAncient3")) { lvl.MapAsset = db.MapList.Find(x => x.name.Contains("Ancient3")); }
                    if (lvl.name.Contains("MapEqualsTribal3")) { lvl.MapAsset = db.MapList.Find(x => x.name.Contains("Tribal3")); }
                }
                lvl.AllowedFactions = allowed.ToArray();
                lvl.AllowedUnits = allowedU.ToArray();

                db.AddCampaignLevelWithID(lvl);
                db.LandfallCampaignLevelList.Add(lvl);
            }

            var sarissaSpear = db.WeaponList.ToList().Find(x => x.name.Contains("Spear_Greek"));
            if (sarissaSpear) { sarissaSpear.GetComponent<Holdable>().holdableData.setRotation = false; }

            ServiceLocator.GetService<CustomContentLoaderModIO>().QuickRefresh(WorkshopContentType.Unit, null);
        }

        public static AssetBundle hiddenUnits = AssetBundle.LoadFromMemory(Properties.Resources.hiddenunits);

        public static Material wet;
    }
}
