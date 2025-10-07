using System;
using System.Text;

namespace Kermalis.ChessThing;

public static class CUtils
{
	/// <summary><see cref="PieceKind.None"/> is not handled here.</summary>
	public static char PieceChar(this PieceKind p, bool uppercase)
	{
		switch (p)
		{
			case PieceKind.Pawn: return uppercase ? 'P' : 'p';
			case PieceKind.Knight: return uppercase ? 'N' : 'n';
			case PieceKind.Bishop: return uppercase ? 'B' : 'b';
			case PieceKind.Rook: return uppercase ? 'R' : 'r';
			case PieceKind.Queen: return uppercase ? 'Q' : 'q';
			case PieceKind.King: return uppercase ? 'K' : 'k';
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

	/// <summary>Does not handle <see cref="TeamedPiece.None"/>. If the parse failed, this returns <see cref="TeamedPiece.MAX"/>.</summary>
	public static TeamedPiece TryParseTeamedPiece(char c)
	{
		switch (c)
		{
			case 'P': return TeamedPiece.W_Pawn;
			case 'p': return TeamedPiece.B_Pawn;
			case 'N': return TeamedPiece.W_Knight;
			case 'n': return TeamedPiece.B_Knight;
			case 'B': return TeamedPiece.W_Bishop;
			case 'b': return TeamedPiece.B_Bishop;
			case 'R': return TeamedPiece.W_Rook;
			case 'r': return TeamedPiece.B_Rook;
			case 'Q': return TeamedPiece.W_Queen;
			case 'q': return TeamedPiece.B_Queen;
			case 'K': return TeamedPiece.W_King;
			case 'k': return TeamedPiece.B_King;
		}

		return TeamedPiece.MAX;
	}

	public static Square ParseSquare(char c0, char c1)
	{
		Row row;
		Col col;

		if (c0 is >= 'a' and <= 'h')
		{
			col = (Col)(c0 - 'a');
		}
		else
		{
			throw new ArgumentOutOfRangeException(nameof(c0), c0, "Invalid column");
		}

		if (c1 is >= '1' and <= '8')
		{
			row = (Row)(c1 - '1');
		}
		else
		{
			throw new ArgumentOutOfRangeException(nameof(c1), c1, "Invalid row");
		}

		return new Square(row, col);
	}

	public static char RowChar(this Row row)
	{
		return (char)((int)row + '1');
	}
	public static char ColumnChar(this Col col, bool uppercase = false)
	{
		if (uppercase)
		{
			return (char)((int)col + 'A');
		}
		return (char)((int)col + 'a');
	}

	public static int SquareIndex(Row row, Col col)
	{
		return ((int)row * 8) + (int)col;
	}

	public static void AppendBoardASCII(StringBuilder sb, Board board, bool blackOnTop)
	{
		void RowLine()
		{
			for (int d = 0; d < 18; d++)
			{
				sb.Append('—');
			}
			sb.AppendLine();
		}

		if (blackOnTop)
		{
			for (int y = 7; y >= 0; y--)
			{
				var row = (Row)y;

				// Print Row legend
				sb.Append(row.RowChar());
				sb.Append('|');

				for (int x = 0; x < 8; x++)
				{
					var col = (Col)x;

					TeamedPiece tp = board[new Square(row, col)];
					if (tp == TeamedPiece.None)
					{
						sb.Append(' ');
					}
					else
					{
						sb.Append(tp.PieceChar());
					}

					sb.Append('|');
				}

				sb.AppendLine();
				RowLine();
			}

			// Print Column legend
			sb.Append(" |");
			for (int x = 0; x < 8; x++)
			{
				var col = (Col)x;

				sb.Append(col.ColumnChar());
				sb.Append('|');
			}
		}
		else
		{
			for (int y = 0; y < 8; y++)
			{
				var row = (Row)y;

				// Print Row legend
				sb.Append(row.RowChar());
				sb.Append('|');

				for (int x = 7; x >= 0; x--)
				{
					var col = (Col)x;

					TeamedPiece tp = board[new Square(row, col)];
					if (tp == TeamedPiece.None)
					{
						sb.Append(' ');
					}
					else
					{
						sb.Append(tp.PieceChar());
					}

					sb.Append('|');
				}

				sb.AppendLine();
				RowLine();
			}

			// Print Column legend
			sb.Append(" |");
			for (int x = 7; x >= 0; x--)
			{
				var col = (Col)x;

				sb.Append(col.ColumnChar());
				sb.Append('|');
			}
		}

		sb.AppendLine();
		sb.AppendLine();

		sb.AppendLine(board.WhiteToMove ? "White to move..." : "Black to move...");
		sb.AppendLine();
		sb.AppendLine($"White castling: {board.WhiteCastling}");
		sb.AppendLine($"Black castling: {board.BlackCastling}");
		if (board.EnPassantTarget is null)
		{
			sb.AppendLine("En passant possibility: None");
		}
		else
		{
			Square s = board.EnPassantTarget.Value;

			sb.AppendLine($"En passant possibility: {s.Col.ColumnChar()}{s.Row.RowChar()}");
		}
		sb.AppendLine($"HalfMoves: {board.NumHalfMoves}");
		sb.AppendLine($"FullMoves: {board.NumFullMoves}");
	}
}