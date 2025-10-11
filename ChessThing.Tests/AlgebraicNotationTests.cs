using System;
using Xunit;

namespace Kermalis.ChessThing.Tests;

public sealed class AlgebraicNotationTests
{
	[Theory]
	[InlineData("a3", 2, null, Col.CA, Row.R3, false, false, false)]
	[InlineData("h6", 2, null, Col.CH, Row.R6, false, false, false)]
	[InlineData("d4", 2, null, Col.CD, Row.R4, false, false, false)]
	[InlineData("a3+", 3, null, Col.CA, Row.R3, false, true, false)]
	[InlineData("a3#", 3, null, Col.CA, Row.R3, false, true, true)]
	[InlineData("exd4", 4, Col.CE, Col.CD, Row.R4, true, false, false)]
	[InlineData("bxa3+", 5, Col.CB, Col.CA, Row.R3, true, true, false)]
	[InlineData("bxa3#", 5, Col.CB, Col.CA, Row.R3, true, true, true)]
	public void SimplePawnMove(string str, int numChars, Col? fromCol, Col toCol, Row toRow, bool capture, bool check, bool checkmate)
	{
		ReadOnlySpan<char> chars = str;
		var m = Move.ParseAlgebraicNotation(ref chars);

		int numCharsRead = str.Length - chars.Length;
		Assert.Equal(numChars, numCharsRead);

		Assert.Equal(PieceKind.Pawn, m.Piece);
		Assert.Equal(fromCol, m.FromColHint);
		Assert.Null(m.FromRowHint);
		Assert.Equal(toCol, m.ToHint.Value.Col);
		Assert.Equal(toRow, m.ToHint.Value.Row);
		Assert.Equal(PieceKind.None, m.PromotedPiece);
		Assert.Equal(capture, m.CaptureHint);
		Assert.False(m.CastleQueensideHint);
		Assert.False(m.CastleKingsideHint);
		Assert.Equal(check, m.CheckHint);
		Assert.Equal(checkmate, m.CheckmateHint);
	}

	[Theory]
	[InlineData("a8=Q", 4, null, Col.CA, Row.R8, PieceKind.Queen, false, false, false)]
	[InlineData("h8=R+", 5, null, Col.CH, Row.R8, PieceKind.Rook, false, true, false)]
	[InlineData("b1=N#", 5, null, Col.CB, Row.R1, PieceKind.Knight, false, true, true)]
	[InlineData("c1=B", 4, null, Col.CC, Row.R1, PieceKind.Bishop, false, false, false)]
	[InlineData("bxa8=Q", 6, Col.CB, Col.CA, Row.R8, PieceKind.Queen, true, false, false)]
	[InlineData("gxh8=R+", 7, Col.CG, Col.CH, Row.R8, PieceKind.Rook, true, true, false)]
	[InlineData("cxb1=N#", 7, Col.CC, Col.CB, Row.R1, PieceKind.Knight, true, true, true)]
	public void PawnPromotionMove(string str, int numChars, Col? fromCol, Col toCol, Row toRow, PieceKind promotedPiece, bool capture, bool check, bool checkmate)
	{
		ReadOnlySpan<char> chars = str;
		var m = Move.ParseAlgebraicNotation(ref chars);

		int numCharsRead = str.Length - chars.Length;
		Assert.Equal(numChars, numCharsRead);

		Assert.Equal(PieceKind.Pawn, m.Piece);
		Assert.Equal(fromCol, m.FromColHint);
		Assert.Null(m.FromRowHint);
		Assert.Equal(toCol, m.ToHint.Value.Col);
		Assert.Equal(toRow, m.ToHint.Value.Row);
		Assert.Equal(promotedPiece, m.PromotedPiece);
		Assert.Equal(capture, m.CaptureHint);
		Assert.False(m.CastleQueensideHint);
		Assert.False(m.CastleKingsideHint);
		Assert.Equal(check, m.CheckHint);
		Assert.Equal(checkmate, m.CheckmateHint);
	}

