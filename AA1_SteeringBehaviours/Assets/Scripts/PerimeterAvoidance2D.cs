using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerimeterAvoidance2D : MonoBehaviour, ISteeringBehavior
{
	// Containment: manté l’agent dins d’un perímetre rectangular aplicant forces de retorn
	// quan s’apropa a les vores (barrier/region constraint).

	public Rect area = new Rect(-10, -6, 20, 12);
	public float border = 1.0f;

	public Vector2 CalculateSteeringForce(Agent2D agent)
	{
		Vector2 pos = agent.transform.position;
		Vector2 desired = Vector2.zero;

		float minX = area.xMin + border;
		float maxX = area.xMax - border;
		float minY = area.yMin + border;
		float maxY = area.yMax - border;

		if (pos.x < minX) desired.x = agent.MaxSpeed;
		else if (pos.x > maxX) desired.x = -agent.MaxSpeed;

		if (pos.y < minY) desired.y = agent.MaxSpeed;
		else if (pos.y > maxY) desired.y = -agent.MaxSpeed;

		if (desired == Vector2.zero) return Vector2.zero;

		Vector2 steering = (desired - agent.Velocity) / Mathf.Max(agent.MaxSpeed, 0.0001f);
		steering *= agent.MaxForce;
		return steering;
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.DrawWireCube(area.center, area.size);
	}
}