#if TOOLS
using Godot;
using GA.GArkanoid;
using System;
using System.Collections.Generic;

[Tool]
public partial class SaveIdPlugin : EditorPlugin
{
	private const string MenuName = "Fix save IDs";

	public override void _EnterTree()
	{
		AddToolMenuItem(MenuName, Callable.From(FixIds));
	}

	public override void _ExitTree()
	{
		RemoveToolMenuItem(MenuName);
	}

	private void FixIds()
	{
		Node root = EditorInterface.Singleton.GetEditedSceneRoot();
		if (root == null)
		{
			GD.PushError("Open a scene before fixing save IDs.");
			return;
		}

		List<Node> nodes = new List<Node>();
		CollectSaveables(root, nodes);
		if (nodes.Count == 0)
		{
			GD.Print("No ISaveable nodes found. Their scripts must use [Tool] to run in the editor.");
			return;
		}
		HashSet<string> reserved = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (Node node in nodes)
		{
			if (!HasExportedId(node))
			{
				GD.PushError($"{node.GetPath()}: ISaveable must export its ID property.");
				return;
			}
			string id = ((ISaveable)node).ID;
			if (!string.IsNullOrWhiteSpace(id))
				reserved.Add(id);
		}

		HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		List<Node> toFix = new List<Node>();
		foreach (Node node in nodes)
		{
			string id = ((ISaveable)node).ID;
			if (!string.IsNullOrWhiteSpace(id) && seen.Add(id))
				continue;

			if (node != root && node.Owner != root)
			{
				GD.PushError($"{node.GetPath()}: open the owning scene to fix this save ID.");
				return;
			}
			toFix.Add(node);
		}

		if (toFix.Count == 0)
		{
			GD.Print($"All {nodes.Count} save IDs are unique.");
			return;
		}

		EditorUndoRedoManager undoRedo = GetUndoRedo();
		undoRedo.CreateAction("Fix save IDs");
		foreach (Node node in toFix)
		{
			string id;
			do
			{
				id = Guid.NewGuid().ToString();
			}
			while (!reserved.Add(id));
			undoRedo.AddDoProperty(node, "ID", id);
			undoRedo.AddUndoProperty(node, "ID", ((ISaveable)node).ID);
		}
		undoRedo.CommitAction();
		GD.Print($"Assigned {toFix.Count} save IDs. Save the scene to keep them.");
	}

	private static void CollectSaveables(Node node, List<Node> nodes)
	{
		if (node is ISaveable)
			nodes.Add(node);
		foreach (Node child in node.GetChildren())
			CollectSaveables(child, nodes);
	}

	private static bool HasExportedId(Node node)
	{
		foreach (Godot.Collections.Dictionary property in node.GetPropertyList())
		{
			PropertyUsageFlags usage = (PropertyUsageFlags)property["usage"].AsInt32();
			if (property["name"].AsString() == "ID" &&
				(usage & (PropertyUsageFlags.Storage | PropertyUsageFlags.Editor)) ==
					(PropertyUsageFlags.Storage | PropertyUsageFlags.Editor))
				return true;
		}
		return false;
	}
}
#endif
