using Godot;
using System;

namespace GA.GArkanoid
{
	public partial class GameManager : Node
	{
		#region Singleton
		public static GameManager Instance
		{
			get;
			private set;
		}

		public GameManager()
		{
			GD.Print("Initializing Game Manager");
			if (Instance == null)
			{
				Instance = this;
			}
			else if (Instance != this)
			{
				// The one and only instance of this class already exist.
				QueueFree();
				return;
			}
		}
		#endregion

		[Signal]
		public delegate void ScoreChangedEventHandler(int score);

		[Signal]
		public delegate void LivesChangedEventHandler(int lives);

		private int _score = 0;
		private int _lives = 3;

		public int Score
		{
			get { return _score; }
			private set
			{
				// TODO: Validate the score value.
				_score = value;
				EmitSignal(SignalName.ScoreChanged, _score);
			}
		}

		public int Lives
		{
			get { return _lives; }
			private set
			{
				_lives = Mathf.Clamp(value, 0, Config.MaxLives);
				EmitSignal(SignalName.LivesChanged, _lives);
				GD.Print($"Lives changed to {_lives}");
			}
		}

		public override void _Ready()
		{
			GD.Print("Game Manager initialized!");
		}

		public void AddScore(int scoreToAdd)
		{
			if (scoreToAdd <= 0)
			{
				// scoreToAdd is not valid.
				return;
			}

			Score += scoreToAdd;

			// if (scoreAddedCallback != null)
			// {
			// 	scoreAddedCallback(scoreToAdd);
			// }
			// Same as above
			// scoreAddedCallback?.Invoke(scoreToAdd);
		}

		public void AddLife()
		{
			Lives++;
		}

		public void RemoveLife()
		{
			Lives--;
		}
	}
}