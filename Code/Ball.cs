using Godot;
using System;

namespace GA.GArkanoid
{
	public partial class Ball : CharacterBody2D
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

		public void Launch(Vector2 direction)
		{
			_direction = direction.Normalized();
			Velocity = _direction * _speed;
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

				if (collider != null)
				{
					doCollide = true;
					if (collider is Block block)
					{
						block.Hit();
					}
					else if (collider is Wall wall && wall.IsHazard)
					{
						// Ball hit the hazard wall, so reset the ball.
						_direction = Vector2.Zero;
						Velocity = Vector2.Zero;

						// Notify the GameManager that a life is lost.
						GameManager.Instance.RemoveLife();

						doCollide = false; // Don't reflect the ball's direction since it's reset.
					}
				}

				if (doCollide)
				{
					// Reflect the ball's direction based on the collision normal.
					_direction = _direction.Bounce(collisionInfo.GetNormal()).Normalized();
					Velocity = _direction * _speed;
				}
			}
		}
	}
}