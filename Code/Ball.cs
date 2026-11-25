using Godot;
using Godot.Collections;
using System;

namespace GA.GArkanoid
{
	public partial class Ball : CharacterBody2D, ISaveable
	{
		[Export] private float _speed = 10;
		[Export] private Vector2 _direction = Vector2.Zero;

		public float Speed
		{
			get { return _speed; }
		}

		public bool IsLaunched
		{
			get { return !_direction.IsZeroApprox(); }
		}

		[Export]
		public string ID { get; set; } = null;

		public void Launch(Vector2 direction)
		{
			_direction = direction.Normalized();
			Velocity = _direction * _speed;
		}

		public bool Load(Dictionary<string, Variant> saveData)
		{
			if (saveData == null || !saveData.ContainsKey(ID))
			{
				GD.PushError($"No save data found for the Ball with ID: {ID}");
				return false;
			}

			var ballData = (Dictionary<string, Variant>)saveData[ID];
			if (ballData == null)
			{
				GD.PrintErr($"Invalid save data for the Ball with ID: {ID}");
				return false;
			}

			bool success = ballData.ContainsKey("PositionX")
				&& ballData.ContainsKey("PositionY")
				&& ballData.ContainsKey("VelocityX")
				&& ballData.ContainsKey("VelocityY");

			if (success)
			{
				float x = (float)ballData["PositionX"];
				float y = (float)ballData["PositionY"];
				GlobalPosition = new Vector2(x, y);

				float velocityX = (float)ballData["VelocityX"];
				float velocityY = (float)ballData["VelocityY"];
				Velocity = new Vector2(velocityX, velocityY);

				_direction = Velocity.Normalized();
			}

			return success;
		}

		public bool Save(ref Dictionary<string, Variant> saveData)
		{
			if (saveData == null)
			{
				GD.PushWarning("Save data was null. This may be a bug!");
				saveData = new Dictionary<string, Variant>();
			}

			var ballData = new Dictionary<string, Variant>();
			ballData["PositionX"] = GlobalPosition.X;
			ballData["PositionY"] = GlobalPosition.Y;
			ballData["VelocityX"] = Velocity.X;
			ballData["VelocityY"] = Velocity.Y;

			saveData[ID] = ballData;
			return true;
		}

		/// <summary>
		/// If physics engine is used, physics calculatons should be done here.
		/// </summary>
		public override void _PhysicsProcess(double delta)
		{
			if (_direction.IsZeroApprox())
			{
				// Ball is not launched yet, so don't move it.
				return;
			}

			float deltaTime = (float)delta;

			var collisionInfo = MoveAndCollide(Velocity * deltaTime);
			if (collisionInfo != null)
			{
				bool doCollide = false;
				GodotObject collider = collisionInfo.GetCollider();

				EffectType effect = EffectType.None;

				if (collider != null)
				{
					effect = EffectType.Bounce;

					doCollide = true;
					if (collider is Block block)
					{
						effect = EffectType.Hit;
						block.Hit();
					}
					else if (collider is Wall wall && wall.IsHazard)
					{
						// Ball hit the hazard wall, so reset the ball.
						effect = EffectType.Death;
						ResetBall();

						// Notify the GameManager that a life is lost.
						GameManager.Instance.RemoveLife();

						doCollide = false; // Don't reflect the ball's direction since it's reset.
					}
				}

				if (effect != EffectType.None)
				{
					Level.Current.EffectPlayer.PlayEffect(effect, GlobalPosition);
				}

				if (doCollide)
				{
					// Reflect the ball's direction based on the collision normal.
					_direction = _direction.Bounce(collisionInfo.GetNormal()).Normalized();
					Velocity = _direction * _speed;
				}
			}
		}

		public void ResetBall()
		{
			_direction = Vector2.Zero;
			Velocity = Vector2.Zero;
		}
	}
}