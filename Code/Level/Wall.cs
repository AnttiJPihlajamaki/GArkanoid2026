using Godot;
using System;

namespace GA.GArkanoid
{
	public partial class Wall : StaticBody2D
	{
		[Export]
		public bool IsHazard { get; private set; } = false;
	}
}