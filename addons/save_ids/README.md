# Save IDs

Build the C# project in Godot and enable **Save IDs** in Project Settings > Plugins.
Open a scene, choose Project > Tools > **Fix save IDs**, and save the scene.

The tool scans the open scene, including its root. It assigns GUIDs to missing IDs and
to every duplicate after the first occurrence. Unique IDs are preserved. Changes can
be undone, but IDs only persist once the scene is saved. Changing an existing ID can
make older saves incompatible; check which duplicate keeps its ID before saving.

An `ISaveable` node must be a `[Tool]` script and export its `ID` property for the
editor tool to find and update it. If a node belongs to a nested scene and cannot be
fixed in the open scene, open its owning scene and run the tool there. Avoid assigning
an ID to a reusable template: its instances would inherit the same ID. Run the tool
on the layout containing the actual instances instead.