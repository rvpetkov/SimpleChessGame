using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : ChessPiece
{
    public bool hasMoved = false;	//Used to determine if the Castle move is available
    
    public override void SetPosition(int x, int y)
    {
        if ((x == 0 && y == 0) || (x == 7 && y == 0) || (x == 0 && y == 7) || (x == 7 && y == 7))
        {
            //This is the initial position on the board. The piece has not made its first move!
        }
        else if (!hasMoved)
        {
//            Debug.Log("Rook has moved - NO CASTLE!");
            hasMoved = true;
        }

        base.SetPosition(x, y);
    }

	/// <summary>
	/// This method is only called by the King.
	/// The coordinates returned are different than the PossibleMoves
	/// because they contain a few extra positions - those affecting the King(check and others)
	/// </summary>
	/// <returns>The check positions.</returns>
    public override bool[,] GetCheckPositions()
    {
        legalMoves = new bool[8, 8];

        int lookUpX, lookUpY;

        //Move up
        lookUpX = CurrentX;
        lookUpY = CurrentY;
        while (lookUpY < 7)
        {
            if(!RookCheckMove(lookUpX, ++lookUpY, ref legalMoves))
                break;
        }

        //Move right
        lookUpX = CurrentX;
        lookUpY = CurrentY;
        while (lookUpX < 7)
        {
            if (!RookCheckMove(++lookUpX, lookUpY, ref legalMoves))
                break;
        }

        //Move down
        lookUpX = CurrentX;
        lookUpY = CurrentY;
        while (lookUpY > 0)
        {
            if (!RookCheckMove(lookUpX, --lookUpY, ref legalMoves))
                break;
        }

        //Move left
        lookUpX = CurrentX;
        lookUpY = CurrentY;
        while (lookUpX > 0)
        {
            if (!RookCheckMove(--lookUpX, lookUpY, ref legalMoves))
                break;
        }

        return legalMoves;
    }

	/// <summary>
	/// Returns the coordinates where the chess piece can be moved to.
	/// </summary>
	/// <returns>All possible moves for the current piece.</returns>
    public override bool[,] PossibleMoves()
    {
        bool[,] result = new bool[8, 8];
        ChessPiece p;
        int lookUpX, lookUpY;

        //Move up
        lookUpX = CurrentX;
        lookUpY = CurrentY;
        while (lookUpY < 7)
        {
            p = RookMove(lookUpX, ++lookUpY, ref result);
            if (p != null)
                break;
        }

        //Move right
        lookUpX = CurrentX;
        lookUpY = CurrentY;
        while (lookUpX < 7)
        {
            p = RookMove(++lookUpX, lookUpY, ref result);
            if (p != null)
                break;
        }

        //Move down
        lookUpX = CurrentX;
        lookUpY = CurrentY;
        while (lookUpY > 0)
        {
            p = RookMove(lookUpX, --lookUpY, ref result);
            if (p != null)
                break;
        }

        //Move left
        lookUpX = CurrentX;
        lookUpY = CurrentY;
        while (lookUpX > 0)
        {
            p = RookMove(--lookUpX, lookUpY, ref result);
            if (p != null)
                break;
        }
        legalMoves = result;

        return result;
    }

    private ChessPiece RookMove(int x, int y, ref bool[,] arr)
    {
        ChessPiece p = BoardManager.Instance.ChessBoardArr[x, y];
        if (p != null)
        {
            if (p.IsWhite != IsWhite)
                arr[x, y] = true;
        }
        else
        {
            arr[x, y] = true;
        }

        return p;
    }

    private bool RookCheckMove(int x, int y, ref bool[,] arr)
    {
        bool result = true;

        ChessPiece p = BoardManager.Instance.ChessBoardArr[x, y];
        arr[x, y] = true;

        if (p != null)
        {
            if (p.GetType() == typeof(King) && p.IsWhite != IsWhite)
            {
                result = true;
            }
            else
            {
                result = false;
            }
        }
        return result;
    }
}
