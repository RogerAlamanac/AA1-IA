using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObstacleAvoidance2D : MonoBehaviour, ISteeringBehavior
{
	[Header("Detección (dinámica)")]
	public LayerMask obstacleMask;          // Capa de obstáculos
	public float baseLookAhead = 3.0f;      // Distancia base de mirada
	public float speedLookAhead = 0.7f;     // +lookAhead según velocidad
	public float sideFeelerAngle = 30f;     // grados feelers laterales
	public float sideFeelerScale = 0.85f;   // % del lookAhead para laterales

	[Header("Detección (fija)")]
	public bool useFixedLookAhead = true;   //  Actívalo para acortar fácil
	public float frontLookAhead = 1.6f;     //  longitud fija del feeler frontal
	public float sideLookAhead = 1.0f;     //  longitud fija de los laterales

	[Header("Respuesta")]
	public float avoidDistance = 2.0f;      // separación adicional respecto a la pared
	public float tangentBias = 1.4f;        // empuje lateral para rodear
	public float clearance = 0.4f;          // holgura extra
	public float agentRadius = 0.5f;        // radio aprox. del zombi (mitad del ancho)
	public float skin = 0.08f;              // colchón extra para no “rozar”

	[Header("Objetivo / mezcla hacia el target")]
	public Transform target;                // Player/objetivo (arrastra tu Player)
	public float targetBias = 0.6f;         // tirón hacia el player mientras evita (0.4–1.0)

	[Header("Gating opcional (puedes dejarlo así)")]
	public float lookAheadToTargetFactor = 0.8f;         // cap dinámico (si usas modo dinámico)
	public float ignoreIfHitBeyondTargetFactor = 0.95f;  // ignora impactos más lejos que el target
	public bool requireLOSBlock = true;                  // solo evitar si bloquea visión al target
	public float losRadiusMultiplier = 1.0f;             // radio LOS

	private Vector2 lastDir = Vector2.right;

	public Vector2 CalculateSteeringForce(Agent2D agent)
	{
		// 1) Dirección frontal robusta
		Vector2 dir;
		if (agent.Velocity.sqrMagnitude > 0.0001f) { dir = agent.Velocity.normalized; lastDir = dir; }
		else { dir = lastDir; }

		// 2) Cálculo del lookAhead
		float lookFront, lookSide;
		if (useFixedLookAhead)
		{
			//  MODO FIJO (corto)
			lookFront = Mathf.Max(0.01f, frontLookAhead);
			lookSide = Mathf.Max(0.01f, sideLookAhead);
		}
		else
		{
			// MODO DINÁMICO (como antes)
			float look = Mathf.Max(baseLookAhead, baseLookAhead + agent.Velocity.magnitude * speedLookAhead);

			// Cap por distancia al target (evita ver muros lejanos) — solo dinámico
			if (target != null)
			{
				float distToTarget = Vector2.Distance(agent.transform.position, target.position);
				float cap = distToTarget * Mathf.Clamp01(lookAheadToTargetFactor);
				look = Mathf.Min(look, Mathf.Max(cap, baseLookAhead));
			}

			lookFront = look;
			lookSide = look * Mathf.Clamp01(sideFeelerScale);
		}

		// 3) Feelers (CIRCLECAST): frontal, izq, dcha
		RaycastHit2D hit;
		bool hasHit = CastFeelerCircle(agent, dir, lookFront, out hit); // frontal

		if (!hasHit)
		{
			Vector2 dirL = Rotate(dir, +sideFeelerAngle * Mathf.Deg2Rad);
			RaycastHit2D hitL; hasHit = CastFeelerCircle(agent, dirL, lookSide, out hitL);
			if (hasHit) hit = hitL;
		}
		if (!hasHit)
		{
			Vector2 dirR = Rotate(dir, -sideFeelerAngle * Mathf.Deg2Rad);
			RaycastHit2D hitR; hasHit = CastFeelerCircle(agent, dirR, lookSide, out hitR);
			if (hasHit) hit = hitR;
		}
		if (!hasHit) return Vector2.zero;

		// (Opcional) gating por distancia al target y LOS (funciona en ambos modos)
		if (target != null)
		{
			float distToTarget = Vector2.Distance(agent.transform.position, target.position);

			if (hit.distance > distToTarget * Mathf.Clamp01(ignoreIfHitBeyondTargetFactor))
				return Vector2.zero;

			if (requireLOSBlock)
			{
				float losRadius = Mathf.Max(0.01f, agentRadius * Mathf.Max(0.1f, losRadiusMultiplier));
				if (!BlocksLOS((Vector2)agent.transform.position, (Vector2)target.position, losRadius))
					return Vector2.zero;
			}
		}

		// 4) Punto de evasión con margen total
		float totalClearance = Mathf.Max(0f, agentRadius + clearance + avoidDistance + skin);
		Vector2 avoidTarget = hit.point + hit.normal * totalClearance;

		// 5) Tangente: elige el lado que más apunta hacia el player (si hay)
		Vector2 tangentA = new Vector2(-hit.normal.y, hit.normal.x);
		Vector2 tangentB = -tangentA;

		Vector2 toTargetDir = Vector2.zero;
		if (target != null)
		{
			Vector2 toTarget = (Vector2)target.position - (Vector2)agent.transform.position;
			if (toTarget.sqrMagnitude > 1e-6f) toTargetDir = toTarget.normalized;
		}

		Vector2 chosenTangent = (target != null)
			? (Vector2.Dot(tangentB, toTargetDir) > Vector2.Dot(tangentA, toTargetDir) ? tangentB : tangentA)
			: (Vector2.Dot(tangentA, dir) >= 0f ? tangentA : tangentB);

		// 6) Dirección deseada = evasión + tangente + sesgo al target
		Vector2 toAvoid = ((avoidTarget - (Vector2)agent.transform.position).normalized);
		Vector2 desiredDir = (toAvoid + chosenTangent * tangentBias + toTargetDir * targetBias).normalized;

		// 7) Anti-penetración
		float inward = Vector2.Dot(desiredDir, -hit.normal);
		if (inward > 0f)
		{
			desiredDir -= (-hit.normal) * inward;
			desiredDir = desiredDir.sqrMagnitude > 1e-6f ? desiredDir.normalized : chosenTangent;
		}

		// 8) Steering estándar
		Vector2 desired = desiredDir * agent.MaxSpeed;
		Vector2 steering = (desired - agent.Velocity) / Mathf.Max(agent.MaxSpeed, 0.0001f);
		steering *= agent.MaxForce;

		// Debug (verás los rayos más cortos)
		Debug.DrawRay((Vector2)agent.transform.position, dir * lookFront, Color.green);
		Debug.DrawRay((Vector2)agent.transform.position, Rotate(dir, +sideFeelerAngle * Mathf.Deg2Rad) * lookSide, Color.green);
		Debug.DrawRay((Vector2)agent.transform.position, Rotate(dir, -sideFeelerAngle * Mathf.Deg2Rad) * lookSide, Color.green);
		Debug.DrawLine(hit.point, hit.point + hit.normal * (agentRadius + clearance + 0.2f), Color.cyan);
		Debug.DrawRay((Vector2)agent.transform.position, desiredDir * 1.5f, Color.yellow);

		return steering;
	}

	// -------- Helpers --------

	private bool CastFeelerCircle(Agent2D agent, Vector2 dir, float dist, out RaycastHit2D hit)
	{
		Vector2 pos = agent.transform.position;
		float startOffset = Mathf.Max(0f, (agentRadius + clearance) * 0.9f);
		Vector2 start = pos + dir * startOffset;

		float radius = Mathf.Max(0.01f, agentRadius + clearance + skin);
		hit = Physics2D.CircleCast(start, radius, dir, dist, obstacleMask);

		// Debug del feeler
		Debug.DrawRay(start, dir * dist, hit.collider ? Color.red : Color.green);
		return hit.collider != null;
	}

	private bool BlocksLOS(Vector2 origin, Vector2 targetPos, float radius)
	{
		Vector2 dir = targetPos - origin;
		float dist = dir.magnitude;
		if (dist <= 1e-6f) return false;
		dir /= dist;
		var losHit = Physics2D.CircleCast(origin, radius, dir, dist, obstacleMask);
		return losHit.collider != null;
	}

	private static Vector2 Rotate(Vector2 v, float rad)
	{
		float c = Mathf.Cos(rad), s = Mathf.Sin(rad);
		return new Vector2(c * v.x - s * v.y, s * v.x + c * v.y);
	}
}
