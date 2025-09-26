using UnityEngine;

public interface ISteeringBehavior
{
    Vector2 GetForce(SteeringAgent agent);
}
