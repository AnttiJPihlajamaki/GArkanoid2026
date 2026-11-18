namespace GA.GArkanoid
{
	// TODO: Create a resource script for all of the configuration data.
	public static class Config
	{
		public const float MaxSpeed = 500;
		public const float MinSpeed = 0;
		public const int MaxLives = 99;
		public const int InitialLives = 3;

		public const string PauseActionName = "Pause";
		public const string MusicMapPath = "res://Data/MusicMap.tres";
		public const string QuickSaveAction = "QuickSave";
		public const string QuickLoadAction = "QuickLoad";
	}
}