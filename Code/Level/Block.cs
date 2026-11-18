using Godot;
using System;
using GA.Common;

namespace GA.GArkanoid
{
	[Tool]
	public partial class Block : StaticBody2D, ISaveable
	{
		[Signal] public delegate void BlockDestroyedEventHandler(Block block);
		[Export] private int _score = 10;

		[Export]


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

		public bool Save()
		{
			throw new NotImplementedException();
		}

		public bool Load()
		{
			throw new NotImplementedException();
		}
	}
}