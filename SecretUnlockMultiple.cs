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
		private RotationShake RotationShake;
		private Rigidbody SecretObject;
		private float LookValue;
		private float UnlockValue;
		private AudioSource LoopSource;
		private Transform MainCamTransform;
		private bool Done;
		
		public string secretKey;
		public List<string> secretDescriptions;
		public Sprite secretIcon;
		public float distanceToUnlock = 5f;
		public AudioClip hitClip;
		public UnityEvent unlockEvent;
		public UnityEvent hideEvent;
		public Color glowColor;
		public GameObject unlockSparkEffect;
	
		protected override void Awake()
		{
			base.Awake();
			if (MainCamTransform == null)
			{
				OnEnterNewScene();
			}
		}
	
		private void Update()
		{
			if (!(MainCamTransform != null) || !SecretObject || Done)
			{
				return;
			}
			LoopSource.volume = UnlockValue <= 0f ? 0f : Mathf.Pow(UnlockValue * 0.25f, 1.3f);
			if (float.IsNaN(LoopSource.volume))
			{
				LoopSource.volume = 0f;
			}
			var pitch = 1f + 1f * UnlockValue;
			LoopSource.pitch = (pitch >= 0f ? pitch : 0f);
			if (UnlockValue > 0f || LookValue > 10f)
			{
				SetColor();
			}
			var num = Vector3.Distance(SecretObject.worldCenterOfMass, MainCamTransform.position);
			if (num > distanceToUnlock)
			{
				UnlockValue -= Time.unscaledDeltaTime * 0.2f;
				return;
			}
			var num2 = Vector3.Angle(MainCamTransform.forward, SecretObject.worldCenterOfMass - MainCamTransform.position);
			LookValue = 1000f / (num * num2);
			if (LookValue > 8f)
			{
				var num3 = 0.2f;
				UnlockValue += num3 * Time.unscaledDeltaTime;
				UnlockProgressFeedback();
				if (UnlockValue > 1f)
				{
					StartCoroutine(UnlockSecret());
				}
			}
			else
			{
				UnlockValue -= Time.unscaledDeltaTime * 0.2f;
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
	
		private void SetColor()
		{
			UnlockValue = Mathf.Clamp(UnlockValue, 0f, float.PositiveInfinity);
			var componentsInChildren = SecretObject.GetComponentsInChildren<Renderer>();
			for (var i = 0; i < componentsInChildren.Length; i++)
			{
				var materials = componentsInChildren[i].materials;
				for (var j = 0; j < materials.Length; j++)
				{
					if (materials[j].HasProperty("_EmissionColor"))
					{
						materials[j].EnableKeyword("_EMISSION");
						materials[j].SetColor("_EmissionColor", glowColor * UnlockValue * 2f);
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
				ScreenShake.Instance.AddForce(Vector3.up * 8f, SecretObject.transform.position);
			}
			if ((bool)unlockSparkEffect)
			{
				var gameObject = Instantiate(unlockSparkEffect, SecretObject.transform.position, SecretObject.transform.rotation);
				gameObject.AddComponent<RemoveAfterSeconds>().seconds = 5f;
				var componentInChildren = SecretObject.GetComponentInChildren<MeshRenderer>();
				if ((bool)componentInChildren)
				{
					var shape = gameObject.GetComponent<ParticleSystem>().shape;
					shape.meshRenderer = componentInChildren;
				}
			}
			SecretObject.gameObject.SetActive(value: false);
			unlockEvent?.Invoke();
			LoopSource.Stop();
			LoopSource.volume = 1f;
			LoopSource.PlayOneShot(hitClip);
			Done = true;
			ServiceLocator.GetService<ISaveLoaderService>().UnlockSecret(secretKey);
			for (var i = 0; i < secretDescriptions.Count; i++)
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
			RotationShake = GetComponentInChildren<RotationShake>();
			SecretObject = GetComponentInChildren<Rigidbody>();
			if ((bool)SecretObject)
			{
				SecretObject.isKinematic = true;
			}
			
			if (!string.IsNullOrWhiteSpace(secretKey) && ServiceLocator.GetService<ISaveLoaderService>().HasUnlockedSecret(secretKey))
			{
				if ((bool)SecretObject)
				{
					SecretObject.gameObject.SetActive(value: false);
				}
				enabled = false;
				hideEvent?.Invoke();
			}
			var mainCam = ServiceLocator.GetService<PlayerCamerasManager>()?.GetMainCam(TFBGames.Player.One);
			MainCamTransform = ((mainCam != null) ? mainCam.transform : null);
		}
	
		public override void OnEnterPlacementState()
		{
		}
	
		public override void OnEnterBattleState()
		{
		}
	}
}