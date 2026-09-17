using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using Landfall.TABS;
using Pathfinding;
using TFBGames;
using TGCore;
using TGCore.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HiddenUnits 
{
    [BepInPlugin("teamgrad.hiddenunits", "Hidden Units", "2.0.7")]
    [BepInDependency("teamgrad.core")]
	public class HULauncher : TGMod
	{
		public override void Launch()
		{
			new HUMain();
		}

		public override void AddSettings()
		{
            configInfiniteScalingEnabled = Config.Bind("Bug", "InfiniteScalingEnabled", false, "Toggles Mathematician/Philosopher projectiles infinitely scaling unit parts.");
            var infiniteScaling = TGAddons.CreateSetting(SettingsInstance.SettingsType.Options, "Infinite scaling science", "Toggles Mathematician/Philosopher projectiles infinitely scaling unit parts.", "BUG", 0f, configInfiniteScalingEnabled.Value ? 1 : 0, new[] { "Off", "On" });
            infiniteScaling.OnValueChanged += delegate(int value)
            {
                configInfiniteScalingEnabled.Value = value == 1;
            };
            
            configEvilBeesEnabled = Config.Bind("Bug", "EvilBeesEnabled", false, "The bees are filled with a blood-red rage.");
            var evilBees = TGAddons.CreateSetting(SettingsInstance.SettingsType.Options, "Make bees very angry", "The bees are filled with a blood-red rage.", "BUG", 0f, configEvilBeesEnabled.Value ? 1 : 0, new[] { "Off", "On" });
            evilBees.OnValueChanged += delegate(int value)
            {
                configEvilBeesEnabled.Value = value == 1;
            };
            
            configEvilTrainEnabled = Config.Bind("Bug", "EvilTrainEnabled", false, "The train station is quite busy today...");
            var evilTrain = TGAddons.CreateSetting(SettingsInstance.SettingsType.Options, "Make trains non-stop", "The train station is quite busy today...", "BUG", 0f, configEvilTrainEnabled.Value ? 1 : 0, new[] { "Off", "On" });
            evilTrain.OnValueChanged += delegate(int value)
            {
                configEvilTrainEnabled.Value = value == 1;
            };
		}

		public override void SceneManager(Scene scene, LoadSceneMode laodSceneMode)
		{
            if (scene.path == "Assets/11 Scenes/MainMenu.unity")
            {

                var saveLoader = ServiceLocator.GetService<ISaveLoaderService>();
                if (!saveLoader.HasUnlockedSecret("SECRET_EGYPT")) {

                    saveLoader.UnlockSecret("SECRET_EGYPT");
                    ServiceLocator.GetService<ModalPanel>().OpenUnlockPanel("You unlocked the Egypt faction!", HUMain.hiddenUnits.LoadAsset<Sprite>("egypt"));
                }
                if (!saveLoader.HasUnlockedSecret("SECRET_STEAMPUNK")) {

                    saveLoader.UnlockSecret("SECRET_STEAMPUNK");
                    ServiceLocator.GetService<ModalPanel>().OpenUnlockPanel("You unlocked the Steampunk faction!", HUMain.hiddenUnits.LoadAsset<Sprite>("steampunk"));
                }
                
                HUMain.CheckUnlockConditions();
            }
            else if (scene.name.Contains("SG_"))
            {
                if (scene.name == "SG_Egypt" && ServiceLocator.GetService<ISaveLoaderService>().HasUnlockedSecret("BILLY_SWORD"))
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("BillyKey_Unlock4"), null, true);
                
                foreach (var obj in scene.GetRootGameObjects())
                {
                    if (obj.name == "Map")
                    {
                        var shadersToReplace = new List<MeshRenderer>(obj.GetComponentsInChildren<MeshRenderer>(true)
                            .ToList().FindAll(x => x.name.Contains("_ReplaceMe")));
                        foreach (var rend in shadersToReplace)
                        {
                            rend.material.shader = Shader.Find(rend.material.shader.name) ?? rend.material.shader;
                            if (rend.GetComponent<PiratePlacementTransparency>())
                            {
                                rend.GetComponent<PiratePlacementTransparency>().Materials[0].m_oldMaterial.shader =
                                    Shader.Find(rend.GetComponent<PiratePlacementTransparency>().Materials[0]
                                        .m_oldMaterial.shader.name) ?? rend.GetComponent<PiratePlacementTransparency>().Materials[0].m_oldMaterial.shader;
                            }
                        }
                    }
                    if (obj.name.Contains("_ReplaceMe"))
                    {
                        obj.GetComponent<MeshRenderer>().material.shader =
                            Shader.Find(obj.GetComponent<MeshRenderer>().material.shader.name) ?? Shader.Find(obj.GetComponent<MeshRenderer>().material.shader.name);
                    }
                    if (obj.name == "WaterManager")
                    {
                        obj.GetComponent<PirateWaterManager>().WaterMaterial = obj.GetComponent<MeshRenderer>().material;
                    }

                    foreach (var audio in obj.GetComponentsInChildren<AudioSource>())
                    {
                        audio.outputAudioMixerGroup = HUMain.AudioMixer;
                    }
                }
            }
            else switch (scene.name)
            {
                case "00_Simulation_Day_VC":
                {
                    var secrets = new GameObject()
                    {
                        name = "Secrets"
                    };
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Saitama_Unlock"), secrets.transform, true);
                    break;
                }
                case "00_Lvl1_Halloween_VC":
                {
                    var secrets = new GameObject()
                    {
                        name = "Secrets"
                    };
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Hadez_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("TwinOgre_Unlock"), secrets.transform, true);
                    break;
                }
                case "00_Lvl2_Halloween_VC":
                {
                    var secrets = new GameObject()
                    {
                        name = "Secrets"
                    };
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("EmpSword_Unlock1"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("EmpSword_Unlock2"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("EmpSword_Unlock3"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("EmpSword_Unlock4"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("EmpSword_Unlock5"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("EmpSword_Unlock6"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("GrievingTitan_Unlock"), secrets.transform, true);
                    break;
                }
                case "01_Lvl1_Tribal_VC":
                {
                    var secrets = new GameObject()
                    {
                        name = "Secrets"
                    };
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("CMM_Unlock4"), secrets.transform, true);
                    break;
                }
                case "01_Lvl2_Tribal_VC":
                {
                    var secrets = new GameObject()
                    {
                        name = "Secrets"
                    };
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Shaman_Unlock"), secrets.transform, true);
                    if (ServiceLocator.GetService<ISaveLoaderService>().HasUnlockedSecret("BILLY_SWORD"))
                        Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("BillyKey_Unlock2"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("WD_Unlock"), secrets.transform, true);
                    break;
                }
                case "01_Sandbox_Tribal_01_VC":
                {
                    var secrets = new GameObject()
                    {
                        name = "Secrets"
                    };
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Gatherer_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Clubmaster_Unlock"), secrets.transform, true);
                    break;
                }
                case "02_Lvl1_Farmer_VC":
                {
                    var secrets = new GameObject()
                    {
                        name = "Secrets"
                    };
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Butcher_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("CMM_Unlock2"), secrets.transform, true);
                    break;
                }
                case "02_Lvl2_Farmer_VC":
                {
                    var secrets = new GameObject()
                    {
                        name = "Secrets"
                    };
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Chicken_Unlock1"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Chicken_Unlock2"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Chicken_Unlock3"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Chicken_Unlock4"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Chicken_Unlock5"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Beekeeper_Unlock"), secrets.transform, true);
                    break;
                }
                case "03_Lvl1_Ancient_VC":
                {
                    var secrets = new GameObject()
                    {
                        name = "Secrets"
                    };
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Helicopter_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Hephaestus_Unlock"), secrets.transform, true);
                    break;
                }
                case "03_Lvl2_Ancient_VC":
                {
                    var secrets = new GameObject()
                    {
                        name = "Secrets"
                    };
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Mathematician_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Philosopher_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Apollo_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Gorgon_Unlock"), secrets.transform, true);
                    foreach (var obj in scene.GetRootGameObjects())
                    {
                        var head = obj.transform.FindChildRecursive("Head");
                        if (head) head.gameObject.SetActive(false);
                    }

                    break;
                }
                case "03_Sandbox_Ancient_01_VC":
                {
                    var secrets = new GameObject()
                    {
                        name = "Secrets"
                    };
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("TrojanChicken_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Ares_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Centaur_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("AncientTank_Unlock"), secrets.transform, true);
                    break;
                }
                case "04_Lvl1_Viking_VC":
                {
                    var secrets = new GameObject()
                    {
                        name = "Secrets"
                    };
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Warlord_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("TheReaver_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("RuneMage_Unlock"), secrets.transform, true);
                    break;
                }
                case "04_Sandbox_Viking_VC":
                {
                    var secrets = new GameObject()
                    {
                        name = "Secrets"
                    };
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("DreadKing_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Thor_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Odin_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("FireGiant_Unlock"), secrets.transform, true);
                    break;
                }
                case "05_Lvl1_Medieval_VC":
                {
                    var secrets = new GameObject()
                    {
                        name = "Secrets"
                    };
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Tower_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Thief_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Warhorn_Unlock"), secrets.transform, true);
                    break;
                }
                case "05_Lvl2_Medieval_VC":
                {
                    var secrets = new GameObject()
                    {
                        name = "Secrets"
                    };
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Ignislasher_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Templar_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Bishop_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Blacksmith_Unlock"), secrets.transform, true);
                    break;
                }
                case "05_AsiaTemple_VC":
                {
                    var secrets = new GameObject()
                    {
                        name = "Secrets"
                    };
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("TheSage_Unlock"), secrets.transform, true);
                    break;
                }
                case "07_lvl1_Renaissance_VC":
                {
                    var secrets = new GameObject()
                    {
                        name = "Secrets"
                    };
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("TheQueen_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("CMM_Unlock1"), secrets.transform, true);
                    break;
                }
                case "08_Lvl1_Pirate_VC":
                {
                    var secrets = new GameObject()
                    {
                        name = "Secrets"
                    };
                    if (ServiceLocator.GetService<ISaveLoaderService>().HasUnlockedSecret("BILLY_SWORD"))
                        Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("BillyKey_Unlock1"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Triton_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("SeaKnight_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Crab_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Whale_Unlock"), secrets.transform, true);
                    break;
                }
                case "09_Lvl1_Western_VC":
                {
                    var secrets = new GameObject()
                    {
                        name = "Secrets"
                    };
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Prospector_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("OilArcher_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("CMM_Unlock3"), secrets.transform, true);
                    break;
                }
                case "05_Sandbox_Medieval_VC":
                {
                    var secrets = new GameObject()
                    {
                        name = "Secrets"
                    };
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("FlailMaster_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("SpiderMage_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("MayhemGunner_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Billy_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("BoxCannon_Unlock"), secrets.transform, true);
                    break;
                }
                case "09_Lvl1_Fantasy_Evil_VC":
                {
                    var secrets = new GameObject()
                    {
                        name = "Secrets"
                    };
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("BusinessMan_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Cthulhu_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("GrakaThor_Unlock1"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Blaze_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Manticore_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Sanguinarian_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("StormKing_Unlock"), secrets.transform, true);
                    if (ServiceLocator.GetService<ISaveLoaderService>().HasUnlockedSecret("BILLY_SWORD"))
                        Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("BillyKey_Unlock3"), secrets.transform, true);
                    break;
                }
                case "09_Lvl1_Fantasy_Good_VC":
                {
                    var secrets = new GameObject()
                    {
                        name = "Secrets"
                    };
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Aetherian_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Angel_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Pegasus_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Seraphim_Unlock"), secrets.transform, true);
                    Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("GrakaThor_Unlock2"), secrets.transform, true);
                    break;
                }
            }
        }

        public override void Localize(LocalizationHolder holder)
        {
            holder.languages.AddRange(HUMain.hiddenUnits
                .LoadAsset<GameObject>("Lang").GetComponent<LocalizationHolder>().languages);
            holder.languages.AddRange(HUMain.huMaps
                .LoadAsset<GameObject>("MapLang").GetComponent<LocalizationHolder>().languages);
        }
		
        public static ConfigEntry<bool> configInfiniteScalingEnabled;
        public static ConfigEntry<bool> configEvilBeesEnabled;
        public static ConfigEntry<bool> configEvilTrainEnabled;
	}
}
