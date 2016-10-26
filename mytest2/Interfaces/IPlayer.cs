using System;
using SSTV4.DataContracts;

namespace SSTV4.Interfaces
{
	public enum PlayerState
	{
		Idle,
		Playing,
		Paused
	}

	public enum PlayerCommand
	{
		SeekF1,
		SeekF2,
		SeekF3,
		SeekB1,
		SeekB2,
		SeekB3,
		SeekStart,
		Mute,
		CycleAudio,
		Pause,
		Stop,
		ShowProgress,
		LoadFile
	}

	public interface IPlayer
	{
		PlayerState CurrentState { get; }
		Media CurrentlyPlaying { get; }

		void Start(Media m);
		void Pause();
		void Command(PlayerCommand cmd);
		void Kill();
	}
}
