using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionAvoidance2D : MonoBehaviour, ISteeringBehavior
{
	// COLLISION AVOIDANCE (entorn dinàmic): selecció del “conflicte crític” dins d’un con frontal
	// i resposta reactiva tipus Flee; evita xocs locals sense perdre l’objectiu global.

	public string neighborTag = "Zombie";
	public float coneAngle = 60f;
	public float coneDistance = 3.0f;

	public Vector2 CalculateSteeringForce(Agent2D agent)
	{
		GameObject[] objs = GameObject.FindGameObjectsWithTag(neighborTag);
		if (objs == null || objs.Length == 0) return Vector2.zero;

		Vector2 pos = agent.transform.position;
		Vector2 dir = agent.Velocity.sqrMagnitude > 0.0001f ? agent.Velocity.normalized : (Vector2)transform.right;

		Transform nearest = null;
		float nearestDist = float.MaxValue;

		foreach (var go in objs)
		{
			if (go == this.gameObject) continue;

			Vector2 to = (Vector2)go.transform.position - pos;
			float dist = to.magnitude;
			if (dist > coneDistance) continue;

			float ang = Vector2.Angle(dir, to);
			if (ang > coneAngle * 0.5f) continue;

			if (dist < nearestDist)
			{
				nearestDist = dist;
				nearest = go.transform;
			}
		}

		if (nearest == null) return Vector2.zero;

		Vector2 desired = (pos - (Vector2)nearest.position).normalized * agent.MaxSpeed;
		Vector2 steering = (desired - agent.Velocity) / Mathf.Max(agent.MaxSpeed, 0.0001f);
		steering *= agent.MaxForce;
		return steering;
	}
}