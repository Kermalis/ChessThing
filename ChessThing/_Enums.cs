using System;

namespace Kermalis.ChessThing;

[Flags]
public enum CastlingAbility : byte
{
	None = 0,
	QueenSide = 1 << 0,
	KingSide = 1 << 1,
}

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

public enum NAG : byte
{
	Null,

	// [1, 9] => Move Assessments

	/// <summary>!</summary>
	GoodMove,
	/// <summary>?</summary>
	PoorMove,
	/// <summary>‼</summary>
	VeryGoodMove,
	/// <summary>⁇</summary>
	VeryPoorMove,
	/// <summary>⁉</summary>
	InterestingMove,
	/// <summary>⁈</summary>
	QuestionableMove,
	/// <summary>□</summary>
	ForcedMove,
	SingularMove,
	WorstMove,

	// [10, 135] Positional Assessments

	/// <summary>=</summary>
	EvenPosition,
	EqualQuietPosition,
	EqualActivePosition,
	/// <summary>∞</summary>
	UnclearPosition,
	/// <summary>⩲</summary>
	SlightWhiteAdvantage,
	/// <summary>⩱</summary>
	SlightBlackAdvantage,
	/// <summary>±</summary>
	ModerateWhiteAdvantage,
	/// <summary>∓</summary>
	ModerateBlackAdvantage,
	/// <summary>+ −</summary>
	DecisiveWhiteAdvantage,
	/// <summary>− +</summary>
	DecisiveBlackAdvantage,
	CrushingWhiteAdvantage,
	CrushingBlackAdvantage,
	/// <summary>⨀</summary>
	WhiteInZugzwang,
	/// <summary>⨀</summary>
	BlackInZugzwang,
	SlightWhiteSpaceAdvantage,
	SlightBlackSpaceAdvantage,
	/// <summary>○</summary>
	ModerateWhiteSpaceAdvantage,
	/// <summary>○</summary>
	ModerateBlackSpaceAdvantage,
	DecisiveWhiteSpaceAdvantage,
	DecisiveBlackSpaceAdvantage,
	SlightWhiteDevelopmentAdvantage,
	SlightBlackDevelopmentAdvantage,
	/// <summary>⟳</summary>
	ModerateWhiteDevelopmentAdvantage,
	/// <summary>⟳</summary>
	ModerateBlackDevelopmentAdvantage,
	DecisiveWhiteDevelopmentAdvantage,
	DecisiveBlackDevelopmentAdvantage,
	/// <summary>↑</summary>
	WhiteInitiative,
	/// <summary>↑</summary>
	BlackInitiative,
	WhiteLastingInitiative,
	BlackLastingInitiative,
	/// <summary>→</summary>
	WhiteAttack,
	/// <summary>→</summary>
	BlackAttack,
	WhiteInsufficientMaterialDeficitCompensation,
	BlackInsufficientMaterialDeficitCompensation,
	/// <summary>⯹</summary>
	WhiteSufficientMaterialDeficitCompensation,
	/// <summary>⯹</summary>
	BlackSufficientMaterialDeficitCompensation,
	WhiteEnoughMaterialDeficitCompensation,
	BlackEnoughMaterialDeficitCompensation,
	SlightWhiteCenterAdvantage,
	SlightBlackCenterAdvantage,
	ModerateWhiteCenterAdvantage,
	ModerateBlackCenterAdvantage,
	DecisiveWhiteCenterAdvantage,
	DecisiveBlackCenterAdvantage,
	SlightWhiteKingsideAdvantage,
	SlightBlackKingsideAdvantage,
	ModerateWhiteKingsideAdvantage,
	ModerateBlackKingsideAdvantage,
	DecisiveWhiteKingsideAdvantage,
	DecisiveBlackKingsideAdvantage,
	SlightWhiteQueensideAdvantage,
	SlightBlackQueensideAdvantage,
	ModerateWhiteQueensideAdvantage,
	ModerateBlackQueensideAdvantage,
	DecisiveWhiteQueensideAdvantage,
	DecisiveBlackQueensideAdvantage,
	VulnerableWhiteFirstRank,
	VulnerableBlackFirstRank,
	WellProtectedWhiteFirstRank,
	WellProtectedBlackFirstRank,
	PoorlyProtectedWhiteKing,
	PoorlyProtectedBlackKing,
	WellProtectedWhiteKing,
	WellProtectedBlackKing,
	PoorlyPlacedWhiteKing,
	PoorlyPlacedBlackKing,
	WellPlacedWhiteKing,
	WellPlacedBlackKing,
	VeryWeakWhitePawnStructure,
	VeryWeakBlackPawnStructure,
	ModeratelyWeakWhitePawnStructure,
	ModeratelyWeakBlackPawnStructure,
	ModeratelyStrongWhitePawnStructure,
	ModeratelyStrongBlackPawnStructure,
	VeryStrongWhitePawnStructure,
	VeryStrongBlackPawnStructure,
	PoorWhiteKnightPlacement,
	PoorBlackKnightPlacement,
	GoodWhiteKnightPlacement,
	GoodBlackKnightPlacement,
	PoorWhiteBishopPlacement,
	PoorBlackBishopPlacement,
	GoodWhiteBishopPlacement,
	GoodBlackBishopPlacement,
	PoorWhiteRookPlacement,
	PoorBlackRookPlacement,
	GoodWhiteRookPlacement,
	GoodBlackRookPlacement,
	PoorWhiteQueenPlacement,
	PoorBlackQueenPlacement,
	GoodWhiteQueenPlacement,
	GoodBlackQueenPlacement,
	PoorWhitePieceCoordination,
	PoorBlackPieceCoordination,
	GoodWhitePieceCoordination,
	GoodBlackPieceCoordination,
	VeryPoorlyPlayedWhiteOpening,
	VeryPoorlyPlayedBlackOpening,
	PoorlyPlayedWhiteOpening,
	PoorlyPlayedBlackOpening,
	WellPlayedWhiteOpening,
	WellPlayedBlackOpening,
	VeryWellPlayedWhiteOpening,
	VeryWellPlayedBlackOpening,
	VeryPoorlyPlayedWhiteMiddlegame,
	VeryPoorlyPlayedBlackMiddlegame,
	PoorlyPlayedWhiteMiddlegame,
	PoorlyPlayedBlackMiddlegame,
	WellPlayedWhiteMiddlegame,
	WellPlayedBlackMiddlegame,
	VeryWellPlayedWhiteMiddlegame,
	VeryWellPlayedBlackMiddlegame,
	VeryPoorlyPlayedWhiteEnding,
	VeryPoorlyPlayedBlackEnding,
	PoorlyPlayedWhiteEnding,
	PoorlyPlayedBlackEnding,
	WellPlayedWhiteEnding,
	WellPlayedBlackEnding,
	VeryWellPlayedWhiteEnding,
	VeryWellPlayedBlackEnding,
	SlightWhiteCounterplay,
	SlightBlackCounterplay,
	/// <summary>⇆</summary>
	ModerateWhiteCounterplay,
	/// <summary>⇆</summary>
	ModerateBlackCounterplay,
	DecisiveWhiteCounterplay,
	DecisiveBlackCounterplay,

