﻿namespace gitter.Git.AccessLayer.CLI
{
	using System;
	using System.Diagnostics;

	/// <summary>Executes git.exe.</summary>
	internal sealed class ProcessExecutor
	{
		#region Data

		private readonly string _path;
		private IOutputReceiver _stdOutReceiver;
		private IOutputReceiver _stdErrReceiver;
		private Process _process;

		#endregion

		#region .ctor

		public ProcessExecutor(string path, IOutputReceiver stdOutReceiver, IOutputReceiver stdErrReceiver)
		{
			Verify.Argument.IsNeitherNullNorWhitespace(path, "path");

			_path = path;
			_stdOutReceiver = stdOutReceiver;
			_stdErrReceiver = stdErrReceiver;
		}

		#endregion

		public int Execute(GitInput input)
		{
			BeginExecute(input);
			return EndExecute();
		}

		public void BeginExecute(GitInput input)
		{
			Verify.Argument.IsNotNull(input, "input");
			Verify.State.IsFalse(IsStarted);

			var psi = new ProcessStartInfo()
			{
				Arguments				= input.GetArguments(),
				WindowStyle				= ProcessWindowStyle.Normal,
				UseShellExecute			= false,
				StandardOutputEncoding	= input.Encoding,
				StandardErrorEncoding	= input.Encoding,
				RedirectStandardInput	= true,
				RedirectStandardOutput	= true,
				RedirectStandardError	= true,
				LoadUserProfile			= true,
				FileName				= _path,
				ErrorDialog				= false,
				CreateNoWindow			= true,
			};
			if(!string.IsNullOrEmpty(input.WorkingDirectory))
			{
				psi.WorkingDirectory = input.WorkingDirectory;
			}
			GitProcess.SetCriticalEnvironmentVariables(psi);
			if(input.Environment != null)
			{
				foreach(var opt in input.Environment)
				{
					psi.EnvironmentVariables[opt.Key] = opt.Value;
				}
			}
			_process = new Process() { StartInfo = psi };
			_process.Start();
			if(_stdOutReceiver != null)
			{
				_stdOutReceiver.Initialize(_process, _process.StandardOutput);
			}
			if(_stdErrReceiver != null)
			{
				_stdErrReceiver.Initialize(_process, _process.StandardError);
			}
		}

		public int EndExecute()
		{
			Verify.State.IsTrue(IsStarted);

			_process.WaitForExit();
			if(_stdErrReceiver != null)
			{
				_stdErrReceiver.Close();
			}
			if(_stdOutReceiver != null)
			{
				_stdOutReceiver.Close();
			}
			int code = _process.ExitCode;
			_process.Dispose();
			_process = null;
			return code;
		}

		public bool IsStarted
		{
			get { return _process != null; }
		}
	}
}
