using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class BlackHole : MonoBehaviour
{
    public float range = 10f;
    public float startingMass;
    public float massGrowingFactor;
    public Vector3 scaleGrowingFactor;
    
    Rigidbody2D ownRb;
    bool isGrowing;
    float initialMass;
    Vector3 initialScale;

    void Awake ()
    {
        ownRb = GetComponent<Rigidbody2D>();
        initialMass = ownRb.mass;
        initialScale = transform.localScale;

        transform.DORotate(new Vector3(0f, 0f, 360f), 2f, RotateMode.LocalAxisAdd).SetLoops(-1).SetEase(Ease.Linear);

        Reset();
    }

    public void Reset()
    {
        isGrowing = false;
        ownRb.mass = initialMass;
        transform.localScale = initialScale;
    }

    public void StartGrowing()
    {
        ownRb.mass *= 2f;
        isGrowing = true;
    }

    void FixedUpdate()
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, range);
        List<Rigidbody2D> rbs = new List<Rigidbody2D>();

        foreach (Collider2D c in cols)
        {
            Rigidbody2D rb = c.attachedRigidbody;
            if (rb != null && rb != ownRb && !rbs.Contains(rb))
            {
                rbs.Add(rb);
                Vector3 offset = transform.position - c.transform.position;
                rb.AddForce(offset / offset.sqrMagnitude * ownRb.mass);
            }
        }
        
        if (isGrowing)
        {
            ownRb.mass += massGrowingFactor;
            //transform.localScale += scaleGrowingFactor;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        SuckIn(collider.gameObject);
    }

    void SuckIn(GameObject objectSuckedIn)
    {
        float dyingDuration = 2f;

        if(objectSuckedIn.tag == "Player")
        {
            objectSuckedIn.GetComponent<PlayerController>().SetDying();
        }
        else
        {
            var rb = objectSuckedIn.GetComponent<Rigidbody2D>();

            if (rb != null)
                rb.drag = 100f;
        }

        objectSuckedIn.transform.DORotate(transform.rotation.eulerAngles - new Vector3(0f, 0f, 720f), dyingDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear);
        objectSuckedIn.transform.DOScale(Vector3.zero, dyingDuration).OnComplete(() =>
        {
            if (objectSuckedIn.tag == "Player")
            {
                objectSuckedIn.GetComponent<PlayerController>().SetDead();
            }
            else
            {
                Destroy(objectSuckedIn);
            }
        });
    }
}
