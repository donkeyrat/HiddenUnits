using System.Collections;
using UnityEngine;

namespace HiddenUnits {

	public class AxeForceAnimation : AxeAttackEffect {

		public void Start() { rig = GetComponentInParent<Rigidbody>(); }

		public void Update() { }

		public override void DoEffect(Rigidbody target) {

			if (!data) { data = GetComponent<Weapon>().connectedData; }
			for (int i = 0; i < animations.Length; i++) { StartCoroutine(PlayAnimationAfterDelay(animations[i], target ? target.position : transform.position, target)); }
		}

		private IEnumerator PlayAnimationAfterDelay(SpellAnimation animation, Vector3 position, Rigidbody targetRig = null) {

			if (upwardsModifier != 0f) { position += Vector3.up * upwardsModifier; }

			Rigidbody[] usedRig = new Rigidbody[] { rig };

			if (animation.animationRig == SpellAnimation.AnimationRig.All) { usedRig = data.allRigs.AllRigs; }
			if (animation.animationRig == SpellAnimation.AnimationRig.Torso) { usedRig[0] = data.mainRig; }
			if (animation.animationRig == SpellAnimation.AnimationRig.Hip) { usedRig[0] = data.hip; }

			Vector3 animationDirection = SetDirection(position, animation);
			yield return new WaitForSeconds(animation.animationDelay);
			float t = animation.rigAnimationCurve.keys[animation.rigAnimationCurve.keys.Length - 1].time;
			float c = 0f;
			float ASM = Mathf.Clamp(data.unit.attackSpeedMultiplier, 0f, 6f);
			while (c < t && data.ragdollControl > 0.7f) {

				if (animation.setDirectionContinious && targetRig) { animationDirection = SetDirection(targetRig.position + Vector3.up * upwardsModifier, animation); }

				if (data.sinceGrounded < 0.3f) {

					int num;
					for (int i = 0; i < usedRig.Length; i = num + 1) {

						usedRig[i].AddForce(animation.rigAnimationForce * animationDirection * Time.deltaTime * ASM * 100f * animation.rigAnimationCurve.Evaluate(c), ForceMode.Acceleration);
						num = i;
					}
				}
				c += Time.deltaTime * ASM;
				yield return null;
			}
			yield break;
		}

		private Vector3 SetDirection(Vector3 position, SpellAnimation animation) {

			Vector3 vector = (position - transform.position).normalized;

			if (animation.animationDirection == RangeWeapon.SpawnRotation.TowardsTargetWithoutY) { vector = new Vector3(vector.x, 0f, vector.z).normalized; }

			else {

				if (animation.animationDirection == RangeWeapon.SpawnRotation.Up) { vector = Vector3.up; }

				else {

					if (animation.animationDirection == RangeWeapon.SpawnRotation.identity) { vector = Vector3.forward; }

					else {

						if (animation.animationDirection == RangeWeapon.SpawnRotation.CharacterForward) { vector = data.characterForwardObject.forward; }
					}
				}
			}

			if (animation.rangeMultiplierCurve.keys.Length != 0) { vector *= animation.rangeMultiplierCurve.Evaluate(Vector3.Distance(transform.position, position)); }

			return vector;
		}

		public SpellAnimation[] animations;

		public float immunityTime;

		public float upwardsModifier;

		private DataHandler data;

		private Rigidbody rig;
	}
}
