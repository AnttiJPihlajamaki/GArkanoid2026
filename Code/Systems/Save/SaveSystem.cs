using Godot;
using Godot.Collections;

namespace GA.GArkanoid
{
	public class SaveSystem
	{
		public const string SaveFileExtension = ".json";
		public const string SaveDirectory = "user://saves/";
		public const string QuickSaveSlot = "QuickSave"; // Filename for the QuickSave file.

		// TODO: Is this the best way to store currently loaded data?
		public Dictionary<string, Variant> LatestLoadData
		{
			get;
			private set;
		} = null;

		public bool QuickSave(Dictionary<string, Variant> saveData)
		{
			return Save(QuickSaveSlot, saveData);
		}

		public Dictionary<string, Variant> QuickLoad()
		{
			return Load(QuickSaveSlot);
		}

		/// <summary>
		/// Saves the game state to a disk.
		/// </summary>
		/// <param name="saveSlot">A file name for the save file.</param>
		/// <param name="saveData">The data to be saved.</param>
		public bool Save(string saveSlot, Dictionary<string, Variant> saveData)
		{
			string saveDirectoryPath = ProjectSettings.GlobalizePath(SaveDirectory);
			if (!DirAccess.DirExistsAbsolute(saveDirectoryPath))
			{
				DirAccess.MakeDirRecursiveAbsolute(saveDirectoryPath);
			}

			string saveFilePath = SaveDirectory + saveSlot + SaveFileExtension;
			// Serialize save data
			string jsonData = Json.Stringify(saveData);

			using (FileAccess file = FileAccess.Open(saveFilePath, FileAccess.ModeFlags.Write))
			{
				if (file == null)
				{
					Error error = FileAccess.GetOpenError();
					GD.PushError($"Failed to open save file for writing: {saveFilePath}, Error: {error}");
					return false;
				}

				file.StoreString(jsonData);
				return true;
			}
		}

		public Dictionary<string, Variant> Load(string saveSlotName)
		{
			string saveFilePath = SaveDirectory + saveSlotName + SaveFileExtension;

			if (!FileAccess.FileExists(saveFilePath))
			{
				GD.PushError($"Save file not found: {saveFilePath}");
				return null;
			}

			using (FileAccess saveFile = FileAccess.Open(saveFilePath, FileAccess.ModeFlags.Read))
			{
				string serializedData = saveFile.GetAsText();
				LatestLoadData = (Dictionary<string, Variant>)Json.ParseString(serializedData);
				return LatestLoadData;
			}
		}

		public void LoadingDone()
		{
			LatestLoadData = null;
		}
	}
}