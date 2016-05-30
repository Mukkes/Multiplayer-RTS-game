using UnityEngine;
using RTS;
using UnityEngine.Networking;

/**
 * Singleton that handles the management of game state. This includes
 * detecting when a game has been finished and what to do from there.
 */

public class GameManager : NetworkBehaviour
{

	private static bool created = false;
	private bool initialised = false;
	private VictoryCondition[] victoryConditions;
	private Player[] players;
	private HUD hud;

	void Awake()
	{
		PlayerManager.Reset();
		
		if (!created)
		{
			DontDestroyOnLoad(transform.gameObject);
			created = true;
			initialised = true;
		}
		else {
			Destroy(this.gameObject);
		}
		if (initialised)
		{
			LoadDetails();
		}
	}

	void OnLevelWasLoaded()
	{
		if (initialised)
		{
			LoadDetails();
		}
	}

	private void LoadDetails()
	{
		players = FindObjectsOfType(typeof(Player)) as Player[];
		foreach (Player player in players)
		{
			if (player.isLocalPlayer && player.human) hud = player.GetComponentInChildren<HUD>();
		}
		victoryConditions = FindObjectsOfType(typeof(VictoryCondition)) as VictoryCondition[];
		if ((victoryConditions != null) && (players.Length > 1))
		{
			foreach (VictoryCondition victoryCondition in victoryConditions)
			{
				victoryCondition.SetPlayers(players);
			}
		}
	}

	void Update()
	{
		UpdatePlayers();

		if ((victoryConditions != null) && (players.Length > 1))
		{
			foreach (VictoryCondition victoryCondition in victoryConditions)
			{
				if (victoryCondition.GameFinished())
				{
					PauseMenu pauseMenu = hud.GetComponent<PauseMenu>();
					if (pauseMenu) pauseMenu.enabled = false;
					ResultsScreen resultsScreen = hud.GetComponent<ResultsScreen>();
					resultsScreen.SetMetVictoryCondition(victoryCondition);
					resultsScreen.enabled = true;
					Time.timeScale = 0.0f;
					Cursor.visible = true;
					ResourceManager.MenuOpen = true;
					hud.enabled = false;
				}
			}
		}
	}

	private void UpdatePlayers()
	{
		Player[] players = FindObjectsOfType(typeof(Player)) as Player[];
		if (this.players.Length != players.Length)
		{
			LoadDetails();
		}
	}

}