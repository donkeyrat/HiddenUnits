using UnityEngine;
using System.Collections;
using Landfall.TABS;
using UnityEngine.Events;

public class Laser : MonoBehaviour
{
    public void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    public void Activate()
    {
        StartCoroutine(Animate(true));
    }

    public void Deactivate()
    {
        StartCoroutine(Animate(false));
    }

    public IEnumerator Animate(bool activating)
    {
        if (!line) line = GetComponent<LineRenderer>();
        var t = 0f;
        if (activating)
        {
            while (t < 1f && line)
            {
                t += Time.deltaTime;
                line.widthMultiplier = Mathf.Lerp(0f, scaleMultiplier, Mathf.Clamp(t, 0f, 1f));
                yield return null;
            }
        }
        else
        {
            while (t < 1f && line)
            {
                t += Time.deltaTime;
                line.widthMultiplier = Mathf.Lerp(scaleMultiplier, 0f, Mathf.Clamp(t, 0f, 1f));
                yield return null;
            }
        }
    }


    public void Update()
    {
        if (!line) line = GetComponent<LineRenderer>();
        line.SetPosition(0, p2.transform.position);
        if (Physics.Raycast(p2.transform.position, p2.transform.forward, out var hit, maxDistance, layer))
        {
            if (hit.collider)
            {
                line.SetPosition(1, hit.point);
                p1.transform.position = hit.point;
                hitEvent.Invoke();
            }
        }
        else
        {
            line.SetPosition(1, p2.transform.forward*maxDistance);
            p1.transform.position = p2.transform.forward*maxDistance;
        }
    }

    private LineRenderer line;
    
    [Header("Line Settings")]
    
    public GameObject p1;

    public GameObject p2;

    public float maxDistance = 10f;
    
    [Header("Animation")]

    public float scaleMultiplier = 1f;

    [Header("Hit")]
    
    public UnityEvent hitEvent = new UnityEvent();

    public LayerMask layer;
}
