using Godot;
using System;

namespace GA.GArkanoid.UI
{
	public partial class MainMenuControl : Control
	{
		[Export] private Button _newGameButton = null;
		[Export] private Button _settingsGameButton = null;
		[Export] private Button _quitGameButton = null;

		public override void _EnterTree()
		{
			// TODO: Subscribe to the rest of the buttons Pressed signals as well.
			_newGameButton.Pressed += OnNewGamePressed;
			_settingsGameButton.Pressed += OnSettingsPressed;
		}

		public override void _ExitTree()
		{
			_newGameButton.Pressed -= OnNewGamePressed;
			_settingsGameButton.Pressed -= OnSettingsPressed;
		}

		private void OnNewGamePressed()
		{
			if (!GameManager.Instance.ChangeState(GameStateType.InGame))
			{
				GD.PushError("Loading a new game failed!");
			}
		}

		public void OnSettingsPressed()
		{
			GameManager.Instance.ChangeState(GameStateType.Settings);
		}
	}
}