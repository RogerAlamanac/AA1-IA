using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SteeringGroup
{
	public string name;
	public BehaviorWeight[] entries; // se mezclan con Weighted Blending interno
}

public class PrioritizedComposite : SteeringProvider
{
	[Header("Grupos de mayor a menor prioridad")]
	public SteeringGroup[] groups;

	[Header("Umbral para aceptar un grupo")]
	public float priorityThreshold = 0.1f; // K_PRIORITY_THRESHOLD

	public override Vector2 CalculateSteeringForce(Agent2D agent)
	{
		for (int g = 0; g < groups.Length; g++)
		{
			Vector2 blended = Vector2.zero;

			// Weighted Blending interno del grupo (mismo que en slides)
			var entries = groups[g].entries;
			for (int i = 0; i < entries.Length; i++)
			{
				var b = entries[i].behavior as ISteeringBehavior;
				if (b == null || entries[i].weight <= 0f) continue;

				blended += b.CalculateSteeringForce(agent) * entries[i].weight;
			}

			if (blended.magnitude > priorityThreshold)
				return blended; // devolver y cortar (arbitration por prioridad)
		}

		return Vector2.zero;
	}
}
