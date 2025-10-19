using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base para poder arrastrar al Agent2D.steeringProvider
public abstract class SteeringProvider : MonoBehaviour, ISteeringBehavior
{
	public abstract Vector2 CalculateSteeringForce(Agent2D agent);
}
