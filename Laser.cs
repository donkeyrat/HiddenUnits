using UnityEngine;
using System.Collections;

public class Laser : MonoBehaviour
{
    public void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    public void Activate()
    {
        if (!hasActivated)
        {
            hasActivated = true;
            activating = true;
            StartCoroutine(Animate());
        }
    }

    public void Deactivate()
    {
        if (!hasDeactivated)
        {
            hasDeactivated = true;
            deactivating = true;
            StartCoroutine(Animate());
        }
    }

    public IEnumerator Animate()
    {
        var t = 0f;
        if (activating)
        {
            while (t < 1f)
            {
                t += Time.deltaTime;
                line.widthMultiplier += activateMultiplier * Time.deltaTime;
                yield return null;
            }
            activating = false;
        }
        else if (deactivating)
        {
            while (t < 1f)
            {
                t += Time.deltaTime;
                line.widthMultiplier -= deactivateMultiplier * Time.deltaTime;
                yield return null;
            }
            deactivating = false;
        }
        yield break;
    }


    public void Update()
    {
        line.SetPosition(0, p2.transform.position);
        RaycastHit hit;
        if (Physics.Raycast(p2.transform.position, p2.transform.forward, out hit, maxDistance, LayerMask.GetMask(new string[] { layer })))
        {
            if (hit.collider)
            {
                line.SetPosition(1, hit.point);
                p1.transform.position = hit.point;
            }
        }
        else
        {
            line.SetPosition(1, p2.transform.forward*maxDistance);
            p1.transform.position = p2.transform.forward*maxDistance;
        }
    }

    public GameObject p1;

    public GameObject p2;

    public float maxDistance = 10f;

    public LineRenderer line;

    private bool activating;

    private bool deactivating;

    public float activateMultiplier;

    public float deactivateMultiplier;

    private bool hasActivated;

    private bool hasDeactivated;

    public string layer = "MainRig";
}
