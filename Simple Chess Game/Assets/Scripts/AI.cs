using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// This class is a desperate attempt at creating an AI system that can play chess.
/// The result so far is a "player" that can randomly choose a chess piece and move it.
/// The move is chosen by the following rule: 
/// 	- if an enemy piece can be taken - do it
/// 		- if more than one enemies can be taken choose in order: Queen, Rook, Bishop, Knight, Pawn
/// 	- if no enemies are for the taking - choose randomly
/// </summary>
public class AI : MonoBehaviour
{
	#region Provate variables

    private List<ChessPiece> allPieces;
    private List<ChessPiece> piecesWithMoves;
    private ChessPiece currentPieceToPlay;
    private bool isComputerInitialized = false;

	#endregion

    [HideInInspector] public static AI instance = null;

    public bool IsComputerInitialized
    {
        get { return isComputerInitialized; }
    }

	#region Initializations

    void Awake()
    {
        if (instance == null)
        {       //Check if instance already exists
            instance = this;
        }
        else if (instance != this)
        {   //If instance already exists and it's not this:
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
        }
    }

	/// <summary>
	/// Use this method to return the AI to its original state
	/// </summary>
    public void Initialize()
    {
        allPieces = new List<ChessPiece>();
        piecesWithMoves = new List<ChessPiece>();

        for (int i = 0; i < BoardManager.BOARD_SIZE; i++)
        {
            for (int j = 0; j < BoardManager.BOARD_SIZE; j++)
            {
                ChessPiece piece = BoardManager.Instance.ChessBoardArr[i, j];
                if (piece != null && piece.IsWhite == false)
                {
                    allPieces.Add(piece);
                }
            }
        }
        isComputerInitialized = true;
    }

	#endregion

	/// <summary>
	/// Randomly chooses a piece that has at least one legal move to make.
	/// </summary>
	/// <returns>The chess piece.</returns>
	public ChessPiece ChooseChessPiece()
    {
        bool addPiece = false;
        piecesWithMoves.Clear();

        foreach (ChessPiece p in allPieces.ToList())
        {
            addPiece = false;
            bool[,] moves = p.PossibleMoves();

            for (int i = 0; i < BoardManager.BOARD_SIZE; i++)
            {
                for (int j = 0; j < BoardManager.BOARD_SIZE; j++)
                {
                    if (moves[i, j] == true)
                    {
                        addPiece = true;
                        break;
                    }
                }
            }
            if(addPiece)
                piecesWithMoves.Add(p);
        }
        currentPieceToPlay = piecesWithMoves.ElementAt(Random.Range(0, piecesWithMoves.Count));
        return currentPieceToPlay;
    }

	/// <summary>
	/// Semi-randomly chooses a move to be made.
	/// Moves which lead to taking an enemy piece are more valuable.
	/// If there are no such legal moves, then it is decided at random.
	/// </summary>
	/// <returns>The ChessPiece target if any.</returns>
	/// <param name="x">The x coordinate of the chosen destination.</param>
	/// <param name="y">The y coordinate of the chosen destination.</param>
    public ChessPiece ChooseMove(out int x, out int y)
    {
        ChessPiece pieceToTake = null;
        ChessPiece[,] piecesOnTheBoard = BoardManager.Instance.ChessBoardArr;
        bool[,] moves = currentPieceToPlay.PossibleMoves();
        int[] possibleX = new int[30];
        int[] possibleY = new int[30];
        int index = 0;

        for (int i = 0; i < BoardManager.BOARD_SIZE; i++)
        {
            for (int j = 0; j < BoardManager.BOARD_SIZE; j++)
            {
                if (moves[i, j])
                {
                    if (piecesOnTheBoard[i, j] != null)
                    {
                        if (piecesOnTheBoard[i, j].GetType() == typeof(Queen))
                        {
                            pieceToTake = piecesOnTheBoard[i, j];
                        }
                        else if (piecesOnTheBoard[i, j].GetType() == typeof(Rook))
                        {
                            if(pieceToTake == null)
                                pieceToTake = piecesOnTheBoard[i, j];
                            else if (pieceToTake.GetType() != typeof(Queen))
                                pieceToTake = piecesOnTheBoard[i, j];
                        }
                        else if (piecesOnTheBoard[i, j].GetType() == typeof(Bishop))
                        {
                            if (pieceToTake == null)
                                pieceToTake = piecesOnTheBoard[i, j];
                            else if ((pieceToTake.GetType() != typeof(Rook)) || (pieceToTake.GetType() != typeof(Queen)))
                                pieceToTake = piecesOnTheBoard[i, j];
                        }
                        else if (piecesOnTheBoard[i, j].GetType() == typeof(Knight))
                        {
                            if (pieceToTake == null)
                                pieceToTake = piecesOnTheBoard[i, j];
                            else if ((pieceToTake.GetType() != typeof(Rook)) 
                                || (pieceToTake.GetType() != typeof(Queen))
                                || (pieceToTake.GetType() != typeof(Bishop)))
                            {
                                pieceToTake = piecesOnTheBoard[i, j];
                            }
                        }
                        else if (piecesOnTheBoard[i, j].GetType() == typeof(Pawn))
                        {
                            if (pieceToTake == null)
                                pieceToTake = piecesOnTheBoard[i, j];
                            else if ((pieceToTake.GetType() != typeof(Rook))
                                || (pieceToTake.GetType() != typeof(Queen))
                                || (pieceToTake.GetType() != typeof(Bishop))
                                || (pieceToTake.GetType() != typeof(Knight)))
                            {
                                pieceToTake = piecesOnTheBoard[i, j];
                            }
                        }
                    }
                    else
                    {
                        if(index < 30)
                        {
                            possibleX[index] = i;
                            possibleY[index] = j;
                            index++;
                        }
                    }
                }
            }
        }
        if (pieceToTake != null)
        {
            x = pieceToTake.CurrentX;
            y = pieceToTake.CurrentY;
        }
        else                        //The randomly chosen piece can't take any of the enemy's pieces
        {
            int position = Random.Range(0, index);
            x = possibleX[position];
            y = possibleY[position];
        }
        return pieceToTake;
    }

	public void RemoveChessPiece(ChessPiece piece)
	{
		allPieces.Remove(piece);
	}

	public void AddChessPiece(ChessPiece piece)
	{
		allPieces.Add(piece);
	}

	public void DeactivateComputer()
	{
		isComputerInitialized = false;
	}
}
