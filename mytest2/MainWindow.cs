using System;
using Gtk;
using Pango;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public partial class MainWindow: Gtk.Window
{
	public enum OutputType
	{
		Info,
		Warn,
		Error
	}

	private class OutputMessage
	{
		public string Message;
		public OutputType OutputType;
	}

	private BlockingCollection<OutputMessage> m_outputQueue = new BlockingCollection<OutputMessage>(new ConcurrentQueue<OutputMessage>());
	private Task m_outputTask;

	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		Build();

		this.label1.ModifyFont (FontDescription.FromString ("Courier 48"));

		StartOutputHandler();

	}

	public void AddOutput (string text, OutputType ot = OutputType.Info)
	{
		var om = new OutputMessage() { Message = text, OutputType = ot };


		m_outputQueue.Add(om);
}

	private void StartOutputHandler()
	{
		m_outputTask = Task.Run(() => {

			while (!m_outputQueue.IsCompleted)
			{
				try
				{
					var msg = m_outputQueue.Take();

					Application.Invoke(msg, null, AddMessageHandler);
				}
				catch (InvalidOperationException)
				{
					break;
				}
			}
		});
	}

	private void AddMessageHandler(object o, EventArgs e)
	{
		var msg = (OutputMessage)o;
		var end = OutputTextView.Buffer.EndIter;

		var tag = new TextTag(null);
		OutputTextView.Buffer.TagTable.Add(tag);

		switch (msg.OutputType)
		{
			case OutputType.Info:
				tag.Foreground = "#000000";
				break;
			case OutputType.Warn:
				tag.Foreground = "#cccc00";
				break;
			case OutputType.Error:
				tag.Foreground = "#cc0000";
				tag.Weight = Weight.Bold;
				break;
		}

		OutputTextView.Buffer.InsertWithTags(ref end, msg.Message + "\n", tag);

		OutputTextView.ScrollToIter(OutputTextView.Buffer.EndIter, 0, false, 0, 0);
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		m_outputQueue.CompleteAdding();
		m_outputTask.Wait();
		
		Gtk.Application.Quit ();
		a.RetVal = true;
	}
}
