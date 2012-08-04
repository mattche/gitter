﻿namespace gitter.Framework
{
	using System;
	using System.Text;
	using System.Threading;
	using System.Drawing;
	using System.Reflection;
	using System.Windows.Forms;

	using gitter.Framework.Options;

	using Resources = gitter.Framework.Properties.Resources;

	public partial class ExceptionDialog : DialogBase
	{
		private const string ReportUrl = @"https://github.com/amgine/gitter/issues/new";

		private Exception _exception;
		private DateTime _date;

		/// <summary>Create <see cref="ExceptionDialog"/>.</summary>
		/// <param name="exception">Related exception.</param>
		public ExceptionDialog(Exception exception)
		{
			if(exception == null) throw new ArgumentNullException("exception");
			_exception = exception;
			_date = DateTime.Now;

			InitializeComponent();

			Text = Resources.StrError;

			GitterApplication.FontManager.InputFont.Apply(_txtStack);

			var f = GitterApplication.FontManager.UIFont.Font;

			_lblExceptionName.Font = new Font(f.FontFamily, 15, FontStyle.Regular, f.Unit, f.GdiCharSet);

			_lblSTack.Text = Resources.StrStack.AddColon();
			_lnkCopyToClipboard.Text = Resources.StrCopyToClipboard;
			_lnkSendBugReport.Text = Resources.StrSendBugreport;

			_lblExceptionName.Text = exception.GetType().FullName;
			_lblMessage.Text = exception.Message;
			string stackTrace = null;
			if(exception.Data.Contains("OriginalStackTrace"))
			{
				stackTrace = (string)exception.Data["OriginalStackTrace"];
			}
			else
			{
				stackTrace = exception.StackTrace;
			}
			_txtStack.Text = stackTrace;
		}

		protected override string ActionVerb
		{
			get { return Resources.StrClose; }
		}

		public override DialogButtons OptimalButtons
		{
			get { return DialogButtons.Ok; }
		}

		public Exception Exception
		{
			get { return _exception; }
		}

		private string GetMessage()
		{
			var str = new StringBuilder();
			str.Append("Date: ");
			str.AppendLine(_date.FormatISO8601());
			AppendExceptionInfo("Exception", _exception, str);
			if(_exception.InnerException != null)
			{
				AppendInnerException(_exception.InnerException, str);
			}
			AppendLoadedAssemblies(str);
			return str.ToString();
		}

		private static void AppendInnerException(Exception exception, StringBuilder str)
		{
			AppendExceptionInfo("Inner Exception", exception, str);
			if(exception.InnerException != null)
			{
				AppendInnerException(exception.InnerException, str);
			}
		}

		private static void AppendExceptionInfo(string header, Exception exception, StringBuilder str)
		{
			str.Append(header);
			str.Append(": ");
			str.AppendLine(exception.GetType().FullName);
			str.AppendLine("Message:");
			str.AppendLine(exception.Message);
			str.AppendLine();
			str.AppendLine("Call Stack:");
			str.AppendLine(exception.StackTrace);
			str.AppendLine();
		}

		private static void AppendLoadedAssemblies(StringBuilder str)
		{
			var asms = AppDomain.CurrentDomain.GetAssemblies();
			Array.Sort(asms, (asm1, asm2) =>
				{
					if(asm1.GlobalAssemblyCache != asm2.GlobalAssemblyCache)
						return asm1.GlobalAssemblyCache ? 1 : -1;
					else
						return string.Compare(asm1.FullName, asm2.FullName);
				});
			str.AppendLine("Loaded Assemblies:");
			for(int i = 0; i < asms.Length; ++i)
			{
				AppendAssemblyInfo(asms[i], str);
			}
		}

		private static void AppendAssemblyInfo(Assembly asm, StringBuilder str)
		{
			if(asm.GlobalAssemblyCache)
			{
				str.Append("[GAC] ");
			}
			str.AppendLine(asm.FullName);
		}

		private void OnCopyToClipboardLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			CopyToClipboardSafe();
		}

		private void OnSendBugReportLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Utility.OpenUrl(ReportUrl);
		}

		private void CopyToClipboardSafe()
		{
			if(Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
			{
				CopyToClipboard();
			}
			else
			{
				var thread = new Thread(CopyToClipboard);
				if(thread.TrySetApartmentState(ApartmentState.STA))
				{
					thread.Start();
					thread.Join();
				}
			}
		}

		private void CopyToClipboard()
		{
			int attempts = 3;
			var message = GetMessage();
			while(attempts > 0)
			{
				try
				{
					Clipboard.SetText(message);
					break;
				}
				catch
				{
				}
				--attempts;
				if(attempts <= 0)
				{
					break;
				}
				Thread.Sleep(50);
			}
		}
	}
}
