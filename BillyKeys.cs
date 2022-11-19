using System.Collections;
using System.Collections.Generic;
using TFBGames;
using UnityEngine;
using UnityEngine.Events;
using Landfall.TABS;
using Landfall.TABS.GameState;

namespace HiddenUnits
{
    public class BillyKeys : GameStateListener
    {
        public override void OnEnterNewScene()
        {
            base.OnEnterNewScene();
            mainCam = ServiceLocator.GetService<PlayerCamerasManager>()?.GetMainCam(TFBGames.Player.One).transform;
            save = ServiceLocator.GetService<ISaveLoaderService>();
            for (int i = 0; i < keys.Count; i++)
            {
                isUnlocking.Add(false);
                if (save.HasUnlockedSecret(toBeUnlocked[i]))
                {
                    keys[i].SetActive(true);
                    keys[i].transform.localPosition = Vector3.zero;
                    keys.RemoveAt(i);
                    alreadyUnlocked.RemoveAt(i);
                    toBeUnlocked.RemoveAt(i);
                    isUnlocking.RemoveAt(i);
                }
            }

            CheckUnlocks();
        }

        public override void OnEnterPlacementState()
        {
        }

        public override void OnEnterBattleState()
        {
        }

        public void Update()
        {
            for (int i = 0; i < keys.Count; i++)
            {
                if (!save.HasUnlockedSecret(alreadyUnlocked[i]) || save.HasUnlockedSecret(toBeUnlocked[i]) || isUnlocking[i]) continue;
                
                var num = Vector3.Distance(keys[i].transform.position, mainCam.position);
                float num2 = Vector3.Angle(mainCam.forward, keys[i].transform.position - mainCam.position);
                var lookValue = 1000f / (num * num2);
                if (lookValue > 8f)
                {
                    StartCoroutine(UnlockKey(i));
                }
            }
        }

        public IEnumerator UnlockKey(int index)
        {
            isUnlocking[index] = true;
            
            keys[index].SetActive(true);
            keys[index].GetComponent<Animator>().Play("BillyKey");

            yield return new WaitForSeconds(unlockDelay);

            save.UnlockSecret(toBeUnlocked[index]);
            GetComponent<AudioSource>().Play();
            
            CheckUnlocks();
        }

        public void CheckUnlocks()
        {
            if (toBeUnlocked.TrueForAll(x => save.HasUnlockedSecret(toBeUnlocked[toBeUnlocked.IndexOf(x)]))) unlockAllEvent.Invoke();
        }

        public void DoFinalUnlock()
        {
            save.UnlockSecret(finalUnlock);
            List<SecretUnlockCondition> list = ServiceLocator.GetService<ISaveLoaderService>().UnlockSecret(finalUnlock);
            if (list != null && list.Count > 0)
            {
                foreach (SecretUnlockCondition item in list)
                {
                    ServiceLocator.GetService<ModalPanel>().OpenUnlockPanel(item.m_unlockDescription, item.m_unlockImage);
                }
            }
            PlacementUI placementUI = FindObjectOfType<PlacementUI>();
            if (placementUI != null)
            {
                placementUI.RedrawUI(finalUnlock);
            }

            Destroy(this);
        }
        
        private Transform mainCam;
        
        private ISaveLoaderService save;
        
        private List<bool> isUnlocking = new List<bool>();
        
        public List<GameObject> keys = new List<GameObject>();

        public List<string> alreadyUnlocked = new List<string>();
        
        public List<string> toBeUnlocked = new List<string>();

        public UnityEvent unlockAllEvent = new UnityEvent();

        public string finalUnlock;

        public float unlockDelay = 2f;
    }
}