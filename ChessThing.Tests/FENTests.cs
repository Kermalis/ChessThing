using System;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Kermalis.ChessThing.Tests;

public sealed class TestFEN_PlacementOnly
{
	private readonly ITestOutputHelper _output;
	public TestFEN_PlacementOnly(ITestOutputHelper output)
	{
		_output = output;
	}

	[Theory]
	[InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR", true)]
	[InlineData("bnnrkbrq/pppppppp/8/8/8/8/PPPPPPPP/BNNRKBRQ", true)]
	[InlineData("6k1/pb1r3p/1p4p1/2p3q1/2Pp4/1P1Nnr2/P2R1QPP/4RBK1", true)]
	[InlineData("rnbqkbnr/pppppppp/44/8/8/8/PPPPPPPP/RNBQKBNR", false)] // Multiple empty specifiers
	[InlineData("rnbqkbnr/ppppppp2/8/8/8/8/PPPPPPPP/RNBQKBNR", false)] // Too many empty squares
	[InlineData("Anbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR", false)] // Invalid piece ('A')
	[InlineData("rnbqkbnr/pppppppp/7/8/8/8/PPPPPPPP/RNBQKBNR", false)] // Invalid piece ('/')
	[InlineData("rnbqkbnrpppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR", false)] // Invalid row terminator ('p')
	[InlineData("rnbqkbnr&pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR", false)] // Invalid row terminator ('&')
	[InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKB1", false)] // Too few chars
	[InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBN", false)] // Too few chars
	public void Test(string testPlacement, bool valid)
	{
		var board = new Board();

		Span<TeamedPiece> pieces = stackalloc TeamedPiece[8 * 8];

		bool threw = false;
		try
		{
			FEN.ParsePlacementOnly(pieces, testPlacement);
		}
		catch (Exception ex)
		{
			_output.WriteLine(ex.ToString());

			threw = true;
		}
		Assert.NotEqual(valid, threw);

		if (valid)
		{
			board.SetPieces(pieces);

			var sb = new StringBuilder();

			CUtils.AppendBoardASCII(sb, board, true);

			_output.WriteLine(sb.ToString());
		}
	}
}