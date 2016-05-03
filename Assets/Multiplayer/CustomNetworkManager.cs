using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using RTS;

public class CustomNetworkManager : NetworkManager {

	private void SpawnWorker(Player player)
	{
		string unitName = "Worker";
		
		player.AddUnit(unitName, Vector3.zero, default(Quaternion));
	}

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		Debug.Log("Spawn new player.");
		var player = (GameObject)Instantiate(playerPrefab, new Vector3(), Quaternion.identity);
		SpawnWorker(player.GetComponent<Player>());
		NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
	}
}
