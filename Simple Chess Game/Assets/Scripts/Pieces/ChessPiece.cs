using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class provides the basic functionality for all chess pieces
/// </summary>
public abstract class ChessPiece : MonoBehaviour {
    public int CurrentX { get; set; }
    public int CurrentY { get; set; }
    public bool IsWhite;

	protected bool[,] legalMoves;		//Coordinates where the chess piece can be moved to.
    
    public virtual void SetPosition(int x, int y)
    {
        CurrentX = x;
        CurrentY = y;
    }

	/// <summary>
	/// This method is only called by the King.
	/// The coordinates returned are different than the PossibleMoves
	/// because they contain a few extra positions - those affecting the King(check and others)
	/// </summary>
	/// <returns>The check positions.</returns>
    public virtual bool[,] GetCheckPositions()
    {
        if (legalMoves == null)
            legalMoves = new bool[8, 8];
        return legalMoves;
    }

	/// <summary>
	/// Returns the coordinates where the chess piece can be moved to.
	/// </summary>
	/// <returns>All possible moves for the current piece.</returns>
    public virtual bool[,] PossibleMoves()
    {
        return new bool[8, 8];
    }
}
