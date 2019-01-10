using System;
using Gtk;
using Glade;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Globalization;

namespace AppServer
{
	class MainForm : Gtk.Window
	{
		XML vista;
		public ClaseServidor svr;

		[Glade.Widget]
		Button bt_conexion;
		[Glade.Widget]
		Button bt_cerrar;
		[Glade.Widget]
		Button bt_desconectar;
		[Glade.Widget]
		Button bt_enviar;
		[Glade.Widget]
		Button bt_historial;
		[Glade.Widget]
		Entry tb_datos;
		[Glade.Widget]
		Entry tb_mensaje;
		[Glade.Widget]
		TextView tb_mostrar;
		[Glade.Widget]
		TextBuffer con_buffer;
		[Glade.Widget]
		ComboBox cb_clientes;
		[Glade.Widget]
		ComboBox cb_seleccion;

		string cnx; 
		CellRendererText cell1;
		ListStore lista_cte = new ListStore (typeof(string),typeof(string));
		CellRendererText cell2;
		ListStore lista_des = new ListStore (typeof(string),typeof(string));

		public MainForm (XML gxml, IntPtr handle) : base (handle)
		{
			this.vista = gxml;
			vista.Autoconnect (this);
			inicializa_componentes ();
		}

		void inicializa_componentes ()
		{
			svr = new ClaseServidor ();
			svr.Pto = "7070";
			svr.NuevaConexion += Nueva_Conexion;
			svr.DatosRecibidos += Datos_Recibidos;
			svr.ConexionTerminada += Conexion_Terminada;
		
			con_buffer = tb_mostrar.Buffer;
			tb_mostrar.Editable = false;
			tb_mostrar.CursorVisible = false;

			cb_clientes.Clear ();
			cell1 = new CellRendererText();
			cb_clientes.PackStart (cell1, false);
			cb_clientes.AddAttribute (cell1,"text",0);
			cb_clientes.Model = lista_cte;
			lista_cte.AppendValues ("Enviar a todos", "enviar a todos");

			cb_seleccion.Clear ();
			cell2 = new CellRendererText();
			cb_seleccion.PackStart (cell2, false);
			cb_seleccion.AddAttribute (cell2,"text",0);
			cb_seleccion.Model = lista_des;
			lista_des.AppendValues ("Desconectar a todos", "enviar a todos");

			this.DeleteEvent += OnDeleteEvent;
			this.bt_conexion.Clicked += Bt_conexion_Clicked;
			this.bt_enviar.Clicked += Bt_enviar_Clicked;
			this.bt_historial.Clicked += Bt_historial_Clicked;
			this.bt_cerrar.Clicked += Bt_cerrar_Clicked;
			this.bt_desconectar.Clicked += Bt_desconectar_Clicked;
		}

		void Conexion_Terminada(IPEndPoint id){
			TextIter iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, id.ToString()+"se desconecto");
			//Borra item del cliente que se desconecta // no esta probado
			TreeIter ti = lista_des.AppendValues ();
			lista_cte.Remove (ref ti);
		}

		public void Nueva_Conexion(IPEndPoint id){
			TextIter iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, DateTime.Now.ToString ()+"->");
			iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, "Llego: "+svr.ObtenerNick (id));
			iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, " desde: "+id.ToString());
			iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, "\n");

			//Agrega a los dos ComboBox el nuevo cliente que llego
			lista_cte.AppendValues (svr.ObtenerNick (id),id.ToString());
			lista_des.AppendValues (svr.ObtenerNick (id), id.ToString());



			//con_buffer.Text += DateTime.Now.ToString()+ svr.ObtenerNick(id) +"desde: "+id.ToString();
			//Console.WriteLine ( "\n"+  DateTime.Now.ToString() +" "+ svr.ObtenerNick(id) +"desde: " + id);
		}

		public void Datos_Recibidos(IPEndPoint id){
			TextIter iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, DateTime.Now.ToString ()+"->");
			iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, svr.ObtenerNick (id)+": ");
			iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, svr.ObtenerDatos(id));
			iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, "\n");
			//con_buffer.Text +=msg;	
		}
			

		public void Bt_conexion_Clicked (object sender, EventArgs e)
		{
			if (svr.Escuchar ()) {
				TextIter iter = con_buffer.EndIter;
				con_buffer.Insert (ref iter, DateTime.Now.ToString ()+"->");
				iter = con_buffer.EndIter;
				con_buffer.Insert (ref iter, "Servidor Iniciado");
				iter = con_buffer.EndIter;
				con_buffer.Insert (ref iter, "\n");
			}
		}

		public static IPEndPoint createIPEndPoint(string endPoint){
			string[] ep = endPoint.Split (':');
			if (ep.Length < 2)
				throw new FormatException ("Invalid endpoint format");
			IPAddress ip;
			if (ep.Length > 2) {
				if (!IPAddress.TryParse (string.Join (":", ep, 0, ep.Length - 1), out ip)) {
					throw new FormatException ("Invalid ip-address");
				}
			} 
			else {
				if (!IPAddress.TryParse (ep [0], out ip)) {
					throw new FormatException ("Invalid ip-address");
				}

			}
			int port;
			if(!int.TryParse(ep[ep.Length-1], NumberStyles.None, NumberFormatInfo.CurrentInfo,out port)){
				throw new FormatException ("Invalid port");
			}
			return new IPEndPoint (ip,port);
	}

		void Bt_enviar_Clicked(object sender, EventArgs e)
		{
			TreeIter tree;
			cb_clientes.GetActiveIter (out tree);
	

			if (cb_clientes.ActiveText == "Enviar a todos") {
				svr.EnviarDatos (tb_mensaje.Text);
			} else {
				string selected = (string)cb_clientes.Model.GetValue (tree, 1);

				svr.EnviarDatos (createIPEndPoint(selected), tb_mensaje.Text);	
			}
			TextIter iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, DateTime.Now.ToString ()+"->");
			iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, "Servidor: ");
			iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, tb_mensaje.Text);
			iter = con_buffer.EndIter;
			con_buffer.Insert (ref iter, "\n");

			tb_mensaje.Text = "";
		}

		void Bt_cerrar_Clicked (object sender, EventArgs e)
		{
			svr.Cerrar ();
		}

		void Bt_desconectar_Clicked (object sender, EventArgs e)
		{
					TreeIter tree;
					cb_seleccion.GetActiveIter (out tree);


					if (cb_seleccion.ActiveText == "Enviar a todos") {
						svr.EnviarDatos (tb_mensaje.Text);
					} else {
						string selected = (string)cb_seleccion.Model.GetValue (tree, 1);

						svr.Cerrar (createIPEndPoint(selected));	
					}
			//svr.Cerrar ((IPEndPoint)ti);
		}
			
		public void nuevomsg(IPEndPoint id){
			//Console.WriteLine("{0} {1}: {2}", DateTime.Now.ToString(), svr.ObtenerNick(id).Trim(), svr.ObtenerDatos(id).Trim());
		}

		public static MainForm Create ()
		{
			
			XML gxml = new XML ("Interfaz.AplicacionServidor.glade", "Servidor");

			return new MainForm (gxml, gxml.GetWidget("Servidor").Handle);
		}

		public void OnDeleteEvent (System.Object sender, DeleteEventArgs e)
		{
			Application.Quit ();
			e.RetVal = true;
		}

		void Bt_historial_Clicked (object sender, EventArgs e)
		{
			Application.Init();
			ChatHistorial ch = ChatHistorial.Create ();
			ch.Show ();
			Application.Run();
		}
	}

}

