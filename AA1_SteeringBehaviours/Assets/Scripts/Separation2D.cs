using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Separation2D : MonoBehaviour, ISteeringBehavior
{
	public string neighborTag = "Zombie";
	public float neighborRadius = 1.5f; // NEIGHBOR_RADIUS

	public Vector2 CalculateSteeringForce(Agent2D agent)
	{
		GameObject[] objs = GameObject.FindGameObjectsWithTag(neighborTag);
		if (objs == null || objs.Length == 0) return Vector2.zero;

		Vector2 pos = agent.transform.position;
		Vector2 sum = Vector2.zero;
		int count = 0;

		foreach (var go in objs)
		{
			if (go == this.gameObject) continue;
			Vector2 toMe = pos - (Vector2)go.transform.position;
			float d = toMe.magnitude;
			if (d > 0 && d < neighborRadius)
			{
				// Inversa al cuadrado para repeler más fuerte al cercano
				sum += toMe / (d * d);
				count++;
			}
		}

		if (count == 0 || sum.sqrMagnitude < 1e-6f) return Vector2.zero;

		Vector2 desired = sum.normalized * agent.MaxSpeed;
		Vector2 steering = (desired - agent.Velocity) / Mathf.Max(agent.MaxSpeed, 0.0001f);
		steering *= agent.MaxForce;
		return steering;
	}
}
