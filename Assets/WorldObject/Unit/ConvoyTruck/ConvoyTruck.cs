using UnityEngine;
using System.Collections;

public class ConvoyTruck : Unit
{
	protected override void Awake()
	{
		base.Awake();
		objectName = "ConvoyTruck";
		hitPoints = 100;
		maxHitPoints = 100;
		cost = 100;
		sellValue = 50;
	}
}