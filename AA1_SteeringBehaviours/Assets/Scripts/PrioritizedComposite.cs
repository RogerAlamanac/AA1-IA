using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SteeringGroup
{
	public string name;
	public BehaviorWeight[] entries; // mescla interna per pesos (blending)
}

public class PrioritizedComposite : SteeringProvider
{
	// Arbitration per prioritats: es combinen pesos dins de cada grup i s’accepta
	// el primer grup que supera un llindar (inhibició de grups inferiors).

	[Header("Grupos de mayor a menor prioridad")]
	public SteeringGroup[] groups;

	[Header("Umbral para aceptar un grupo")]
	public float priorityThreshold = 0.1f;

	public override Vector2 CalculateSteeringForce(Agent2D agent)
	{
		for (int g = 0; g < groups.Length; g++)
		{
			Vector2 blended = Vector2.zero;

			// Blending intragrup (equilibra objectius compatibles dins la mateixa capa).
			var entries = groups[g].entries;
			for (int i = 0; i < entries.Length; i++)
			{
				var b = entries[i].behavior as ISteeringBehavior;
				if (b == null || entries[i].weight <= 0f) continue;

				blended += b.CalculateSteeringForce(agent) * entries[i].weight;
			}

			if (blended.magnitude > priorityThreshold)
				return blended; // guanya el grup prioritari (resolució de conflictes)
		}

		return Vector2.zero;
	}
}
