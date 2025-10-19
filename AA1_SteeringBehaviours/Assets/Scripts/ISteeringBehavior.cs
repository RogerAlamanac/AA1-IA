using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISteeringBehavior
{
	// Node de Strategy/Composite que retorna la for�a resultant per a l�agent en aquest �tick�.
	Vector2 CalculateSteeringForce(Agent2D agent);
}