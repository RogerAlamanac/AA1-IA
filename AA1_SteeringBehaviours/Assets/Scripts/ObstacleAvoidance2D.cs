using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleAvoidance2D : MonoBehaviour, ISteeringBehavior
{
	// OBSTACLE AVOIDANCE (entorn est�tic): percepci� predictiva amb feelers (CircleCast),
	// c�lcul d�un punt d�evitaci� i component tangencial per vorejar, amb �blending� cap al target.
	// Gating opcional per filtrar obstacles que no comprometen la l�nia de visi�/progr�s.

	[Header("Detecci�n (din�mica)")]
	public LayerMask obstacleMask;
	public float baseLookAhead = 3.0f;
	public float speedLookAhead = 0.7f;
	public float sideFeelerAngle = 30f;
	public float sideFeelerScale = 0.85f;

	[Header("Detecci�n (fija)")]
	public bool useFixedLookAhead = true;
	public float frontLookAhead = 1.6f;
	public float sideLookAhead = 1.0f;

	[Header("Respuesta")]
	public float avoidDistance = 2.0f;
	public float tangentBias = 1.4f;
	public float clearance = 0.4f;
	public float agentRadius = 0.5f;
	public float skin = 0.08f;

	[Header("Objetivo / mezcla hacia el target")]
	public Transform target;
	public float targetBias = 0.6f;

	[Header("Gating opcional (puedes dejarlo as�)")]
	public float lookAheadToTargetFactor = 0.8f;
	public float ignoreIfHitBeyondTargetFactor = 0.95f;
	public bool requireLOSBlock = true;
	public float losRadiusMultiplier = 1.0f;

	private Vector2 lastDir = Vector2.right;

	public Vector2 CalculateSteeringForce(Agent2D agent)
	{
		// Direcci� de refer�ncia per a la predicci� (estable quan la v �s baixa).
		Vector2 dir;
		if (agent.Velocity.sqrMagnitude > 0.0001f) { dir = agent.Velocity.normalized; lastDir = dir; }
		else { dir = lastDir; }

		// Anticipaci� fixa o din�mica segons velocitat (trade-off estabilitat/reacci�).
		float lookFront, lookSide;
		if (useFixedLookAhead)
		{
			lookFront = Mathf.Max(0.01f, frontLookAhead);
			lookSide = Mathf.Max(0.01f, sideLookAhead);
		}
		else
		{
			float look = Mathf.Max(baseLookAhead, baseLookAhead + agent.Velocity.magnitude * speedLookAhead);
			if (target != null)
			{
				float distToTarget = Vector2.Distance(agent.transform.position, target.position);
				float cap = distToTarget * Mathf.Clamp01(lookAheadToTargetFactor);
				look = Mathf.Min(look, Mathf.Max(cap, baseLookAhead));
			}
			lookFront = look;
			lookSide = look * Mathf.Clamp01(sideFeelerScale);
		}

		// Feelers frontal + laterals (volum�trics).
		RaycastHit2D hit;
		bool hasHit = CastFeelerCircle(agent, dir, lookFront, out hit);

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

		// Filtrat per rellev�ncia respecte el target (l�nia de visi� i dist�ncia).
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

		// Punt d�evitaci� + tang�ncia per vorejar i �blending� cap a l�objectiu.
		float totalClearance = Mathf.Max(0f, agentRadius + clearance + avoidDistance + skin);
		Vector2 avoidTarget = hit.point + hit.normal * totalClearance;

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

		Vector2 toAvoid = ((avoidTarget - (Vector2)agent.transform.position).normalized);
		Vector2 desiredDir = (toAvoid + chosenTangent * tangentBias + toTargetDir * targetBias).normalized;

		// For�a final com a �seek� a la direcci� corregida.
		float inward = Vector2.Dot(desiredDir, -hit.normal);
		if (inward > 0f)
		{
			desiredDir -= (-hit.normal) * inward;
			desiredDir = desiredDir.sqrMagnitude > 1e-6f ? desiredDir.normalized : chosenTangent;
		}

		Vector2 desired = desiredDir * agent.MaxSpeed;
		Vector2 steering = (desired - agent.Velocity) / Mathf.Max(agent.MaxSpeed, 0.0001f);
		steering *= agent.MaxForce;

		return steering;
	}

	private bool CastFeelerCircle(Agent2D agent, Vector2 dir, float dist, out RaycastHit2D hit)
	{
		Vector2 pos = agent.transform.position;
		float startOffset = Mathf.Max(0f, (agentRadius + clearance) * 0.9f);
		Vector2 start = pos + dir * startOffset;

		float radius = Mathf.Max(0.01f, agentRadius + clearance + skin);
		hit = Physics2D.CircleCast(start, radius, dir, dist, obstacleMask);
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