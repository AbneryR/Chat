using System;
using Gtk;
using Glade;

namespace AppCliente
{
	class MainForm : Gtk.Window
	{
		XML vista;

		[Glade.Widget]
		Button bt_conectar;
		[Glade.Widget]
		Button bt_enviar;
		[Glade.Widget]
		Button bt_desconectar;
		[Glade.Widget]
		Entry tb_mensaje;
		[Glade.Widget]
		Entry tb_datos;
		[Glade.Widget]
		Entry tb_nombre;
		[Glade.Widget]
		TextView tb_mostrar;
		[Glade.Widget]
		TextBuffer con_buffer;


		public static ClaseCliente cl;

		public MainForm (XML gxml, IntPtr handle) : base (handle)
		{
			this.vista = gxml;
			vista.Autoconnect (this);
			inicializa_componentes ();
		}

		void inicializa_componentes ()
		{
			cl = new ClaseCliente ();
			cl.ptoSvr = "7070";
			cl.DatosRecibidos += Datos_Recibidos;
			cl.ConexionTerminada += Conexion_Terminada;

			con_buffer = tb_mostrar.Buffer;
			tb_mostrar.Editable = false;
			tb_mostrar.CursorVisible = false;

			this.DeleteEvent += OnDeleteEvent;
			this.bt_conectar.Clicked += Bt_conectar_Clicked;
			this.bt_enviar.Clicked += Bt_enviar_Clicked;
			this.bt_desconectar.Clicked += Bt_desconectar_Clicked;
		}
			
		void Bt_enviar_Clicked (object sender, EventArgs e)
		{
			cl.EnviarDatos (tb_mensaje.Text);
			TextIter iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, DateTime.Now.ToString ()+"->");
			iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, tb_nombre.Text+": ");
			iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, tb_mensaje.Text);
			iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, "\n");

			tb_mensaje.Text = "";
		}

		void Bt_conectar_Clicked (object sender, EventArgs e)
		{

			cl.ipSvr = tb_datos.Text;
			cl.Conectar (tb_nombre.Text);
			TextIter iter = con_buffer.EndIter;

			con_buffer.Insert (ref iter, DateTime.Now.ToString ()+"->");
			iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, tb_nombre.Text+"->");
			iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, "Conectado");
			iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, "\n");
		}

		void Bt_desconectar_Clicked (object sender, EventArgs e)
		{
			Conexion_Terminada ();
		}

		public void Datos_Recibidos(String msg){
			TextIter iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, DateTime.Now.ToString ()+"->");
//			iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, "Servidor: ");
//			iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, msg);
//			iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, "\n");
		}

		public void Conexion_Terminada(){
			TextIter iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, DateTime.Now.ToString () + "->");
		}

		public static MainForm Create ()
		{
			XML gxml = new XML ("InterfazCliente.AplicacionCliente.glade", "Cliente");

			return new MainForm (gxml, gxml.GetWidget("Cliente").Handle);
		}

		public void OnDeleteEvent (System.Object sender, DeleteEventArgs e)
		{
			Application.Quit ();
			e.RetVal = true;
		}
	}

}

