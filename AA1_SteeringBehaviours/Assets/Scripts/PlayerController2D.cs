using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
	// Control cinemàtic del “target” humà: actor exogen que genera estímuls
	// perquè els comportaments dels NPC (seek/chase/avoidance) reaccionin.

	public float moveSpeed = 5f;

	[Header("Solo bloquear paredes")]
	public LayerMask obstacleMask;

	Rigidbody2D rb;
	Collider2D col;
	Vector2 input;
	ContactFilter2D filter;
	RaycastHit2D[] hits = new RaycastHit2D[4];

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		col = GetComponent<Collider2D>();

		rb.bodyType = RigidbodyType2D.Kinematic;
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
		// Desplaçament subjecte a col·lisió amb l’entorn (no interfereix amb NPCs).
		Vector2 delta = input * moveSpeed * Time.fixedDeltaTime;
		if (delta.sqrMagnitude < 1e-10f) return;

		int hitCount = col.Cast(delta.normalized, filter, hits, delta.magnitude);

		if (hitCount == 0)
		{
			rb.MovePosition(rb.position + delta);
		}
	}
}