using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Cambia dinámicamente el 'steeringProvider' del Agent2D
public class SteeringController : MonoBehaviour
{
	public Agent2D agent;

	[Header("Providers preparados en la escena")]
	public SteeringProvider wanderTree;   // aquí tienes tu PatrolBehavior2D
	public SteeringProvider chaseTree;
	public Transform player;
	public float detectRadius = 8f;
	public float exitDelay = 2f;

	// referencia directa al patrullaje para limpiar el destino
	private PatrolBehavior2D patrol;
	private bool chasing = false;
	private float lastInsideTime = -999f;

	void Awake()
	{
		// intenta obtener el PatrolBehavior2D desde wanderTree
		if (wanderTree != null)
			patrol = wanderTree as PatrolBehavior2D;
		if (patrol == null)
			patrol = GetComponent<PatrolBehavior2D>(); // por si está en el mismo GO
	}

	void Update()
	{
		if (player == null || agent == null) return;

		float d = Vector2.Distance((Vector2)agent.transform.position, (Vector2)player.position);

		// lógica con retardo de salida (2s)
		if (!chasing)
		{
			if (d <= detectRadius)
			{
				chasing = true;
				// avisa al patrullaje: elimina destino actual
				if (patrol != null) patrol.OnChaseStart();
			}
		}
		else
		{
			if (d <= detectRadius) lastInsideTime = Time.time;
			if ((Time.time - lastInsideTime) >= exitDelay)
			{
				chasing = false;
				// opcional: al volver a patrullar, fuerza nuevo destino
				if (patrol != null) patrol.OnChaseEnd();
			}
		}

		var desired = chasing ? chaseTree : wanderTree;
		if (desired != null && agent.steeringProvider != desired)
			agent.steeringProvider = desired;
	}

	void OnDrawGizmos()
	{
		Vector3 center = (agent != null) ? agent.transform.position : transform.position;
		Gizmos.color = new Color(0f, 0.8f, 1f, 0.9f);
		Gizmos.DrawWireSphere(center, detectRadius);
	}
}
