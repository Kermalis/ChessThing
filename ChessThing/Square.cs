using System;

namespace Kermalis.ChessThing;

public readonly struct Square
{
	public static readonly Square Invalid = new(Col.MAX, Row.MAX);

	public readonly Col Col;
	public readonly Row Row;

	public int Index => CUtils.SquareIndex(Col, Row);

	public bool IsInvalid => Col >= Col.MAX || Row >= Row.MAX;

	public Square(Col col, Row row)
	{
		Col = col;
		Row = row;
	}

	public static Square Parse(char c0, char c1)
	{
		Col col = CUtils.TryParseCol(c0);
		if (col >= Col.MAX)
		{
			throw new ArgumentOutOfRangeException(nameof(c0), c0, "Invalid column");
		}

		Row row = CUtils.TryParseRow(c1);
		if (row >= Row.MAX)
		{
			throw new ArgumentOutOfRangeException(nameof(c1), c1, "Invalid row");
		}

		return new Square(col, row);
	}

	public override string ToString()
	{
		if (IsInvalid)
		{
			return "Invalid";
		}
		return $"{Col.ColumnChar()}{Row.RowChar()}";
	}
}