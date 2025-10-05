using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Kermalis.ChessThing;

public static class FEN
{
	private const int FEN_MAX_LEN = 96;

	public static string ToFEN(Board board)
	{
		var sb = new StringBuilder(FEN_MAX_LEN);

		// 1: Placement
		WritePlacement(sb, board);
		sb.Append(' ');

		// 2: Who's turn it is + space
		WriteWhiteToMove(sb, board.WhiteToMove);

		// 3: Castling rights + space
		WriteCastling(sb, board.WhiteCastling, board.BlackCastling);

		// 4: En passant + space
		WriteEnPassant(sb, board.EnPassantTarget);

		// 5: Half moves + space
		WriteHalfMoves(sb, board.NumHalfMoves);

		// 6: Full moves
		WriteFullMoves(sb, board.NumFullMoves);

		return sb.ToString();
	}

	private static void WritePlacement(StringBuilder sb, Board board)
	{
		void WriteNumEmpty(byte numEmpty)
		{
			sb.Append((char)(numEmpty + '0'));
		}

		for (int y = 7; y >= 0; y--)
		{
			var row = (Row)y;

			Col col = Col.CA;
			byte numEmpty = 0;

			while (true)
			{
				TeamedPiece tp = board[new Square(row,col)];
				if (tp == TeamedPiece.None)
				{
					numEmpty++;
				}
				else
				{
					if (numEmpty != 0)
					{
						WriteNumEmpty(numEmpty);
						numEmpty = 0;
					}
					sb.Append(tp.PieceChar());
				}

				col++;
				if (col == Col.MAX)
				{
					if (numEmpty != 0)
					{
						WriteNumEmpty(numEmpty);
					}
					if (row != Row.R1)
					{
						sb.Append('/');
					}
					break;
				}
			}
		}
	}

	private static void WriteWhiteToMove(StringBuilder sb, bool whiteToMove)
	{
		if (whiteToMove)
		{
			sb.Append("w ");
		}
		else
		{
			sb.Append("b ");
		}
	}

	private static void WriteCastling(StringBuilder sb, CastlingAbility wCas, CastlingAbility bCas)
	{
		if (wCas == CastlingAbility.None && bCas == CastlingAbility.None)
		{
			sb.Append("- ");
		}
		else
		{
			if (wCas.HasFlag(CastlingAbility.QueenSide))
			{
				sb.Append('Q');
			}
			if (wCas.HasFlag(CastlingAbility.KingSide))
			{
				sb.Append('K');
			}
			if (bCas.HasFlag(CastlingAbility.QueenSide))
			{
				sb.Append('q');
			}
			if (bCas.HasFlag(CastlingAbility.KingSide))
			{
				sb.Append('k');
			}
			sb.Append(' ');
		}
	}

	private static void WriteEnPassant(StringBuilder sb, Square? enPassant)
	{
		if (enPassant is null)
		{
			sb.Append("- ");
		}
		else
		{
			Square s = enPassant.Value;
			sb.Append($"{s.Col.ColumnChar()}{s.Row.RowChar()} ");
		}
	}

	private static void WriteHalfMoves(StringBuilder sb, byte halfMoves)
	{
		sb.Append($"{halfMoves:D} ");
	}

	private static void WriteFullMoves(StringBuilder sb, uint fullMoves)
	{
		sb.Append($"{fullMoves:D}");
	}

