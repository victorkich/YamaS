using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityFix : MonoBehaviour
{
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(FixGravity());
    }

    IEnumerator FixGravity()
    {
        rb.useGravity = false;
        yield return new WaitForEndOfFrame();
        rb.useGravity = true;
    }
}