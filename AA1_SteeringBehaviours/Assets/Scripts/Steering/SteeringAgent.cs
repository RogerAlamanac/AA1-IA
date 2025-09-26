using UnityEngine;

public class SteeringAgent : MonoBehaviour 
{
    public float maxSpeed = 5f;
    public float maxForce = 10f;

    private Rigidbody rb;
    private ISteeringBehavior steeringBehavior;

    public Vector2 Velocity => rb.velocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetBehavior(ISteeringBehavior behavior)
    {
        steeringBehavior = behavior;
    }

    void FixedUpdate()
    {
        if (steeringBehavior == null) return;

        Vector2 force = steeringBehavior.GetForce(this);
        force = Vector2.ClampMagnitude(force, maxForce);

        rb.AddForce(force, ForceMode.Acceleration);

        // Limitar velocitat
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxSpeed);
    }
}
