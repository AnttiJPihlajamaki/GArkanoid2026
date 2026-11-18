using Godot;

namespace GA.GArkanoid
{
	[GlobalClass]
	public partial class StateMusicMapping : Resource
	{
		[Export] public GameStateType StateType { get; set; }
		[Export] public AudioStream MusicTrack { get; set; }
	}
}