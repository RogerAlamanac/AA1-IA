using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISteeringBehavior
{
	// Devuelve la fuerza de steering para este frame (Vector2)
	Vector2 CalculateSteeringForce(Agent2D agent);
}
