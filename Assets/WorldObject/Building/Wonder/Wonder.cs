using UnityEngine;
using System.Collections;

public class Wonder : Building
{
	protected override void Awake()
	{
		base.Awake();
		objectName = "Wonder";
		maxHitPoints = 100;
		cost = 1000;
		sellValue = 500;
	}

	protected override bool ShouldMakeDecision()
	{
		return false;
	}
}