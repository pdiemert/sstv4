using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using SSTV4.DataContracts;
using SSTV4.Interfaces;
using Mono.Unix;
using System.Security;

namespace SSTV4
{
	public class Player : IPlayer
	{
		private static IPlayer sm_Player;

		private object m_lock = new object();
		private Media m_currentItem;
		private Process m_currentPlayer;
		private StreamWriter m_commandWriter;
		private StreamReader m_commandReader;
		private PlayerState m_state = PlayerState.Idle;

		private static Dictionary<PlayerCommand, string> sm_commandMap = new Dictionary<PlayerCommand, string>()
		{
			{ PlayerCommand.SeekF1, "seek 10" },
			{ PlayerCommand.SeekF2, "seek 60" },
			{ PlayerCommand.SeekF3, "seek 600" },
			{ PlayerCommand.SeekB1, "seek -10" },
			{ PlayerCommand.SeekB2, "seek -60" },
			{ PlayerCommand.SeekB3, "seek -600" },
			{ PlayerCommand.SeekStart, "seek -60000" },
			{ PlayerCommand.Mute, "cycle mute" },
			{ PlayerCommand.CycleAudio, "cycle audio" },
			{ PlayerCommand.Pause, "cycle pause" },
			{ PlayerCommand.Stop, "quit_watch_later" },
			{ PlayerCommand.LoadFile, "loadfile" }
		};

		public Player()
		{
		}

		public static IPlayer Instance
		{
			get
			{
				if (sm_Player == null)
				{
					sm_Player = new Player();
				}

				return sm_Player;
			}
		}

		public PlayerState CurrentState
		{
			get
			{
				return m_state;
			}
		}

		public Media CurrentlyPlaying
		{
			get
			{
				return m_currentItem;
			}
		}

		public void Start(Media m)
		{
			if (m == null)
				return;

			Transition(PlayerState.Playing, m);
		}


		public void Seek(int sec)
		{
			
		}

		public void Command(PlayerCommand command)
		{
			CommandInt(command, null);
		}

		private void CommandInt(PlayerCommand command, string arg)
		{
			// Treat pause as a state transition
			if (command == PlayerCommand.Pause)
			{
				Transition(m_state == PlayerState.Paused ? PlayerState.Playing : PlayerState.Paused);
				return;
			}

			SendCommand(command, arg);
		}

		private void SendCommand(PlayerCommand command, string arg)
		{
			string txt = sm_commandMap[command];

			if (arg != null)
			{
				txt += " \"" + arg + "\"";
			}

			m_commandWriter.Write(txt + "\n");
			m_commandWriter.Flush();
		}

		public void Pause()
		{
			Transition(m_state == PlayerState.Paused ? PlayerState.Playing : PlayerState.Paused);
		}

		private void Transition(PlayerState target, Media m = null)
		{
			lock(m_lock)
			{
				var stack = new Stack<PlayerState>();

				stack.Push(target);

				while (stack.Count > 0)
				{
					target = stack.Pop();

					switch (m_state)
					{
						case PlayerState.Idle:
							switch (target)
							{
								case PlayerState.Idle:
									break;
									
								case PlayerState.Paused:
									break;
									
								case PlayerState.Playing:
									m_currentItem = m;
									m_state = PlayerState.Playing;
									StartVideoPlayer();
									break;
							}
							break;

						case PlayerState.Playing:
							switch (target)
							{
								case PlayerState.Idle:
									StopVideoPlayer();
									m_currentItem = null;
									m_state = PlayerState.Idle;
									break;
									
								case PlayerState.Playing:
									m_currentItem = m;
									SendCommand(PlayerCommand.LoadFile, Escape(m.Path));
									break;
									
								case PlayerState.Paused:
									SendCommand(PlayerCommand.Pause, null);
									m_state = PlayerState.Paused;
									break;
							}
							break;

						case PlayerState.Paused:
							switch (target)
							{
								case PlayerState.Idle:
									StopVideoPlayer();
									m_state = PlayerState.Idle;
									break;
									
								case PlayerState.Playing:
									if (m != null)
									{
										// We are pause and a new video is being request
										m_currentItem = m;
										SendCommand(PlayerCommand.LoadFile, Escape(m.Path));
									}
									else
									{
										SendCommand(PlayerCommand.Pause, null);
									}

									m_state = PlayerState.Playing;
									break;
									
								case PlayerState.Paused:
									break;
							}
							break;
					}
				}
			}
		}

		private string Escape(string txt)
		{
			return txt.Replace(@"\", @"\\");
		}

		private void StartVideoPlayer()
		{
			string player;
			string exepath;
			string mpvpath;

			if (!MainClass.IsOSX)
			{
				exepath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
				mpvpath = Path.Combine(exepath, "mpv");
				player = Path.Combine(mpvpath, @"mpv.exe");
			}
			else
			{
				exepath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				mpvpath = Path.Combine(exepath, Path.Combine("..", "..", "mpv.mac", "MacOS"));
				player = Path.Combine(mpvpath, "mpv");

			}

			var args = "--audio-channels=6 --quiet --ontop --input-ipc-server=/tmp/mpv-socket --script=\"" + Path.Combine(exepath, "mpvslave.lua") + "\"";

			if (MainClass.Configuration.Fullscreen)
				args += " --fullscreen";

			args += " " + m_currentItem.Path;

			Console.WriteLine(player + " " + args);

			var psi = new ProcessStartInfo()
			{   
				FileName = player,
				Arguments = args,
				UseShellExecute = false,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				WorkingDirectory = mpvpath,
				//UserName = "Pete Diemert",
				//Verb = "runas"
			};
			unsafe
			{
				fixed(char * p = "tunafish")
				{
					var ss = new SecureString(p, 8);
					ss.MakeReadOnly();
					//psi.Password = ss;
				}
			}
			   
			m_currentPlayer = Process.Start(psi);


			if (MainClass.IsOSX)
			{
				var ep = new UnixEndPoint("/tmp/mpv-socket");
				var sock = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);

				sock.Connect(ep);

				var ns = new NetworkStream(sock);

				m_commandWriter = new StreamWriter(ns);
				m_commandReader = new StreamReader(ns);
			}
			else
			{
				var client = new NamedPipeClientStream(@"tmp\mpv-socket");
				client.Connect();
				m_commandWriter = new StreamWriter(client);
				m_commandReader = new StreamReader(client);
			}

			/*
			Task.Run(() =>
			{
				m_currentPlayer.WaitForExit();
				Console.WriteLine("Player exited!!!!");
			});

			Task.Run(new Action(HandleOutput));
			*/
		}

		private void StopVideoPlayer()
		{
			if (m_currentPlayer == null)
			{
				return;
			}

			Command(PlayerCommand.Stop);

			m_currentPlayer.WaitForExit();
		}

		private void HandleOutput()
		{
			while (!m_currentPlayer.StandardOutput.EndOfStream)
			{
				var text = m_currentPlayer.StandardOutput.ReadLine();

				Console.WriteLine(text);
			}
		}

		public void Kill()
		{
			if (m_currentPlayer == null)
			{
				Transition(PlayerState.Idle);
				
				return;
			}

			m_currentPlayer.Kill();
			m_currentPlayer = null;

			Transition(PlayerState.Idle);
		}
	}
}
