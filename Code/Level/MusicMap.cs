using Godot;

namespace GA.GArkanoid
{
	[GlobalClass]
	public partial class MusicMap : Resource
	{
		[Export] public StateMusicMapping[] StateMappings { get; set; } = new StateMusicMapping[0];

		/// <summary>
		/// Gets the music track for a specific game state.
		/// </summary>
		/// <param name="stateType">The state to get music for.</param>
		/// <returns>The AudioStream for the state, or null if no music is mapped.</returns>
		public AudioStream GetMusicForState(GameStateType stateType)
		{
			foreach (var mapping in StateMappings)
			{
				if (mapping.StateType == stateType)
				{
					return mapping.MusicTrack;
				}
			}
			return null;
		}
	}
}