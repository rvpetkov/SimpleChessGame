using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible for taking care of the chess pieces that were removed from play.
/// </summary>
public class TakenPieces : MonoBehaviour {

	private List<GameObject> takenPieces;
	private Vector3 nextPosition;

    private const float SCALE_FACTOR = 0.7f;
    private const int  PIECES_IN_A_ROW = 7;

    void Start()
	{
        takenPieces = new List<GameObject>();
        nextPosition = Vector3.zero;
	}

	public void AddTakenPiece(GameObject piece)
	{
		takenPieces.Add (piece);
		MovePieceToPile (piece);
	}

	private void MovePieceToPile (GameObject piece)
	{
		piece.transform.position = nextPosition;
        piece.transform.localScale = Vector3.one * SCALE_FACTOR;

        piece.transform.SetParent (transform, false);

		UpdateNextPosition ();
	}

	private void UpdateNextPosition ()
	{
		if (nextPosition.x < (PIECES_IN_A_ROW - 1) * SCALE_FACTOR) {
			nextPosition.x += SCALE_FACTOR; 
		}
		else
		{
			nextPosition.x = 0;
			nextPosition.y -= SCALE_FACTOR;
		}
	}

	/// <summary>
	/// Used to reset the pile when a new match starts.
	/// </summary>
    public void DestroyTakenPieces()
    {
        foreach (GameObject go in takenPieces)
        {
            Destroy(go);
        }
    }
}
