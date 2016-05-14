using System;
using Gtk;
using Pango;

public partial class MainWindow: Gtk.Window
{
	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		Build ();

		this.label1.ModifyFont (FontDescription.FromString ("Courier 48"));

		AddOutput ("Foobar");
	}

	private void AddOutput (string text)
	{
		var end = OutputTextView.Buffer.EndIter;
		OutputTextView.Buffer.Insert (ref end, text + "\n");
		OutputTextView.ScrollToIter(OutputTextView.Buffer.EndIter, 0, false, 0, 0);
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
}
