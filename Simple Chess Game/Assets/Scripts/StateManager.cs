using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    GAME_STARTING = 0,
    CHOOSING_CHESS_PIECE = 1,
    CHOOSING_MOVE = 2,
    MOVING = 3,
    GAME_FINISHED = 4
}

public class StateManager : MonoBehaviour {
	[Tooltip("This Canvas is used as a Menu(pre-, post- and in-game Menu).")]
    public Canvas menu;

    [HideInInspector] public static GameState CurrentState { get; set; }
    [HideInInspector] public static bool WhiteIsPlaying { get; set; }
    [HideInInspector] public static StateManager instance = null;
    
    void Awake () {
        if (instance == null)
        {       //Check if instance already exists
            instance = this;
        }
        else if (instance != this)
        {   //If instance already exists and it's not this:
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
        }

        CurrentState = GameState.GAME_STARTING;
    }

	/// <summary>
	/// Changes the current state of the state machine.
	/// </summary>
    public void NextState()
    {
        switch (CurrentState)
        {
            case GameState.GAME_STARTING:
                CurrentState = GameState.CHOOSING_CHESS_PIECE;
                WhiteIsPlaying = true;
                break;
                
            case GameState.CHOOSING_CHESS_PIECE:
                CurrentState = GameState.CHOOSING_MOVE;
                break;

            case GameState.CHOOSING_MOVE:
                CurrentState = GameState.MOVING;
                break;

            case GameState.MOVING:
                WhiteIsPlaying = !WhiteIsPlaying;
                CurrentState = GameState.CHOOSING_CHESS_PIECE;
                break;
        }
    }

    public void PreviousState()
    {
        switch (CurrentState)
        {
            case GameState.CHOOSING_MOVE:
                CurrentState = GameState.CHOOSING_CHESS_PIECE;
                break;
        }
    }

	public void EndMatch(bool whiteWon)
	{
		CurrentState = GameState.GAME_FINISHED;

		AI.instance.DeactivateComputer();

		menu.gameObject.SetActive(true);

		Transform label = menu.transform.Find("EndGameLabel");
		label.GetComponent<Text>().text = !whiteWon ? "White Won!" : "Black Won!";
		label.gameObject.SetActive(true);
	}

	#region Menu Related Methods

    public void StartGame()
    {
        CurrentState = GameState.GAME_STARTING;

        BoardManager.Instance.InitializeBoard();
        BoardVisualEffects.Instance.HideAllVisuals();

        Transform label = menu.transform.Find("EndGameLabel");
        label.gameObject.SetActive(false);
        menu.gameObject.SetActive(false);
        NextState();
    }

    public void StartGameVsComp()
    {
        CurrentState = GameState.GAME_STARTING;

        BoardManager.Instance.InitializeBoard();
        BoardVisualEffects.Instance.HideAllVisuals();

        Transform label = menu.transform.Find("EndGameLabel");
        label.gameObject.SetActive(false);
        menu.gameObject.SetActive(false);

        AI.instance.Initialize();
        NextState();
    }

    public void QuitGame()
    {
        Debug.Log("Game will quit!");
        Application.Quit();
    }

	public void ShowMenu()
	{
		menu.gameObject.SetActive(true);
	}

	public void HideMenu()
	{
		menu.gameObject.SetActive(false);
	}

	#endregion
}
