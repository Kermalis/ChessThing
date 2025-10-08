using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Kermalis.ChessThing;

public sealed class PGN
{
	public struct PGNMove
	{
		public Move Move;
		public NAG NAG;
		/// <summary>Comments may include whitespace and newlines.</summary>
		public string? Comment;
	}

	public readonly IReadOnlyDictionary<string, string> Tags;
	public readonly IReadOnlyList<PGNMove> Moves;
	public readonly PGNTermination Termination;

	private PGN(Dictionary<string, string> tags, List<PGNMove> moves, PGNTermination end)
	{
		Tags = tags;
		Moves = moves;
		Termination = end;
	}

	/// <summary><paramref name="chars"/> must not have any whitespace at the end.
	/// The "Seven Tag Roster" tags are not actually validated or required by this parser.</summary>
	public static PGN Parse(ReadOnlySpan<char> chars)
	{
		try
		{
			var tags = new Dictionary<string, string>();

			ParseTags(ref chars, tags);

			if (tags.Count == 0)
			{
				throw new InvalidDataException("Missing PGN tags.");
			}

			var moves = new List<PGNMove>();

			uint turnNum = 1;
			string turnNumStr = "1";
			bool whiteToMove = true;
			PGNTermination termination = PGNTermination.Unknown;

			while (true)
			{
				bool done = ParseTurn(ref chars, turnNumStr, whiteToMove, moves, ref termination);

				if (done)
				{
					break;
				}

				if (whiteToMove)
				{
					whiteToMove = false;
				}
				else
				{
					whiteToMove = true;
					turnNum++;
					turnNumStr = turnNum.ToString("D");
				}
			}

			if (moves.Count == 0)
			{
				throw new InvalidDataException("Missing PGN moves.");
			}

			return new PGN(tags, moves, termination);
		}
		catch (IndexOutOfRangeException)
		{
			throw new ArgumentException("Too few characters in PGN string.", nameof(chars));
		}
	}
	private static void ParseTags(ref ReadOnlySpan<char> chars, Dictionary<string, string> tags)
	{
		while (true)
		{
			int lineEndN = chars.IndexOf('\n');
			int lineEndRN = chars.IndexOf("\r\n", StringComparison.Ordinal);

			if (lineEndN == -1 && lineEndRN == -1)
			{
				throw new InvalidDataException("Missing newline while reading tags.");
			}

			if (lineEndRN != -1 && lineEndN != -1)
			{
				// We have /r/n or mixed newlines.
				// Find the nearest one and use that.
				if (lineEndRN < lineEndN)
				{
					lineEndN = -1;
				}
				else // lineEndN < lineEndRN
				{
					lineEndRN = -1;
				}
			}

			ReadOnlySpan<char> lineChars;
			if (lineEndRN == -1) // && lineEndN != -1
			{
				lineChars = chars.Slice(0, lineEndN);
				chars = chars.Slice(lineEndN + 1);
				if (lineEndN == 0)
				{
					break;
				}
			}
			else // lineEndRN != -1 && lineEndN == -1
			{
				lineChars = chars.Slice(0, lineEndRN);
				chars = chars.Slice(lineEndRN + 2);
				if (lineEndRN == 0)
				{
					break;
				}
			}

			// Read a tag
			if (lineChars[0] != '[')
			{
				throw new InvalidDataException($"PGN tag invalid start: '{lineChars[0]}'");
			}

			int spaceIndex = lineChars.IndexOf(" \"", StringComparison.Ordinal);
			if (spaceIndex == -1)
			{
				throw new InvalidDataException("Missing space with quotation mark in PGN tag.");
			}

			ReadOnlySpan<char> valueSpan = lineChars.Slice(spaceIndex + 2);
			int valueEndIndex = valueSpan.Length - 2;
			if (valueSpan[valueEndIndex] != '\"' || valueSpan[valueEndIndex + 1] != ']')
			{
				throw new InvalidDataException("PGN tag invalid end");
			}


			string key = new(lineChars.Slice(1, spaceIndex - 1));
			string value = new(valueSpan.Slice(0, valueEndIndex));

			tags.Add(key, value);
		}
	}
	private static bool ParseTurn(ref ReadOnlySpan<char> chars, ReadOnlySpan<char> turnNumStr, bool whiteToMove, List<PGNMove> moves, ref PGNTermination termination)
	{
		// Check for termination string

		if (chars.SequenceEqual("1/2-1/2"))
		{
			termination = PGNTermination.Draw;
			return true;
		}
		if (chars.SequenceEqual("1-0"))
		{
			termination = PGNTermination.WhiteWin;
			return true;
		}
		if (chars.SequenceEqual("0-1"))
		{
			termination = PGNTermination.BlackWin;
			return true;
		}

		// Parse turn number
		AbsorbTurnNumber(ref chars, turnNumStr, whiteToMove);

		PGNMove m;

		// Parse move
		m.Move = Move.ParseAlgebraicNotation(ref chars);
		AbsorbSpaceOrNewline(ref chars, true);

		// Parse move comment
		m.NAG = ParseNAG(ref chars);
		m.Comment = ParseComment(ref chars);
		moves.Add(m);

		return chars.Length == 0;
	}

