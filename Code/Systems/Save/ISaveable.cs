using Godot;
using Godot.Collections;

namespace GA.GArkanoid
{
	public interface ISaveable
	{
		/// <summary>
		/// The ID that identifies the saved object uniquely.
		/// </summary>
		string ID { get; set; }

		// Ref: See https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/ref
		bool Save(ref Dictionary<string, Variant> saveData);
		bool Load(Dictionary<string, Variant> saveData);
	}
}