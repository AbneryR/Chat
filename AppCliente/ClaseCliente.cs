using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AppCliente
{
	public class ClaseCliente
	{
		#region variables
		private Stream stm;
		#endregion

		#region eventos
		public delegate void DConexionTerminada();
		public delegate void DDatosRecibidos(string s);

		public DConexionTerminada ConexionTerminada;
		public DDatosRecibidos DatosRecibidos;
		#endregion

		#region propiedades
		public string ipSvr;
		string IpSvr
		{
			get { return ipSvr; }
			set { ipSvr = value; }
		}

		public string ptoSvr;
		string PtoSvr
		{
			get { return ptoSvr; }
			set { ptoSvr = value; }
		}
		#endregion

		#region metodos
		public void Conectar(string nick)
		{
			TcpClient socket = new TcpClient();
			Thread thread;
			//ESCRIBE NICKNAME
//			string nick;
//			do {
//				Console.WriteLine ("Escribe tu nick (no mas de 30 caracteres): ");	
//				nick = Console.ReadLine ();
//			} while(nick.Length <= 1 && nick.Length >= 30);
			//

			socket.Connect(ipSvr, Convert.ToInt32(ptoSvr));
			stm = socket.GetStream();
			//Mandamos personalmente el primer mensaje con el NICK
			byte[] buffer = Encoding.ASCII.GetBytes(nick);
			stm.Write (buffer, 0, buffer.Length);
			//
			thread = new Thread(new ThreadStart(LeerSocket));
			thread.Start();

		}

		public void EnviarDatos(string datos)
		{

			byte[] buffer = Encoding.ASCII.GetBytes(datos);
			if(stm.CanRead){
				stm.Write(buffer, 0, buffer.Length);
			}

			/*
			  if (stm.Length != 0)
			{
				stm.Write(buffer, 0, buffer.Length);
			}
			*/
		}
		#endregion

		#region metodos privados
		private void LeerSocket()
		{
			byte[] buffer = new byte[128];
			while (true)
			{
				try
				{
					stm.Read(buffer, 0, buffer.Length);
					this.DatosRecibidos(Encoding.ASCII.GetString(buffer));
				}
				catch (Exception e)
				{
					break;
				}
			}
			this.ConexionTerminada();
		}
		#endregion

	}
}

