using System;
using System.Text;

namespace Kermalis.ChessThing.CMD;

internal static class Program
{
	private static void Main()
	{
		var board = new Board();

		board[new Square(Row.R2, Col.CG)] = TeamedPiece.W_Queen;
		board[new Square(Row.R2, Col.CH)] = TeamedPiece.B_Queen;

		var sb = new StringBuilder();

		AppendBoardASCII(sb, board, true);

		sb.AppendLine();
		sb.AppendLine();
		sb.AppendLine();

		AppendBoardASCII(sb, board, false);

		Console.WriteLine(sb.ToString());

		Console.ReadKey();
	}

	private static void AppendBoardASCII(StringBuilder sb, Board board, bool blackOnTop)
	{
		void RowLine()
		{
			for (int d = 0; d < 18; d++)
			{
				sb.Append('—');
			}
			sb.AppendLine();
		}

		if (blackOnTop)
		{
			for (int y = 7; y >= 0; y--)
			{
				var row = (Row)y;

				// Print Row legend
				sb.Append(row.RowChar());
				sb.Append('|');

				for (int x = 0; x < 8; x++)
				{
					var col = (Col)x;

					TeamedPiece tp = board[new Square(row, col)];
					if (tp == TeamedPiece.None)
					{
						sb.Append(' ');
					}
					else
					{
						sb.Append(tp.PieceChar());
					}

					sb.Append('|');
				}

				sb.AppendLine();
				RowLine();
			}

			// Print Column legend
			sb.Append(" |");
			for (int x = 0; x < 8; x++)
			{
				var col = (Col)x;

				sb.Append(col.ColumnChar());
				sb.Append('|');
			}
		}
		else
		{
			for (int y = 0; y < 8; y++)
			{
				var row = (Row)y;

				// Print Row legend
				sb.Append(row.RowChar());
				sb.Append('|');

				for (int x = 7; x >= 0; x--)
				{
					var col = (Col)x;

					TeamedPiece tp = board[new Square(row, col)];
					if (tp == TeamedPiece.None)
					{
						sb.Append(' ');
					}
					else
					{
						sb.Append(tp.PieceChar());
					}

					sb.Append('|');
				}

				sb.AppendLine();
				RowLine();
			}

			// Print Column legend
			sb.Append(" |");
			for (int x = 7; x >= 0; x--)
			{
				var col = (Col)x;

				sb.Append(col.ColumnChar());
				sb.Append('|');
			}
		}
	}
}