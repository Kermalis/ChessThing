using System;
using System.Text;

namespace Kermalis.ChessThing.CMD;

internal static class Program
{
	private static void Main()
	{
		int which = 2;

		switch (which)
		{
			case 0: Test0(); break;
			case 1: Test1(); break;
			case 2: Test2(); break;
		}

		Console.ReadKey();
	}

	private static void Test0()
	{
		var board = new Board();

		board[new Square(Row.R2, Col.CG)] = TeamedPiece.W_Queen;
		board[new Square(Row.R2, Col.CH)] = TeamedPiece.B_Queen;

		var sb = new StringBuilder();

		CUtils.AppendBoardASCII(sb, board, true);

		sb.AppendLine();
		sb.AppendLine();
		sb.AppendLine();

		CUtils.AppendBoardASCII(sb, board, false);

		Console.WriteLine(sb.ToString());
	}
	private static void Test1()
	{
		var board = new Board();

		// Valid:
		//string testPlacement = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
		//string testPlacement = "bnnrkbrq/pppppppp/8/8/8/8/PPPPPPPP/BNNRKBRQ";
		string testPlacement = "6k1/pb1r3p/1p4p1/2p3q1/2Pp4/1P1Nnr2/P2R1QPP/4RBK1";

		Span<TeamedPiece> pieces = stackalloc TeamedPiece[8 * 8];

		FEN.ParsePlacementOnly(pieces, testPlacement);

		board.SetPieces(pieces);

		var sb = new StringBuilder();

		CUtils.AppendBoardASCII(sb, board, true);

		Console.WriteLine(sb.ToString());
	}
	private static void Test2()
	{
		var board = new Board();

		// Valid:
		//string testFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w QKqk - 0 1";
		//string testFEN = "6k1/pb1r3p/1p4p1/2p3q1/2Pp4/1P1Nnr2/P2R1QPP/4RBK1 w - - 0 34";
		//string testFEN = "8/4r2k/2p4P/1pPp1pP1/pP1PrP2/P2Kp3/4B3/5R2 w - - 59 170";
		//string testFEN = "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1";
		string testFEN = "r1bq1rk1/pppn1ppQ/4p1B1/3pP3/8/2P1P1P1/PP1N1PP1/R3K3 b Q - 3 17";

		FEN.Parse(board, testFEN);

		var sb = new StringBuilder();

		CUtils.AppendBoardASCII(sb, board, true);

		Console.WriteLine(sb.ToString());

		string doneFEN = FEN.ToFEN(board);
		Console.WriteLine($"\"{doneFEN}\"");
	}
}