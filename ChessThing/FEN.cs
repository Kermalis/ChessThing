using System;
using System.IO;

namespace Kermalis.ChessThing;

public static class FEN
{
	public static void ParsePlacement(Span<TeamedPiece> pieces, ReadOnlySpan<char> chars)
	{
		if (pieces.Length != 8 * 8)
		{
			throw new ArgumentException("Must have 8 rows and 8 columns.", nameof(pieces));
		}

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

		// TODO: Verify that we reached the end of "chars"
	}
}