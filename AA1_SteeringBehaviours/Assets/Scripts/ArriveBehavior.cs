using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArriveBehavior : MonoBehaviour, ISteeringBehavior
{
	// ARRIVE: política de “seek” amb desacceleració progressiva (slowingRadius) i condició d’arribada (stopRadius).
	// Es materialitza com a velocitat desitjada dependent de la distància i força (vd - v) saturada.

	public Transform target;
	public float slowingRadius = 2f;
	public float stopRadius = 0.2f;

	public Vector2 CalculateSteeringForce(Agent2D agent)
	{
		Vector2 toTarget = (Vector2)(target.position - agent.transform.position);
		float distance = toTarget.magnitude;

		if (distance < stopRadius)
			return Vector2.zero;

		float desiredSpeed = agent.MaxSpeed;
		if (distance < slowingRadius)
			desiredSpeed = agent.MaxSpeed * (distance / slowingRadius);

		Vector2 desiredVelocity = toTarget.normalized * desiredSpeed;

		Vector2 steering = desiredVelocity - agent.Velocity;
		steering /= Mathf.Max(agent.MaxSpeed, 0.0001f);
		steering *= agent.MaxForce;

		return steering;
	}
}