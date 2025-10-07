using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Kermalis.ChessThing;

/// <summary>Not everything is verified here.
/// Character count is verified.
/// Invalid characters are verified.
/// Position notation is verified.
/// Number of Kings/other pieces are NOT verified.
/// Positions of pieces are NOT verified.
/// White/Black turn notation is verified.
/// Castling rights notation is verified, but King/Rook positions/relation are NOT verified (for example, KQkq but a1 is empty).
/// En passant square notation is verified, but the squares themselves are NOT verified (for example: a1).
/// Half move counter is verified to be <= 100.
/// Full move counter is verified to be >= 1.</summary>
public static class FEN
{
	private const int MAX_HALFMOVES = 100; // We can share a board state once the 50 move draw occurs
	private const int FEN_MAX_LEN = 96;

	/// <summary>If <paramref name="gmInfo"/> is <see cref="Chess960"/>, this assumes <see cref="Chess960.QueensideRook"/> and <see cref="Chess960.KingsideRook"/> are properly set.</summary>
	public static string ToFEN(GameModeInfo gmInfo, Board board)
	{
		switch (gmInfo)
		{
			case RegularChess:
			{
				break;
			}
			case Chess960 c960:
			{
				if (!c960.IsInitialized)
				{
					throw new ArgumentException("Chess960 is not initialized.", nameof(gmInfo));
				}
				break;
			}
			default:
			{
				throw new ArgumentException("Unsupported gamemode.", nameof(gmInfo));
			}
		}

		var sb = new StringBuilder(FEN_MAX_LEN);

		// 1: Placement
		WritePlacement(sb, board);
		sb.Append(' ');

		// 2: Who's turn it is + space
		WriteWhiteToMove(sb, board.WhiteToMove);

		// 3: Castling rights + space
		WriteCastling(sb, gmInfo, board.WhiteCastling, board.BlackCastling);

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

	private static void WriteCastling(StringBuilder sb, GameModeInfo gmInfo, CastlingAbility wCas, CastlingAbility bCas)
	{
		if (wCas == CastlingAbility.None && bCas == CastlingAbility.None)
		{
			sb.Append("- ");
		}
		else
		{
			switch (gmInfo)
			{
				case RegularChess:
				{
					WriteCastlingNonEmpty(sb, wCas, bCas,
						'K', 'Q', 'k', 'q');
					break;
				}
				case Chess960 c960:
				{
					WriteCastlingNonEmpty(sb, wCas, bCas,
						c960.KingsideRook.ColumnChar(uppercase: true), c960.QueensideRook.ColumnChar(uppercase: true), c960.KingsideRook.ColumnChar(uppercase: false), c960.QueensideRook.ColumnChar(uppercase: false));
					break;
				}
				default:
				{
					throw new Exception(); // Not possible due to earlier check
				}
			}
		}
	}
	private static void WriteCastlingNonEmpty(StringBuilder sb, CastlingAbility wCas, CastlingAbility bCas, char wK, char wQ, char bK, char bQ)
	{
		if (wCas.HasFlag(CastlingAbility.QueenSide))
		{
			sb.Append(wQ);
		}
		if (wCas.HasFlag(CastlingAbility.KingSide))
		{
			sb.Append(wK);
		}
		if (bCas.HasFlag(CastlingAbility.QueenSide))
		{
			sb.Append(bQ);
		}
		if (bCas.HasFlag(CastlingAbility.KingSide))
		{
			sb.Append(bK);
		}
		sb.Append(' ');
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

	/// <summary>If <paramref name="gmInfo"/> is <see cref="Chess960"/>, special handling occurs.
	/// If <see cref="Chess960.IsInitialized"/> is <see langword="false"/>, we will treat this FEN as the initial setup with white to move.
	/// This means we will initialize the <paramref name="gmInfo"/> using the castling notation, require white to move, require both sides full castling ability, require halfmoves == 0, and require fullmoves == 1.
	/// If it is already initialized, we will validate the castling notation in 960 style (using column letters).</summary>
	public static void Parse(GameModeInfo gmInfo, Board board, ReadOnlySpan<char> chars)
	{
		if (chars.Length > FEN_MAX_LEN)
		{
			throw new ArgumentException("Too many characters in FEN string.", nameof(chars));
		}

		bool mustInit960 = false;

		switch (gmInfo)
		{
			case RegularChess:
			{
				break;
			}
			case Chess960 c960:
			{
				mustInit960 = !c960.IsInitialized;
				break;
			}
			default:
			{
				throw new ArgumentException("Unsupported gamemode.", nameof(gmInfo));
			}
		}

		try
		{
			Span<TeamedPiece> pieces = stackalloc TeamedPiece[8 * 8];

			// 1: Parse placement
			int numParsedChars = ParsePlacement(pieces, chars);
			chars = chars.Slice(numParsedChars);
			ValidateSpace(ref chars);

			if (mustInit960)
			{
				ValidateAndInitChess960(pieces, (Chess960)gmInfo);
			}

			// 2: Parse who's turn it is
			bool whiteToMove = ParseWhiteToMove(ref chars, mustInit960);
			ValidateSpace(ref chars);

			// 3: Parse castling rights + space
			(CastlingAbility wCas, CastlingAbility bCas) = ParseCastling(gmInfo, ref chars);

			// 4: Parse en passant
			Square? enPassant = ParseEnPassant(ref chars);
			ValidateSpace(ref chars);

			// 5: Parse half moves + space
			byte halfMoves = ParseHalfMoves(ref chars, mustInit960);

			// 6: Parse full moves + end
			uint fullMoves = ParseFullMoves(chars, mustInit960);

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

	private static void ValidateAndInitChess960(ReadOnlySpan<TeamedPiece> pieces, Chess960 c960)
	{
		// When initializing 960, check that there are 2 rooks for each team with the kings between them.
		// Validate that the two teams have the rooks and king mirrored.
		// Don't validate other rows or other piece mirrors because we could be playing a variant or custom board.

		// Scan rows 1 and 8 only.
		ScanChess960InitialPos(pieces, Row.R1, "White", TeamedPiece.W_Rook, TeamedPiece.W_King,
			out Col wQRook, out Col wK, out Col wKRook);
		ScanChess960InitialPos(pieces, Row.R8, "Black", TeamedPiece.B_Rook, TeamedPiece.B_King,
			out Col bQRook, out Col bK, out Col bKRook);

		if (wQRook != bQRook || wK != bK || wKRook != bKRook)
		{
			throw new InvalidDataException("Invalid King/Rook mirroring in Chess960 initial position.");
		}

		c960.Initialize(wQRook, wKRook);
	}
	private static void ScanChess960InitialPos(ReadOnlySpan<TeamedPiece> pieces, Row y, string team, TeamedPiece r, TeamedPiece k, out Col queensideRook, out Col kingCol, out Col kingsideRook)
	{
		queensideRook = Col.MAX;
		kingCol = Col.MAX;
		kingsideRook = Col.MAX;

		for (Col x = Col.CA; x < Col.MAX; x++)
		{
			TeamedPiece tp = pieces[CUtils.SquareIndex(y, x)];
			if (tp == r)
			{
				// Found a rook, make sure we don't have more than 2
				if (kingCol == Col.MAX)
				{
					// This is a queenside rook since we haven't found the king yet
					if (queensideRook != Col.MAX)
					{
						throw new InvalidDataException($"Multiple {team} Queen-side Rooks in Chess960 initial position.");
					}
					queensideRook = x;
				}
				else
				{
					// This is a kingside rook since we found the king already
					if (kingsideRook != Col.MAX)
					{
						throw new InvalidDataException($"Multiple {team} King-side Rooks in Chess960 initial position.");
					}
					kingsideRook = x;
				}
			}
			else if (tp == k)
			{
				// Found a king, make sure we don't have more than 1
				// We already guarantee that it's between the rooks by checking if there are multiple rooks before/after finding the king
				if (kingCol != Col.MAX)
				{
					throw new InvalidDataException($"Multiple {team} Kings in Chess960 initial position.");
				}
				kingCol = x;
			}
		}

		if (queensideRook == Col.MAX || kingCol == Col.MAX || kingsideRook == Col.MAX)
		{
			throw new InvalidDataException($"Missing {team} Kings and Rooks in Chess960 initial position.");
		}
	}

	private static bool ParseWhiteToMove(ref ReadOnlySpan<char> chars, bool forceInitialPosition)
	{
		char c = chars[0];

		bool whiteToMove;
		switch (c)
		{
			case 'b': whiteToMove = false; break;
			case 'w': whiteToMove = true; break;
			default: throw new InvalidDataException($"Invalid turn: '{c}'");
		}

		if (forceInitialPosition && !whiteToMove)
		{
			throw new InvalidDataException("Initial position must have white to move.");
		}

		chars = chars.Slice(1);
		return whiteToMove;
	}

	private static (CastlingAbility, CastlingAbility) ParseCastling(GameModeInfo gmInfo, ref ReadOnlySpan<char> chars)
	{
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

			switch (gmInfo)
			{
				case RegularChess:
				{
					ParseCastlingNonEmpty(ref chars, ref white, ref black,
						'K', 'Q', 'k', 'q');
					break;
				}
				case Chess960 c960:
				{
					// In Chess960, the FEN is not "KQkq".
					// Instead, it includes the column letters.
					// For example, with rooks on A and H, it is "HAha".
					// For example, with rooks on C and G, it is "GCgc".
					ParseCastlingNonEmpty(ref chars, ref white, ref black,
						c960.KingsideRook.ColumnChar(uppercase: true), c960.QueensideRook.ColumnChar(uppercase: true), c960.KingsideRook.ColumnChar(uppercase: false), c960.QueensideRook.ColumnChar(uppercase: false));
					break;
				}
				default:
				{
					throw new Exception(); // Not possible due to earlier check
				}
			}
		}

		return (white, black);
	}
	private static void ParseCastlingNonEmpty(ref ReadOnlySpan<char> chars, ref CastlingAbility white, ref CastlingAbility black, char wK, char wQ, char bK, char bQ)
	{
		int charIdx = 0;
		bool beganBlack = false;
		bool done = false;
		while (!done)
		{
			char c = chars[charIdx++];

			if (c == wK)
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
			}
			else if (c == wQ)
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
			}
			else if (c == bK)
			{
				if (black.HasFlag(CastlingAbility.KingSide))
				{
					throw new InvalidDataException("Duplicate black King castling rights.");
				}
				black |= CastlingAbility.KingSide;
				beganBlack = true;
			}
			else if (c == bQ)
			{
				if (black.HasFlag(CastlingAbility.QueenSide))
				{
					throw new InvalidDataException("Duplicate black Queen castling rights.");
				}
				black |= CastlingAbility.QueenSide;
				beganBlack = true;
			}
			else if (c == ' ')
			{
				if (white == CastlingAbility.None && black == CastlingAbility.None)
				{
					throw new InvalidDataException("Missing castling rights");
				}
				done = true;
			}
			else
			{
				throw new InvalidDataException($"Invalid castling rights character: '{c}'");
			}
		}

		chars = chars.Slice(charIdx);
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

	private static byte ParseHalfMoves(ref ReadOnlySpan<char> chars, bool forceInitialPosition)
	{
		int idxOfSpace = chars.IndexOf(' ');

		if (idxOfSpace == -1)
		{
			throw new InvalidDataException("Missing space after half move counter");
		}

		ReadOnlySpan<char> valueChars = chars.Slice(0, idxOfSpace);
		byte b = byte.Parse(valueChars, style: NumberStyles.None);

		if (forceInitialPosition && b != 0)
		{
			throw new InvalidDataException("Initial position must have 0 half moves.");
		}

		if (b > MAX_HALFMOVES)
		{
			throw new InvalidDataException($"Invalid half move count: '{b}'");
		}

		chars = chars.Slice(idxOfSpace + 1);

		return b;
	}

	private static uint ParseFullMoves(ReadOnlySpan<char> chars, bool forceInitialPosition)
	{
		uint u = uint.Parse(chars, style: NumberStyles.None);

		if (forceInitialPosition && u != 1)
		{
			throw new InvalidDataException("Initial position must have 1 full move.");
		}

		if (u < 1)
		{
			throw new InvalidDataException($"Invalid full move count: '{u}'");
		}

		return u;
	}
}