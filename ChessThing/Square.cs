namespace Kermalis.ChessThing;

public readonly struct Square
{
	public readonly Row Row;
	public readonly Col Col;

	public int Index => CUtils.SquareIndex(Row, Col);

	public Square(Row row, Col col)
	{
		Row = row;
		Col = col;
	}
}