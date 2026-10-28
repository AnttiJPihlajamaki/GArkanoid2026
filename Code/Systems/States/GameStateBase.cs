using System.Collections.Generic;
using Godot;

namespace GA.GArkanoid.State
{
	public abstract class GameStateBase
	{
		/// <summary>
		/// The type of the game state.
		/// </summary>
		public abstract GameStateType StateType { get; }

		/// <summary>
		/// The path to the scene file associated with this state.
		/// </summary>
		public abstract string ScenePath { get; }

		/// <summary>
		/// Should the associated scene be loaded additively or not.
		/// </summary>
		public virtual bool IsAdditive => false;

		/// <summary>
		/// The HashSet of states this state can transition to.
		/// </summary>
		private HashSet<GameStateType> _targetStates = new HashSet<GameStateType>();

		/// <summary>
		/// The reference to the scene resource associated with this state.
		/// </summary>
		private PackedScene _sceneResource = null;

		/// <summary>
		/// A reference to the scene associated with this state.
		/// </summary>
		private Node _scene = null;

		protected GameStateBase()
		{
			// Preload the scene file and cache the result.
			// Makes loading states a bit faster since they are already in memory.
			_sceneResource = GD.Load<PackedScene>(ScenePath);
			if (_sceneResource == null)
			{
				GD.PushError($"Scene resource at the path {ScenePath} is not available");
			}
		}

		protected bool AddTargetState(GameStateType gameStateType)
		{
			return _targetStates.Add(gameStateType);
		}

		protected bool RemoveTargetState(GameStateType gameStateType)
		{
			return _targetStates.Remove(gameStateType);
		}

		public bool CanTransisionTo(GameStateType targetStateType)
		{
			return _targetStates.Contains(targetStateType);
		}

		/// <summary>
		/// State activation logic. This should be run just after the state is activated.
		/// </summary>
		public virtual void OnEnter(bool forceLoad = false)
		{
			if (forceLoad || _scene == null)
			{
				// Scene is not loaded or reloading it is forced.
				if (_scene != null)
				{
					// TODO: Perhaps a bug???
					_scene.QueueFree();
				}

				_scene = _sceneResource.Instantiate();

				// TODO: Show what happens if scene addition is not delayed.
				GameManager.Instance.SceneTree.Root.CallDeferred(Node.MethodName.AddChild, _scene);

				// TODO: Threaded level loading:
				// https://docs.godotengine.org/en/stable/tutorials/io/background_loading.html
			}
		}

		/// <summary>
		/// State deactivation logic. This should be run just before the state is deactivated.
		/// </summary>
		public virtual void OnExit(bool keepLoaded)
		{
			if (!keepLoaded && _scene != null)
			{
				// This scene should be unloaded and it is still loaded. Unload it.
				_scene.QueueFree();
				_scene = null;
			}
		}
	}
}