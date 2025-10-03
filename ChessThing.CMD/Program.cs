using System;
using System.Text;

namespace Kermalis.ChessThing.CMD;

internal static class Program
{
	private static void Main()
	{
		int which = 1;

		switch (which)
		{
			case 0: Test1(); break;
			case 1: Test2(); break;
		}

		Console.ReadKey();
	}

	private static void Test1()
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
	private static void Test2()
	{
		var board = new Board();

		// Valid:
		//string testPlacement = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
		//string testPlacement = "bnnrkbrq/pppppppp/8/8/8/8/PPPPPPPP/BNNRKBRQ";
		string testPlacement = "6k1/pb1r3p/1p4p1/2p3q1/2Pp4/1P1Nnr2/P2R1QPP/4RBK1";

		Span<TeamedPiece> pieces = stackalloc TeamedPiece[8 * 8];

		FEN.ParsePlacement(pieces, testPlacement);

		board.SetPieces(pieces);

		var sb = new StringBuilder();

		CUtils.AppendBoardASCII(sb, board, true);

		Console.WriteLine(sb.ToString());
	}
}