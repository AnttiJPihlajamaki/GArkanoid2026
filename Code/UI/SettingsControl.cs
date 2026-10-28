using Godot;
using System;

namespace GA.GArkanoid.UI
{
	public partial class SettingsControl : Control
	{
		[Export] private Button _saveButton = null;
		[Export] private Button _cancelButton = null;
		[Export] private Button _okButton = null;

		public override void _EnterTree()
		{
			_saveButton.Pressed += Save;
			_cancelButton.Pressed += Close;

			_okButton.Pressed += Save;
			_okButton.Pressed += Close;
		}

		public override void _ExitTree()
		{
			_saveButton.Pressed -= Save;
			_cancelButton.Pressed -= Close;

			_okButton.Pressed -= Save;
			_okButton.Pressed -= Close;
		}

		public void Close()
		{
			GameManager.Instance.ActivatePreviousState();
		}

		public void Save()
		{
			// TODO: Implement saving Settings
			GD.Print("Saving the settings.");
		}
	}
}
