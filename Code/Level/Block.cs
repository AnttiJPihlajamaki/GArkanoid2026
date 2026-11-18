using Godot;
using System;

namespace GA.GArkanoid
{
	public partial class Block : StaticBody2D
	{
		[Signal] public delegate void BlockDestroyedEventHandler(Block block);
		[Export] private int _score = 10;

		public void Hit()
		{
			// Should be called whenever a ball hits the block.
			// For now all blocks gets destroyed when they are hit once.
			// TODO: Implement health functionality to enable multiple hit support.
			GameManager.Instance.AddScore(_score);
			EmitSignal(SignalName.BlockDestroyed, this);
			QueueFree(); // Use this instead of Free().
		}
	}
}