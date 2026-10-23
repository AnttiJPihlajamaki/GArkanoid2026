using Godot;

namespace GA.GArkanoid.State
{
	public class InGameState : GameStateBase
	{
		public override GameStateType StateType => GameStateType.InGame;

		public override string ScenePath => "res://Scenes/Levels/Level.tscn";

		public InGameState() : base()
		{
			AddTargetState(GameStateType.Pause);
			AddTargetState(GameStateType.Win);
			AddTargetState(GameStateType.GameOver);
			AddTargetState(GameStateType.InGame); // For level completion
		}

		public override void OnExit(bool keepLoaded)
		{
			// Call parent class's implementation first.
			base.OnExit(keepLoaded);

			// Do the rest of the deactivation logic here!
		}
	}
}