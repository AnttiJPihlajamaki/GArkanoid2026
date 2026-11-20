using Godot;
using System;
using GA.Common;
using Godot.Collections;

namespace GA.GArkanoid
{
	[Tool]
	public partial class Block : StaticBody2D, ISaveable
	{
		[Signal] public delegate void BlockDestroyedEventHandler(Block block);
		[Export] private int _score = 10;

		private bool _isDestroyed = false;
		private CollisionShape2D _collider = null;

		public bool IsDestroyed
		{
			get => _isDestroyed;
			set
			{
				_isDestroyed = value;
				if (_isDestroyed)
				{
					EmitSignal(SignalName.BlockDestroyed, this);
					Disable();
				}
				else
				{
					Enable();
				}
			}
		}

		[Export]
		public string ID { get; set; } = System.Guid.NewGuid().ToString();

		public override void _Ready()
		{
			_collider = this.GetNode<CollisionShape2D>(recursive: true);
		}

		private void Enable()
		{
			Show();
			if (_collider != null)
			{
				_collider.SetDeferred("disabled", false);
			}
		}

		private void Disable()
		{
			Hide();
			if (_collider != null)
			{
				_collider.SetDeferred("disabled", true);
			}
		}

		public void Hit()
		{
			// Should be called whenever a ball hits the block.
			// For now all blocks gets destroyed when they are hit once.
			// TODO: Implement health functionality to enable multiple hit support.
			GameManager.Instance.AddScore(_score);
			IsDestroyed = true;
		}

		public bool Save(ref Dictionary<string, Variant> saveData)
		{
			if (saveData == null)
			{
				GD.PushWarning("Save data was null. This may be a bug!");
				saveData = new Dictionary<string, Variant>();
			}

			var blockData = new Dictionary<string, Variant>();
			blockData["IsDestroyed"] = IsDestroyed;

			saveData[ID] = blockData;

			return true;
		}

		public bool Load(Dictionary<string, Variant> saveData)
		{
			if (saveData == null || !saveData.ContainsKey(ID))
			{
				GD.PushError($"No save data found for block with ID: {ID}");
				return false;
			}

			var blockData = (Dictionary<string, Variant>)saveData[ID];
			if (blockData == null)
			{
				GD.PrintErr($"Invalid save data for block with ID: {ID}");
				return false;
			}

			if (blockData.ContainsKey("IsDestroyed"))
			{
				IsDestroyed = (bool)blockData["IsDestroyed"];
			}

			return true;
		}
	}
}