using Godot;
using System;
using System.Collections.Generic;

namespace GA.GArkanoid.UI
{
	public partial class SettingsControl : Control
	{
		[Export] private Button _saveButton = null;
		[Export] private Button _cancelButton = null;
		[Export] private Button _okButton = null;
		[Export] private Button _backToMenuButton = null;

		#region Audio
		[Export] private AudioControl _masterAudioControl = null;
		[Export] private AudioControl _musicAudioControl = null;
		[Export] private AudioControl _sfxAudioControl = null;

		[Export] private string _masterBusName = null;
		[Export] private string _musicBusName = null;
		[Export] private string _sfxBusName = null;

		[Export] private string _masterDisplayName = null;
		[Export] private string _musicDisplayName = null;
		[Export] private string _sfxDisplayName = null;
		#endregion Audio

		#region Video
		[Export] private Button _fullscreenButton = null;
		[Export] private OptionButton _resolutionsButton = null;

		private Dictionary<int, Resolution> _resolutions = new Dictionary<int, Resolution>();
		#endregion Video

		public override void _EnterTree()
		{
			_saveButton.Pressed += Save;
			_cancelButton.Pressed += Close;

			_okButton.Pressed += Save;
			_okButton.Pressed += Close;

			if (_backToMenuButton != null)
			{
				_backToMenuButton.Pressed += OnBackToMenuPressed;
			}

			if (_masterAudioControl != null)
			{
				_masterAudioControl.VolumeChanged += OnVolumeChanged;
			}
			if (_musicAudioControl != null)
			{
				_musicAudioControl.VolumeChanged += OnVolumeChanged;
			}
			if (_sfxAudioControl != null)
			{
				_sfxAudioControl.VolumeChanged += OnVolumeChanged;
			}

			if (_fullscreenButton != null)
			{
				_fullscreenButton.Toggled += OnFullscreenToggled;
			}
			if (_resolutionsButton != null)
			{
				_resolutionsButton.ItemSelected += OnResolutionSelected;
			}
		}

		private void OnBackToMenuPressed()
		{
			GameManager.Instance.ChangeState(GameStateType.MainMenu);
		}

		public override void _ExitTree()
		{
			_saveButton.Pressed -= Save;
			_cancelButton.Pressed -= Close;

			_okButton.Pressed -= Save;
			_okButton.Pressed -= Close;

			if (_backToMenuButton != null)
			{
				_backToMenuButton.Pressed -= OnBackToMenuPressed;
			}

			if (_masterAudioControl != null)
			{
				_masterAudioControl.VolumeChanged -= OnVolumeChanged;
			}
			if (_musicAudioControl != null)
			{
				_musicAudioControl.VolumeChanged -= OnVolumeChanged;
			}
			if (_sfxAudioControl != null)
			{
				_sfxAudioControl.VolumeChanged -= OnVolumeChanged;
			}

			if (_fullscreenButton != null)
			{
				_fullscreenButton.Toggled -= OnFullscreenToggled;
			}
			if (_resolutionsButton != null)
			{
				_resolutionsButton.ItemSelected -= OnResolutionSelected;
			}
		}

		private void OnResolutionSelected(long index)
		{
			if (_resolutions.TryGetValue((int)index, out Resolution resolution))
			{
				DisplayServer.WindowSetSize((Vector2I)resolution);
			}
			else
			{
				GD.PushError($"Unknown resolution index selected: {index}");
			}
		}

		private void OnFullscreenToggled(bool toggledOn)
		{
			DisplayServer.WindowSetMode(toggledOn ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
			_resolutionsButton.Disabled = toggledOn;
		}

		public override void _Ready()
		{
			if (_masterAudioControl != null)
			{
				// TODO: When settings data is implemented, read this from the data.
				// For now, let's query the volume from the AudioServer.
				SetUpAudioControl(_masterAudioControl, _masterBusName, _masterDisplayName);
			}

			if (_musicAudioControl != null)
			{
				SetUpAudioControl(_musicAudioControl, _musicBusName, _musicDisplayName);
			}

			if (_sfxAudioControl != null)
			{
				SetUpAudioControl(_sfxAudioControl, _sfxBusName, _sfxDisplayName);
			}

			InitializeScreenControls();
		}

		private void InitializeScreenControls()
		{
			Vector2I viewPortSize = GameManager.MinWindowSize;
			Vector2I screenSize = DisplayServer.ScreenGetSize();
			int multiplyer = 1;
			int id = 0;
			int selected = 0; // TODO: When settings data is implemented, read this from the data.

			while (GetWidth(viewPortSize, multiplyer) <= screenSize.X && GetHeight(viewPortSize, multiplyer) <= screenSize.Y)
			{
				Resolution resolution = new Resolution()
				{
					Width = GetWidth(viewPortSize, multiplyer),
					Height = GetHeight(viewPortSize, multiplyer),
					Multiplyer = multiplyer,
				};

				_resolutions.Add(id, resolution);

				_resolutionsButton.AddItem(resolution.ToString(), id);
				multiplyer++;
				id++;
			}

			_resolutionsButton.Select(selected);

			DisplayServer.WindowMode windowMode = DisplayServer.WindowGetMode();
			_fullscreenButton.ButtonPressed = false; // TODO: When settings data is implemented, read this from the data.

			static int GetWidth(Vector2I viewPortSize, int multiplyer)
			{
				return viewPortSize.X * multiplyer;
			}

			static int GetHeight(Vector2I viewPortSize, int multiplyer)
			{
				return viewPortSize.Y * multiplyer;
			}
		}

		private void SetUpAudioControl(AudioControl audioControl, string busName, string displayName)
		{
			int audioBusIndex = AudioServer.GetBusIndex(busName);
			if (audioBusIndex >= 0)
			{
				float volume = AudioServer.GetBusVolumeDb(audioBusIndex);
				audioControl.Setup(busName, displayName, volume);
			}
		}

		private void OnVolumeChanged(string audioBus, float decibel)
		{
			// TODO: Add support for previewing audio and them saving (or restoring) volume 
			// separately.
			int audioBusIndex = AudioServer.GetBusIndex(audioBus);
			if (audioBusIndex < 0)
			{
				return;
			}

			AudioServer.SetBusVolumeDb(audioBusIndex, decibel);
		}

		public void Close()
		{
			GameManager.Instance.ActivatePreviousState();
		}

		public void Save()
		{
			// TODO: Implement saving Settings
			GD.Print("Saving the settings.");
		}
	}
}
