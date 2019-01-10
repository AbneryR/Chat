using System;
using Gtk;

namespace AppCliente
{
	public class App
	{
		static void Main (string[] args)
		{
			Application.Init();
			MainForm form = MainForm.Create ();
			form.Show ();
			Application.Run ();
		}
	}
}