	public static void Parse(Board board, ReadOnlySpan<char> chars)
	{
		if (chars.Length > FEN_MAX_LEN)
		{
			throw new ArgumentException("Too many characters in FEN string.", nameof(chars));
		}

		try
		{
			Span<TeamedPiece> pieces = stackalloc TeamedPiece[8 * 8];

			// 1: Parse placement
			int numParsedChars = ParsePlacement(pieces, chars);
			chars = chars.Slice(numParsedChars);
			ValidateSpace(ref chars);

			// 2: Parse who's turn it is
			bool whiteToMove = ParseWhiteToMove(ref chars);
			ValidateSpace(ref chars);

			// 3: Parse castling rights + space
			(CastlingAbility wCas, CastlingAbility bCas) = ParseCastling(ref chars);

			// 4: Parse en passant
			Square? enPassant = ParseEnPassant(ref chars);
			ValidateSpace(ref chars);

			// 5: Parse half moves + space
			byte halfMoves = ParseHalfMoves(ref chars);

			// 6: Parse full moves + end
			uint fullMoves = ParseFullMoves(chars);

			// Done
			board.SetPieces(pieces);
			board.WhiteToMove = whiteToMove;
			board.WhiteCastling = wCas;
			board.BlackCastling = bCas;
			board.EnPassantTarget = enPassant;
			board.NumHalfMoves = halfMoves;
			board.NumFullMoves = fullMoves;
		}
		catch (IndexOutOfRangeException)
		{
			throw new ArgumentException("Too few characters in FEN string.", nameof(chars));
		}
	}

	public static void ParsePlacementOnly(Span<TeamedPiece> pieces, ReadOnlySpan<char> chars)
	{
		if (pieces.Length != 8 * 8)
		{
			throw new ArgumentException("Must have 8 rows and 8 columns.", nameof(pieces));
		}

		try
		{
			int numParsedChars = ParsePlacement(pieces, chars);

			// Verify that we reached the end of "chars"
			if (numParsedChars != chars.Length)
			{
				throw new ArgumentException("Too many characters in span.", nameof(chars));
			}
		}
		catch (IndexOutOfRangeException)
		{
			throw new ArgumentException("Too few characters in span.", nameof(chars));
		}
	}
	private static int ParsePlacement(Span<TeamedPiece> pieces, ReadOnlySpan<char> chars)
	{
		int charPos = 0;

		for (int y = 7; y >= 0; y--)
		{
			var row = (Row)y;

			Col col = Col.CA;

			bool prevWasEmpty = false;

			while (true)
			{
				char c = chars[charPos++];

				if (c is >= '1' and <= '8')
				{
					if (prevWasEmpty)
					{
						throw new InvalidDataException($"Multiple empty specifiers in row {row.RowChar()}");
					}

					int numEmpty = c - '0';
					if ((int)col + numEmpty > (int)Col.MAX)
					{
						throw new InvalidDataException($"Too many empty pieces in row {row.RowChar()}: '{c}'");
					}

					for (int e = 0; e < numEmpty; e++)
					{
						pieces[CUtils.SquareIndex(row, col)] = TeamedPiece.None;
						col++;
					}

					prevWasEmpty = true;
				}
				else
				{
					prevWasEmpty = false;

					TeamedPiece tp = CUtils.TryParseTeamedPiece(c);
					if (tp == TeamedPiece.MAX)
					{
						throw new InvalidDataException($"Invalid piece in row {row.RowChar()}: '{c}'");
					}
					else
					{
						pieces[CUtils.SquareIndex(row, col)] = tp;
						col++;
					}
				}

				if (col == Col.MAX)
				{
					if (row != Row.R1)
					{
						c = chars[charPos++];
						if (c != '/')
						{
							throw new InvalidDataException($"Row {row.RowChar()} ending is invalid: '{c}'");
						}
					}
					break;
				}
			}
		}

		return charPos;
	}

	private static void ValidateSpace(ref ReadOnlySpan<char> chars)
	{
		char c = chars[0];
		if (c != ' ')
		{
			throw new InvalidDataException($"Invalid space: '{c}'");
		}

		chars = chars.Slice(1);
	}

	private static bool ParseWhiteToMove(ref ReadOnlySpan<char> chars)
	{
		char c = chars[0];

		bool whiteToMove;
		switch (c)
		{
			case 'b': whiteToMove = false; break;
			case 'w': whiteToMove = true; break;
			default: throw new InvalidDataException($"Invalid turn: '{c}'");
		}

		chars = chars.Slice(1);
		return whiteToMove;
	}

