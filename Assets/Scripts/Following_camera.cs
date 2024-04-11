using UnityEngine;

public class Following_camera : MonoBehaviour
{
    public Transform target; // The target object the camera should follow
    public float height = 10.0f; // The height of the camera above the target object
    public float distance = 5.0f; // Distance from the target object
    public float rotationSpeed = 50.0f; // Speed at which the camera rotates around the target

    private Vector3 offset; // Offset from the target object

    void Start()
    {
        // Calculate initial offset based on the target's bounds (if it has a renderer)
        if (target.GetComponent<Renderer>())
        {
            Bounds bounds = target.GetComponent<Renderer>().bounds;
            offset = new Vector3(0, bounds.size.y + height, bounds.size.z + distance);
        }
        else
        {
            offset = new Vector3(0, height, distance);
        }
    }

    void FixedUpdate()
    {
        if (target)
        {
            // Rotate around the target
            transform.RotateAround(target.position, Vector3.up, rotationSpeed * Time.deltaTime);

            // Calculate the current offset
            offset = Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, Vector3.up) * offset;

            // Set the position of the camera
            transform.position = target.position + offset;

            // Look at the target
            transform.LookAt(target.position);
        }
    }
}
