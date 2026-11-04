using Godot;
using System;

namespace GA.GArkanoid.UI
{
	public partial class UIShowOnState : Control
	{
		[Export] private GameStateType _enabledState = GameStateType.None;

		public override void _Ready()
		{
			base._Ready();
			Control parent = GetParent<Control>();

			EnableParent(parent, _enabledState == GameManager.Instance.ActiveState.StateType);
		}

		private void EnableParent(Control parent, bool enable)
		{
			if (parent == null)
			{
				return;
			}

			parent.Visible = enable;
		}
	}
}