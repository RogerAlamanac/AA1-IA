using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekBehavior : MonoBehaviour, ISteeringBehavior
{
	// Seek: direcció al target amb saturació clàssica (desitjada - actual).
	public Transform target;

	public Vector2 CalculateSteeringForce(Agent2D agent)
	{
		Vector2 desired = (Vector2)(target.position - agent.transform.position);
		if (desired.sqrMagnitude > 0.000001f)
			desired = desired.normalized * agent.MaxSpeed;
		else
			desired = Vector2.zero;

		Vector2 velDelta = desired - agent.Velocity;
		velDelta /= Mathf.Max(agent.MaxSpeed, 0.0001f);
		Vector2 steeringForce = velDelta * agent.MaxForce;

		return steeringForce;
	}
}