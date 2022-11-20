using System.Collections;
using System.Collections.Generic;
using InControl;
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
            if (GetComponentInChildren<RotationShake>()) rotationShake = GetComponentInChildren<RotationShake>();
            if (GetComponentInChildren<Rigidbody>()) secretObject = GetComponentInChildren<Rigidbody>();
            loopSource = GetComponent<AudioSource>();

            var keysToRemove = new List<BillyKey>();
            if (keys.Count > 0)
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    isUnlocking.Add(false);
                    if (save.HasUnlockedSecret(toBeUnlocked[i]))
                    {
                        keys[i].SetActive(true);
                        keys[i].GetComponent<Animator>().Play("BillyKey");
                        keysToRemove.Add(new BillyKey(keys[i], alreadyUnlocked[i], toBeUnlocked[i], isUnlocking[i]));
                    }
                }
            }

            foreach (var key in keysToRemove)
            {
                keys.Remove(key.key);
                alreadyUnlocked.Remove(key.alreadyUnlocked);
                toBeUnlocked.Remove(key.toBeUnlocked);
                isUnlocking.Remove(key.isUnlocking);
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
            if (billyUnlocked) return;
            
            var num = Vector3.Distance(secretObject.worldCenterOfMass, mainCam.position);
            float num2 = Vector3.Angle(mainCam.forward, secretObject.worldCenterOfMass - mainCam.position);
            var lookValue = 1000f / (num * num2);
            loopSource.volume = Mathf.Pow(unlockValue * 0.25f, 1.3f);
            loopSource.pitch = 1f + 1f * unlockValue;
            if (done)
            {
                if (unlockValue > 0f || lookValue > 10f) SetColor();
                if (num > unlockDistance) unlockValue -= Time.unscaledDeltaTime * 0.2f;
                else if (lookValue > 8f)
                {
                    unlockValue += Time.unscaledDeltaTime * 0.2f;
                    UnlockProgressFeedback();
                    if (unlockValue > 1f)
                    {
                        UnlockSelf();
                    }
                }
                else unlockValue -= Time.unscaledDeltaTime * 0.2f;
            }
            else
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    if (save.HasUnlockedSecret(alreadyUnlocked[i]) && !save.HasUnlockedSecret(toBeUnlocked[i]) && !isUnlocking[i] && lookValue > 8f)
                    {
                        StartCoroutine(UnlockKey(i));
                    }
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
            
            loopSource.Stop();
            loopSource.volume = 1f;
            loopSource.PlayOneShot(hitClip);
            
            CheckUnlocks();
        }

        public void CheckUnlocks()
        {
            if (toBeUnlocked.TrueForAll(x => save.HasUnlockedSecret(toBeUnlocked[toBeUnlocked.IndexOf(x)])))
            {
                done = true;
                loopSource.Play();
            }
        }

        public void UnlockSelf()
        {
            save.UnlockSecret(finalUnlock);
            ServiceLocator.GetService<ModalPanel>().OpenUnlockPanel(unlockDescription, unlockImage);
            PlacementUI placementUI = FindObjectOfType<PlacementUI>();
            if (placementUI != null)
            {
                placementUI.RedrawUI(finalUnlock);
            }
            loopSource.PlayOneShot(hitClip);
            unlockAllEvent.Invoke();
            billyUnlocked = true;
            StartCoroutine(ShrinkUnlockValue());
        }

        public IEnumerator ShrinkUnlockValue()
        {
            while (unlockValue > 0f)
            {
                unlockValue -= Time.unscaledDeltaTime * 0.2f;
                SetColor();
                UnlockProgressFeedback();
                loopSource.volume = Mathf.Pow(unlockValue * 0.25f, 1.3f);
                loopSource.pitch = 1f + 1f * unlockValue;
                yield return null;
            }
         }
        
        private void SetColor()
        {
            unlockValue = Mathf.Clamp(unlockValue, 0f, float.PositiveInfinity);
            var allRends = GetComponentsInChildren<Renderer>();
            foreach (var renderer in allRends)
            {
                if (!renderer.GetComponent<ParticleSystemRenderer>())
                {
                    Material[] materials = renderer.materials;
                    for (int j = 0; j < materials.Length; j++)
                    {
                        if (materials[j].HasProperty("_EmissionColor"))
                        {
                            materials[j].EnableKeyword("_EMISSION");
                            materials[j].SetColor("_EmissionColor", glowColor * unlockValue * 2f);
                        }
                    }
                    renderer.materials = materials;
                }
            }
        }
        
        private void UnlockProgressFeedback()
        {
            if ((bool)rotationShake)
            {
                if (unlockValue <= 0f)
                {
                    rotationShake.AddForce(Random.onUnitSphere * 2f);
                    unlockValue = 0f;
                }
                rotationShake.enabled = true;
                rotationShake.AddForce(Random.onUnitSphere * unlockValue * Time.deltaTime * 50f);
            }
        }
        
        private Transform mainCam;
        
        private ISaveLoaderService save;
        
        private List<bool> isUnlocking = new List<bool>();

        private RotationShake rotationShake;
        
        private Rigidbody secretObject;

        private AudioSource loopSource;

        private bool done;

        private bool billyUnlocked;

        private float unlockValue;
        
        [Header("Keys")]
        
        public List<GameObject> keys = new List<GameObject>();

        public List<string> alreadyUnlocked = new List<string>();
        
        public List<string> toBeUnlocked = new List<string>();
        
        [Header("Final Unlock")]
        
        public UnityEvent unlockAllEvent = new UnityEvent();

        public float unlockDelay = 2f;
        
        public float unlockDistance = 4f;
        
        public string finalUnlock;

        public string unlockDescription;

        public Sprite unlockImage;

        public AudioClip hitClip;

        public Color glowColor;

        private class BillyKey
        {
            public BillyKey(GameObject key, string alreadyUnlocked, string toBeUnlocked, bool isUnlocking)
            {
                this.key = key;
                this.alreadyUnlocked = alreadyUnlocked;
                this.toBeUnlocked = toBeUnlocked;
                this.isUnlocking = isUnlocking;
            }
            
            public readonly GameObject key;

            public readonly string alreadyUnlocked;

            public readonly string toBeUnlocked;

            public readonly bool isUnlocking;
        }
    }
}