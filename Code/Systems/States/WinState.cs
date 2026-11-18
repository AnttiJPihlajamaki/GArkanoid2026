namespace GA.GArkanoid.State
{
	public class WinState : GameStateBase
	{
		public override GameStateType StateType => GameStateType.Win;

		public override string ScenePath => "res://Scenes/Menus/Victory.tscn";

		public WinState()
		{
			AddTargetState(GameStateType.MainMenu);
			AddTargetState(GameStateType.InGame);
		}
	}
}