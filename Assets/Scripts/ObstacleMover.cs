using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ObstacleMover : MonoBehaviour
{
    public Environment environment;
    public Transform start;  
    public Transform end;  

    [Header("Velocidad")]
    public float speedMultiplier = 1.25f;
    public float extraSpeed = 0f;

    [Header("Dirección")]
    public bool moveRight = false;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        if (!environment) return;

        float v = environment.environmentVelocity * speedMultiplier + extraSpeed;
        float dir = moveRight ? 1f : -1f;

        transform.position += Vector3.right * (dir * v * Time.deltaTime);

        
        if (!moveRight && end && transform.position.x <= end.position.x - 1.5f)
            Destroy(gameObject);

        if (moveRight && start && transform.position.x >= start.position.x + 1.5f)
            Destroy(gameObject);
    }
}


