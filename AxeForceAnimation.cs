using System.Collections;
using UnityEngine;

namespace HiddenUnits {

	public class AxeForceAnimation : AxeAttackEffect {

		public void Start()
		{
			rig = GetComponentInParent<Rigidbody>();
			if (!data) data = GetComponent<Weapon>().connectedData;
		}

		public override void DoEffect(Rigidbody target)
		{
			foreach (var t in animations)
			{
				StartCoroutine(PlayAnimationAfterDelay(t, target ? target.position : transform.position, target));
			}
		}

		private IEnumerator PlayAnimationAfterDelay(SpellAnimation animation, Vector3 position, Rigidbody targetRig = null) {

			if (upwardsModifier != 0f) { position += Vector3.up * upwardsModifier; }

			Rigidbody[] usedRig = new Rigidbody[] { rig };

			switch (animation.animationRig)
			{
				case SpellAnimation.AnimationRig.All:
					usedRig = data.allRigs.AllRigs;
					break;
				case SpellAnimation.AnimationRig.Torso:
					usedRig[0] = data.mainRig;
					break;
				case SpellAnimation.AnimationRig.Hip:
					usedRig[0] = data.hip;
					break;
				case SpellAnimation.AnimationRig.This:
					break;
				case SpellAnimation.AnimationRig.ThisRig:
					break;
				default:
					usedRig = data.allRigs.AllRigs;
					break;
			}

			Vector3 animationDirection = SetDirection(position, animation);
			yield return new WaitForSeconds(animation.animationDelay);
			var t = animation.rigAnimationCurve.keys[animation.rigAnimationCurve.keys.Length - 1].time;
			var c = 0f;
			var asm = Mathf.Clamp(data.unit.attackSpeedMultiplier, 0f, 6f);
			while (c < t && data.ragdollControl > 0.7f) {

				if (animation.setDirectionContinious && targetRig) { animationDirection = SetDirection(targetRig.position + Vector3.up * upwardsModifier, animation); }

				if (data.sinceGrounded < 0.3f) {

					int num;
					for (int i = 0; i < usedRig.Length; i = num + 1) {

						usedRig[i].AddForce(animationDirection * (animation.rigAnimationForce * Time.deltaTime * asm * 100f * animation.rigAnimationCurve.Evaluate(c)), ForceMode.Acceleration);
						num = i;
					}
				}
				c += Time.deltaTime * asm;
				yield return null;
			}
		}

		private Vector3 SetDirection(Vector3 position, SpellAnimation animation) {

			Vector3 vector = (position - transform.position).normalized;

			if (animation.animationDirection == RangeWeapon.SpawnRotation.TowardsTargetWithoutY) vector = new Vector3(vector.x, 0f, vector.z).normalized;

			else 
			{
				if (animation.animationDirection == RangeWeapon.SpawnRotation.Up) vector = Vector3.up;

				else 
				{
					if (animation.animationDirection == RangeWeapon.SpawnRotation.identity) vector = Vector3.forward;

					else 
					{
						if (animation.animationDirection == RangeWeapon.SpawnRotation.CharacterForward) vector = data.characterForwardObject.forward;
					}
				}
			}

			if (animation.rangeMultiplierCurve.keys.Length != 0) vector *= animation.rangeMultiplierCurve.Evaluate(Vector3.Distance(transform.position, position));

			return vector;
		}

		public SpellAnimation[] animations;

		public float upwardsModifier;

		private DataHandler data;

		private Rigidbody rig;
	}
}
