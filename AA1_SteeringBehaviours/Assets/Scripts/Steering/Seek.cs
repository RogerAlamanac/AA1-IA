using UnityEngine;

public class Seek : ISteeringBehavior
{
    private Transform target;

    public Seek(Transform target)
    {
        this.target = target;
    }

    public Vector2 GetForce(SteeringAgent agent)
    {
        Vector2 desired = (target.position - agent.transform.position).normalized * agent.maxSpeed;
        return desired - agent.Velocity;
    }
}
