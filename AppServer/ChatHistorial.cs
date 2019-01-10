using System;
using Gtk;
using Glade;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Data.SQLite;

namespace AppServer
{
	class ChatHistorial : Gtk.Window
	{
		XML ver;
		[Glade.Widget]
		TextView tb_mostrar;
		[Glade.Widget]
		TextBuffer his_buffer;

		public ChatHistorial (XML gxml, IntPtr handle) : base (handle)
		{
			this.ver = gxml;
			ver.Autoconnect (this);
			inicializa_componentes ();
		}

		void inicializa_componentes ()
		{
			his_buffer = tb_mostrar.Buffer;
			tb_mostrar.Editable = false;
			tb_mostrar.CursorVisible = false;

			VerHistorial ();
		}

		void VerHistorial ()
		{
			try 
			{
				string cnxStr = @"Data Source=CHAT.db;Version=3;New=True;Compress=True;"; 
				SQLiteConnection cnx = new SQLiteConnection (cnxStr); 
				try
				{
					cnx.Open();
					string consulta="SELECT FECHA, NOMBRE, MENSAJE FROM CONVERSACION ORDER BY FECHA;";
					SQLiteCommand cmd = new SQLiteCommand (consulta, cnx);
					SQLiteDataReader lector = cmd.ExecuteReader();
					while (lector.Read())
					{
						TextIter iter = his_buffer.EndIter;
						his_buffer.Insert (ref iter, lector.GetString(0));
						iter = his_buffer.EndIter;
						his_buffer.Insert (ref iter, lector.GetString(1));
						iter = his_buffer.EndIter;
						his_buffer.Insert (ref iter, lector.GetString(2));
						iter = his_buffer.EndIter;
						his_buffer.Insert (ref iter, "\n");
					}
				}

				catch (Exception e)
				{
					TextIter iter = his_buffer.EndIter;
					his_buffer.Insert (ref iter, "No se pueden ver los datos\n");
					iter = his_buffer.EndIter;
					his_buffer.Insert (ref iter, e.ToString());
				}

				cnx.Close ();
			}


			catch(Exception e) 
			{
				TextIter iter = his_buffer.EndIter;
				his_buffer.Insert (ref iter, "No se pueden acceder a la base de datos");
			}
		}

		public static ChatHistorial Create ()
		{
			XML gxml = new XML ("Interfaz.Historial.glade", "Historial");

			return new ChatHistorial (gxml, gxml.GetWidget("Historial").Handle);
		}

		public void OnDeleteEvent (System.Object sender, DeleteEventArgs e)
		{
			e.RetVal = true;
		}

	}


}

