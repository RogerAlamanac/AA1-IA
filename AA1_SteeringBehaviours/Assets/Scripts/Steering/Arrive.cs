using UnityEngine;

public class Arrive : ISteeringBehavior
{
    private Transform target;
    private float slowingRadius;

    public Arrive(Transform target, float slowingRadius = 3f)
    {
        this.target = target;
        this.slowingRadius = slowingRadius;
    }

    public Vector2 GetForce(SteeringAgent agent)
    {
        Vector2 toTarget = target.position - agent.transform.position;
        float distance = toTarget.magnitude;

        if (distance < 0.1f) return Vector2.zero;

        float speed = agent.maxSpeed;
        if (distance < slowingRadius)
        {
            speed = agent.maxSpeed * (distance / slowingRadius);
        }

        Vector2 desired = toTarget.normalized * speed;
        return desired - agent.Velocity;
    }
}
