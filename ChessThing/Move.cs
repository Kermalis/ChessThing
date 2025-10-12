using System;

namespace Kermalis.ChessThing;

public readonly struct Move
{
	// 26 bits of data:
	// 3 bits [ 0,  2] : Piece [0, 6]
	// 1 bit  [ 3,  3] : FromColHintExists
	// 3 bits [ 4,  6] : FromColHint [0, 7]
	// 1 bit  [ 7,  7] : FromRowHintExists
	// 3 bits [ 8, 10] : FromRowHint [0, 7]
	// 1 bit  [11, 11] : ToHintExists
	// 3 bits [12, 14] : ToHintCol [0, 7]
	// 3 bits [15, 17] : ToHintRow [0, 7]
	// 3 bits [18, 20] : PromotedPiece [0, 4]
	// 1 bit  [21, 21] : CaptureHint
	// 1 bit  [22, 22] : CastleQueensideHint
	// 1 bit  [23, 23] : CastleKingsideHint
	// 1 bit  [24, 24] : CheckHint
	// 1 bit  [25, 25] : CheckmateHint
	private readonly uint _data;

	public readonly bool IsInitialized => _data != 0;

	public readonly PieceKind Piece => (PieceKind)(_data & 0b111);

	/// <summary>Returns <see cref="Col.MAX"/> if there is no hint.</summary>
	public readonly Col FromColHint
	{
		get
		{
			if ((_data & (1u << 3)) != 0)
			{
				return (Col)((_data >> 4) & 0b111);
			}
			return Col.MAX;
		}
	}
	/// <summary>Returns <see cref="Row.MAX"/> if there is no hint.</summary>
	public readonly Row FromRowHint
	{
		get
		{
			if ((_data & (1u << 7)) != 0)
			{
				return (Row)((_data >> 8) & 0b111);
			}
			return Row.MAX;
		}
	}
	/// <summary>Returns <see cref="Square.Invalid"/> if there is no hint.
	/// Castling notation infers the "to" square.</summary>
	public readonly Square ToHint
	{
		get
		{
			if ((_data & (1u << 11)) != 0)
			{
				var col = (Col)((_data >> 12) & 0b111);
				var row = (Row)((_data >> 15) & 0b111);
				return new Square(col, row);
			}
			return Square.Invalid;
		}
	}

	public readonly PieceKind PromotedPiece
	{
		get
		{
			uint val = (_data >> 18) & 0b111;
			if (val != 0)
			{
				return (PieceKind)(val + 1);
			}
			return PieceKind.None;
		}
	}
	public readonly bool CaptureHint => (_data & (1u << 21)) != 0;
	public readonly bool CastleQueensideHint => (_data & (1u << 22)) != 0;
	public readonly bool CastleKingsideHint => (_data & (1u << 23)) != 0;
	/// <summary>This is <see langword="true"/> if <see cref="CheckmateHint"/> is <see langword="true"/>.</summary>
	public readonly bool CheckHint => (_data & (1u << 24)) != 0;
	public readonly bool CheckmateHint => (_data & (1u << 25)) != 0;

	private Move(PieceKind p, Col fromColHint, Row fromRowHint, Square toHint,
		bool captureHint, bool checkHint, bool checkmateHint, PieceKind promote = PieceKind.None, bool castleQueenside = false, bool castleKingside = false)
	{
		_data |= (uint)p;

		if (fromColHint != Col.MAX)
		{
			_data |= 1u << 3;
			_data |= (uint)fromColHint << 4;
		}
		if (fromRowHint != Row.MAX)
		{
			_data |= 1u << 7;
			_data |= (uint)fromRowHint << 8;
		}
		if (!toHint.IsInvalid)
		{
			_data |= 1u << 11;
			_data |= (uint)toHint.Col << 12;
			_data |= (uint)toHint.Row << 15;
		}

		if (promote != PieceKind.None)
		{
			_data |= (uint)(promote - 1) << 18;
		}
		if (captureHint)
		{
			_data |= 1u << 21;
		}
		if (castleQueenside)
		{
			_data |= 1u << 22;
		}
		if (castleKingside)
		{
			_data |= 1u << 23;
		}
		if (checkHint)
		{
			_data |= 1u << 24;
		}
		if (checkmateHint)
		{
			_data |= 1u << 25;
		}
	}

	/// <summary><paramref name="fromColHint"/> and <paramref name="fromRowHint"/> should be <see cref="Col.MAX"/> and <see cref="Row.MAX"/> for a lack of hint.</summary>
	public static Move CreatePawnPromotion(Col fromColHint, Row fromRowHint, Square to, PieceKind promotedPiece, bool captureHint, bool checkHint, bool checkmateHint)
	{
		if (fromColHint > Col.MAX)
		{
			throw new ArgumentOutOfRangeException(nameof(fromColHint), fromColHint, null);
		}
		if (fromRowHint > Row.MAX)
		{
			throw new ArgumentOutOfRangeException(nameof(fromRowHint), fromRowHint, null);
		}
		if (to.IsInvalid)
		{
			throw new ArgumentOutOfRangeException(nameof(to), to, null);
		}
		if (promotedPiece is PieceKind.None or PieceKind.Pawn or PieceKind.King or >= PieceKind.MAX)
		{
			throw new ArgumentOutOfRangeException(nameof(promotedPiece), promotedPiece, null);
		}

		return new Move(PieceKind.Pawn, fromColHint, fromRowHint, to,
			captureHint, checkHint || checkmateHint, checkmateHint, promote: promotedPiece);
	}
	/// <summary><paramref name="fromColHint"/> and <paramref name="fromRowHint"/> and <paramref name="toHint"/> should be <see cref="Col.MAX"/> and <see cref="Row.MAX"/> and <see cref="Square.Invalid"/> for a lack of hint.</summary>
	public static Move CreateCastle(Col fromColHint, Row fromRowHint, Square toHint, bool isKingside, bool checkHint, bool checkmateHint)
	{
		if (fromColHint > Col.MAX)
		{
			throw new ArgumentOutOfRangeException(nameof(fromColHint), fromColHint, null);
		}
		if (fromRowHint > Row.MAX)
		{
			throw new ArgumentOutOfRangeException(nameof(fromRowHint), fromRowHint, null);
		}

		return new Move(PieceKind.King, fromColHint, fromRowHint, toHint,
			false, checkHint || checkmateHint, checkmateHint, castleQueenside: !isKingside, castleKingside: isKingside);
	}
	/// <summary><paramref name="fromColHint"/> and <paramref name="fromRowHint"/> should be <see cref="Col.MAX"/> and <see cref="Row.MAX"/> for a lack of hint.</summary>
	public static Move CreateOrdinary(PieceKind piece, Col fromColHint, Row fromRowHint, Square to, bool captureHint, bool checkHint, bool checkmateHint)
	{
		if (piece is PieceKind.None or >= PieceKind.MAX)
		{
			throw new ArgumentOutOfRangeException(nameof(piece), piece, null);
		}
		if (fromColHint > Col.MAX)
		{
			throw new ArgumentOutOfRangeException(nameof(fromColHint), fromColHint, null);
		}
		if (fromRowHint > Row.MAX)
		{
			throw new ArgumentOutOfRangeException(nameof(fromRowHint), fromRowHint, null);
		}
		if (to.IsInvalid)
		{
			throw new ArgumentOutOfRangeException(nameof(to), to, null);
		}

		return new Move(piece, fromColHint, fromRowHint, to,
			captureHint, checkHint || checkmateHint, checkmateHint);
	}

	/// <summary>This does not validate the legality of the move.
	/// So "b8", "a3=Q", "Kd1#", or "hxc6" will be parsed even though they're impossible in regular chess.</summary>
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

			return new Move(PieceKind.King, Col.MAX, Row.MAX, Square.Invalid,
				false, checkHint, checkmateHint, castleQueenside: true);
		}

		// Parse "O-O" "O-O+" "O-O#"
		if (chars.Length >= 3 && chars[0] == 'O' && chars[1] == '-' && chars[2] == 'O')
		{
			if (!TryParseCheckHints(ref chars, 3, out checkHint, out checkmateHint))
			{
				chars = chars.Slice(3);
			}

			return new Move(PieceKind.King, Col.MAX, Row.MAX, Square.Invalid,
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

							return new Move(PieceKind.Pawn, fromCol, Row.MAX, new Square(toCol, toRow),
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

						return new Move(PieceKind.Pawn, Col.MAX, Row.MAX, new Square(toCol, toRow),
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

						return new Move(PieceKind.Pawn, fromCol, Row.MAX, new Square(toCol, toRow),
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

					return new Move(PieceKind.Pawn, Col.MAX, Row.MAX, new Square(toCol, toRow),
						false, checkHint, checkmateHint);
				}
			}
		}

		// Parse stuff like "Nf3xd2" "Nf3xd2+" "Nf3xd2#"
		if (chars.Length >= 6 && chars[3] == 'x')
		{
			PieceKind p = TryParseNonPawnPiece(chars[0]);
			if (p < PieceKind.MAX)
			{
				Col fromCol = CUtils.TryParseCol(chars[1]);
				if (fromCol < Col.MAX)
				{
					Row fromRow = CUtils.TryParseRow(chars[2]);
					if (fromRow < Row.MAX)
					{
						Col toCol = CUtils.TryParseCol(chars[4]);
						if (toCol < Col.MAX)
						{
							Row toRow = CUtils.TryParseRow(chars[5]);
							if (toRow < Row.MAX)
							{
								if (!TryParseCheckHints(ref chars, 6, out checkHint, out checkmateHint))
								{
									chars = chars.Slice(6);
								}

								return new Move(p, fromCol, fromRow, new Square(toCol, toRow),
									true, checkHint, checkmateHint);
							}
						}
					}
				}
			}
		}

		// Parse stuff like "Nf3d2" "Nf3d2+" "Nf3d2#"
		if (chars.Length >= 5)
		{
			PieceKind p = TryParseNonPawnPiece(chars[0]);
			if (p < PieceKind.MAX)
			{
				Col fromCol = CUtils.TryParseCol(chars[1]);
				if (fromCol < Col.MAX)
				{
					Row fromRow = CUtils.TryParseRow(chars[2]);
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

								return new Move(p, fromCol, fromRow, new Square(toCol, toRow),
									false, checkHint, checkmateHint);
							}
						}
					}
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

							return new Move(p, fromCol, Row.MAX, new Square(toCol, toRow),
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

								return new Move(p, Col.MAX, fromRow, new Square(toCol, toRow),
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

						return new Move(p, Col.MAX, Row.MAX, new Square(toCol, toRow),
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

							return new Move(p, fromCol, Row.MAX, new Square(toCol, toRow),
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

								return new Move(p, Col.MAX, fromRow, new Square(toCol, toRow),
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

						return new Move(p, Col.MAX, Row.MAX, new Square(toCol, toRow),
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