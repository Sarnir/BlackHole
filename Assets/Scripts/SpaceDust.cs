using UnityEngine;
using System.Collections;

public class SpaceDust : MonoBehaviour
{
    Rigidbody2D rb;

    public float startVelocity;

    void Start ()
    {
        rb = GetComponent<Rigidbody2D>();

        Reset();
	}

    void Reset()
    {
        rb.velocity = transform.up * startVelocity;
    }
	
	void Update ()
    {
	
	}
}
