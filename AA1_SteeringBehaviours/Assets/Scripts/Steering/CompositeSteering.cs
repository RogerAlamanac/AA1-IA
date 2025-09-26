using UnityEngine;
using System.Collections.Generic;

public class CompositeSteering : ISteeringBehavior
{
    private List<(ISteeringBehavior behavior, float weight)> behaviors = new List<(ISteeringBehavior, float)>();

    public void AddBehavior(ISteeringBehavior behavior, float weight)
    {
        behaviors.Add((behavior, weight));
    }

    public Vector2 GetForce(SteeringAgent agent)
    {
        Vector2 totalForce = Vector2.zero;
        float totalWeight = 0;

        foreach (var (behavior, weight) in behaviors)
        {
            totalForce += behavior.GetForce(agent) * weight;
            totalWeight += weight;
        }

        if (totalWeight > 0)
            totalForce /= totalWeight;

        return totalForce;
    }
}
