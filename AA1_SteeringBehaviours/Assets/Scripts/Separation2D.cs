using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Separation2D : MonoBehaviour, ISteeringBehavior
{
	// Flocking—Separation: repulsió dins d’un radi veïnal, pesant més els propers (1/d^2).
	public string neighborTag = "Zombie";
	public float neighborRadius = 1.5f;

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