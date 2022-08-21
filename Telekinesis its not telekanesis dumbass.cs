using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020002DA RID: 730
public class Telekinesis_its_not_telekanesis_dumbass : MonoBehaviour
{
	// Token: 0x06000E7B RID: 3707 RVA: 0x00056DC4 File Offset: 0x00054FC4
	private void Start()
	{
		this.effectTransform = base.transform.GetChild(0);
		this.spawnedAttack = base.GetComponent<SpawnedAttack>();
		Collider[] array = Physics.OverlapSphere(this.effectTransform.position, this.radius);
		for (int i = 0; i < array.Length; i++)
		{
			bool flag = array[i].attachedRigidbody && !this.rigs.Contains(array[i].attachedRigidbody);
			if (flag)
			{
				this.rigs.Add(array[i].attachedRigidbody);
				this.randomOffset.Add(UnityEngine.Random.insideUnitSphere * 2f);
				this.floatynes.Add(UnityEngine.Random.Range(0.5f, 1f));
			}
		}
		this.healths = new Damagable[this.rigs.Count];
		for (int j = 0; j < this.rigs.Count; j++)
		{
			this.healths[j] = this.rigs[j].GetComponentInParent<Damagable>();
		}
		base.StartCoroutine(this.AnimateSpell());
		base.transform.rotation = Quaternion.identity;
	}

	// Token: 0x06000E7C RID: 3708 RVA: 0x00056F07 File Offset: 0x00055107
	private IEnumerator AnimateSpell()
	{
		float t = this.upCurve.keys[this.upCurve.keys.Length - 1].time;
		float c = 0f;
		while (c < t)
		{
			this.Pull();
			this.effectTransform.localPosition = Vector3.zero + Vector3.up * this.upCurve.Evaluate(c);
			c += Time.deltaTime;
			yield return null;
		}
		yield break;
	}

	// Token: 0x06000E7D RID: 3709 RVA: 0x00056F18 File Offset: 0x00055118
	private void Pull()
	{
		for (int i = 0; i < this.rigs.Count; i++)
		{
			bool flag = !(this.rigs[i] == null);
			if (flag)
			{
				float time = Vector3.Distance(this.rigs[i].position, this.effectTransform.position);
				float d = this.curve.Evaluate(time);
				WilhelmPhysicsFunctions.AddForceWithMinWeight(this.rigs[i], (this.effectTransform.position + this.randomOffset[i] - this.rigs[i].position).normalized * this.force * d * this.floatynes[i], ForceMode.Force, 60f);
				bool flag2 = this.healths[i];
				if (flag2)
				{
					this.healths[i].TakeDamage(Time.deltaTime * this.damageMultiplier, Vector3.zero, null, DamageType.Default);
				}
				this.rigs[i].AddForce(this.rigs[i].velocity * Time.deltaTime * -200f * this.floatynes[i], ForceMode.Acceleration);
			}
		}
	}

	// Token: 0x06000E7E RID: 3710 RVA: 0x0005708C File Offset: 0x0005528C
	private void Throw(Vector3 target, float curveMultiplier)
	{
		for (int i = 0; i < this.rigs.Count; i++)
		{
			float time = Vector3.Distance(this.rigs[i].position, this.effectTransform.position);
			float d = this.curve.Evaluate(time);
			WilhelmPhysicsFunctions.AddForceWithMinWeight(this.rigs[i], (target + this.randomOffset[i] * 0.5f - this.rigs[i].position).normalized * curveMultiplier * this.throwForce * d * this.floatynes[i], ForceMode.Force, 60f);
			this.rigs[i].AddForce(this.rigs[i].velocity * Time.deltaTime * -400f * this.floatynes[i], ForceMode.Acceleration);
		}
	}

	// Token: 0x06000E7F RID: 3711 RVA: 0x000571B0 File Offset: 0x000553B0
	public Telekinesis_its_not_telekanesis_dumbass()
	{
	}

	// Token: 0x04001156 RID: 4438
	private SpawnedAttack spawnedAttack;

	// Token: 0x04001157 RID: 4439
	private List<Rigidbody> rigs = new List<Rigidbody>();

	// Token: 0x04001158 RID: 4440
	private List<Vector3> randomOffset = new List<Vector3>();

	// Token: 0x04001159 RID: 4441
	private List<float> floatynes = new List<float>();

	// Token: 0x0400115A RID: 4442
	private Transform effectTransform;

	// Token: 0x0400115B RID: 4443
	public float radius = 5f;

	// Token: 0x0400115C RID: 4444
	public float force = 1000f;

	// Token: 0x0400115D RID: 4445
	public AnimationCurve curve;

	// Token: 0x0400115E RID: 4446
	public AnimationCurve upCurve;

	// Token: 0x0400115F RID: 4447
	public AnimationCurve throwCurve;

	// Token: 0x04001160 RID: 4448
	public float throwForce;

	// Token: 0x04001161 RID: 4449
	private Damagable[] healths;

	public float damageMultiplier = 20f;
}
