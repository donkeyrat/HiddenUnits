using System.Collections;
using System.Collections.Generic;
using Landfall.TABS;
using Landfall.TABS.GameState;
using TFBGames;
using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits
{
	public class SecretUnlockMultiple : GameStateListener
	{
		public string secretKey;
	
		public List<string> secretDescriptions = new List<string>();
	
		public Sprite secretIcon;
	
		public float distanceToUnlock = 5f;
	
		private RotationShake MRotationShake;
	
		private Rigidbody MSecretObject;
	
		private float MLookValue;
	
		private float MUnlockValue;
	
		public AudioClip hitClip;
	
		private AudioSource LoopSource;
	
		private Transform MMainCamTransform;
	
		public UnityEvent unlockEvent;
	
		public UnityEvent hideEvent;
	
		private bool Done;
	
		public Color glowColor;
	
		public GameObject unlockSparkEffect;
	
		protected override void Awake()
		{
			base.Awake();
			if (MMainCamTransform == null)
			{
				OnEnterNewScene();
			}
		}
	
		private void Update()
		{
			if (!(MMainCamTransform != null) || !MSecretObject || Done)
			{
				return;
			}
			LoopSource.volume = MUnlockValue <= 0f ? 0f : Mathf.Pow(MUnlockValue * 0.25f, 1.3f);
			if (float.IsNaN(LoopSource.volume))
			{
				LoopSource.volume = 0f;
			}
			var pitch = 1f + 1f * MUnlockValue;
			LoopSource.pitch = (pitch >= 0f ? pitch : 0f);
			if (MUnlockValue > 0f || MLookValue > 10f)
			{
				SetColor();
			}
			float num = Vector3.Distance(MSecretObject.worldCenterOfMass, MMainCamTransform.position);
			if (num > distanceToUnlock)
			{
				MUnlockValue -= Time.unscaledDeltaTime * 0.2f;
				return;
			}
			float num2 = Vector3.Angle(MMainCamTransform.forward, MSecretObject.worldCenterOfMass - MMainCamTransform.position);
			MLookValue = 1000f / (num * num2);
			if (MLookValue > 8f)
			{
				float num3 = 0.2f;
				MUnlockValue += num3 * Time.unscaledDeltaTime;
				UnlockProgressFeedback();
				if (MUnlockValue > 1f)
				{
					StartCoroutine(UnlockSecret());
				}
			}
			else
			{
				MUnlockValue -= Time.unscaledDeltaTime * 0.2f;
			}
		}
	
		private void UnlockProgressFeedback()
		{
			if ((bool)MRotationShake)
			{
				if (MUnlockValue <= 0f)
				{
					MRotationShake.AddForce(Random.onUnitSphere * 2f);
					MUnlockValue = 0f;
				}
				MRotationShake.enabled = true;
				MRotationShake.AddForce(Random.onUnitSphere * MUnlockValue * Time.deltaTime * 50f);
			}
		}
	
		private void SetColor()
		{
			MUnlockValue = Mathf.Clamp(MUnlockValue, 0f, float.PositiveInfinity);
			Renderer[] componentsInChildren = MSecretObject.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Material[] materials = componentsInChildren[i].materials;
				for (int j = 0; j < materials.Length; j++)
				{
					if (materials[j].HasProperty("_EmissionColor"))
					{
						materials[j].EnableKeyword("_EMISSION");
						materials[j].SetColor("_EmissionColor", glowColor * MUnlockValue * 2f);
					}
				}
				componentsInChildren[i].materials = materials;
			}
		}
	
		private IEnumerator UnlockSecret()
		{
			if (!enabled || string.IsNullOrWhiteSpace(secretKey))
			{
				yield break;
			}
			if ((bool)ScreenShake.Instance)
			{
				ScreenShake.Instance.AddForce(Vector3.up * 8f, MSecretObject.transform.position);
			}
			if ((bool)unlockSparkEffect)
			{
				GameObject gameObject = Instantiate(unlockSparkEffect, MSecretObject.transform.position, MSecretObject.transform.rotation);
				gameObject.AddComponent<RemoveAfterSeconds>().seconds = 5f;
				MeshRenderer componentInChildren = MSecretObject.GetComponentInChildren<MeshRenderer>();
				if ((bool)componentInChildren)
				{
					ParticleSystem.ShapeModule shape = gameObject.GetComponent<ParticleSystem>().shape;
					shape.meshRenderer = componentInChildren;
				}
			}
			MSecretObject.gameObject.SetActive(value: false);
			unlockEvent?.Invoke();
			LoopSource.Stop();
			LoopSource.volume = 1f;
			LoopSource.PlayOneShot(hitClip);
			Done = true;
			ServiceLocator.GetService<ISaveLoaderService>().UnlockSecret(secretKey);
			for (int i = 0; i < secretDescriptions.Count; i++)
			{
				ServiceLocator.GetService<ModalPanel>().OpenUnlockPanel(secretDescriptions[i], secretIcon);
				yield return new WaitForSeconds(0.1f);
			}
		}
	
		public override void OnEnterNewScene()
		{
			base.OnEnterNewScene();
			LoopSource = GetComponent<AudioSource>();
			if ((bool)LoopSource)
			{
				LoopSource.volume = 0f;
			}
			MRotationShake = GetComponentInChildren<RotationShake>();
			MSecretObject = GetComponentInChildren<Rigidbody>();
			if ((bool)MSecretObject)
			{
				MSecretObject.isKinematic = true;
			}
			
			if (!string.IsNullOrWhiteSpace(secretKey) && ServiceLocator.GetService<ISaveLoaderService>().HasUnlockedSecret(secretKey))
			{
				if ((bool)MSecretObject)
				{
					MSecretObject.gameObject.SetActive(value: false);
				}
				enabled = false;
				hideEvent?.Invoke();
			}
			MainCam mainCam = ServiceLocator.GetService<PlayerCamerasManager>()?.GetMainCam(TFBGames.Player.One);
			MMainCamTransform = ((mainCam != null) ? mainCam.transform : null);
		}
	
		public override void OnEnterPlacementState()
		{
		}
	
		public override void OnEnterBattleState()
		{
		}
	
		public static void CheckAchievements()
		{
			AchievementService service = ServiceLocator.GetService<AchievementService>();
			ISaveLoaderService secretService = ServiceLocator.GetService<ISaveLoaderService>();
			if (HasUnlockedFaction(874593522))
			{
				service.UnlockAchievement("UNLOCKED_ALL_SECRET");
			}
			if (HasUnlockedFaction(673578412))
			{
				service.UnlockAchievement("UNLOCKED_ALL_LEGACY");
			}
			bool HasUnlockedFaction(int factionId)
			{
				UnitBlueprint[] units = LandfallUnitDatabase.GetDatabase().GetFactionByGUID(new DatabaseID(-1, factionId)).Units;
				for (int i = 0; i < units.Length; i++)
				{
					string unlockKey = units[i].Entity.UnlockKey;
					if (!string.IsNullOrEmpty(unlockKey) && !secretService.HasUnlockedSecret(unlockKey))
					{
						return false;
					}
				}
				return true;
			}
		}
	}
}