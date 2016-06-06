﻿using UnityEngine;
using System.Collections;

public class WarFactory : Building
{
	protected override void Awake()
	{
		base.Awake();
		objectName = "WarFactory";
		//maxHitPoints = 100;
		maxHitPoints = 5;
		cost = 250;
		sellValue = 125;
	}

	protected override void Start () {
		base.Start();
		actions = new string[] { "Tank", "ConvoyTruck" };
	}

	public override void PerformAction(string actionToPerform)
	{
		base.PerformAction(actionToPerform);
		CreateUnit(actionToPerform);
	}

	protected override bool ShouldMakeDecision()
	{
		return false;
	}
}
