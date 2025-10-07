using System;

namespace Kermalis.ChessThing;

public readonly struct Move
{
	// TODO: Pack all of this into a u32
	public readonly PieceKind Piece;

	public readonly Col? FromColHint;
	public readonly Row? FromRowHint;

	/// <summary>Castling notation infers the "to" square.</summary>
	public readonly Square? ToHint;

	public readonly PieceKind PromotedPiece;
	public readonly bool CaptureHint;
	public readonly bool CastleQueensideHint;
	public readonly bool CastleKingsideHint;
	/// <summary>This is <see langword="true"/> if <see cref="CheckmateHint"/> is <see langword="true"/>.</summary>
	public readonly bool CheckHint;
	public readonly bool CheckmateHint;

	public readonly bool IsInitialized => Piece != PieceKind.None; // _data != 0

	private Move(PieceKind p, Col? fromColHint, Row? fromRowHint, Square? to,
		bool captureHint, bool checkHint, bool checkmateHint, PieceKind promote = PieceKind.None, bool castleQueenside = false, bool castleKingside = false)
	{
		Piece = p;
		FromColHint = fromColHint;
		FromRowHint = fromRowHint;
		ToHint = to;
		PromotedPiece = promote;
		CaptureHint = captureHint;
		CastleQueensideHint = castleQueenside;
		CastleKingsideHint = castleKingside;
		CheckHint = checkHint;
		CheckmateHint = checkmateHint;
	}

	/// <summary>This does not validate the legality of the move.
	/// So "a3=Q", "Kd1#", "Bfg2" or "hxc6" will be parsed even though they're impossible in regular chess.</summary>
	public static Move ParseAlgebraicNotation(ref ReadOnlySpan<char> chars)
	{
		bool checkHint;
		bool checkmateHint;

		// Parse "O-O-O" "O-O-O+" "O-O-O#"
		if (chars.Length >= 5 && chars[0] == 'O' && chars[1] == '-' && chars[2] == 'O' && chars[3] == '-' && chars[4] == 'O')
		{
			if (!TryParseCheckHints(ref chars, 5, out checkHint, out checkmateHint))
			{
				chars = chars.Slice(5);
			}

			return new Move(PieceKind.King, null, null, null,
				false, checkHint, checkmateHint, castleQueenside: true);
		}

		// Parse "O-O" "O-O+" "O-O#"
		if (chars.Length >= 3 && chars[0] == 'O' && chars[1] == '-' && chars[2] == 'O')
		{
			if (!TryParseCheckHints(ref chars, 3, out checkHint, out checkmateHint))
			{
				chars = chars.Slice(3);
			}

			return new Move(PieceKind.King, null, null, null,
				false, checkHint, checkmateHint, castleKingside: true);
		}

		// Parse stuff like "bxa8=Q" "bxa8=Q+" "bxa8=Q#"
		if (chars.Length >= 6 && chars[1] == 'x' && chars[4] == '=')
		{
			Col fromCol = CUtils.TryParseCol(chars[0]);
			if (fromCol < Col.MAX)
			{
				Col toCol = CUtils.TryParseCol(chars[2]);
				if (toCol < Col.MAX)
				{
					// Don't validate row 1 or 8 because we're not checking legality.
					Row toRow = CUtils.TryParseRow(chars[3]);
					if (toRow < Row.MAX)
					{
						PieceKind pp = CUtils.TryParsePromotionPiece(chars[5]);
						if (pp != PieceKind.MAX)
						{
							if (!TryParseCheckHints(ref chars, 6, out checkHint, out checkmateHint))
							{
								chars = chars.Slice(6);
							}

							return new Move(PieceKind.Pawn, fromCol, null, new Square(toCol, toRow),
								true, checkHint, checkmateHint, promote: pp);
						}
					}
				}
			}
		}

		// Parse stuff like "a8=Q" "a8=Q+" "a8=Q#"
		if (chars.Length >= 4 && chars[2] == '=')
		{
			Col toCol = CUtils.TryParseCol(chars[0]);
			if (toCol < Col.MAX)
			{
				// Don't validate row 1 or 8 because we're not checking legality.
				Row toRow = CUtils.TryParseRow(chars[1]);
				if (toRow < Row.MAX)
				{
					PieceKind pp = CUtils.TryParsePromotionPiece(chars[3]);
					if (pp != PieceKind.MAX)
					{
						if (!TryParseCheckHints(ref chars, 4, out checkHint, out checkmateHint))
						{
							chars = chars.Slice(4);
						}

						return new Move(PieceKind.Pawn, null, null, new Square(toCol, toRow),
							false, checkHint, checkmateHint, promote: pp);
					}
				}
			}
		}

		// Parse stuff like "dxc6" "dxc6+" "dxc6#"
		if (chars.Length >= 4 && chars[1] == 'x')
		{
			Col fromCol = CUtils.TryParseCol(chars[0]);
			if (fromCol < Col.MAX)
			{
				Col toCol = CUtils.TryParseCol(chars[2]);
				if (toCol < Col.MAX)
				{
					Row toRow = CUtils.TryParseRow(chars[3]);
					if (toRow < Row.MAX)
					{
						if (!TryParseCheckHints(ref chars, 4, out checkHint, out checkmateHint))
						{
							chars = chars.Slice(4);
						}

						return new Move(PieceKind.Pawn, fromCol, null, new Square(toCol, toRow),
							true, checkHint, checkmateHint);
					}
				}
			}
		}

		// Parse stuff like "a3" "a3+" "a3#"
		if (chars.Length >= 2)
		{
			Col toCol = CUtils.TryParseCol(chars[0]);
			if (toCol < Col.MAX)
			{
				Row toRow = CUtils.TryParseRow(chars[1]);
				if (toRow < Row.MAX)
				{
					if (!TryParseCheckHints(ref chars, 2, out checkHint, out checkmateHint))
					{
						chars = chars.Slice(2);
					}

					return new Move(PieceKind.Pawn, null, null, new Square(toCol, toRow),
						false, checkHint, checkmateHint);
				}
			}
		}

		// Parse stuff like "Nfxd2" "Nfxd2+" "Nfxd2#"
		// Parse stuff like "N3xd2" "N3xd2+" "N3xd2#"
		if (chars.Length >= 5 && chars[2] == 'x')
		{
			PieceKind p = TryParseNonPawnPiece(chars[0]);
			if (p < PieceKind.MAX)
			{
				Col fromCol = CUtils.TryParseCol(chars[1]);
				if (fromCol < Col.MAX)
				{
					Col toCol = CUtils.TryParseCol(chars[3]);
					if (toCol < Col.MAX)
					{
						Row toRow = CUtils.TryParseRow(chars[4]);
						if (toRow < Row.MAX)
						{
							if (!TryParseCheckHints(ref chars, 5, out checkHint, out checkmateHint))
							{
								chars = chars.Slice(5);
							}

							return new Move(p, fromCol, null, new Square(toCol, toRow),
								true, checkHint, checkmateHint);
						}
					}
				}
				else
				{
					Row fromRow = CUtils.TryParseRow(chars[1]);
					if (fromRow < Row.MAX)
					{
						Col toCol = CUtils.TryParseCol(chars[3]);
						if (toCol < Col.MAX)
						{
							Row toRow = CUtils.TryParseRow(chars[4]);
							if (toRow < Row.MAX)
							{
								if (!TryParseCheckHints(ref chars, 5, out checkHint, out checkmateHint))
								{
									chars = chars.Slice(5);
								}

								return new Move(p, null, fromRow, new Square(toCol, toRow),
									true, checkHint, checkmateHint);
							}
						}
					}
				}
			}
		}

		// Parse stuff like "Nxf3" "Nxf3+" "Nxf3#"
		if (chars.Length >= 4 && chars[1] == 'x')
		{
			PieceKind p = TryParseNonPawnPiece(chars[0]);
			if (p < PieceKind.MAX)
			{
				Col toCol = CUtils.TryParseCol(chars[2]);
				if (toCol < Col.MAX)
				{
					Row toRow = CUtils.TryParseRow(chars[3]);
					if (toRow < Row.MAX)
					{
						if (!TryParseCheckHints(ref chars, 4, out checkHint, out checkmateHint))
						{
							chars = chars.Slice(4);
						}

						return new Move(p, null, null, new Square(toCol, toRow),
							true, checkHint, checkmateHint);
					}
				}
			}
		}

		// Parse stuff like "Nfd2" "Nfd2+" "Nfd2#"
		// Parse stuff like "N3d2" "N3d2+" "N3d2#"
		if (chars.Length >= 4)
		{
			PieceKind p = TryParseNonPawnPiece(chars[0]);
			if (p < PieceKind.MAX)
			{
				Col fromCol = CUtils.TryParseCol(chars[1]);
				if (fromCol < Col.MAX)
				{
					Col toCol = CUtils.TryParseCol(chars[2]);
					if (toCol < Col.MAX)
					{
						Row toRow = CUtils.TryParseRow(chars[3]);
						if (toRow < Row.MAX)
						{
							if (!TryParseCheckHints(ref chars, 4, out checkHint, out checkmateHint))
							{
								chars = chars.Slice(4);
							}

							return new Move(p, fromCol, null, new Square(toCol, toRow),
								false, checkHint, checkmateHint);
						}
					}
				}
				else
				{
					Row fromRow = CUtils.TryParseRow(chars[1]);
					if (fromRow < Row.MAX)
					{
						Col toCol = CUtils.TryParseCol(chars[2]);
						if (toCol < Col.MAX)
						{
							Row toRow = CUtils.TryParseRow(chars[3]);
							if (toRow < Row.MAX)
							{
								if (!TryParseCheckHints(ref chars, 4, out checkHint, out checkmateHint))
								{
									chars = chars.Slice(4);
								}

								return new Move(p, null, fromRow, new Square(toCol, toRow),
									false, checkHint, checkmateHint);
							}
						}
					}
				}
			}
		}

		// Parse stuff like "Nf3" "Nf3+" "Nf3#"
		if (chars.Length >= 3)
		{
			PieceKind p = TryParseNonPawnPiece(chars[0]);
			if (p < PieceKind.MAX)
			{
				Col toCol = CUtils.TryParseCol(chars[1]);
				if (toCol < Col.MAX)
				{
					Row toRow = CUtils.TryParseRow(chars[2]);
					if (toRow < Row.MAX)
					{
						if (!TryParseCheckHints(ref chars, 3, out checkHint, out checkmateHint))
						{
							chars = chars.Slice(3);
						}

						return new Move(p, null, null, new Square(toCol, toRow),
							false, checkHint, checkmateHint);
					}
				}
			}
		}

		// Fail
		throw new ArgumentException("Unable to parse move from algebraic notation.", nameof(chars));
	}

	/// <summary>If the parse failed, this returns <see cref="PieceKind.MAX"/>.</summary>
	private static PieceKind TryParseNonPawnPiece(char c)
	{
		switch (c)
		{
			case 'N': return PieceKind.Knight;
			case 'B': return PieceKind.Bishop;
			case 'R': return PieceKind.Rook;
			case 'Q': return PieceKind.Queen;
			case 'K': return PieceKind.King;
		}

		return PieceKind.MAX;
	}
	private static bool TryParseCheckHints(ref ReadOnlySpan<char> chars, int start, out bool checkHint, out bool checkmateHint)
	{
		if (chars.Length > start)
		{
			if (chars[start] == '+')
			{
				checkHint = true;
				checkmateHint = false;
			}
			else if (chars[start] == '#')
			{
				checkHint = true;
				checkmateHint = true;
			}
			else
			{
				goto bottom;
			}

			chars = chars.Slice(start + 1);
			return true;
		}

	bottom:
		checkHint = false;
		checkmateHint = false;
		return false;
	}
}