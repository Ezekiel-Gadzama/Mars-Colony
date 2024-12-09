using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 10f; // Movement speed

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody is missing! Please attach a Rigidbody component to the player.");
        }
        else
        {
            rb.useGravity = false; // Disable gravity
        }
    }

    void FixedUpdate()
    {
        // Get input for movement
        float moveHorizontal = Input.GetAxis("Horizontal"); // Left (-1) and Right (+1)
        float moveVertical = Input.GetAxis("Vertical"); // Forward (+1) and Backward (-1)

        // Calculate movement direction
        Vector3 movement = new Vector3(moveHorizontal, 0, moveVertical);

        // Apply movement
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }
}