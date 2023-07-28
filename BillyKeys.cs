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
            
            MainCam = ServiceLocator.GetService<PlayerCamerasManager>()?.GetMainCam(TFBGames.Player.One).transform;
            Save = ServiceLocator.GetService<ISaveLoaderService>();
            if (GetComponentInChildren<RotationShake>()) RotationShake = GetComponentInChildren<RotationShake>();
            if (GetComponentInChildren<Rigidbody>()) SecretObject = GetComponentInChildren<Rigidbody>();
            LoopSource = GetComponent<AudioSource>();

            var keysToRemove = new List<BillyKey>();
            if (keys.Count > 0)
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    IsUnlocking.Add(false);
                    if (Save.HasUnlockedSecret(toBeUnlocked[i]))
                    {
                        keys[i].SetActive(true);
                        keys[i].GetComponent<Animator>().Play("BillyKey");
                        keysToRemove.Add(new BillyKey(keys[i], alreadyUnlocked[i], toBeUnlocked[i], IsUnlocking[i]));
                    }
                }
            }

            foreach (var key in keysToRemove)
            {
                keys.Remove(key.key);
                alreadyUnlocked.Remove(key.alreadyUnlocked);
                toBeUnlocked.Remove(key.toBeUnlocked);
                IsUnlocking.Remove(key.isUnlocking);
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
            if (Save.HasUnlockedSecret(finalUnlock)) return;
            
            var num = Vector3.Distance(SecretObject.worldCenterOfMass, MainCam.position);
            float num2 = Vector3.Angle(MainCam.forward, SecretObject.worldCenterOfMass - MainCam.position);
            var lookValue = 1000f / (num * num2);
            LoopSource.volume = Mathf.Pow(UnlockValue * 0.25f, 1.3f);
            LoopSource.pitch = 1f + 1f * UnlockValue;
            if (Done)
            {
                if (UnlockValue > 0f || lookValue > 10f) SetColor();
                if (num > unlockDistance) UnlockValue -= Time.unscaledDeltaTime * 0.2f;
                else if (lookValue > 8f)
                {
                    UnlockValue += Time.unscaledDeltaTime * 0.2f;
                    UnlockProgressFeedback();
                    if (UnlockValue > 1f)
                    {
                        UnlockSelf();
                    }
                }
                else UnlockValue -= Time.unscaledDeltaTime * 0.2f;
            }
            else
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    if (Save.HasUnlockedSecret(alreadyUnlocked[i]) && !Save.HasUnlockedSecret(toBeUnlocked[i]) && !IsUnlocking[i] && lookValue > 8f)
                    {
                        StartCoroutine(UnlockKey(i));
                    }
                }
            }
        }
        
        public IEnumerator UnlockKey(int index)
        {
            IsUnlocking[index] = true;
            
            keys[index].SetActive(true);
            keys[index].GetComponent<Animator>().Play("BillyKey");

            yield return new WaitForSeconds(unlockDelay);

            Save.UnlockSecret(toBeUnlocked[index]);
            
            LoopSource.Stop();
            LoopSource.volume = 1f;
            LoopSource.PlayOneShot(hitClip);
            
            CheckUnlocks();
        }

        public void CheckUnlocks()
        {
            if (toBeUnlocked.TrueForAll(x => Save.HasUnlockedSecret(toBeUnlocked[toBeUnlocked.IndexOf(x)])))
            {
                Done = true;
                LoopSource.Play();
            }
        }

        public void UnlockSelf()
        {
            if (Save.HasUnlockedSecret(finalUnlock)) return;
            Save.UnlockSecret(finalUnlock);
            ServiceLocator.GetService<ModalPanel>().OpenUnlockPanel(unlockDescription, unlockImage);
            PlacementUI placementUI = FindObjectOfType<PlacementUI>();
            if (placementUI != null)
            {
                placementUI.RedrawUI(finalUnlock);
            }
            LoopSource.PlayOneShot(hitClip);
            unlockAllEvent.Invoke();
            StartCoroutine(ShrinkUnlockValue());
        }

        public IEnumerator ShrinkUnlockValue()
        {
            while (UnlockValue > 0f)
            {
                UnlockValue -= Time.unscaledDeltaTime * 0.2f;
                SetColor();
                UnlockProgressFeedback();
                LoopSource.volume = Mathf.Pow(UnlockValue * 0.25f, 1.3f);
                LoopSource.pitch = 1f + 1f * UnlockValue;
                yield return null;
            }
         }
        
        private void SetColor()
        {
            UnlockValue = Mathf.Clamp(UnlockValue, 0f, float.PositiveInfinity);
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
                            materials[j].SetColor("_EmissionColor", glowColor * UnlockValue * 2f);
                        }
                    }
                    renderer.materials = materials;
                }
            }
        }
        
        private void UnlockProgressFeedback()
        {
            if ((bool)RotationShake)
            {
                if (UnlockValue <= 0f)
                {
                    RotationShake.AddForce(Random.onUnitSphere * 2f);
                    UnlockValue = 0f;
                }
                RotationShake.enabled = true;
                RotationShake.AddForce(Random.onUnitSphere * UnlockValue * Time.deltaTime * 50f);
            }
        }
        
        private Transform MainCam;
        
        private ISaveLoaderService Save;
        
        private List<bool> IsUnlocking = new List<bool>();

        private RotationShake RotationShake;
        
        private Rigidbody SecretObject;

        private AudioSource LoopSource;

        private bool Done;

        private float UnlockValue;
        
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