namespace GA.GArkanoid.State
{
	public class GameOverState : GameStateBase
	{
		public override GameStateType StateType => GameStateType.GameOver;
		public override string ScenePath => "res://Scenes/Menus/GameOver.tscn";
		public override bool IsAdditive => false;

		public GameOverState() : base()
		{
			AddTargetState(GameStateType.InGame);
			AddTargetState(GameStateType.MainMenu);
		}
	}
}