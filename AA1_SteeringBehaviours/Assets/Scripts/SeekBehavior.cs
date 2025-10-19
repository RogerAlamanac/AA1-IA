using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Adjunta este script al mismo GameObject que Agent2D.
// En el Agent2D, arrástralo al campo "steeringProvider".
public class SeekBehavior : MonoBehaviour, ISteeringBehavior
{
	public Transform target; // objetivo a buscar

	public Vector2 CalculateSteeringForce(Agent2D agent)
	{
		// DesiredVelocity = Target - Agent; normalizar y escalar por MaxSpeed
		Vector2 desired = (Vector2)(target.position - agent.transform.position);
		if (desired.sqrMagnitude > 0.000001f)
			desired = desired.normalized * agent.MaxSpeed;
		else
			desired = Vector2.zero;

		// SteeringForce = ((DesiredVelocity - Velocity) / MaxSpeed) * MaxForce
		Vector2 velDelta = desired - agent.Velocity;
		velDelta /= Mathf.Max(agent.MaxSpeed, 0.0001f);
		Vector2 steeringForce = velDelta * agent.MaxForce;

		return steeringForce;
	}
}
