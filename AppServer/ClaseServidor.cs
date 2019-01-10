using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Data;
using System.Data.SQLite;

namespace AppServer
{
	public class ClaseServidor
	{
		string nombre="";
		string mensaje="";
		string fecha="";
		string nombre_c="";
		SQLiteConnection cnx;

		#region IMPORTANTE
		/*
			El flujo es este

		1) Cuando declaras el delegate estas declarando qué va a llevar tu evento y que datos va a regresar
		//Los public delegate void DNuevaConexion... etc

		2)Luego declaras tus TRIGGERS, los llamados que vas a poner que activaran el evento.
			//Los this.NuevaConexion(cteAct)

		3)Le agregué el nick al struct para la info del cliente y cambié un delegate y agregué una funcion para ObtenerNick
		y cambié la de esperar cliente y conectar en cliente y server

		4)Después de esas tres cosas, todo lo demás se hace en el main
		*/

		#endregion

		#region estructuras
		private struct InfoCte
		{
			public Socket socket;
			public Thread thread;
			public string lastMsg;
			//Agregamos string para guardar nick de cada cliente
			public string nick;
		}
		#endregion

		#region variables
		private TcpListener tcpLsn;
		private Hashtable clientes = new Hashtable();
		private Thread tcpThd;
		private IPEndPoint cteAct;
		#endregion

		#region eventos
		//Cambiamos el delegate para pasarle los nick de cada usuario
		public delegate void DNuevaConexion(IPEndPoint id);
		//a las otras no les agregué el nick porque agregué otra funcion pública para obtenerlo, así que no hay flato

		public delegate void DDatosRecibidos(IPEndPoint id);

		public delegate void DConexionTerminada(IPEndPoint id);

		public DNuevaConexion NuevaConexion;
		public DDatosRecibidos DatosRecibidos;
		public DConexionTerminada ConexionTerminada;
		#endregion

		#region Propiedades
		private string pto;
		public string Pto
		{
			get { return pto; }
			set { pto = value; }
		}
		#endregion

		#region metodos
		public bool Escuchar()
		{
			try{
				this.tcpLsn = new TcpListener(Convert.ToInt32(pto));
				tcpLsn.Start();
				tcpThd = new Thread(new ThreadStart(EsperarCliente));
				tcpThd.Start();	
				Gtk.MessageDialog msg = new Gtk.MessageDialog (null, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Info, Gtk.ButtonsType.Ok, "Se ha establecido la conexion");     
				msg.Title = "Conectado";
				msg.Run ();
				msg.Destroy ();
				try 
				{
					string cnxStr = "Data Source=CHAT.db;Version=3;New=True;Compress=True;"; 
					cnx = new SQLiteConnection (cnxStr); 
					//Crea la tabla 
					VerificaTabla();
					return true;
				}
				catch(Exception e) 
				{
					Gtk.MessageDialog m = new Gtk.MessageDialog (null, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Info, Gtk.ButtonsType.Ok, "No se puede acceder a la base de datos\n"+e);     
					m.Title = "Error en Base de Datos";
					m.Run ();
					m.Destroy ();
					return false;
				}
			}
			catch (Exception e){
				Gtk.MessageDialog msg = new Gtk.MessageDialog (null, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Info, Gtk.ButtonsType.Ok, "Ya estas conectado");     
				msg.Title = "Error";
				msg.Run ();
				msg.Destroy ();
				return false;
			}
		}

		void VerificaTabla ()
		{
			cnx.Open ();
			try
			{
				string tabla="CREATE TABLE IF NOT EXISTS CONVERSACION (CLAVE INTEGER PRIMARY KEY AUTOINCREMENT, FECHA TEXT NOT NULL, NOMBRE TEXT NOT NULL, MENSAJE TEXT NOT NULL);";
				SQLiteCommand cmd = new SQLiteCommand (tabla, cnx);
				cmd.ExecuteNonQuery ();	
			}

			catch (Exception e)
			{
				Gtk.MessageDialog msg = new Gtk.MessageDialog (null, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Info, Gtk.ButtonsType.Ok, "No se pudo crear la tabla");     
				msg.Title = "Error Tabla";
				msg.Run ();
				msg.Destroy ();
			}
			cnx.Close ();
		}

