using System;

namespace Kermalis.ChessThing;

public sealed class Board
{
	private readonly TeamedPiece[] _squares;

	public bool WhiteToMove;
	public CastlingAbility WhiteCastling;
	public CastlingAbility BlackCastling;
	public Square? EnPassantTarget;
	public byte NumHalfMoves;
	public uint NumFullMoves;

	public TeamedPiece this[Square pos]
	{
		get => _squares[pos.Index];
		set => _squares[pos.Index] = value;
	}

	public Board()
	{
		_squares = new TeamedPiece[8 * 8];
	}

	public void SetPieces(ReadOnlySpan<TeamedPiece> pieces)
	{
		if (pieces.Length != 8 * 8)
		{
			throw new ArgumentException("Must have 8 rows and 8 columns.", nameof(pieces));
		}

		pieces.CopyTo(_squares);
	}
}