using System;

namespace Kermalis.ChessThing;

public static class CUtils
{
	/// <summary><see cref="PieceKind.None"/> is not handled here.</summary>
	public static char PieceChar(this PieceKind p, bool capital)
	{
		switch (p)
		{
			case PieceKind.Pawn: return capital ? 'P' : 'p';
			case PieceKind.Knight: return capital ? 'N' : 'n';
			case PieceKind.Bishop: return capital ? 'B' : 'b';
			case PieceKind.Rook: return capital ? 'R' : 'r';
			case PieceKind.Queen: return capital ? 'Q' : 'q';
			case PieceKind.King: return capital ? 'K' : 'k';
		}

		throw new ArgumentOutOfRangeException(nameof(p), p, null);
	}
	/// <summary><see cref="TeamedPiece.None"/> is not handled here.</summary>
	public static char PieceChar(this TeamedPiece tp)
	{
		if (tp is TeamedPiece.None or >= TeamedPiece.MAX)
		{
			throw new ArgumentOutOfRangeException(nameof(tp), tp, null);
		}

		PieceKind p;
		bool white;
		if (tp >= TeamedPiece.B_Pawn)
		{
			p = (PieceKind)(tp - TeamedPiece.W_King);
			white = false;
		}
		else
		{
			p = (PieceKind)tp;
			white = true;
		}

		return PieceChar(p, white);
	}

	public static char RowChar(this Row row)
	{
		return (char)((int)row + '1');
	}
	public static char ColumnChar(this Col col)
	{
		return (char)((int)col + 'a');
	}

	public static int SquareIndex(Row row, Col col)
	{
		return ((int)row * 8) + (int)col;
	}
}