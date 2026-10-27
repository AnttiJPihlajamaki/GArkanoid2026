using System;
using Godot;

namespace GA.GArkanoid
{
	public partial class ScoreTest : Node
	{
		// In general, it is a good practise to subsribe to events in _EnterTree and unsubscribe 
		// from them in _ExitTree.
		public override void _EnterTree()
		{
			base._EnterTree();
			CallDeferred(MethodName.Subscibe);
		}

		private void Subscibe()
		{
			GameManager.Instance.ScoreChanged += OnScoreChanged;
		}

		public override void _ExitTree()
		{
			base._ExitTree();
			GameManager.Instance.ScoreChanged -= OnScoreChanged;
		}

		private void OnScoreChanged(int score)
		{
			GD.Print($"Current score: {score}");
		}
	}
}