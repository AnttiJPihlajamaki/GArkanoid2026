namespace GA.GArkanoid.State
{
	public class SettingsState : GameStateBase
	{
		public override GameStateType StateType => GameStateType.Settings;

		public override string ScenePath => "res://Scenes/Menus/Settings.tscn";

		public override bool IsAdditive => true;

		public SettingsState() : base()
		{
			AddTargetState(GameStateType.MainMenu);
		}
	}
}