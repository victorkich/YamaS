using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceGravity : MonoBehaviour
{
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Verifica se a gravidade está desativada
        if (!rb.useGravity)
        {
            // Se estiver desativada, reative
            rb.useGravity = true;
        }
    }
}
