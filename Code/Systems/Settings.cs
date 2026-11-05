using Godot;

namespace GA.GArkanoid
{
	public class Settings
	{
		public const string SettingsFilePath = "user://settings.cfg";
		private SettingsData _data = null;

		public SettingsData Data
		{
			get => _data;
			set
			{
				_data = value;
				ApplySettings(_data);
			}
		}

		public Settings()
		{
			Data = Load();
		}

		public Settings(SettingsData data)
		{
			Data = data;
		}

		public static void ApplySettings(SettingsData data)
		{
			AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), data.MasterVolume);
			AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), data.MusicVolume);
			AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"), data.SFXVolume);

			DisplayServer.WindowSetSize(data.Resolution);

			DisplayServer.WindowMode windowMode = data.Fullscreen
				? DisplayServer.WindowMode.Fullscreen
				: DisplayServer.WindowMode.Windowed;
			DisplayServer.WindowSetMode(windowMode);
		}

		public void Save()
		{
			ConfigFile settingsFile = new ConfigFile();
			settingsFile.SetValue("Audio", "MasterVolume", Data.MasterVolume);
			settingsFile.SetValue("Audio", "MusicVolume", Data.MusicVolume);
			settingsFile.SetValue("Audio", "SFXVolume", Data.SFXVolume);

			settingsFile.SetValue("Video", "Resolution", Data.Resolution);
			settingsFile.SetValue("Video", "Fullscreen", Data.Fullscreen);

			settingsFile.Save(SettingsFilePath);
		}

		// TODO: Return loaded data.
		public SettingsData Load()
		{
			ConfigFile savedSettings = new ConfigFile();
			Error readError = savedSettings.Load(SettingsFilePath);
			SettingsData defaultData = SettingsData.Default;

			if (readError != Error.Ok)
			{
				return defaultData;
			}

			return new SettingsData()
			{
				MasterVolume = (float)savedSettings.GetValue("Audio", "MasterVolume", defaultData.MasterVolume),
				MusicVolume = (float)savedSettings.GetValue("Audio", "MusicVolume", defaultData.MusicVolume),
				SFXVolume = (float)savedSettings.GetValue("Audio", "SFXVolume", defaultData.SFXVolume),
				Resolution = (Vector2I)savedSettings.GetValue("Video", "Resolution", defaultData.Resolution),
				Fullscreen = (bool)savedSettings.GetValue("Video", "Fullscreen", defaultData.Fullscreen)
			};
		}
	}
}