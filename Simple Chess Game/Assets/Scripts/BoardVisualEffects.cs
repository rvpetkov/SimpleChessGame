using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible for highlighting selections, moves and other chess board situations.
/// </summary>
public class BoardVisualEffects : MonoBehaviour {
    #region Public variables

    public static BoardVisualEffects Instance { get; set; }

	[Tooltip("This object is used to represent if the King is in check.")]
    public GameObject checkSelectionBox;
	[Tooltip("This object is used to represent if the King is in checkmate.")]
    public GameObject checkMateVizualizationTile;

	[Tooltip("This object is used to represent the currently selected piece.")]
    public GameObject selectionBoxPrefab;
	[Tooltip("This object is used to display the last move.")]
    public GameObject moveDisplayTilePrefab;

    #endregion

    #region Private variables

    private GameObject currentHighlight;

    private GameObject kingInCheck;
    private GameObject kingInCheckMate;

    private GameObject selectionBox;
    private GameObject moveDisplayTileBegin;
    private GameObject moveDisplayTileEnd;

    #endregion

    void Awake()
    {
        if (Instance == null)
        {       //Check if instance already exists
            Instance = this;
        }
        else if (Instance != this)
        {   //If instance already exists and it's not this:
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InstantiateEffectPrefabs();
    }

    private void InstantiateEffectPrefabs()
    {
        //The kingInCheck is used to visualize when the King is in check.
        kingInCheck = Instantiate(checkSelectionBox);
        kingInCheck.SetActive(false);

        //The kingInCheck is used to visualize when the King is in checkmate.
        kingInCheckMate = Instantiate(checkMateVizualizationTile);
        kingInCheckMate.SetActive(false);

        //The selectionBox is used to display the currently selected chess piece.
        selectionBox = Instantiate(selectionBoxPrefab);
        selectionBox.SetActive(false);

        //The moveDisplayTileBegin and moveDisplayTileEnd are used to display the last move that was made.
        moveDisplayTileBegin = Instantiate(moveDisplayTilePrefab);
        moveDisplayTileBegin.SetActive(false);
        moveDisplayTileEnd = Instantiate(moveDisplayTilePrefab);
        moveDisplayTileEnd.SetActive(false);
    }

	public void DisplaySelectionBox(int x, int y)
	{
		selectionBox.transform.position = new Vector3(x, y);
		selectionBox.SetActive(true);
	}

    public void DisplayLegalMoves(bool [,] moves)
    {
        for (int x = 0; x < BoardManager.BOARD_SIZE; x++)
        {
            for (int y = 0; y < BoardManager.BOARD_SIZE; y++)
            {
                if(moves[x, y])
                {
                    currentHighlight = ObjectPool.instance.GetPooledObject();
                    currentHighlight.transform.position = new Vector3(x, y);
                    currentHighlight.SetActive(true);
                }
            }
        }
    }

    public void DisplayKingInCheck(int x, int y)
    {
        kingInCheck.transform.position = new Vector3(x, y);
        kingInCheck.SetActive(true);
    }

    public void DisplayCheckMate(int x, int y)
    {
        HideDisplayedMoves();

        kingInCheckMate.transform.position = new Vector3(x, y);
        kingInCheckMate.SetActive(true);
    }

	public void DisplayLastMove(Vector3 beginPosition, Vector3 endPosition)
	{
		moveDisplayTileBegin.transform.position = beginPosition;
		moveDisplayTileBegin.SetActive(true);

		moveDisplayTileEnd.transform.position = endPosition;
		moveDisplayTileEnd.SetActive(true);
	}

	public void HideSelectionBox()
	{
		selectionBox.SetActive(false);
	}

    public void HideDisplayedMoves()
    {
        ObjectPool.instance.DisableAllPooledObjects();
    }

    public void HideKingVisuals()
    {
        kingInCheck.SetActive(false);
        kingInCheckMate.SetActive(false);
    }

    public void HideVisualizedLastMove()
    {
        moveDisplayTileBegin.SetActive(false);
        moveDisplayTileEnd.SetActive(false);
    }

    public void HideAllVisuals()
    {
        HideSelectionBox();
        HideKingVisuals();
        HideDisplayedMoves();
        HideVisualizedLastMove();
    }
}
