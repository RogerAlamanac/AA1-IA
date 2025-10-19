using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SteeringProvider : MonoBehaviour, ISteeringBehavior
{
	// Node base per aplicar Strategy/Composite sobre l’Agent2D (retorna força resultant d’aquest “subarbre”).
	public abstract Vector2 CalculateSteeringForce(Agent2D agent);
}