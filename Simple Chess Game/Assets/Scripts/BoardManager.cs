using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChessPieceType
{
    WhiteKing = 0,
    WhiteQueen = 1,
    WhiteBishop = 2,
    WhiteKnight = 3,
    WhiteRook = 4,
    WhitePawn = 5,
    BlackKing = 6,
    BlackQueen = 7,
    BlackBishop = 8,
    BlackKnight = 9,
    BlackRook = 10,
    BlackPawn = 11
}

/// <summary>
/// This class is responsible for the flow of the game.
/// </summary>
public class BoardManager : MonoBehaviour {

    public static BoardManager Instance { get; set; }
    
	#region Public variables

	[Header("Chess piece prefabs")]
	public GameObject[] chessPieceTypes = new GameObject[CHESS_PIECE_TYPES];
	[Header("Piles of removed pieces")]
	public TakenPieces WhiteTakenPieces;
	public TakenPieces BlackTakenPieces;

	public const int BOARD_SIZE = 8;

	#endregion

    #region Private variables

	//selection of move
    private ChessPiece selectedChessPiece;
	private GameObject selectedMovePosition;
    private bool[,] legalMoves;
	private int selectedX = -1;
	private int selectedY = -1;

	//special moves
    private Pawn lastPawnInEnPassant;
	private bool enPassantDuringWhite;
	private bool hasPromotionOccurred;

	//visuals
    private bool legalMovesDisplayed = false;
	private bool isMenuDisplayed = false;

	//initialization
    private ChessPiece[,] chessBoardArr;		//representation of the chess board
    private bool boardInitialized = false;
    private Transform chessPiecesParent;
	private King whiteKing;
	private King blackKing;

    private static int CHESS_PIECE_TYPES = 12;
	private static readonly string SELECTABLE_LAYER = "Selectable";
	private const string CHESS_PIECES_PARENT = "ChessPieces";
    private const float Z_AXIS_OFFSET = -1f;


    #endregion

	#region Properties

    public ChessPiece[,] ChessBoardArr
    {
        get { return chessBoardArr; }
        set { chessBoardArr = value; }
    }

    public Pawn LastPawnInEnPassant
    {
        get { return lastPawnInEnPassant; }
        set
        {
            lastPawnInEnPassant = value;
            enPassantDuringWhite = StateManager.WhiteIsPlaying;
        }
    }

	#endregion

    // Update is called once per frame
    void Update () {
		CheckForUserInput ();

        switch (StateManager.CurrentState)
        {
            case GameState.CHOOSING_CHESS_PIECE:
                if (legalMovesDisplayed)
                {
                    HideLegalMoves();
                    legalMovesDisplayed = false;
                }

                if (AI.instance.IsComputerInitialized && !StateManager.WhiteIsPlaying)
                {
                    selectedChessPiece = AI.instance.ChooseChessPiece();
                    StateManager.instance.NextState();
                }
                else
                {
                    ChooseChessPiece();
                }
                break;

            case GameState.CHOOSING_MOVE:
                if (!legalMovesDisplayed)
                {
                    DisplayLegalMoves();
                    legalMovesDisplayed = true;
                }

                if (AI.instance.IsComputerInitialized && !StateManager.WhiteIsPlaying)
                {
                    ChessPiece piece = AI.instance.ChooseMove(out selectedX, out selectedY);
                    MakeAIMove(piece);
                }
                else
                {
                    ChooseLegalMove();
                }
                break;

            case GameState.MOVING:
                //TODO: Add animated transitions when a chess piece is moved!
                StateManager.instance.NextState();
                break;

            case GameState.GAME_FINISHED:
                break;
        }
    }
    