		private void GuardarMensaje(string fecha, string nombre, string mensaje)
		{
			cnx.Open ();
			try 
			{
				if (nombre == "")
				{
					string g_ser="INSERT INTO CONVERSACION (FECHA, NOMBRE, MENSAJE) VALUES (@fecha,'Servidor',@mensaje);";
					SQLiteCommand cmd = new SQLiteCommand (g_ser, cnx);
					cmd.Parameters.Add(new SQLiteParameter ("@fecha",fecha));
					cmd.Parameters.Add(new SQLiteParameter ("@mensaje",mensaje));
					cmd.ExecuteNonQuery ();		
				}

				else
				{
					string g_c="INSERT INTO CONVERSACION (FECHA, NOMBRE, MENSAJE) VALUES (@fecha, @nombre,@mensaje);";
					SQLiteCommand cmd = new SQLiteCommand (g_c, cnx);
					cmd.Parameters.Add(new SQLiteParameter ("@fecha",fecha));
					cmd.Parameters.Add(new SQLiteParameter ("@nombre",nombre));
					cmd.Parameters.Add(new SQLiteParameter ("@mensaje",mensaje));
					cmd.ExecuteNonQuery ();
				}
			}

			catch (Exception e)
			{
				Gtk.MessageDialog msg = new Gtk.MessageDialog (null, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Info, Gtk.ButtonsType.Ok, "No se pudo guardar el mensaje");     
				msg.Title = "Error al Guardar";
				msg.Run ();
				msg.Destroy ();
			}
			cnx.Close ();
		}

		public string ObtenerNick(IPEndPoint id)
		{
			InfoCte cte = (InfoCte)clientes[id];
			return cte.nick;
		}

		public string ObtenerDatos(IPEndPoint id)
		{
			InfoCte cte = (InfoCte)clientes[id];
			return cte.lastMsg;
		}

		public void Cerrar(IPEndPoint id)
		{
			InfoCte cte = (InfoCte)clientes[id];
			cte.socket.Close();
		}

		public void Cerrar()
		{
			foreach (InfoCte cte in clientes)
				Cerrar((IPEndPoint)cte.socket.RemoteEndPoint);
		}

		public void EnviarDatos(IPEndPoint id, string datos)
		{
			//Guarda Mensaje que envia servidor
			GuardarMensaje (Convert.ToString(DateTime.Now),nombre,datos);

			InfoCte cte = (InfoCte)clientes[id];
			cte.socket.Send(Encoding.ASCII.GetBytes(datos));
		}

		public void EnviarDatos(string datos)
		{
			foreach (InfoCte cte in clientes.Values) {
				EnviarDatos ((IPEndPoint)cte.socket.RemoteEndPoint, datos);
			}
		}
		#endregion

		#region metodos
		private void EsperarCliente()
		{
			InfoCte cte = new InfoCte();
			while (true)
			{
				cte.socket = tcpLsn.AcceptSocket();
				byte[] buffer = new byte[30];
				//Recibimos el primer mensaje personalmente para guardarlo como nick
				int ret = cte.socket.Receive(buffer, SocketFlags.None);
				cte.nick = Encoding.ASCII.GetString(buffer);
				nombre_c = cte.nick;
				this.cteAct = (IPEndPoint)cte.socket.RemoteEndPoint;
				cte.thread = new Thread(new ThreadStart(LeerSocket));
				lock (this)
				{
					clientes.Add(cteAct, cte);

				}
				//cada vez que alguien se conecte, dispara este evento y dale estos datos
				this.NuevaConexion(cteAct);
				//this.NuevaConexion(cteAct);
				cte.thread.Start();
			}
		}

		private void LeerSocket()
		{
			IPEndPoint id = this.cteAct;
			byte[] buffer;

			InfoCte cte = (InfoCte)clientes[id];
			int ret = 0;

			while (true)
			{
				if (cte.socket.Connected)
				{
					buffer = new byte[128];
					try
					{
						ret = cte.socket.Receive(buffer, buffer.Length,
							SocketFlags.None);
						if (ret > 0)
						{
							cte.lastMsg = Encoding.ASCII.GetString(buffer);
							//Guarda mensaje que recive
							GuardarMensaje (Convert.ToString(DateTime.Now),cte.nick,cte.lastMsg);
							clientes[id] = cte;
							//Cada vez que lleguen datos. dispara este evento y dime de quien fue
							//this.DatosRecibidos(id, cte.nick);
							this.DatosRecibidos(id);
						}
						else
						{
							//si no, dispara este evento
							this.ConexionTerminada(id);
							break;
						}
					}
					catch (Exception ex)
					{
						if (!cte.socket.Connected)
						{
							//si ya no está conectado este socket, dispara este evento y dime de quien fue
							this.ConexionTerminada(id);
							break;
						}
					}
				}
			}

			CerrarThread(id);
		}

		private void CerrarThread(IPEndPoint id)
		{
			InfoCte cte = (InfoCte)clientes[id];
			try
			{
				cte.thread.Abort();
			}
			catch (Exception ex)
			{
				lock(this)
				{
					clientes.Remove(id);
				}
			}
		}
		#endregion
	}
}

