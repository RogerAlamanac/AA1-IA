using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent2D : MonoBehaviour
{
	// Agent �contenidor� del bucle de steering: Strategy/Composite com a prove�dor de la for�a resultant
	// i integraci� F->a->v->x amb saturaci� per MaxForce/MaxSpeed (model cl�ssic de Reynolds).

	[Header("Steering")]
	public float MaxForce = 20f;
	public float MaxSpeed = 5f;
	public float Mass = 1f;

	[Header("Estado")]
	public Vector2 Velocity;

	[Header("Proveedor de steering")]
	public MonoBehaviour steeringProvider;

	[Header("Movimiento por f�sica")]
	public bool useRigidbody2DMovement = true;

	[Header("Orientaci�n hacia el player (eje Y)")]
	public bool faceEnabled = true;
	public Transform target;
	public float minFaceSpeed = 0.001f;

	private Rigidbody2D rb;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	void Update()
	{
		// Orientaci� �Face�: nom�s per coher�ncia visual respecte l�objectiu; no altera la decisi� de moviment.
		if (!faceEnabled || target == null) return;

		Vector2 dir = (Vector2)target.position - (Vector2)transform.position;
		if (dir.sqrMagnitude < minFaceSpeed * minFaceSpeed) return;

		dir.Normalize();
		transform.up = dir;
	}

	void FixedUpdate()
	{
		Step(Time.fixedDeltaTime, useRigidbody2DMovement && rb != null);
	}

	private void Step(float dt, bool usePhysics)
	{
		// Demana la for�a d�steering al node actiu (Strategy/Composite).
		ISteeringBehavior steering = null;
		if (steeringProvider != null) steering = steeringProvider as ISteeringBehavior;
		if (steering == null) steering = GetComponent<ISteeringBehavior>();

		Vector2 steeringForce = Vector2.zero;
		if (steering != null) steeringForce = steering.CalculateSteeringForce(this);

		// Integraci� de la din�mica de l�agent (F->a->v->x) amb l�mits de velocitat.
		Vector2 acceleration = steeringForce / Mathf.Max(Mass, 0.0001f);

		Velocity += acceleration * dt;

		float spd = Velocity.magnitude;
		if (spd > MaxSpeed) Velocity = Velocity / spd * MaxSpeed;

		// Dues vies d�efectors: via f�sica (rb) o transform (equivalent a �locomotion layer� simple).
		if (usePhysics)
		{
			Vector2 next = rb.position + Velocity * dt;
			rb.MovePosition(next);
		}
		else
		{
			transform.position += (Vector3)(Velocity * dt);
		}
	}
}