using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringController : MonoBehaviour
{
	// Commutador d’estratègies: FSM externa que alterna patrulla <-> persecució
	// segons distància al jugador, amb histèresi temporal (exitDelay).

	public Agent2D agent;

	[Header("Providers preparados en la escena")]
	public SteeringProvider wanderTree;   // patrulla/comportament base
	public SteeringProvider chaseTree;    // persecució (p.ex. Seek/Arrive + evitació)
	public Transform player;
	public float detectRadius = 8f;
	public float exitDelay = 2f;

	private PatrolBehavior2D patrol;
	private bool chasing = false;
	private float lastInsideTime = -999f;

	void Awake()
	{
		if (wanderTree != null)
			patrol = wanderTree as PatrolBehavior2D;
		if (patrol == null)
			patrol = GetComponent<PatrolBehavior2D>();
	}

	void Update()
	{
		if (player == null || agent == null) return;

		float d = Vector2.Distance((Vector2)agent.transform.position, (Vector2)player.position);

		// Histeresi: entrada immediata, sortida retardada per estabilitzar l’estat.
		if (!chasing)
		{
			if (d <= detectRadius)
			{
				chasing = true;
				if (patrol != null) patrol.OnChaseStart();
			}
		}
		else
		{
			if (d <= detectRadius) lastInsideTime = Time.time;
			if ((Time.time - lastInsideTime) >= exitDelay)
			{
				chasing = false;
				if (patrol != null) patrol.OnChaseEnd();
			}
		}

		// Injecció del “proveïdor” actiu a l’agent (Strategy swap).
		var desired = chasing ? chaseTree : wanderTree;
		if (desired != null && agent.steeringProvider != desired)
			agent.steeringProvider = desired;
	}

	void OnDrawGizmos()
	{
		// Feedback visual del radi de percepció.
		Vector3 center = (agent != null) ? agent.transform.position : transform.position;
		Gizmos.color = new Color(0f, 0.8f, 1f, 0.9f);
		Gizmos.DrawWireSphere(center, detectRadius);
	}
}