	[Theory]
	[InlineData("Nf3", 3, null, null, Col.CF, Row.R3, PieceKind.Knight, false, false, false)]
	[InlineData("Rg4", 3, null, null, Col.CG, Row.R4, PieceKind.Rook, false, false, false)]
	[InlineData("Bc8+", 4, null, null, Col.CC, Row.R8, PieceKind.Bishop, false, true, false)]
	[InlineData("Qa7#", 4, null, null, Col.CA, Row.R7, PieceKind.Queen, false, true, true)]
	[InlineData("Kd1", 3, null, null, Col.CD, Row.R1, PieceKind.King, false, false, false)]
	[InlineData("Bxc8+", 5, null, null, Col.CC, Row.R8, PieceKind.Bishop, true, true, false)]
	[InlineData("Qxa7#", 5, null, null, Col.CA, Row.R7, PieceKind.Queen, true, true, true)]
	[InlineData("Kxd1", 4, null, null, Col.CD, Row.R1, PieceKind.King, true, false, false)]
	// These are semi-rare since they appear when multiple of the same piece spy the same square.
	// Typically Knights and Rooks.
	[InlineData("Nfd2", 4, Col.CF, null, Col.CD, Row.R2, PieceKind.Knight, false, false, false)]
	[InlineData("Nfd2+", 5, Col.CF, null, Col.CD, Row.R2, PieceKind.Knight, false, true, false)]
	[InlineData("Nfd2#", 5, Col.CF, null, Col.CD, Row.R2, PieceKind.Knight, false, true, true)]
	[InlineData("Nfxd2", 5, Col.CF, null, Col.CD, Row.R2, PieceKind.Knight, true, false, false)]
	[InlineData("Nfxd2+", 6, Col.CF, null, Col.CD, Row.R2, PieceKind.Knight, true, true, false)]
	[InlineData("Nfxd2#", 6, Col.CF, null, Col.CD, Row.R2, PieceKind.Knight, true, true, true)]
	[InlineData("N3d2", 4, null, Row.R3, Col.CD, Row.R2, PieceKind.Knight, false, false, false)]
	[InlineData("N3d2+", 5, null, Row.R3, Col.CD, Row.R2, PieceKind.Knight, false, true, false)]
	[InlineData("N3d2#", 5, null, Row.R3, Col.CD, Row.R2, PieceKind.Knight, false, true, true)]
	[InlineData("N3xd2", 5, null, Row.R3, Col.CD, Row.R2, PieceKind.Knight, true, false, false)]
	[InlineData("N3xd2+", 6, null, Row.R3, Col.CD, Row.R2, PieceKind.Knight, true, true, false)]
	[InlineData("N3xd2#", 6, null, Row.R3, Col.CD, Row.R2, PieceKind.Knight, true, true, true)]
	// These are extremely rare since they appear when promoting Pawns to "something" and 3 or more of that "something" spy the same square.
	// Typically Knights and Queens, but Bishops can do it if they're on the same color.
	[InlineData("Nf3d2", 5, Col.CF, Row.R3, Col.CD, Row.R2, PieceKind.Knight, false, false, false)]
	[InlineData("Nf3d2+", 6, Col.CF, Row.R3, Col.CD, Row.R2, PieceKind.Knight, false, true, false)]
	[InlineData("Nf3d2#", 6, Col.CF, Row.R3, Col.CD, Row.R2, PieceKind.Knight, false, true, true)]
	[InlineData("Nf3xd2", 6, Col.CF, Row.R3, Col.CD, Row.R2, PieceKind.Knight, true, false, false)]
	[InlineData("Nf3xd2+", 7, Col.CF, Row.R3, Col.CD, Row.R2, PieceKind.Knight, true, true, false)]
	[InlineData("Nf3xd2#", 7, Col.CF, Row.R3, Col.CD, Row.R2, PieceKind.Knight, true, true, true)]
	public void OtherPieceMove(string str, int numChars, Col? fromCol, Row? fromRow, Col toCol, Row toRow, PieceKind piece, bool capture, bool check, bool checkmate)
	{
		ReadOnlySpan<char> chars = str;
		var m = Move.ParseAlgebraicNotation(ref chars);

		int numCharsRead = str.Length - chars.Length;
		Assert.Equal(numChars, numCharsRead);

		Assert.Equal(piece, m.Piece);
		Assert.Equal(fromCol, m.FromColHint);
		Assert.Equal(fromRow, m.FromRowHint);
		Assert.Equal(toCol, m.ToHint.Value.Col);
		Assert.Equal(toRow, m.ToHint.Value.Row);
		Assert.Equal(PieceKind.None, m.PromotedPiece);
		Assert.Equal(capture, m.CaptureHint);
		Assert.False(m.CastleQueensideHint);
		Assert.False(m.CastleKingsideHint);
		Assert.Equal(check, m.CheckHint);
		Assert.Equal(checkmate, m.CheckmateHint);
	}

	[Theory]
	[InlineData("O-O", 3, true, false, false)]
	[InlineData("O-O+", 4, true, true, false)]
	[InlineData("O-O#", 4, true, true, true)]
	[InlineData("O-O-O", 5, false, false, false)]
	[InlineData("O-O-O+", 6, false, true, false)]
	[InlineData("O-O-O#", 6, false, true, true)]
	public void CastleMove(string str, int numChars, bool kingside, bool check, bool checkmate)
	{
		ReadOnlySpan<char> chars = str;
		var m = Move.ParseAlgebraicNotation(ref chars);

		int numCharsRead = str.Length - chars.Length;
		Assert.Equal(numChars, numCharsRead);

		Assert.Equal(PieceKind.King, m.Piece);
		Assert.Null(m.FromColHint);
		Assert.Null(m.FromRowHint);
		Assert.Null(m.ToHint);
		Assert.Equal(PieceKind.None, m.PromotedPiece);
		Assert.False(m.CaptureHint);
		Assert.NotEqual(kingside, m.CastleQueensideHint);
		Assert.Equal(kingside, m.CastleKingsideHint);
		Assert.Equal(check, m.CheckHint);
		Assert.Equal(checkmate, m.CheckmateHint);
	}
}