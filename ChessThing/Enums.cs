namespace Kermalis.ChessThing;

public enum Col : byte
{
	CA,
	CB,
	CC,
	CD,
	CE,
	CF,
	CG,
	CH,
	MAX
}

public enum PieceKind : byte
{
	None,
	Pawn,
	Knight,
	Bishop,
	Rook,
	Queen,
	King,
	MAX
}

public enum Row : byte
{
	R1,
	R2,
	R3,
	R4,
	R5,
	R6,
	R7,
	R8,
	MAX
}

public enum TeamedPiece : byte
{
	None,
	W_Pawn,
	W_Knight,
	W_Bishop,
	W_Rook,
	W_Queen,
	W_King,
	B_Pawn,
	B_Knight,
	B_Bishop,
	B_Rook,
	B_Queen,
	B_King,
	MAX
}