using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using RTS;
using UnityEngine.SceneManagement;

public class MainMenu : Menu
{
	protected override void SetButtons()
	{
		buttons = new string[] { "New Game", "Multiplayer", "Load Game", "Change Player", "Quit Game" };
	}

	protected override void HandleButton(string text)
	{
		base.HandleButton(text);
		switch (text)
		{
			case "New Game": NewGame(); break;
			case "Multiplayer": Multiplayer(); break;
			case "Load Game": LoadGame(); break;
			case "Change Player": ChangePlayer(); break;
			case "Quit Game": ExitGame(); break;
			default: break;
		}
	}

	private void NewGame()
	{
		ResourceManager.MenuOpen = false;
		SceneManager.LoadScene("Map");
		//makes sure that the loaded level runs at normal speed
		Time.timeScale = 1.0f;
	}

	private void Multiplayer()
	{
		GetComponent<MainMenu>().enabled = false;
		GetComponent<MultiplayerMenu>().enabled = true;
	}

	void OnLevelWasLoaded()
	{
		Cursor.visible = true;
		if (PlayerManager.GetPlayerName() == "")
		{
			//no player yet selected so enable SetPlayerMenu
			GetComponent<MainMenu>().enabled = false;
			GetComponent<SelectPlayerMenu>().enabled = true;
		}
		else {
			//player selected so enable MainMenu
			GetComponent<MainMenu>().enabled = true;
			GetComponent<SelectPlayerMenu>().enabled = false;
		}
	}

	private void ChangePlayer()
	{
		GetComponent<MainMenu>().enabled = false;
		GetComponent<SelectPlayerMenu>().enabled = true;
		SelectionList.LoadEntries(PlayerManager.GetPlayerNames());
	}

	protected override void HideCurrentMenu()
	{
		GetComponent<MainMenu>().enabled = false;
	}
}