    #region Initializations

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
		chessPiecesParent = new GameObject(CHESS_PIECES_PARENT).transform;
        boardInitialized = true;
    }

	/// <summary>
	/// This method returns the chessBoard to its original state.
	/// </summary>
    public void InitializeBoard()
    {
        if (boardInitialized)
        {
            DestroyChessPieces();
            DestroyTakenChessPieces();
        }
        ChessBoardArr = new ChessPiece[BOARD_SIZE, BOARD_SIZE];
        legalMoves = new bool[BOARD_SIZE, BOARD_SIZE];

        for (int y = 0; y < BOARD_SIZE; y++)
        {
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                #region White pieces
                if (y == 0)
                {
                    switch (x)
                    {
                        //rooks
                        case 0:
                        case 7: SpawnChessPiece(ChessPieceType.WhiteRook, x, y); break;
                        //knights
                         case 1:
                        case 6: SpawnChessPiece(ChessPieceType.WhiteKnight, x, y); break;
                        //bishops
                        case 2:
                        case 5: SpawnChessPiece(ChessPieceType.WhiteBishop, x, y); break;
                        //queen
                        case 3: SpawnChessPiece(ChessPieceType.WhiteQueen, x, y); break;
                        //king
                        case 4: whiteKing = SpawnChessPiece(ChessPieceType.WhiteKing, x, y).GetComponent<King>(); break;
                    }
                } else if(y == 1)
                {
                    SpawnChessPiece(ChessPieceType.WhitePawn, x, y);
                }
                #endregion

                #region Black pieces
                if (y == 6)
                {
                    SpawnChessPiece(ChessPieceType.BlackPawn, x, y);
                } else if (y == 7)
                {
                    switch (x)
                    {
                        //rooks
                        case 0:
                        case 7: SpawnChessPiece(ChessPieceType.BlackRook, x, y); break;
                        //knights
                        case 1:
                        case 6: SpawnChessPiece(ChessPieceType.BlackKnight, x, y); break;
                        //bishops
                        case 2:
                        case 5: SpawnChessPiece(ChessPieceType.BlackBishop, x, y); break;
                        //queen
                        case 3: SpawnChessPiece(ChessPieceType.BlackQueen, x, y); break;
                        //king
                        case 4: blackKing = SpawnChessPiece(ChessPieceType.BlackKing, x, y).GetComponent<King>(); break;
                    }
                }
                #endregion
            }
        }
    }

    #endregion

	/// <summary>
	/// Currently the used can only use the Esc key to use the menu.
	/// </summary>
	private void CheckForUserInput ()
	{
		if (Input.GetKeyDown (KeyCode.Escape))
		{
			if (isMenuDisplayed)
			{
				StateManager.instance.HideMenu ();
				isMenuDisplayed = false;
			}
			else
			{
				StateManager.instance.ShowMenu ();
				isMenuDisplayed = true;
			}
		}
	}

    private GameObject CheckForMouseSelection()
    {
        GameObject result = null;

        Collider2D selectedPiece = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition), LayerMask.GetMask(SELECTABLE_LAYER));
        if (selectedPiece != null)
        {
            result = selectedPiece.gameObject;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            selectedX = (int)mousePos.x;
            selectedY = (int)mousePos.y;
        }

        return result;
    }

    private void ChooseChessPiece()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            GameObject selectedGameObj = CheckForMouseSelection();
            
			if (selectedGameObj != null)
                {
                selectedChessPiece = selectedGameObj.GetComponent<ChessPiece> ();
				if ((selectedChessPiece != null) && (StateManager.WhiteIsPlaying == selectedChessPiece.IsWhite))
				{
                    BoardVisualEffects.Instance.DisplaySelectionBox(selectedChessPiece.CurrentX, selectedChessPiece.CurrentY);
					StateManager.instance.NextState();
				}
				else
				{
//					Debug.Log ((StateManager.WhiteIsPlaying ? "White's" : "Black's") + "turn!");
				}
			}
        }
    }

    private void DisplayLegalMoves()
    {
        legalMoves = selectedChessPiece.PossibleMoves();
        BoardVisualEffects.Instance.DisplayLegalMoves(legalMoves);
    }

    private void HideLegalMoves()
    {
        BoardVisualEffects.Instance.HideDisplayedMoves();
    }

	/// <summary>
	/// Used by the AI system to make a move.
	/// </summary>
	/// <param name="piece">Piece.</param>
    private void MakeAIMove(ChessPiece piece)
    {
        Vector3 previousPosition = selectedChessPiece.transform.position;
		Nullable<bool> previousHasMoved = selectedChessPiece.GetType() == typeof(Rook) ? (Nullable<bool>)selectedChessPiece.GetComponent<Rook>().hasMoved : null;	//keeping the previous state of a Rook
        King currentKing = StateManager.WhiteIsPlaying ? whiteKing : blackKing;
        bool wasKingInChess = currentKing.IsKingInCheck;
        bool moveIsValid = true;
		hasPromotionOccurred = false;

        MoveChessPiece(piece);
        
        //Update the kings
        if (StateManager.WhiteIsPlaying)
        {
            blackKing.UpdateKingsCurrentState();
            if (whiteKing.IsKingInCheck)
            {
                whiteKing.UpdateKingsCurrentState();
            }
            if (whiteKing.IsKingInCheck && wasKingInChess)
            {
                moveIsValid = false;
            }
        }
        else
        {
            whiteKing.UpdateKingsCurrentState();
            if (blackKing.IsKingInCheck)
            {
                blackKing.UpdateKingsCurrentState();
            }
            if (blackKing.IsKingInCheck && wasKingInChess)
            {
                moveIsValid = false;
            }
        }

        if (moveIsValid)
        {
			if (piece != null)
			{
				RemoveFromBoard(piece);
				if (AI.instance.IsComputerInitialized)
				{
					AI.instance.RemoveChessPiece(piece);
				}
			}

            if (selectedChessPiece.GetType() == typeof(King))
                BoardVisualEffects.Instance.HideKingVisuals();

            //Update the last En passant
            if (lastPawnInEnPassant != null && enPassantDuringWhite != StateManager.WhiteIsPlaying)
            {
                lastPawnInEnPassant.CanBeTakenViaEnPassant = false;
                LastPawnInEnPassant = null;
            }

			BoardVisualEffects.Instance.DisplayLastMove(previousPosition, new Vector3(selectedX, selectedY, Z_AXIS_OFFSET));

			if (StateManager.CurrentState != GameState.GAME_FINISHED)
			{
				if (hasPromotionOccurred)
					PromotePawn ();
				
				StateManager.instance.NextState();
			}
        }
        else
        {
			ReturnToPreviousMove(previousPosition, previousHasMoved);

			if (piece != null)
			{
				chessBoardArr [piece.CurrentX, piece.CurrentY] = piece;
				piece.transform.position = new Vector3 (piece.CurrentX, piece.CurrentY, Z_AXIS_OFFSET);
			}
			StateManager.instance.PreviousState();
        }
    }

    private void ChooseLegalMove()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            selectedMovePosition = CheckForMouseSelection();

            if ((selectedMovePosition != null) && (legalMoves[selectedX, selectedY]))
            {
                Vector3 previousPosition = selectedChessPiece.transform.position;
				Nullable<bool> previousHasMoved = selectedChessPiece.GetType() == typeof(Rook) ? (Nullable<bool>)selectedChessPiece.GetComponent<Rook>().hasMoved : null;	//keeping the previous state of a Rook
                King currentKing = StateManager.WhiteIsPlaying ? whiteKing : blackKing;
                bool wasKingInChess = currentKing.IsKingInCheck;
                bool moveIsValid;
                ChessPiece piece = selectedMovePosition.GetComponent<ChessPiece> ();
				hasPromotionOccurred = false;

				if (piece != null)
                {
					if (piece.IsWhite == StateManager.WhiteIsPlaying)
                    {
        //                Debug.Log("You can't take your own pieces!");
                        moveIsValid = false;
                    }
                    else
                    {
                        MoveChessPiece(piece);
                        moveIsValid = true;
                    }
                }
                else
                {
                    //The selected move is on an empty tile
                    MoveChessPiece();
                    moveIsValid = true;
                }

                if (moveIsValid)
                {
                    //Update the kings
                    if (StateManager.WhiteIsPlaying)
                    {
                        blackKing.UpdateKingsCurrentState();
                        if (whiteKing.IsKingInCheck)
                        {
                            whiteKing.UpdateKingsCurrentState();
                        }
                        if (whiteKing.IsKingInCheck && wasKingInChess)
                        {
                            moveIsValid = false;
                        }
                    }
                    else
                    {
                        whiteKing.UpdateKingsCurrentState();
                        if (blackKing.IsKingInCheck)
                        {
                            blackKing.UpdateKingsCurrentState();
                        }
                        if (blackKing.IsKingInCheck && wasKingInChess)
                        {
                            moveIsValid = false;
                        }
                    }

                    if (moveIsValid)	//...is STILL a valid move
                    {
						if (piece != null)
						{
							RemoveFromBoard(piece);
							if (AI.instance.IsComputerInitialized)
							{
								AI.instance.RemoveChessPiece(piece);
							}
						}

                        if (selectedChessPiece.GetType() == typeof(King))
                            BoardVisualEffects.Instance.HideKingVisuals();

                        //Update the last En passant
                        if (lastPawnInEnPassant != null && enPassantDuringWhite != StateManager.WhiteIsPlaying)
                        {
                            lastPawnInEnPassant.CanBeTakenViaEnPassant = false;
                            LastPawnInEnPassant = null;
                        }
						BoardVisualEffects.Instance.DisplayLastMove(previousPosition, new Vector3(selectedX, selectedY, Z_AXIS_OFFSET));

						if (StateManager.CurrentState != GameState.GAME_FINISHED)
						{
							if (hasPromotionOccurred)
								PromotePawn ();
								
							StateManager.instance.NextState();
						}
                    }
                    else
                    {
						ReturnToPreviousMove(previousPosition, previousHasMoved);
						if (piece != null)
						{
							chessBoardArr [piece.CurrentX, piece.CurrentY] = piece;
							piece.transform.position = new Vector3 (piece.CurrentX, piece.CurrentY, Z_AXIS_OFFSET);
						}
                        StateManager.instance.PreviousState();
                    }
                }
                else
                {
                    StateManager.instance.PreviousState();
                }
            }
            else
            {
                StateManager.instance.PreviousState();
            }

            BoardVisualEffects.Instance.HideSelectionBox();
        }
    }

    private void MoveChessPiece(ChessPiece targetPiece = null)
    {
        Vector3 beginPos = selectedChessPiece.transform.position;
		Vector3 destinationPos = new Vector3(selectedX, selectedY, Z_AXIS_OFFSET);

        selectedChessPiece.transform.position = destinationPos;
		ChessBoardArr[(int)Mathf.Floor(beginPos.x), (int)Mathf.Floor(beginPos.y)] = null;
        ChessBoardArr[selectedX, selectedY] = selectedChessPiece.GetComponent<ChessPiece>();
        ChessBoardArr[selectedX, selectedY].SetPosition(selectedX, selectedY);

        // Check for possible Pawn moves - Promotion and En passant
        if (selectedChessPiece.GetType() == typeof(Pawn))
        {
            Pawn pawn = selectedChessPiece.GetComponent<Pawn>();
            // Check for En passant Move
            if (StateManager.WhiteIsPlaying)
            {
                ChessPiece pawnInEnPassant = chessBoardArr[selectedX, selectedY - 1];
                if ((pawnInEnPassant != null)
                    && (pawnInEnPassant.GetType() == typeof(Pawn))
                    && (pawnInEnPassant.GetComponent<Pawn>().CanBeTakenViaEnPassant))
                {
                    if (pawnInEnPassant == LastPawnInEnPassant)
                    {
                        RemoveFromBoard(pawnInEnPassant);
                        ChessBoardArr[pawnInEnPassant.CurrentX, pawnInEnPassant.CurrentY] = null;
                    }
                    else
                    {
                        pawnInEnPassant.GetComponent<Pawn>().CanBeTakenViaEnPassant = false;
                    }
                }
            }
            else
            {
                ChessPiece pawnInEnPassant = chessBoardArr[selectedX, selectedY + 1];
                if ((pawnInEnPassant != null)
                    && (pawnInEnPassant.GetType() == typeof(Pawn))
                    && (pawnInEnPassant.GetComponent<Pawn>().CanBeTakenViaEnPassant))
                {
                    RemoveFromBoard(pawnInEnPassant);
                    ChessBoardArr[pawnInEnPassant.CurrentX, pawnInEnPassant.CurrentY] = null;
                }
            }

            if (pawn.IsReadyForPromotion)         // Check for the Promotion Move
				hasPromotionOccurred = true;
        }

        // Check for the Castle Move
        if (selectedChessPiece.GetType() == typeof(King) && selectedChessPiece.GetComponent<King>().CanDoCastle)
        {
            if (selectedX == 2)
            {
                ChessPiece castlingRook = chessBoardArr[0, selectedY];

				castlingRook.transform.position = new Vector3(3, selectedY, Z_AXIS_OFFSET);
                ChessBoardArr[0, selectedY] = null;
                castlingRook.SetPosition(3, selectedY);
                ChessBoardArr[3, selectedY] = castlingRook.GetComponent<ChessPiece>();
            }
            else if (selectedX == 6)
            {
                ChessPiece castlingRook = chessBoardArr[7, selectedY];

				castlingRook.transform.position = new Vector3(5, selectedY, Z_AXIS_OFFSET);
                ChessBoardArr[7, selectedY] = null;
                castlingRook.SetPosition(5, selectedY);
                ChessBoardArr[5, selectedY] = castlingRook.GetComponent<ChessPiece>();
            }
        }
    }

    private void RemoveFromBoard(ChessPiece piece)
    {
		piece.GetComponent<BoxCollider2D> ().enabled = false;

        if (piece.IsWhite)
        {
            WhiteTakenPieces.AddTakenPiece(piece.gameObject);
        }
        else
        {
            BlackTakenPieces.AddTakenPiece(piece.gameObject);
        }
    }

    private GameObject SpawnChessPiece(ChessPieceType index, int x, int y)
    {
		GameObject chessPiece = Instantiate(chessPieceTypes[(int)index], new Vector3(x, y, Z_AXIS_OFFSET), Quaternion.identity);
        
        chessPiece.transform.SetParent(chessPiecesParent);
        //TODO: Think about parenting them to an empty object
        ChessBoardArr[x, y] = chessPiece.GetComponent<ChessPiece>();
        ChessBoardArr[x, y].SetPosition(x, y);

        return chessPiece;
    }

	private void ReturnToPreviousMove(Vector3 previousPosition, Nullable<bool> previousState = null)
    {
        ChessBoardArr[selectedChessPiece.CurrentX, selectedChessPiece.CurrentY] = null;
        ChessBoardArr[(int)previousPosition.x, (int)previousPosition.y] = selectedChessPiece;
        selectedChessPiece.transform.position = previousPosition;
        selectedChessPiece.SetPosition((int)previousPosition.x, (int)previousPosition.y);

		if (previousState != null)
		{
			if (selectedChessPiece.GetType () == typeof(Rook)) {
				selectedChessPiece.GetComponent<Rook> ().hasMoved = previousState.GetValueOrDefault();
			}
			else if (selectedChessPiece.GetType () == typeof(Pawn))
			{
				selectedChessPiece.GetComponent<Pawn> ().CanBeTakenViaEnPassant = previousState.GetValueOrDefault();
			}
		}
    }

    private void DestroyChessPieces()
    {
        foreach (Transform piece in chessPiecesParent)
        {
            Destroy(piece.gameObject);
        }
    }

    private void DestroyTakenChessPieces()
    {
        WhiteTakenPieces.DestroyTakenPieces();
        BlackTakenPieces.DestroyTakenPieces();
    }

	private void PromotePawn()
	{
		RemoveFromBoard(selectedChessPiece);

		GameObject newQueen = SpawnChessPiece(
			StateManager.WhiteIsPlaying ? ChessPieceType.WhiteQueen : ChessPieceType.BlackQueen,
			selectedX,
			selectedY
		);

		if (AI.instance.IsComputerInitialized && !StateManager.WhiteIsPlaying)
		{
			AI.instance.RemoveChessPiece (selectedChessPiece);
			AI.instance.AddChessPiece (newQueen.GetComponent<ChessPiece>());
		}
	}
}
