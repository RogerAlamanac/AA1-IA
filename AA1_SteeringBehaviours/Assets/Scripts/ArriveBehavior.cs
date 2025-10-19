using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArriveBehavior : MonoBehaviour, ISteeringBehavior
{
	public Transform target;
	public float slowingRadius = 2f;   // Radio de frenado
	public float stopRadius = 0.2f;    // Distancia mínima para considerar "llegado"

	public Vector2 CalculateSteeringForce(Agent2D agent)
	{
		Vector2 toTarget = (Vector2)(target.position - agent.transform.position);
		float distance = toTarget.magnitude;

		if (distance < stopRadius)
			return Vector2.zero;

		// Desired speed disminuye dentro del slowing radius
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
