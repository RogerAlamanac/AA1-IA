using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent2D : MonoBehaviour
{
	// Agent “contenidor” del bucle de steering: Strategy/Composite com a proveïdor de la força resultant
	// i integració F->a->v->x amb saturació per MaxForce/MaxSpeed (model clàssic de Reynolds).

	[Header("Steering")]
	public float MaxForce = 20f;
	public float MaxSpeed = 5f;
	public float Mass = 1f;

	[Header("Estado")]
	public Vector2 Velocity;

	[Header("Proveedor de steering")]
	public MonoBehaviour steeringProvider;

	[Header("Movimiento por física")]
	public bool useRigidbody2DMovement = true;

	[Header("Orientación hacia el player (eje Y)")]
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
		// Orientació “Face”: només per coherència visual respecte l’objectiu; no altera la decisió de moviment.
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
		// Demana la força d’steering al node actiu (Strategy/Composite).
		ISteeringBehavior steering = null;
		if (steeringProvider != null) steering = steeringProvider as ISteeringBehavior;
		if (steering == null) steering = GetComponent<ISteeringBehavior>();

		Vector2 steeringForce = Vector2.zero;
		if (steering != null) steeringForce = steering.CalculateSteeringForce(this);

		// Integració de la dinàmica de l’agent (F->a->v->x) amb límits de velocitat.
		Vector2 acceleration = steeringForce / Mathf.Max(Mass, 0.0001f);

		Velocity += acceleration * dt;

		float spd = Velocity.magnitude;
		if (spd > MaxSpeed) Velocity = Velocity / spd * MaxSpeed;

		// Dues vies d’efectors: via física (rb) o transform (equivalent a “locomotion layer” simple).
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