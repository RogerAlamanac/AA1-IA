using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderBehavior : MonoBehaviour, ISteeringBehavior
{
	public float wanderRadius = 1.2f;      // Radio del c�rculo de wander
	public float wanderDistance = 2.0f;    // Distancia del c�rculo frente al agente
	public float wanderJitter = 40f;       // Grados/seg de variaci�n aleatoria

	private float wanderAngle = 0f;

	public Vector2 CalculateSteeringForce(Agent2D agent)
	{
		float dt = Time.deltaTime;

		// Cambiar el �ngulo aleatoriamente
		wanderAngle += Random.Range(-wanderJitter, wanderJitter) * dt;

		// Posici�n del c�rculo delante del agente
		Vector2 circleCenter = agent.Velocity.normalized * wanderDistance;
		Vector2 displacement = new Vector2(Mathf.Cos(wanderAngle), Mathf.Sin(wanderAngle)) * wanderRadius;

		Vector2 wanderTarget = circleCenter + displacement;

		// Steering hacia el punto de wander
		Vector2 desired = wanderTarget.normalized * agent.MaxSpeed;
		Vector2 steering = desired - agent.Velocity;
		steering /= Mathf.Max(agent.MaxSpeed, 0.0001f);
		steering *= agent.MaxForce;

		return steering;
	}
}
