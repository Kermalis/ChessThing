using System;

namespace Kermalis.ChessThing;

public sealed class Chess960 : GameModeInfo
{
	public bool IsInitialized { get; private set; }
	public Col QueensideRook { get; private set; }
	public Col KingsideRook { get; private set; }

	private Chess960()
	{
		IsInitialized = false;
	}

	public static Chess960 Create(Col queensideRook, Col kingsideRook)
	{
		if (queensideRook >= Col.MAX)
		{
			throw new ArgumentOutOfRangeException(nameof(queensideRook), queensideRook, null);
		}
		if (kingsideRook >= Col.MAX)
		{
			throw new ArgumentOutOfRangeException(nameof(kingsideRook), kingsideRook, null);
		}

		var c960 = new Chess960();
		c960.Initialize(queensideRook, kingsideRook);
		return c960;
	}
	public static Chess960 CreateUninitialized()
	{
		return new Chess960();
	}

	internal void Initialize(Col queensideRook, Col kingsideRook)
	{
		QueensideRook = queensideRook;
		KingsideRook = kingsideRook;
		IsInitialized = true;
	}
}