using System;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Kermalis.ChessThing.Tests;

partial class ChessTests
{
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
			Assert.Equal(valid, threw);

			if (valid)
			{
				board.SetPieces(pieces);

				var sb = new StringBuilder();

				CUtils.AppendBoardASCII(sb, board, true);

				_output.WriteLine(sb.ToString());
			}
		}
	}
	/*public sealed class TestFEN_TODO
	{
		private readonly ITestOutputHelper _output;
		public TestFEN_TODO(ITestOutputHelper output)
		{
			_output = output;
		}

		[Theory]
		[InlineData(0_500_000_000, 7, 10)]
		public void Test(uint numRuns, int chanceNumerator, int chanceDenominator)
		{
			var rand = new KRand();
			uint[] buckets = new uint[2];

			for (uint i = 0; i < numRuns; ++i)
			{
				int idx = rand.NextBoolean(chanceNumerator, chanceDenominator) ? 1 : 0;
				buckets[idx]++;
			}

			decimal r = numRuns;
			for (int i = 0; i < buckets.Length; ++i)
			{
				uint num = buckets[i];

				_output.WriteLine(string.Format("Bucket {0} | {1:N0} ({2:P2})",
					i != 0, num, num / r));
			}
		}
	}*/
}