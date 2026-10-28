using GA.GArkanoid.State;
using Godot;
using System;
using System.Collections.Generic;

namespace GA.GArkanoid
{
	/// <summary>
	/// Contains game's data and acts as the main state machine.
	/// </summary>
	public partial class GameManager : Node
	{
		#region Singleton
		public static GameManager Instance
		{
			get;
			private set;
		}

		public GameManager()
		{
			GD.Print("Initializing Game Manager");
			if (Instance == null)
			{
				Instance = this;
			}
			else if (Instance != this)
			{
				// The one and only instance of this class already exist.
				QueueFree();
				return;
			}
		}
		#endregion

		[Signal]
		public delegate void ScoreChangedEventHandler(int score);

		[Signal]
		public delegate void LivesChangedEventHandler(int lives);

		private int _score = 0;
		private int _lives = 3;
		private SceneTree _sceneTree = null;

		// TODO: Use Dictionary instead.
		private List<GameStateBase> _gameStates = new List<GameStateBase>
		{
			new MainMenuState(),
			new InGameState(),
		};

		public Stack<GameStateBase> LoadedStates { get; private set; } = new Stack<GameStateBase>();

		public GameStateBase ActiveState
		{
			get
			{
				return LoadedStates.Count > 0
					? LoadedStates.Peek()
					: null;
			}
		}

		public int Score
		{
			get { return _score; }
			private set
			{
				// TODO: Validate the score value.
				_score = value;
				EmitSignal(SignalName.ScoreChanged, _score);
			}
		}

		public int Lives
		{
			get { return _lives; }
			private set
			{
				_lives = Mathf.Clamp(value, 0, Config.MaxLives);
				EmitSignal(SignalName.LivesChanged, _lives);
				GD.Print($"Lives changed to {_lives}");
			}
		}

		public SceneTree SceneTree
		{
			get
			{
				// Cache the SceneTree reference for performance.
				// Lazy initialization (means that the value is 
				// only computed when accessed for the first time).
				if (_sceneTree == null)
				{
					_sceneTree = GetTree();
				}

				return _sceneTree;
			}
		}

		public override void _Ready()
		{
			// HACK: Initialize the first state
			// TODO: Replace this with a proper implementation.

			// Override currently loaded scene.
			// TODO: Keep Godot's SceneTree up-to-date with currently active scene.
			SceneTree.CurrentScene.QueueFree();

			GameStateBase mainMenu = GetState(GameStateType.MainMenu);
			mainMenu.OnEnter();
			LoadedStates.Push(mainMenu);
		}

		/// <summary>
		/// Finds and returns the state matching with the stateType.
		/// </summary>
		/// <param name="stateType"></param>
		/// <returns>The state or null, if state is not found.</returns>
		private GameStateBase GetState(GameStateType stateType)
		{
			foreach (GameStateBase state in _gameStates)
			{
				if (state.StateType == stateType)
				{
					return state;
				}
			}

			return null;
		}

		/// <summary>
		/// Changes the current game state to a new state.
		/// NOTE: Don't use this to transition back to previous state. For that, use the ActivatePreviousState method.
		/// </summary>
		/// <param name="targetStateType">The new state to transition to.</param>
		/// <param name="force">Is state activation forced or not.</param>
		/// <returns>True if the state was changed successfully, false otherwise.</returns>
		public bool ChangeState(GameStateType targetStateType, bool force = false)
		{
			// Is the transition from the current state to the new state valid?
			if (ActiveState != null && !ActiveState.CanTransisionTo(targetStateType))
			{
				GD.PushError($"Invalid state transition from {ActiveState.StateType} to {targetStateType}");
				return false;
			}

			GameStateBase nextState = GetState(targetStateType);
			if (nextState == null)
			{
				// State not found.
				GD.PushError($"State {targetStateType} not found");
				return false;
			}

			if (nextState.IsAdditive)
			{
				// The new state is shown on top of the current one, which stays loaded.
				ActiveState?.OnExit(keepLoaded: true);
			}
			else
			{
				while (LoadedStates.Count > 0)
				{
					GameStateBase poppedState = LoadedStates.Pop();
					poppedState.OnExit(keepLoaded: false);
				}
			}

			LoadedStates.Push(nextState);
			ActiveState.OnEnter(force);

			return true;
		}

		/// <summary>
		/// Activates the previous game state.
		/// TODO: Refactor ChangeState to support this corner case and get rid of this method.
		/// </summary>
		/// <returns>True if the previous state was activated successfully, false otherwise.</returns>
		public bool ActivatePreviousState()
		{
			if (LoadedStates.Count < 2)
			{
				GD.PushError("No previous state to activate");
				return false;
			}

			// Pop the current state from the stack.
			GameStateBase previousState = LoadedStates.Pop();
			previousState.OnExit(keepLoaded: false);

			// Activate the already loaded, new active state.
			ActiveState.OnEnter();

			return true;
		}

		public void AddScore(int scoreToAdd)
		{
			if (scoreToAdd <= 0)
			{
				// scoreToAdd is not valid.
				return;
			}

			Score += scoreToAdd;

			// if (scoreAddedCallback != null)
			// {
			// 	scoreAddedCallback(scoreToAdd);
			// }
			// Same as above
			// scoreAddedCallback?.Invoke(scoreToAdd);
		}

		public void AddLife()
		{
			Lives++;
		}

		public void RemoveLife()
		{
			Lives--;
		}
	}
}