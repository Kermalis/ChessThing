using System;
using System.IO;
using System.Text;

namespace Kermalis.ChessThing.CMD;

internal static class Program
{
	private static void Main()
	{
		int which = 5;

		switch (which)
		{
			case 0: Test0(); break;
			case 1: Test1(); break;
			case 2: Test2(); break;
			case 3: Test3(); break;
			case 4: Test4(); break;
			case 5: Test5(); break;
		}

		Console.ReadKey();
	}

	private static void Test0()
	{
		var board = new Board();

		board[new Square(Col.CG, Row.R2)] = TeamedPiece.W_Queen;
		board[new Square(Col.CH, Row.R2)] = TeamedPiece.B_Queen;

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
		RegularChess gmInfo = RegularChess.Instance;
		var board = new Board();

		// Valid:
		//string testFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w QKqk - 0 1";
		//string testFEN = "6k1/pb1r3p/1p4p1/2p3q1/2Pp4/1P1Nnr2/P2R1QPP/4RBK1 w - - 0 34";
		//string testFEN = "8/4r2k/2p4P/1pPp1pP1/pP1PrP2/P2Kp3/4B3/5R2 w - - 59 170";
		//string testFEN = "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1";
		string testFEN = "r1bq1rk1/pppn1ppQ/4p1B1/3pP3/8/2P1P1P1/PP1N1PP1/R3K3 b Q - 3 17";

		FEN.Parse(gmInfo, board, testFEN);

		var sb = new StringBuilder();

		CUtils.AppendBoardASCII(sb, board, true);

		Console.WriteLine(sb.ToString());

		string doneFEN = FEN.ToFEN(gmInfo, board);
		Console.WriteLine($"\"{doneFEN}\"");
	}
	private static void Test3()
	{
		var gmInfo = Chess960.CreateUninitialized();
		var board = new Board();

		// Valid:
		string testFEN = "bnnrkbrq/pppppppp/8/8/8/8/PPPPPPPP/BNNRKBRQ w GDgd - 0 1";

		FEN.Parse(gmInfo, board, testFEN);

		var sb = new StringBuilder();

		CUtils.AppendBoardASCII(sb, board, true);

		sb.AppendLine();
		sb.AppendLine($"960 rooks: {gmInfo.QueensideRook.ColumnChar()}, {gmInfo.KingsideRook.ColumnChar()}");

		Console.WriteLine(sb.ToString());

		string doneFEN = FEN.ToFEN(gmInfo, board);
		Console.WriteLine($"\"{doneFEN}\"");
	}
	private static void Test4()
	{
		var gmInfo = Chess960.Create(Col.CA, Col.CH);
		var board = new Board();

		// Valid:
		string testFEN = "rb2kn1r/p3p1p1/1p2bpq1/2P4p/4pP1P/2P2N1R/PPBQP1P1/R2K2B1 w A - 1 13";

		FEN.Parse(gmInfo, board, testFEN);

		var sb = new StringBuilder();

		CUtils.AppendBoardASCII(sb, board, true);

		sb.AppendLine();
		sb.AppendLine($"960 rooks: {gmInfo.QueensideRook.ColumnChar()}, {gmInfo.KingsideRook.ColumnChar()}");

		Console.WriteLine(sb.ToString());

		string doneFEN = FEN.ToFEN(gmInfo, board);
		Console.WriteLine($"\"{doneFEN}\"");
	}
	private static void Test5()
	{
		//string pgnTextStr = File.ReadAllText("../../../../PGN Examples/Hikaru Bot 1.pgn");
		string pgnTextStr = File.ReadAllText("../../../../PGN Examples/Kermalis London 2 (Verbose).pgn");

		ReadOnlySpan<char> pgnText = pgnTextStr;
		pgnText = pgnText.TrimEnd();
		var pgn = PGN.Parse(pgnText);
		;
	}
}