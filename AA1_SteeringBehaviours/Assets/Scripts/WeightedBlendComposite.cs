using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BehaviorWeight
{
	public MonoBehaviour behavior; // ISteeringBehavior
	[Range(0f, 5f)] public float weight;
}

public class WeightedBlendComposite : SteeringProvider
{
	// Mescla lineal ponderada: integra múltiples objectius compatibles i delega el truncament a l’agent.

	public BehaviorWeight[] entries;

	public override Vector2 CalculateSteeringForce(Agent2D agent)
	{
		Vector2 total = Vector2.zero;

		for (int i = 0; i < entries.Length; i++)
		{
			var b = entries[i].behavior as ISteeringBehavior;
			if (b == null || entries[i].weight <= 0f) continue;

			Vector2 f = b.CalculateSteeringForce(agent);
			total += f * entries[i].weight;
		}

		return total;
	}
}