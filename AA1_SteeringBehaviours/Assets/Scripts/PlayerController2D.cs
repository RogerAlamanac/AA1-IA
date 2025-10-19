using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
	public float moveSpeed = 5f;

	[Header("Solo bloquear paredes")]
	public LayerMask obstacleMask;   // asigna aquí la capa "Obstacles"

	Rigidbody2D rb;
	Collider2D col;
	Vector2 input;
	ContactFilter2D filter;
	RaycastHit2D[] hits = new RaycastHit2D[4];

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		col = GetComponent<Collider2D>();

		rb.bodyType = RigidbodyType2D.Kinematic;                   // el enemy no empuja al player
		rb.interpolation = RigidbodyInterpolation2D.Interpolate;

		filter = new ContactFilter2D
		{
			useLayerMask = true,
			layerMask = obstacleMask,
			useTriggers = false
		};
	}

	void Update()
	{
		input.x = Input.GetAxisRaw("Horizontal");
		input.y = Input.GetAxisRaw("Vertical");
		input = input.normalized;
	}

	void FixedUpdate()
	{
		Vector2 delta = input * moveSpeed * Time.fixedDeltaTime;
		if (delta.sqrMagnitude < 1e-10f) return;

		// ¿Colisionaría con Obstacles?
		int hitCount = col.Cast(delta.normalized, filter, hits, delta.magnitude);

		if (hitCount == 0)
		{
			rb.MovePosition(rb.position + delta);
		}
		// Si quieres que, al chocar, permita mover en el otro eje (deslizamiento simple),
		// te paso una versión con split X/Y.
	}
}