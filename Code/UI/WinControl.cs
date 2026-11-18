using Godot;
using System;

namespace GA.GArkanoid.UI
{
	public partial class WinControl : Control
	{
		[Export] private Button _restartButton = null;
		[Export] private Button _backToMenuButton = null;
		[Export] private Label _winLabel = null;

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

			if (_winLabel != null)
			{
				Tween tween = CreateTween();
				tween.SetLoops();
				tween.TweenProperty(_winLabel, "theme_override_colors/font_color", new Color(0, 1, 0), 0.5f)
					.SetTrans(Tween.TransitionType.Sine)
					.SetEase(Tween.EaseType.InOut);
				tween.TweenProperty(_winLabel, "theme_override_colors/font_color", new Color(1, 1, 0), 0.5f)
					.SetTrans(Tween.TransitionType.Sine)
					.SetEase(Tween.EaseType.InOut);
				tween.Play();
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
