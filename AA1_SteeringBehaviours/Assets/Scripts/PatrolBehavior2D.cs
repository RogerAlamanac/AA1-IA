using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class PatrolBehavior2D : SteeringProvider
{
	// Patrulla autònoma: selecció estocàstica de waypoints dins d’un radi
	// amb mostreig conscient d’obstacles i arribada suau (Arrive) + pauses (estats).

	[Header("Zona de patrulla")]
	public float patrolRadius = 6f;
	public float minStep = 2f;

	[Header("Movimiento (Arrive)")]
	public float slowingRadius = 2f;
	public float stopRadius = 0.35f;

	[Header("Detección de obstáculos (Raycast)")]
	public LayerMask obstacleMask;
	public float wallCheckDistance = 1.2f;

	[Header("Margen contra paredes para el destino")]
	public float agentRadius = 0.45f;
	public float goalObstacleMargin = 0.25f;

	[Header("Espera")]
	public float waitSeconds = 1.0f;

	[Header("Gizmos")]
	public Color targetColor = new Color(0.2f, 1f, 0.3f, 0.9f);

	private enum State { Moving, Waiting }
	private State state = State.Moving;

	private Vector2 currentTarget;
	private bool hasTarget = false;
	private float waitUntilTime = -1f;
	private Vector2 lastMoveDir = Vector2.right;

	public override Vector2 CalculateSteeringForce(Agent2D agent)
	{
		if (agent == null) return Vector2.zero;
		float now = Time.time;

		// FSM mínima: Moving/Waiting per introduir pauses i evitar oscil·lacions locals.
		if (state == State.Waiting)
		{
			if (agent.Velocity.sqrMagnitude > 0f) agent.Velocity = Vector2.zero;
			if (now < waitUntilTime) return Vector2.zero;

			state = State.Moving;
			hasTarget = false;
		}

		// Generació de destins “validats” (sampling amb restriccions ambientals).
		if (!hasTarget) PickNewTarget(agent);

		Vector2 pos = agent.transform.position;

		// Percepció reactiva frontal: si hi ha bloqueig immediat, pausa (evita thrashing).
		Vector2 dirMove = agent.Velocity.sqrMagnitude > 0.0001f
			? agent.Velocity.normalized
			: (hasTarget ? (currentTarget - pos).normalized : lastMoveDir);

		RaycastHit2D hitFront = Physics2D.Raycast(pos, dirMove, wallCheckDistance, obstacleMask);
		Debug.DrawRay(pos, dirMove * wallCheckDistance, hitFront.collider ? Color.red : Color.green);

		if (hitFront.collider)
		{
			EnterWait(now, agent);
			return Vector2.zero;
		}

		// Arrive: modulació de velocitat per arribar suau al waypoint.
		Vector2 to = currentTarget - pos;
		float dist = to.magnitude;
		if (dist <= stopRadius)
		{
			EnterWait(now, agent);
			return Vector2.zero;
		}

		float desiredSpeed = agent.MaxSpeed;
		if (dist < slowingRadius)
			desiredSpeed = agent.MaxSpeed * (dist / Mathf.Max(0.001f, slowingRadius));

		Vector2 desiredVel = (dist > 0.0001f) ? (to / dist) * desiredSpeed : Vector2.zero;
		Vector2 steering = (desiredVel - agent.Velocity) / Mathf.Max(agent.MaxSpeed, 0.0001f);
		steering *= agent.MaxForce;

		// Memòria direccional per estabilitzar la percepció quan v es aproximadament igual a 0.
		if (agent.Velocity.sqrMagnitude > 0.0001f)
			lastMoveDir = agent.Velocity.normalized;
		else if (dist > 0.0001f)
			lastMoveDir = to / dist;

		return steering;
	}

	private void EnterWait(float now, Agent2D agent)
	{
		// Tall d’inèrcia + transició d’estat (política de descans de patrulla).
		agent.Velocity = Vector2.zero;
		state = State.Waiting;
		waitUntilTime = now + waitSeconds;
		hasTarget = false;
	}

	private void PickNewTarget(Agent2D agent)
	{
		// Selecció de waypoint amb “gating” geomètric: marge a obstacles i línia clara.
		Vector2 center = agent.transform.position;
		float clearance = Mathf.Max(0.01f, agentRadius + goalObstacleMargin);

		for (int i = 0; i < 20; i++)
		{
			float r = Random.Range(minStep, patrolRadius);
			float ang = Random.Range(0f, Mathf.PI * 2f);
			Vector2 candidate = center + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * r;

			if (Physics2D.OverlapCircle(candidate, clearance, obstacleMask) != null)
				continue;

			Vector2 dir = candidate - center;
			float d = dir.magnitude;
			if (d < minStep) continue;
			dir /= d;

			if (Physics2D.Raycast(center, dir, d, obstacleMask))
				continue;

			currentTarget = candidate;
			hasTarget = true;
			return;
		}

		// Reintenta al següent tick si no hi ha candidat (robustesa).
		hasTarget = false;
	}

	void OnDrawGizmos()
	{
		// Diagnòstic visual del “goal” actiu (estil depuració de steering).
		if (state == State.Moving && hasTarget)
		{
			Gizmos.color = targetColor;
			Gizmos.DrawSphere(new Vector3(currentTarget.x, currentTarget.y, 0f), 0.1f);
			Gizmos.DrawLine(transform.position, new Vector3(currentTarget.x, currentTarget.y, 0f));
		}
	}

	// Hooks per coordinar amb la FSM externa (persecució vs patrulla).
	public void OnChaseStart()
	{
		hasTarget = false;
		state = State.Moving;
		waitUntilTime = -1f;
	}

	public void OnChaseEnd()
	{
		hasTarget = false;
		state = State.Moving;
	}
}