	// [136, 139] Time Pressure Commentaries

	ModerateWhiteTimePressure,
	ModerateBlackTimePressure,
	/// <summary>⨁</summary>
	SevereWhiteTimePressure,
	/// <summary>⨁</summary>
	SevereBlackTimePressure,

	// [140, 255] Undefined

	Undefined140,
	Undefined141,
	Undefined142,
	Undefined143,
	Undefined144,
	Undefined145,
	Undefined146,
	Undefined147,
	Undefined148,
	Undefined149,
	Undefined150,
	Undefined151,
	Undefined152,
	Undefined153,
	Undefined154,
	Undefined155,
	Undefined156,
	Undefined157,
	Undefined158,
	Undefined159,
	Undefined160,
	Undefined161,
	Undefined162,
	Undefined163,
	Undefined164,
	Undefined165,
	Undefined166,
	Undefined167,
	Undefined168,
	Undefined169,
	Undefined170,
	Undefined171,
	Undefined172,
	Undefined173,
	Undefined174,
	Undefined175,
	Undefined176,
	Undefined177,
	Undefined178,
	Undefined179,
	Undefined180,
	Undefined181,
	Undefined182,
	Undefined183,
	Undefined184,
	Undefined185,
	Undefined186,
	Undefined187,
	Undefined188,
	Undefined189,
	Undefined190,
	Undefined191,
	Undefined192,
	Undefined193,
	Undefined194,
	Undefined195,
	Undefined196,
	Undefined197,
	Undefined198,
	Undefined199,
	Undefined200,
	Undefined201,
	Undefined202,
	Undefined203,
	Undefined204,
	Undefined205,
	Undefined206,
	Undefined207,
	Undefined208,
	Undefined209,
	Undefined210,
	Undefined211,
	Undefined212,
	Undefined213,
	Undefined214,
	Undefined215,
	Undefined216,
	Undefined217,
	Undefined218,
	Undefined219,
	Undefined220,
	Undefined221,
	Undefined222,
	Undefined223,
	Undefined224,
	Undefined225,
	Undefined226,
	Undefined227,
	Undefined228,
	Undefined229,
	Undefined230,
	Undefined231,
	Undefined232,
	Undefined233,
	Undefined234,
	Undefined235,
	Undefined236,
	Undefined237,
	Undefined238,
	Undefined239,
	Undefined240,
	Undefined241,
	Undefined242,
	Undefined243,
	Undefined244,
	Undefined245,
	Undefined246,
	Undefined247,
	Undefined248,
	Undefined249,
	Undefined250,
	Undefined251,
	Undefined252,
	Undefined253,
	Undefined254,
	Undefined255
}

public enum PGNTermination : byte
{
	Unknown,
	Draw,
	WhiteWin,
	BlackWin,
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