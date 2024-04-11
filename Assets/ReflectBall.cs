using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ReflectBall : MonoBehaviour
{
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Only reflect off of walls
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Calculate Reflection
            Vector3 velocity = rb.velocity;
            Vector3 normal = collision.contacts[0].normal;
            Vector3 reflectedVelocity = Vector3.Reflect(velocity, normal);

            // Assign new velocity
            rb.velocity = reflectedVelocity;
        }
    }
}