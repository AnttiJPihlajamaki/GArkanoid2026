using Godot;
using Godot.Collections;
using System;

namespace GA.GArkanoid
{
	[Tool]
	public partial class Paddle : CharacterBody2D, ISaveable
	{
		public enum ControlMode
		{
			Keyboard,
			Mouse
		}

		[Export] private float _speed = 300.0f;
		[Export] private Sprite2D _sprite;
		[Export] private ControlMode _controlMode = ControlMode.Keyboard;
		[Export] private Node2D _ballStartPoint;
		[Export] private Vector2 _launchDirection = new Vector2(1, -1).Normalized();

		private float _horizontalInput = 0.0f;
		private float _mouseTargetX = 0.0f;

		public Vector2 Size
		{
			get
			{
				Vector2 size = _sprite.GetRect().Size;
				return size * _sprite.Scale;
			}
		}

		public Ball Ball
		{
			get { return Level.Current.Ball; }
		}

		[Export]
		public string ID { get; set; } = null;

		public override void _Process(double delta)
		{
			if (_controlMode == ControlMode.Keyboard)
			{
				_horizontalInput = Input.GetAxis("MoveLeft", "MoveRight");
			}
			else if (_controlMode == ControlMode.Mouse)
			{
				_mouseTargetX = GetGlobalMousePosition().X;
			}

			if (Input.IsActionJustPressed("LaunchBall"))
			{
				if (Ball != null && !Ball.IsLaunched)
				{
					Ball.Launch(_launchDirection);
				}
			}

			if (Ball != null && !Ball.IsLaunched)
			{
				// Ball is not launched yet, so keep it on the paddle.
				Vector2 ballPosition = _ballStartPoint.GlobalPosition;
				Ball.GlobalPosition = ballPosition;
			}
		}

		public override void _PhysicsProcess(double delta)
		{
			float deltaTime = (float)delta;
			Vector2 velocity = Vector2.Zero;

			if (_controlMode == ControlMode.Mouse)
			{
				// Match the distance to the cursor exactly instead of moving at a fixed speed, so the paddle stops instead of oscillating around the cursor.
				float distance = _mouseTargetX - Position.X;
				velocity.X = Mathf.Clamp(distance / deltaTime, -_speed, _speed);
			}
			else
			{
				// "Consume" the horizontal input and move the paddle accordingly.
				float horizontaInput = _horizontalInput;
				_horizontalInput = 0.0f;

				if (!Mathf.IsZeroApprox(horizontaInput))
				{
					velocity.X = horizontaInput * _speed;
				}
				else
				{
					velocity.X = Mathf.MoveToward(Velocity.X, 0, _speed);
				}
			}

			Velocity = velocity;
			MoveAndCollide(velocity * deltaTime);
			ClampToScreen();
		}


		private void ClampToScreen()
		{
			// Let's clamp the paddle to the screen bounds.
			// This solution assumes that the zero coortinate is at the top left of the screen.
			Vector2 position = Position;
			Vector2 windowSize = Level.Current.WindowSize;
			float minX = 0;
			float maxX = windowSize.X;
			float minPaddleX = minX + Size.X / 2 + Level.Current.WallWidth;
			float maxPaddleX = maxX - Size.X / 2 - Level.Current.WallWidth;
			position.X = Mathf.Clamp(position.X, minPaddleX, maxPaddleX);
			Position = position;
		}

		public bool Save(ref Dictionary<string, Variant> saveData)
		{
			if (saveData == null)
			{
				GD.PushWarning("Save data was null. This may be a bug!");
				saveData = new Dictionary<string, Variant>();
			}

			var paddleData = new Dictionary<string, Variant>();
			paddleData["PositionX"] = GlobalPosition.X;
			paddleData["PositionY"] = GlobalPosition.Y;
			paddleData["VelocityX"] = Velocity.X;
			paddleData["VelocityY"] = Velocity.Y;

			saveData[ID] = paddleData;
			return true;
		}

		public bool Load(Dictionary<string, Variant> saveData)
		{
			if (saveData == null || !saveData.ContainsKey(ID))
			{
				GD.PushError($"No save data found for the Paddle with ID: {ID}");
				return false;
			}

			var paddleData = (Dictionary<string, Variant>)saveData[ID];
			if (paddleData == null)
			{
				GD.PrintErr($"Invalid save data for the Paddle with ID: {ID}");
				return false;
			}

			bool success = paddleData.ContainsKey("PositionX")
				&& paddleData.ContainsKey("PositionY")
				&& paddleData.ContainsKey("VelocityX")
				&& paddleData.ContainsKey("VelocityY");

			if (success)
			{
				float x = (float)paddleData["PositionX"];
				float y = (float)paddleData["PositionY"];
				GlobalPosition = new Vector2(x, y);

				float velocityX = (float)paddleData["VelocityX"];
				float velocityY = (float)paddleData["VelocityY"];
				Velocity = new Vector2(velocityX, velocityY);
			}

			return success;
		}
	}
}