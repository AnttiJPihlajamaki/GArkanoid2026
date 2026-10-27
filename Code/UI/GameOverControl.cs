using Godot;
using System;

namespace GA.GArkanoid.UI
{
	public partial class GameOverControl : Control
	{
		[Export] private Button _restartButton = null;
		[Export] private Button _backToMenuButton = null;

		public override void _EnterTree()
		{
			if (_restartButton != null)
			{
				_restartButton.Pressed += OnRestartButtonPressed;
			}

			if (_backToMenuButton != null)
			{
				_backToMenuButton.Pressed += OnBackToMenuPressed;
			}
		}

		public override void _ExitTree()
		{
			if (_restartButton != null)
			{
				_restartButton.Pressed -= OnRestartButtonPressed;
			}

			if (_backToMenuButton != null)
			{
				_backToMenuButton.Pressed -= OnBackToMenuPressed;
			}
		}

		public void OnRestartButtonPressed()
		{
			GameManager.Instance.ChangeState(GameStateType.InGame);
		}

		public void OnBackToMenuPressed()
		{
			GameManager.Instance.ChangeState(GameStateType.MainMenu);
		}
	}
}
