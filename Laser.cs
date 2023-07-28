using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class Laser : MonoBehaviour
{
    public void Awake()
    {
        Line = GetComponent<LineRenderer>();
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
        var t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            Line.widthMultiplier = Mathf.Lerp(activating ? 0f : scaleMultiplier, activating ? scaleMultiplier : 0f, Mathf.Clamp(t, 0f, 1f));
            yield return null;
        }
    }


    public void Update()
    {
        Line.SetPosition(0, p2.transform.position);
        if (Physics.Raycast(p2.transform.position, p2.transform.forward, out var hit, maxDistance, layer))
        {
            if (hit.collider)
            {
                Line.SetPosition(1, hit.point);
                p1.transform.position = hit.point;
                hitEvent.Invoke();
            }
        }
        else
        {
            Line.SetPosition(1, p2.transform.forward*maxDistance);
            p1.transform.position = p2.transform.forward*maxDistance;
        }
    }

    private LineRenderer Line;
    
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
