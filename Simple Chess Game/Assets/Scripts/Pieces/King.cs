using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class King : ChessPiece
{
	#region Private variables

    private bool hasMoved = false;			//Used to determine the Castle move
    private bool isKingInCheck = false;
    private bool canDoCastle = false;
    private int numberOfPossibleMoves;		//Does the King has any more places to go
    private bool[,] movesInCheck;
    private List<ChessPiece> army;      //These are the chess pieces of the same color as the King

	#endregion

	#region Properties

    public bool IsKingInCheck
    {
        get { return isKingInCheck; }
        set { isKingInCheck = value; }
    }

    public bool CanDoCastle
    {
        get { return canDoCastle; }
        set { canDoCastle = value; }
    }

	#endregion

    public override void SetPosition(int x, int y)
    {
        if ((x == 4 && y == 0) || (x == 4 && y == 7))
        {
            //This is the initial position on the board. The piece has not made its first move!
        }
        else if (!hasMoved)
        {
//            Debug.Log("King has moved - NO CASTLE!");
            hasMoved = true;
        }

        base.SetPosition(x, y);
    }

	/// <summary>
	/// This method is only called by either King.
	/// The coordinates returned are different than the PossibleMoves
	/// because they contain a few extra positions - those affecting the King(check and others)
	/// </summary>
	/// <returns>The check positions.</returns>
    public override bool[,] GetCheckPositions()
    {
        legalMoves = new bool[8, 8];

        //Move up
        KingCheckMove(CurrentX, CurrentY + 1, ref legalMoves);

        //Move up-right diagonal
        KingCheckMove(CurrentX + 1, CurrentY + 1, ref legalMoves);

        //Move right
        KingCheckMove(CurrentX + 1, CurrentY, ref legalMoves);

        //Move right-down diagonal
        KingCheckMove(CurrentX + 1, CurrentY - 1, ref legalMoves);

        //Move down
        KingCheckMove(CurrentX, CurrentY - 1, ref legalMoves);

        //Move down-left diagonal
        KingCheckMove(CurrentX - 1, CurrentY - 1, ref legalMoves);

        //Move left
        KingCheckMove(CurrentX - 1, CurrentY, ref legalMoves);

        //Move left-up diagonal
        KingCheckMove(CurrentX - 1, CurrentY + 1, ref legalMoves);

        return legalMoves;
    }

	/// <summary>
	/// Returns the coordinates where the chess piece can be moved to.
	/// </summary>
	/// <returns>All possible moves for the current piece.</returns>
    public override bool[,] PossibleMoves()
    {
        bool[,] result = new bool[8, 8];
        if(movesInCheck == null)
            movesInCheck = new bool[8, 8];
        numberOfPossibleMoves = 0;

        //Move up
        KingMove(CurrentX, CurrentY + 1, ref result);

        //Move up-right diagonal
        KingMove(CurrentX + 1, CurrentY + 1, ref result);

        //Move right
        KingMove(CurrentX + 1, CurrentY, ref result);

        //Move right-down diagonal
        KingMove(CurrentX + 1, CurrentY - 1, ref result);

        //Move down
        KingMove(CurrentX, CurrentY - 1, ref result);

        //Move down-left diagonal
        KingMove(CurrentX - 1, CurrentY - 1, ref result);

        //Move left
        KingMove(CurrentX - 1, CurrentY, ref result);

        //Move left-up diagonal
        KingMove(CurrentX - 1, CurrentY + 1, ref result);

        //Castle move
        CheckForCastleMove(ref result);

        legalMoves = result;

        return result;
    }

    private void KingMove(int x, int y, ref bool[,] arr)
    {
        if (x >= 0 && x <= 7 && y >= 0 && y <= 7)
        {
            ChessPiece p = BoardManager.Instance.ChessBoardArr[x, y];
            if (p == null || (p.GetType() != typeof(King)) && (p.IsWhite != IsWhite))
            {
                if (movesInCheck[x, y] == false)        //this position is not in check
                {
                    arr[x, y] = true;
                    numberOfPossibleMoves++;
                }
            }
        }
    }

    private void KingCheckMove(int x, int y, ref bool[,] arr)
    {
        if (x >= 0 && x <= 7 && y >= 0 && y <= 7)
        {
            arr[x, y] = true;
        }
    }

	/// <summary>
	/// If the King and one of the Rooks have not moved yet
	/// and there are no other pieces on the board between them
	/// the King can make the Castle move!
	/// </summary>
	/// <param name="arr">Arr.</param>
    private void CheckForCastleMove(ref bool[,] arr)
    {
        if (!hasMoved)
        {
            int lookUpX = CurrentX;
            int lookUpY = CurrentY;

            //Castle left
            while (lookUpX > 0)
            {
                var p = BoardManager.Instance.ChessBoardArr[--lookUpX, lookUpY];
                if (p != null)
                {
                    if (lookUpX != 0)   //The Rook should be at 0 on the X axis
                    {
                        break;
                    }
                    else
                    {
						if (p.GetType() == typeof(Rook) && !p.GetComponent<Rook>().hasMoved && !IsKingInCheck)
                        {
                            //possible castle to the left
                            CanDoCastle = true;
                            arr[CurrentX - 1, CurrentY] = true;
                            arr[CurrentX - 2, CurrentY] = true;
                            numberOfPossibleMoves++;
                        }
                    }
                }
            }

            //Castle right
            lookUpX = CurrentX;
            lookUpY = CurrentY;
            while (lookUpX < 7)
            {
                var p = BoardManager.Instance.ChessBoardArr[++lookUpX, lookUpY];
                if (p != null)
                {
                    if (lookUpX != 7)   //The Rook should be at 7 on the X axis
                    {
                        break;
                    }
                    else
                    {
                        if (p.GetType() == typeof(Rook) && !p.GetComponent<Rook>().hasMoved)
                        {
                            //possible castle to the right
                            CanDoCastle = true;
                            arr[CurrentX + 1, CurrentY] = true;
                            arr[CurrentX + 2, CurrentY] = true;
                        }
                    }
                }
            }
        }
        else if (CanDoCastle)
        {
            CanDoCastle = false;
        }
    }

	/// <summary>
	/// This method is used to determine the situation on the board after each move.
	/// The king has to know what has moved and how is this affecting him.
	/// </summary>
    public void UpdateKingsCurrentState()
    {
        movesInCheck = new bool[8, 8];
		bool[,] possitionsToBreakCheck = new bool[8, 8];
        army = new List<ChessPiece>();
        IsKingInCheck = false;
        ChessPiece causeOfCheck = null;

        BoardVisualEffects.Instance.HideKingVisuals();
        PossibleMoves();

        for (int i = 0; i < BoardManager.BOARD_SIZE; i++)
        {
            for (int j = 0; j < BoardManager.BOARD_SIZE; j++)
            {
                ChessPiece piece = BoardManager.Instance.ChessBoardArr[i, j];
                if (piece != null)
                {
                    if (piece.IsWhite != IsWhite)
                    {
                        bool[,] pieceCheckPositions = piece.GetCheckPositions();
                        if (pieceCheckPositions[CurrentX, CurrentY])
                        {
                            //in check
                            BoardVisualEffects.Instance.DisplayKingInCheck(CurrentX, CurrentY);
                            IsKingInCheck = true;
                            causeOfCheck = piece;
							possitionsToBreakCheck = GetCheckPossitionsToKing(causeOfCheck);
                        }

                        for (int x = 0; x < BoardManager.BOARD_SIZE; x++)
                        {
                            for (int y = 0; y < BoardManager.BOARD_SIZE; y++)
                            {
                                if (pieceCheckPositions[x, y] == true)
                                {
                                    if (pieceCheckPositions[x, y] == legalMoves[x, y])
                                    {
                                        if (movesInCheck[x, y] != true)
                                        {
                                            movesInCheck[x, y] = true;
                                            numberOfPossibleMoves--;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        army.Add(piece);
                    }
                }
            }
        }
        if(numberOfPossibleMoves == 0 && IsKingInCheck)
        {
            //if any of the King's pieces can hit the enemy causing the check => King is not yet dead!
            bool kingIsStillAlive = false;

            foreach (ChessPiece p in army)
            {
                bool[,] possibleMoves = p.PossibleMoves();

                for (int x = 0; x < BoardManager.BOARD_SIZE; x++)
                {
                    for (int y = 0; y < BoardManager.BOARD_SIZE; y++)
                    {
						if ((possibleMoves[causeOfCheck.CurrentX, causeOfCheck.CurrentY])
							|| (possitionsToBreakCheck[x, y]))
                        {
                            kingIsStillAlive = true;
							break;
                        }
                    }
                }
            }
            if(!kingIsStillAlive)
                CheckMate();    // weeell... at least we tried
        }
    }

	private bool[,] GetCheckPossitionsToKing(ChessPiece checkPiece)
	{
		bool[,] result = new bool[8, 8];

		if (checkPiece.GetType () != typeof(Knight))
		{
			if (checkPiece.CurrentX == CurrentX)	//vertical
			{
				if (checkPiece.CurrentY > CurrentY)
				{
					for (int y = checkPiece.CurrentY - 1; y > CurrentY; y--)
						result [CurrentX, y] = true;
				}
				else
				{
					for (int y = checkPiece.CurrentY + 1; y < CurrentY; y++)
						result [CurrentX, y] = true;
				}
			}
			else if (checkPiece.CurrentY == CurrentY)	//horizontal
			{
				if (checkPiece.CurrentX > CurrentX)
				{
					for (int x = checkPiece.CurrentX - 1; x > CurrentX; x--)
						result [x, CurrentY] = true;
				}
				else
				{
					for (int x = checkPiece.CurrentX + 1; x < CurrentX; x++)
						result [x, CurrentY] = true;
				}
			}
			else 		//diagonal
			{
				int x = checkPiece.CurrentX, y = checkPiece.CurrentY;

				if (checkPiece.CurrentX > CurrentX && checkPiece.CurrentY > CurrentY)
				{
					while (x - 1 > CurrentX && y - 1 > CurrentY)
					{
						result [--x, --y] = true;
					}
				}
				else if (checkPiece.CurrentX > CurrentX && checkPiece.CurrentY < CurrentY)
				{
					while (x - 1 > CurrentX && y + 1 < CurrentY)
					{
						result [--x, ++y] = true;
					}
				}
				else if (checkPiece.CurrentX < CurrentX && checkPiece.CurrentY < CurrentY)
				{
					while (x + 1 < CurrentX && y + 1 < CurrentY)
					{
						result [++x, ++y] = true;
					}
				}
				else
				{
					while (x + 1 < CurrentX && y - 1 > CurrentY)
					{
						result [++x, --y] = true;
					}
				}
			}
		}

		return result;
	}

    private void CheckMate()
    {
        BoardVisualEffects.Instance.DisplayCheckMate(CurrentX, CurrentY);
        StateManager.instance.EndMatch(IsWhite);
    }
}
