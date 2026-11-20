using Godot;
using GA.Common;
using System.Collections.Generic;
using System;

namespace GA.GArkanoid
{
	public partial class Level : Node2D, ISaveable
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
		private Dictionary<string, Block> _blocks = new Dictionary<string, Block>();

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

		[Export]
		public string ID { get; set; } = new System.Guid().ToString();

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

			if (GameManager.Instance.SaveSystem.LatestLoadData != null)
			{
				Load(GameManager.Instance.SaveSystem.LatestLoadData);
				GameManager.Instance.SaveSystem.LoadingDone();
			}
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
			_blocks.Clear();

			foreach (Block block in blocks)
			{
				// TODO: Make sure all signal are disconnected before loading.
				block.BlockDestroyed += OnBlockDestroyed;

				if (!string.IsNullOrEmpty(block.ID))
				{
					if (_blocks.ContainsKey(block.ID))
					{
						GD.PushError($"Duplicate block ID found: {block.ID}. Each block must have a unique ID.");
					}
					else
					{
						_blocks[block.ID] = block;
					}
				}
				else
				{
					GD.PushError($"Block at {block.GetPath()} does not have an ID assigned. Each block must have a unique ID.");
				}
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

		public bool Save(ref Godot.Collections.Dictionary<string, Variant> saveData)
		{
			if (saveData == null)
			{
				GD.PushWarning("Save data was null. This may be a bug!");
				saveData = new Godot.Collections.Dictionary<string, Variant>();
			}

			bool success = true;

			var levelData = new Godot.Collections.Dictionary<string, Variant>();

			// TODO: Save the level index

			// Save all blocks.
			foreach (KeyValuePair<string, Block> blockEntry in _blocks)
			{
				Block block = blockEntry.Value;
				if (!block.Save(ref levelData))
				{
					GD.PushError($"Failed to save data for block with ID: {block.ID}");
					success = false;
				}
			}

			// TODO: Save the ball.
			// TODO: Save the paddle.

			saveData["Level"] = levelData;

			return success;
		}

		public bool Load(Godot.Collections.Dictionary<string, Variant> saveData)
		{
			bool success = true;
			if (saveData == null || !saveData.ContainsKey("Level"))
			{
				GD.PushError("No level save data found.");
				return false;
			}

			var levelData = (Godot.Collections.Dictionary<string, Variant>)saveData["Level"];
			foreach (var blockEntry in _blocks)
			{
				Block block = blockEntry.Value;

				if (!block.Load(levelData))
				{
					GD.PushError($"Failed to load data for block with ID: {block.ID}");
					success = false;
				}
			}

			// TODO: Load ball and paddle.

			return success;
		}
	}
}