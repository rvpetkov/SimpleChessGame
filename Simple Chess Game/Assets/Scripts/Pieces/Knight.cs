using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece {
	/// <summary>
	/// This method is only called by the King.
	/// The coordinates returned are different than the PossibleMoves
	/// because they contain a few extra positions - those affecting the King(check and others)
	/// </summary>
	/// <returns>The check positions.</returns>
    public override bool[,] GetCheckPositions()
    {
        legalMoves = new bool[8, 8];

        //Move up-left
        KnightCheckMove(CurrentX - 1, CurrentY + 2, ref legalMoves);
        //Move up-right
        KnightCheckMove(CurrentX + 1, CurrentY + 2, ref legalMoves);

        //Move right-up
        KnightCheckMove(CurrentX + 2, CurrentY + 1, ref legalMoves);
        //Move right-down
        KnightCheckMove(CurrentX + 2, CurrentY - 1, ref legalMoves);

        //Move down-left
        KnightCheckMove(CurrentX - 1, CurrentY - 2, ref legalMoves);
        //Move down-right
        KnightCheckMove(CurrentX + 1, CurrentY - 2, ref legalMoves);

        //Move left-up
        KnightCheckMove(CurrentX - 2, CurrentY + 1, ref legalMoves);
        //Move left-down
        KnightCheckMove(CurrentX - 2, CurrentY - 1, ref legalMoves);

        return legalMoves;
    }

	/// <summary>
	/// Returns the coordinates where the chess piece can be moved to.
	/// </summary>
	/// <returns>All possible moves for the current piece.</returns>
    public override bool[,] PossibleMoves()
    {
        bool[,] result = new bool[8, 8];

        //Move up-left
        KnightMove(CurrentX - 1, CurrentY + 2, ref result);
        //Move up-right
        KnightMove(CurrentX + 1, CurrentY + 2, ref result);

        //Move right-up
        KnightMove(CurrentX + 2, CurrentY + 1, ref result);
        //Move right-down
        KnightMove(CurrentX + 2, CurrentY - 1, ref result);

        //Move down-left
        KnightMove(CurrentX - 1, CurrentY - 2, ref result);
        //Move down-right
        KnightMove(CurrentX + 1, CurrentY - 2, ref result);

        //Move left-up
        KnightMove(CurrentX - 2, CurrentY + 1, ref result);
        //Move left-down
        KnightMove(CurrentX - 2, CurrentY - 1, ref result);

        legalMoves = result;

        return result;
    }

    private void KnightMove(int x, int y, ref bool[,] arr)
    {
        if (x >= 0 && x <=7 && y >= 0 && y <= 7)
        {
            ChessPiece p = BoardManager.Instance.ChessBoardArr[x, y];
            if (p == null)
            {
                arr[x, y] = true;
            }
            else if (p.IsWhite != IsWhite)
            {
                arr[x, y] = true;
            }
        }
    }

    private void KnightCheckMove(int x, int y, ref bool[,] arr)
    {
        if (x >= 0 && x <= 7 && y >= 0 && y <= 7)
        {
            arr[x, y] = true;
        }
    }
}