	private static (CastlingAbility, CastlingAbility) ParseCastling(ref ReadOnlySpan<char> chars)
	{
		// Note: In Chess960, the FEN is not "KQkq".
		// Instead, it includes the column letters.
		// For example, with rooks on A and H, it is "HAha".
		// For example, with rooks on C and G, it is "GCgc".
		// However, I'm not handling that here for now.

		CastlingAbility white;
		CastlingAbility black;

		if (chars[0] == '-')
		{
			if (chars[1] != ' ')
			{
				throw new InvalidDataException("Missing space after castling ability");
			}

			white = CastlingAbility.None;
			black = CastlingAbility.None;

			chars = chars.Slice(2);
		}
		else
		{
			// Do white first, then black, then space
			white = CastlingAbility.None;
			black = CastlingAbility.None;

			int charIdx = 0;
			bool beganBlack = false;
			bool done = false;
			while (!done)
			{
				char c = chars[charIdx++];

				switch (c)
				{
					case 'K':
					{
						if (beganBlack)
						{
							throw new InvalidDataException("Castling rights for white appeared after black.");
						}
						if (white.HasFlag(CastlingAbility.KingSide))
						{
							throw new InvalidDataException("Duplicate white King castling rights.");
						}
						white |= CastlingAbility.KingSide;
						break;
					}
					case 'Q':
					{
						if (beganBlack)
						{
							throw new InvalidDataException("Castling rights for white appeared after black.");
						}
						if (white.HasFlag(CastlingAbility.QueenSide))
						{
							throw new InvalidDataException("Duplicate white Queen castling rights.");
						}
						white |= CastlingAbility.QueenSide;
						break;
					}
					case 'k':
					{
						if (black.HasFlag(CastlingAbility.KingSide))
						{
							throw new InvalidDataException("Duplicate black King castling rights.");
						}
						black |= CastlingAbility.KingSide;
						beganBlack = true;
						break;
					}
					case 'q':
					{
						if (black.HasFlag(CastlingAbility.QueenSide))
						{
							throw new InvalidDataException("Duplicate black Queen castling rights.");
						}
						black |= CastlingAbility.QueenSide;
						beganBlack = true;
						break;
					}
					case ' ':
					{
						if (white == CastlingAbility.None && black == CastlingAbility.None)
						{
							throw new InvalidDataException("Missing castling rights");
						}
						done = true;
						break;
					}
					default:
					{
						throw new InvalidDataException($"Invalid castling rights character: '{c}'");
					}
				}
			}

			chars = chars.Slice(charIdx);
		}

		return (white, black);
	}

	private static Square? ParseEnPassant(ref ReadOnlySpan<char> chars)
	{
		if (chars[0] == '-')
		{
			chars = chars.Slice(1);
			return null;
		}

		Square s = CUtils.ParseSquare(chars[0], chars[1]);
		chars = chars.Slice(2);
		return s;
	}

	private static byte ParseHalfMoves(ref ReadOnlySpan<char> chars)
	{
		int idxOfSpace = chars.IndexOf(' ');

		if (idxOfSpace == -1)
		{
			throw new InvalidDataException("Missing space after half move counter");
		}

		ReadOnlySpan<char> valueChars = chars.Slice(0, idxOfSpace);
		byte b = byte.Parse(valueChars, style: NumberStyles.None);

		if (b >= 100)
		{
			throw new InvalidDataException($"Invalid half move count: '{b}'");
		}

		chars = chars.Slice(idxOfSpace + 1);

		return b;
	}

	private static uint ParseFullMoves(ReadOnlySpan<char> chars)
	{
		uint u = uint.Parse(chars, style: NumberStyles.None);
		if (u == 0)
		{
			throw new InvalidDataException($"Invalid full move count: '{u}'");
		}

		return u;
	}
}