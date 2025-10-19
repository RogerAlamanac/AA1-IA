using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderBehavior : MonoBehaviour, ISteeringBehavior
{
	// Wander: random walk suau mitjançant cercle projectat al davant + jitter angular (exploració coherent).

	public float wanderRadius = 1.2f;
	public float wanderDistance = 2.0f;
	public float wanderJitter = 40f;

	private float wanderAngle = 0f;

	public Vector2 CalculateSteeringForce(Agent2D agent)
	{
		float dt = Time.deltaTime;

		wanderAngle += Random.Range(-wanderJitter, wanderJitter) * dt;

		Vector2 circleCenter = agent.Velocity.normalized * wanderDistance;
		Vector2 displacement = new Vector2(Mathf.Cos(wanderAngle), Mathf.Sin(wanderAngle)) * wanderRadius;

		Vector2 wanderTarget = circleCenter + displacement;

		Vector2 desired = wanderTarget.normalized * agent.MaxSpeed;
		Vector2 steering = desired - agent.Velocity;
		steering /= Mathf.Max(agent.MaxSpeed, 0.0001f);
		steering *= agent.MaxForce;

		return steering;
	}
}