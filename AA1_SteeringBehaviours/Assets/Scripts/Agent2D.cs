using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent2D : MonoBehaviour
{
	[Header("Steering")]
	public float MaxForce = 20f;
	public float MaxSpeed = 5f;
	public float Mass = 1f;

	[Header("Estado")]
	public Vector2 Velocity;

	[Header("Proveedor de steering")]
	public MonoBehaviour steeringProvider; // Debe implementar ISteeringBehavior

	[Header("Movimiento por física")]
	public bool useRigidbody2DMovement = true;

	[Header("Orientación hacia el player (eje Y)")]
	public bool faceEnabled = true;     // activar orientación automática
	public Transform target;            // asigna aquí el Player
	public float minFaceSpeed = 0.001f; // umbral para considerar dirección válida

	private Rigidbody2D rb;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	void Update()
	{
		if (!faceEnabled || target == null) return;

		Vector2 dir = (Vector2)target.position - (Vector2)transform.position;
		if (dir.sqrMagnitude < minFaceSpeed * minFaceSpeed) return;

		dir.Normalize();
		// La “punta” del sprite debe estar modelada mirando a +Y
		transform.up = dir;   // eje Y local apunta al player
	}

	void FixedUpdate()
	{
		Step(Time.fixedDeltaTime, useRigidbody2DMovement && rb != null);
	}

	private void Step(float dt, bool usePhysics)
	{
		ISteeringBehavior steering = null;
		if (steeringProvider != null) steering = steeringProvider as ISteeringBehavior;
		if (steering == null) steering = GetComponent<ISteeringBehavior>();

		Vector2 steeringForce = Vector2.zero;
		if (steering != null) steeringForce = steering.CalculateSteeringForce(this);

		Vector2 acceleration = steeringForce / Mathf.Max(Mass, 0.0001f);

		Velocity += acceleration * dt;

		float spd = Velocity.magnitude;
		if (spd > MaxSpeed) Velocity = Velocity / spd * MaxSpeed;

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
