using System.Collections;
using UnityEngine;

public class RiseOnLife : MonoBehaviour
{
	private DataHandler data;

	private bool done;

	public float time = 2f;

	public float moveMultiplier = 0.6f;

	public bool kinematic;

	public void Start()
	{
		Rise();
	}

	public void Rise()
	{
		if (!done)
		{
			data = GetComponentInChildren<DataHandler>();
			done = true;
			StartCoroutine(DoRise());
		}
	}

	private IEnumerator DoRise()
	{
		// yield return new WaitForSeconds(0.05f);
;		Rigidbody[] componentsInChildren = data.GetComponentsInChildren<Rigidbody>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i] && kinematic && !componentsInChildren[i].GetComponent<HandLeft>() && !componentsInChildren[i].GetComponent<HandRight>() && !componentsInChildren[i].GetComponent<ArmLeft>() && !componentsInChildren[i].GetComponent<ArmRight>())
			{
				componentsInChildren[i].isKinematic = true;
			}
			if (componentsInChildren[i] && !kinematic && componentsInChildren[i].GetComponent<Torso>()) { componentsInChildren[i].isKinematic = true; }
			if (componentsInChildren[i].GetComponent<HandLeft>() || componentsInChildren[i].GetComponent<HandRight>() || componentsInChildren[i].GetComponent<ArmLeft>() || componentsInChildren[i].GetComponent<ArmRight>()) { componentsInChildren[i].isKinematic = false; }
		}
		float t = 0f;
		while (t < time)
		{
			transform.position += Vector3.up * Mathf.Clamp(t * 0.1f, 0f, 1f) * Time.deltaTime * moveMultiplier;
			t += Time.deltaTime;
			yield return null;
		}
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i])
			{
				componentsInChildren[i].isKinematic = false;
			}
		}
	}
}
