using UnityEngine;
using UnityEngine.SceneManagement;
using Pathfinding;
using Landfall.TABS;
using System.Collections;
using System.Linq;
using Landfall.TABS.UnitEditor;
using System.Collections.Generic;
using Pathfinding;

namespace HiddenUnits
{
    public class HUSecretManager : MonoBehaviour
    {
        public HUSecretManager()
        {
            SceneManager.sceneLoaded += SceneLoaded;
        }

        public void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.path == "Assets/11 Scenes/MainMenu.unity" && !doneStealing)
            {
                doneStealing = true;
                StartCoroutine(LoadAsync());
            }
            if (scene.path == "Assets/11 Scenes/MainMenu.unity" && doneStealing) {

                if (!ServiceLocator.GetService<ISaveLoaderService>().HasUnlockedSecret("SECRET_EGYPT")) {

                    ServiceLocator.GetService<ISaveLoaderService>().UnlockSecret("SECRET_EGYPT");
                    ServiceLocator.GetService<ModalPanel>().OpenUnlockPanel("You unlocked the Egypt faction, maps, and campaign!", HUMain.hiddenUnits.LoadAsset<Sprite>("egypt"));
                }
            }
            if (scene.name.Contains("SG_"))
            {
                GameObject astar = null;
                GameObject map = null;
                GameObject water = null;
                foreach (var obj in scene.GetRootGameObjects())
                {
                    if (obj.name == "AStar_Lvl1_Grid")
                    {
                        astar = obj;
                    }
                    if (obj.name == "Map")
                    {
                        map = obj;
                    }
                    if (obj.name == "Water")
                    {
                        water = obj;
                        obj.GetComponent<MeshRenderer>().material = HUMain.wet;
                    }
                    if (obj.name == "WaterManager" && water)
                    {
                        obj.GetComponent<PirateWaterManager>().WaterMaterial = water.GetComponent<MeshRenderer>().material;
                    }
                }
                if (astar != null && map != null)
                {
                    var path = astar.GetComponentInChildren<AstarPath>(true);
                    astar.SetActive(true);
                    if (path.data.graphs.Length > 0) { path.data.RemoveGraph(path.data.graphs[0]); }
                    path.data.AddGraph(typeof(RecastGraph));
                    path.data.recastGraph.minRegionSize = 0.1f;
                    path.data.recastGraph.characterRadius = 0.3f;
                    path.data.recastGraph.cellSize = 0.2f;
                    path.data.recastGraph.forcedBoundsSize = new Vector3(map.GetComponent<MapSettings>().m_mapRadius * 2f, map.GetComponent<MapSettings>().m_mapRadius * map.GetComponent<MapSettings>().mapRadiusYMultiplier * 2f, map.GetComponent<MapSettings>().m_mapRadius * 2f);
                    path.data.recastGraph.rasterizeMeshes = false;
                    path.data.recastGraph.rasterizeColliders = true;
                    path.data.recastGraph.mask = HUMain.hiddenUnits.LoadAsset<GameObject>("AStarDummy").GetComponent<Explosion>().layerMask;
                    path.Scan();

                    //path.data.GetNodes(delegate (GraphNode node)
                    //{
                    //    GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //    gameObject.transform.position = (Vector3)node.position;
                    //    gameObject.GetComponent<Renderer>().material.color = Color.green;
                    //    gameObject.GetComponent<Collider>().enabled = false;
                    //    gameObject.transform.localScale *= 0.5f;
                    //});
                }
            }
            if (scene.name == "00_Simulation_Day_VC")
            {
                var secrets = new GameObject()
                {
                    name = "Secrets"
                };
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Saitama_Unlock"), secrets.transform, true);
            }
            if (scene.name == "00_Lvl2_Halloween_VC")
            {
                var secrets = new GameObject()
                {
                    name = "Secrets"
                };
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Hadez_Unlock"), secrets.transform, true);
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("EmpSword_Unlock1"), secrets.transform, true);
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("EmpSword_Unlock2"), secrets.transform, true);
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("EmpSword_Unlock3"), secrets.transform, true);
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("EmpSword_Unlock4"), secrets.transform, true);
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("EmpSword_Unlock5"), secrets.transform, true);
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("EmpSword_Unlock6"), secrets.transform, true);
            }
            if (scene.name == "01_Lvl1_Tribal_VC")
            {
                var secrets = new GameObject()
                {
                    name = "Secrets"
                };
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("WD_Unlock"), secrets.transform, true);
            }
            if (scene.name == "01_Lvl2_Tribal_VC")
            {
                var secrets = new GameObject()
                {
                    name = "Secrets"
                };
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Shaman_Unlock"), secrets.transform, true);
            }
            if (scene.name == "01_Sandbox_Tribal_01_VC")
            {
                var secrets = new GameObject()
                {
                    name = "Secrets"
                };
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Gatherer_Unlock"), secrets.transform, true);
            }
            if (scene.name == "02_Lvl1_Farmer_VC")
            {
                var secrets = new GameObject()
                {
                    name = "Secrets"
                };
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Butcher_Unlock"), secrets.transform, true);
            }
            if (scene.name == "02_Lvl2_Farmer_VC")
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
            }
            if (scene.name == "03_Lvl2_Ancient_VC")
            {
                var secrets = new GameObject()
                {
                    name = "Secrets"
                };
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Mathematician_Unlock"), secrets.transform, true);
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Philosopher_Unlock"), secrets.transform, true);
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Apollo_Unlock"), secrets.transform, true);
            }
            if (scene.name == "03_Sandbox_Ancient_01_VC")
            {
                var secrets = new GameObject()
                {
                    name = "Secrets"
                };
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("TrojanChicken_Unlock"), secrets.transform, true);
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Ares_Unlock"), secrets.transform, true);
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Centaur_Unlock"), secrets.transform, true);
            }
            if (scene.name == "04_Lvl1_Viking_VC")
            {
                var secrets = new GameObject()
                {
                    name = "Secrets"
                };
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Warlord_Unlock"), secrets.transform, true);
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("TheReaver_Unlock"), secrets.transform, true);
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("RuneMage_Unlock"), secrets.transform, true);
            }
            if (scene.name == "04_Sandbox_Viking_VC")
            {
                var secrets = new GameObject()
                {
                    name = "Secrets"
                };
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("DreadKing_Unlock"), secrets.transform, true);
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Thor_Unlock"), secrets.transform, true);
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Odin_Unlock"), secrets.transform, true);
            }
            if (scene.name == "05_Lvl1_Medieval_VC")
            {
                var secrets = new GameObject()
                {
                    name = "Secrets"
                };
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Tower_Unlock"), secrets.transform, true);
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Thief_Unlock"), secrets.transform, true);
            }
            if (scene.name == "05_Lvl2_Medieval_VC")
            {
                var secrets = new GameObject()
                {
                    name = "Secrets"
                };
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Ignislasher_Unlock"), secrets.transform, true);
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Templar_Unlock"), secrets.transform, true);
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Bishop_Unlock"), secrets.transform, true);
            }
            if (scene.name == "05_Sandbox_Medieval_VC")
            {
                var secrets = new GameObject()
                {
                    name = "Secrets"
                };
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("FlailMaster_Unlock"), secrets.transform, true);
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("SpiderMage_Unlock"), secrets.transform, true);
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("MayhemGunner_Unlock"), secrets.transform, true);
            }
            //if (scene.name == "08_Lvl1_Pirate_VC")
            //{
            //    var secrets = new GameObject()
            //    {
            //        name = "Secrets"
            //    };
            //}
            if (scene.name == "09_Lvl1_Fantasy_Good_VC")
            {
                var secrets = new GameObject()
                {
                    name = "Secrets"
                };
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Aetherian_Unlock"), secrets.transform, true);
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Angel_Unlock"), secrets.transform, true);
                Instantiate(HUMain.hiddenUnits.LoadAsset<GameObject>("Pegasus_Unlock"), secrets.transform, true);
            }
        }
        public IEnumerator LoadAsync()
        {
            var async = SceneManager.LoadSceneAsync("05_Lvl2_Medieval_VC", LoadSceneMode.Additive);
            yield return new WaitUntil(() => async.isDone);
            foreach (var obj in SceneManager.GetSceneByName("05_Lvl2_Medieval_VC").GetRootGameObjects())
            {
                if (obj.name == "Map")
                {
                    HUMain.wet = obj.GetComponentInChildren<Water>(true).GetComponent<MeshRenderer>().material;
                }
            }
            async = SceneManager.UnloadSceneAsync("05_Lvl2_Medieval_VC");
            yield return new WaitUntil(() => async.isDone);
            SceneManager.LoadScene("Assets/11 Scenes/MainMenu.unity");
            doneStealing = true;
            yield break;
        }

        public bool doneStealing;
    }
}
