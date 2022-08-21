using UnityEngine;
using UnityEngine.Events;

public class FindMeshForParticleSystem : MonoBehaviour
{
	public enum MeshType
	{
		Meshrenderer,
		SkinnedMeshRenderer,
		NonUnitMeshrenderer
	}

	public MeshType meshType;

	private MeshRenderer[] AllmeshRend;

	private MeshRenderer meshRend;

	private SkinnedMeshRenderer[] AllSkinnedmeshRend;

	private SkinnedMeshRenderer skinnedMeshRend;

	private ParticleSystem[] particleSystems;

	private ParticleSystem part;

	public UnityEvent FindEvent;

	public bool disableMesh;

	public bool disableParticles;

	private bool meshAssigned;

	private void Start()
	{
		if (meshType == MeshType.Meshrenderer)
		{
			AllmeshRend = base.transform.root.GetComponentsInChildren<MeshRenderer>();
			for (int i = 0; i < AllmeshRend.Length; i++)
			{
				if (AllmeshRend[i].CompareTag("UnitMesh") && !meshAssigned)
				{
					meshRend = AllmeshRend[i];
					meshAssigned = true;
				}
				if (disableMesh)
				{
					AllmeshRend[i].enabled = false;
				}
			}
			if ((bool)meshRend)
			{
				part = GetComponent<ParticleSystem>();
				ParticleSystem.ShapeModule shape = part.shape;
				shape.meshRenderer = meshRend;
				FindEvent.Invoke();
			}
			if (!disableParticles)
			{
				return;
			}
			particleSystems = base.transform.root.GetComponentsInChildren<ParticleSystem>();
			for (int j = 0; j < particleSystems.Length; j++)
			{
				if (!particleSystems[j].CompareTag("DontRemove"))
				{
					particleSystems[j].Stop();
				}
			}
		}
		else if (meshType == MeshType.SkinnedMeshRenderer)
		{
			AllSkinnedmeshRend = base.transform.root.GetComponentsInChildren<SkinnedMeshRenderer>();
			for (int k = 0; k < AllSkinnedmeshRend.Length; k++)
			{
				if (AllSkinnedmeshRend[k].CompareTag("UnitMesh"))
				{
					skinnedMeshRend = AllSkinnedmeshRend[k];
					break;
				}
			}
			if ((bool)skinnedMeshRend)
			{
				ParticleSystem component = GetComponent<ParticleSystem>();
				ParticleSystem.ShapeModule shape2 = component.shape;
				shape2.skinnedMeshRenderer = skinnedMeshRend;
				FindEvent.Invoke();
			}
		}
		else
		{
			if (meshType != MeshType.NonUnitMeshrenderer)
			{
				return;
			}
			AllmeshRend = base.transform.root.GetComponentsInChildren<MeshRenderer>();
			for (int l = 0; l < AllmeshRend.Length; l++)
			{
				if (!meshAssigned)
				{
					meshRend = AllmeshRend[l];
					meshAssigned = true;
				}
				if (disableMesh)
				{
					AllmeshRend[l].enabled = false;
				}
			}
			if ((bool)meshRend)
			{
				part = GetComponent<ParticleSystem>();
				ParticleSystem.ShapeModule shape3 = part.shape;
				shape3.meshRenderer = meshRend;
				FindEvent.Invoke();
			}
		}
	}
}
