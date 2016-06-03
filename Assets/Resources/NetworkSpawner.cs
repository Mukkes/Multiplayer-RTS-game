using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkSpawner : NetworkBehaviour
{
	public GameObject oreDepositPrefab;
	public List<Vector3> positions = new List<Vector3>() {
		new Vector3(0, 0, -10)
	};

	public override void OnStartServer()
	{
		foreach(Vector3 position in positions)
		{
			Quaternion rotation = Quaternion.Euler(0, 0, 0);

			GameObject oreDeposit = (GameObject)Instantiate(oreDepositPrefab, position, rotation);
			NetworkServer.Spawn(oreDeposit);
		}
	}
}