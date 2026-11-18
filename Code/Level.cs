using Godot;
using GA.Common;
using System.Collections.Generic;
using System;

namespace GA.GArkanoid
{
	public partial class Level : Node2D
	{
		#region Statics
		public static Level Current
		{
			get;
			private set;
		}
		#endregion Statics

		[Export] private Paddle _paddle = null;
		[Export] private Ball _ball = null;
		[Export] private int _wallWidth = 0;
		[Export] private LevelBlockMapping _levelBlockMapping = null;
		[Export] private int _levelId = 1; // TODO: Move this to GameManager!

		private int _blockCount = 0;

		public Paddle Paddle
		{
			get { return _paddle; }
		}

		public Ball Ball
		{
			get { return _ball; }
		}

		public int WallWidth { get { return _wallWidth; } }

		public Vector2 WindowSize
		{
			get
			{
				return GetViewport().GetVisibleRect().Size;
			}
		}

		public EffectPlayer EffectPlayer
		{
			get;
			private set;
		}

		override public void _EnterTree()
		{
			GameManager.Instance.LivesChanged += OnLivesChanged;
		}

		public override void _ExitTree()
		{
			GameManager.Instance.LivesChanged -= OnLivesChanged;
		}

		private void OnLivesChanged(int lives)
		{
			if (lives <= 0)
			{
				GameManager.Instance.ChangeState(GameStateType.GameOver);
			}
		}

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			Current = this;

			if (_paddle == null)
			{
				_paddle = this.GetNode<Paddle>();
				// Same as this
				//_paddle = NodeExtensions.GetNode<Paddle>(this);
			}

			if (_ball == null)
			{
				_ball = this.GetNode<Ball>();
			}

			EffectPlayer = this.GetNode<EffectPlayer>();
			if (EffectPlayer == null)
			{
				GD.PushError("No EffectPlayer found fron the scene.");
			}

			InstantiateLevel();
		}

		public override void _Input(InputEvent @event)
		{
			if (@event.IsActionPressed(Config.PauseActionName))
			{
				GameManager.Instance.ChangeState(GameStateType.Pause);
			}
		}

		private void InstantiateLevel()
		{
			if (_levelBlockMapping != null)
			{
				PackedScene levelBlocksScene = _levelBlockMapping.GetLevelBlocks(_levelId);
				if (levelBlocksScene != null)
				{
					Node2D levelBlocksInstance = levelBlocksScene.Instantiate() as Node2D;
					if (levelBlocksInstance != null)
					{
						AddChild(levelBlocksInstance);

						InitializeBlocks();
					}
					else
					{
						GD.PrintErr($"Failed to instantiate level blocks for level {_levelId}.");
					}
				}
				else
				{
					GD.PrintErr($"No level blocks found for level {_levelId}.");
				}
			}
			else
			{
				GD.PrintErr("LevelBlockMapping is not assigned.");
			}
		}

		private void InitializeBlocks()
		{
			IList<Block> blocks = this.GetNodes<Block>(recursive: true);
			_blockCount = blocks.Count;

			foreach (Block block in blocks)
			{
				block.BlockDestroyed += OnBlockDestroyed;
			}
		}

		private void OnBlockDestroyed(Block block)
		{
			block.BlockDestroyed -= OnBlockDestroyed;

			_blockCount--;
			if (_blockCount <= 0)
			{
				GameManager.Instance.ChangeState(GameStateType.Win);
			}
		}
	}
}