	private static void AbsorbSpaceOrNewline(ref ReadOnlySpan<char> chars, bool required)
	{
		int i = 0;
		while (true)
		{
			int numIncrement;

			if (chars[i] is ' ' or '\n')
			{
				numIncrement = 1;
			}
			else if (i + 1 < chars.Length && chars[i] == '\r' && chars[i + 1] == '\n')
			{
				numIncrement = 2;
			}
			else
			{
				// Non-space non-newline character

				if (required && i == 0)
				{
					throw new InvalidDataException($"Invalid whitespace: '{chars[i]}'");
				}
				chars = chars.Slice(i);
				return;
			}

			i += numIncrement;
			if (i >= chars.Length)
			{
				chars = [];
				return;
			}
		}
	}

	private static void AbsorbTurnNumber(ref ReadOnlySpan<char> chars, ReadOnlySpan<char> turnNumStr, bool whiteToMove)
	{
		bool startsWithNum = chars.StartsWith(turnNumStr, StringComparison.Ordinal);

		if (whiteToMove)
		{
			// White turn counter must exist.

			if (!startsWithNum
				|| chars[turnNumStr.Length] != '.')
			{
				throw new InvalidDataException("Invalid turn count for white.");
			}

			chars = chars.Slice(turnNumStr.Length + 1);
			AbsorbSpaceOrNewline(ref chars, false);
		}
		else
		{
			// Black turn counter is optional.

			if (startsWithNum)
			{
				if (chars[turnNumStr.Length] != '.'
					|| chars[turnNumStr.Length + 1] != '.'
					|| chars[turnNumStr.Length + 2] != '.')
				{
					throw new InvalidDataException("Invalid turn count for black.");
				}

				chars = chars.Slice(turnNumStr.Length + 3);
				AbsorbSpaceOrNewline(ref chars, false);
			}
		}
	}
	private static NAG ParseNAG(ref ReadOnlySpan<char> chars)
	{
		if (chars[0] != '$')
		{
			return NAG.Null;
		}

		// NAG are at most 3 digits [0, 255]
		int i = 0;
		while (true)
		{
			char c = chars[1 + i];
			if (c is >= '0' and <= '9')
			{
				if (c == '0' && i == 0)
				{
					throw new InvalidDataException("Leading zeroes in NAG value.");
				}
				i++;
				if (i >= 3)
				{
					break;
				}
			}
			else
			{
				if (i == 0)
				{
					throw new InvalidDataException("Missing NAG value.");
				}
				break;
			}
		}

		ReadOnlySpan<char> valueSpan = chars.Slice(1, i);
		var nag = (NAG)byte.Parse(valueSpan, style: NumberStyles.None);

		if (nag == NAG.Null)
		{
			throw new InvalidDataException("Null NAG is not allowed.");
		}

		chars = chars.Slice(1 + i);
		AbsorbSpaceOrNewline(ref chars, true);

		return nag;
	}
	private static string? ParseComment(ref ReadOnlySpan<char> chars)
	{
		// Only handling `{}` comments for now, not `;` comments.

		if (chars[0] == '{')
		{
			int indexOfEnd = chars.IndexOf('}');
			if (indexOfEnd == -1)
			{
				throw new InvalidDataException("PGN move comment was incomplete.");
			}

			string comment = new(chars.Slice(1, indexOfEnd - 1));

			chars = chars.Slice(indexOfEnd + 1);
			AbsorbSpaceOrNewline(ref chars, true);
			return comment;
		}

		// No comment
		return null;
	}
}