namespace GA.GArkanoid
{
	public interface ISaveable
	{
		/// <summary>
		/// The ID that identifies the saved object uniquely.
		/// </summary>
		string ID { get; set; }

		// TODO: Design what kind of parameter these methods should accept.
		bool Save();
		bool Load();
	}
}