using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolBehavior2D : SteeringProvider
{
	[Header("Zona de patrulla")]
	public float patrolRadius = 6f;
	public float minStep = 2f;

	[Header("Movimiento (Arrive)")]
	public float slowingRadius = 2f;
	public float stopRadius = 0.35f;

	[Header("Detección de obstáculos (Raycast)")]
	public LayerMask obstacleMask;          // capa "Obstacles"
	public float wallCheckDistance = 1.2f;  // longitud del raycast frontal

	[Header("Margen contra paredes para el destino")]
	public float agentRadius = 0.45f;       // ~mitad del ancho del enemigo
	public float goalObstacleMargin = 0.25f;// margen extra respecto a paredes

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

		// Espera dura: no moverse ni crear destinos
		if (state == State.Waiting)
		{
			if (agent.Velocity.sqrMagnitude > 0f) agent.Velocity = Vector2.zero;
			if (now < waitUntilTime) return Vector2.zero;

			state = State.Moving;
			hasTarget = false; // forzar nuevo destino al salir de la espera
		}

		// Asegurar único destino activo
		if (!hasTarget) PickNewTarget(agent);

		Vector2 pos = agent.transform.position;

		// Raycast frontal: si hay pared, parar inmediato y entrar en espera (sin crear destino aquí)
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

		// Llegada al destino: parar y esperar
		Vector2 to = currentTarget - pos;
		float dist = to.magnitude;
		if (dist <= stopRadius)
		{
			EnterWait(now, agent);
			return Vector2.zero;
		}

		// Arrive estándar
		float desiredSpeed = agent.MaxSpeed;
		if (dist < slowingRadius)
			desiredSpeed = agent.MaxSpeed * (dist / Mathf.Max(0.001f, slowingRadius));

		Vector2 desiredVel = (dist > 0.0001f) ? (to / dist) * desiredSpeed : Vector2.zero;
		Vector2 steering = (desiredVel - agent.Velocity) / Mathf.Max(agent.MaxSpeed, 0.0001f);
		steering *= agent.MaxForce;

		// Recordar una dirección razonable para el próximo frame
		if (agent.Velocity.sqrMagnitude > 0.0001f)
			lastMoveDir = agent.Velocity.normalized;
		else if (dist > 0.0001f)
			lastMoveDir = to / dist;

		return steering;
	}

	private void EnterWait(float now, Agent2D agent)
	{
		agent.Velocity = Vector2.zero;   // cortar inercia
		state = State.Waiting;
		waitUntilTime = now + waitSeconds;
		hasTarget = false;               // invalidar destino actual
	}

	private void PickNewTarget(Agent2D agent)
	{
		Vector2 center = agent.transform.position;
		float clearance = Mathf.Max(0.01f, agentRadius + goalObstacleMargin);

		// Buscar un punto válido: margen a paredes + línea directa libre
		for (int i = 0; i < 20; i++)
		{
			float r = Random.Range(minStep, patrolRadius);
			float ang = Random.Range(0f, Mathf.PI * 2f);
			Vector2 candidate = center + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * r;

			// 1) margen suficiente respecto a obstáculos
			if (Physics2D.OverlapCircle(candidate, clearance, obstacleMask) != null)
				continue;

			// 2) línea directa libre desde el agente
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

		// si no encuentra, reintenta en el siguiente frame
		hasTarget = false;
	}

	void OnDrawGizmos()
	{
		if (state == State.Moving && hasTarget)
		{
			Gizmos.color = targetColor;
			Gizmos.DrawSphere(new Vector3(currentTarget.x, currentTarget.y, 0f), 0.1f);
			Gizmos.DrawLine(transform.position, new Vector3(currentTarget.x, currentTarget.y, 0f));
		}
	}

	// Llamar cuando el zombie entra en persecución
	public void OnChaseStart()
	{
		// elimina cualquier destino previo y sale de cualquier estado de espera
		hasTarget = false;
		state = State.Moving;
		waitUntilTime = -1f;
	}

	// (opcional) por si quieres limpiar también al terminar la persecución
	public void OnChaseEnd()
	{
		hasTarget = false;      // forzar que al volver a patrullar escoja uno nuevo
		state = State.Moving;
	}
}
