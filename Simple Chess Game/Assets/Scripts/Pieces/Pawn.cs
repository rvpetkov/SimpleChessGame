using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public bool IsReadyForPromotion = false;		//Used for the Promotion move
    private bool canBeTakenViaEnPassant = false;	//Used for the EnPassant move

    public bool CanBeTakenViaEnPassant
    {
        get { return canBeTakenViaEnPassant; }
        set { canBeTakenViaEnPassant = value; }
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

		if ((CurrentX < 7 && CurrentX > 0) && (CurrentY < 7 && CurrentY > 0))
		{
			if (IsWhite)
			{
				//Left diagonal
				legalMoves[CurrentX - 1, CurrentY + 1] = true;

				//Right diagonal
				legalMoves[CurrentX + 1, CurrentY + 1] = true;
			}
			else
			{
				//Left diagonal
				legalMoves[CurrentX - 1, CurrentY - 1] = true;

				//Right diagonal
				legalMoves[CurrentX + 1, CurrentY - 1] = true;
			}
		}

        return legalMoves;
    }

    public override void SetPosition(int x, int y)
    {
        int posX = CurrentX;
        int posY = CurrentY;

        base.SetPosition(x, y);

        if (IsWhite)
        {
            // Check for promotion
            if (CurrentY == 7)
                IsReadyForPromotion = true;

            // Check for En passant
            if (posY == 1 && y == 3)
            {
                CanBeTakenViaEnPassant = true;
                BoardManager.Instance.LastPawnInEnPassant = this;
            }
            else
            {
                CanBeTakenViaEnPassant = false;
            }
        }
        else
        {
            // Check for promotion
            if (CurrentY == 0)
                IsReadyForPromotion = true;

            // Check for En passant
            if (posY == 6 && y == 4)
            {
                CanBeTakenViaEnPassant = true;
                BoardManager.Instance.LastPawnInEnPassant = this;
            }
            else
            {
                CanBeTakenViaEnPassant = false;
            }
        }
    }

	/// <summary>
	/// Returns the coordinates where the chess piece can be moved to.
	/// </summary>
	/// <returns>All possible moves for the current piece.</returns>
    public override bool[,] PossibleMoves()
    {
        bool[,] result = new bool[8, 8];
        ChessPiece piece0, piece1;

        if (IsWhite)
        {
            //Move forward
            if (CurrentY != 7)
            {
                piece0 = BoardManager.Instance.ChessBoardArr[CurrentX, CurrentY + 1];
                if(piece0 == null)
                    result[CurrentX, CurrentY + 1] = true;
            }

            //Move forward on first move (doble jump)
            if (CurrentY == 1)
            {
                piece0 = BoardManager.Instance.ChessBoardArr[CurrentX, CurrentY + 1];
                piece1 = BoardManager.Instance.ChessBoardArr[CurrentX, CurrentY + 2];
                if (piece0 == null && piece1 == null)
                {
                    //piece0 is already set to true in the previous IF
                    result[CurrentX, CurrentY + 2] = true;
                }
            }

            //Move left diagonal
            if (CurrentX > 0 && CurrentY < 7)
            {
                piece0 = BoardManager.Instance.ChessBoardArr[CurrentX - 1, CurrentY + 1];
                if (piece0 != null && !piece0.IsWhite)
                    result[piece0.CurrentX, piece0.CurrentY] = true;
            }

            //Move right diagonal
            if (CurrentX < 7 && CurrentY < 7)
            {
                piece0 = BoardManager.Instance.ChessBoardArr[CurrentX + 1, CurrentY + 1];
                if (piece0 != null && !piece0.IsWhite)
                    result[piece0.CurrentX, piece0.CurrentY] = true;
            }

            //En passant
            if (CurrentX > 0)               //left
            {
                piece0 = BoardManager.Instance.ChessBoardArr[CurrentX - 1, CurrentY];
                if (piece0 != null && piece0.GetType() == typeof(Pawn) && piece0.GetComponent<Pawn>().CanBeTakenViaEnPassant)
                {
                    result[CurrentX - 1, CurrentY + 1] = true;
                }
            }

            if (CurrentX < 7)          //right
            {
                piece0 = BoardManager.Instance.ChessBoardArr[CurrentX + 1, CurrentY];
                if (piece0 != null && piece0.GetType() == typeof(Pawn) && piece0.GetComponent<Pawn>().CanBeTakenViaEnPassant)
                {
                    result[CurrentX + 1, CurrentY + 1] = true;
                }
            }
        }
        else
        {
            //Move forward
            if (CurrentY != 0)
            {
                piece0 = BoardManager.Instance.ChessBoardArr[CurrentX, CurrentY - 1];
                if (piece0 == null)
                    result[CurrentX, CurrentY - 1] = true;
            }

            //Move forward on first move (doble jump)
            if (CurrentY == 6)
            {
                piece0 = BoardManager.Instance.ChessBoardArr[CurrentX, CurrentY - 1];
                piece1 = BoardManager.Instance.ChessBoardArr[CurrentX, CurrentY - 2];
                if (piece0 == null && piece1 == null)
                {
                    //piece0 is already set to true in the previous IF
                    result[CurrentX, CurrentY - 2] = true;
                }
            }

            //Move left diagonal
            if (CurrentX > 0 && CurrentY > 0)
            {
                piece0 = BoardManager.Instance.ChessBoardArr[CurrentX - 1, CurrentY - 1];
                if (piece0 != null && piece0.IsWhite)
                    result[piece0.CurrentX, piece0.CurrentY] = true;
            }

            //Move right diagonal
            if (CurrentX < 7 && CurrentY > 0)
            {
                piece0 = BoardManager.Instance.ChessBoardArr[CurrentX + 1, CurrentY - 1];
                if (piece0 != null && piece0.IsWhite)
                    result[piece0.CurrentX, piece0.CurrentY] = true;
            }

            //En passant
            if (CurrentX > 0)                   //left
            {
                piece0 = BoardManager.Instance.ChessBoardArr[CurrentX - 1, CurrentY];
                if (piece0 != null && piece0.GetType() == typeof(Pawn) && piece0.GetComponent<Pawn>().CanBeTakenViaEnPassant)
                {
                    result[CurrentX - 1, CurrentY - 1] = true;
                }
            }

            if (CurrentX < 7)              //right
            {
                piece0 = BoardManager.Instance.ChessBoardArr[CurrentX + 1, CurrentY];
                if (piece0 != null && piece0.GetType() == typeof(Pawn) && piece0.GetComponent<Pawn>().CanBeTakenViaEnPassant)
                {
                    result[CurrentX + 1, CurrentY - 1] = true;
                }
            }
        }
        return result;
    }
}
