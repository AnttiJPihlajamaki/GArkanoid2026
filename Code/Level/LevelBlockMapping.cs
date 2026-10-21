using Godot;
using Godot.Collections;

namespace GA.GArkanoid
{
	[GlobalClass]
	public partial class LevelBlockMapping : Resource
	{
		[Export]
		private Array<PackedScene> _blockMapping = new Array<PackedScene>();

		public PackedScene GetLevelBlocks(int levelId)
		{
			if (levelId < 1 || levelId > _blockMapping.Count)
			{
				GD.PrintErr($"Invalid levelId {levelId}. Valid range is 1 to {_blockMapping.Count}.");
				return null;
			}

			return _blockMapping[levelId - 1];
		}
	}
}