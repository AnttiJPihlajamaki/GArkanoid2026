using System;
using Godot;

namespace GA.GArkanoid
{
	public struct Resolution
	{
		public int Height;
		public int Width;
		public int Multiplyer;

		public override string ToString()
		{
			return $"{Width}x{Height}";
		}

		// Either explicit or implicit can be implemented at the same time. Not both.
		public static explicit operator Vector2I(Resolution resolution)
		{
			return new Vector2I(resolution.Width, resolution.Height);
		}

		// public static implicit operator Vector2I(Resolution resolution)
		// {
		// 	return new Vector2I(resolution.Width, resolution.Height);
		// }

		public static bool operator ==(Resolution a, Resolution b)
		{
			return a.Width == b.Width && a.Height == b.Height;
		}

		public static bool operator !=(Resolution a, Resolution b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj is Resolution resolution)
			{
				return this == resolution;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Width, Height);
		}
	}
}