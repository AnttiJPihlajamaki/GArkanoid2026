using Godot;
using System;

namespace GA.GArkanoid.UI
{
	public partial class SettingsControl : Control
	{
		public void Close()
		{
			GameManager.Instance.ActivatePreviousState();
		}

		public void Save()
		{
			// TODO: Implement saving Settings
		}
	}
}
