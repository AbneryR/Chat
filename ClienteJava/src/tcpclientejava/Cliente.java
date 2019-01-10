/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package tcpclientejava;

import java.io.*;
import java.net.*;
import java.nio.charset.Charset;
import java.util.*;

public class Cliente {            

    //Propiedades
    String ipn="";
    public String getIpn() {return ipn;}
    public void setIpn(String ipn) {this.ipn = ipn;}

    String puerto="";
    public String getPuerto() {return puerto;}  
    public void setPuerto(String puerto) {this.puerto = puerto;}
    
    String  nick="";
    public String getNick() {return nick;}
    public void setNick(String nick) {this.nick = nick;}
    
    //Variables
    private Socket socket;
    private DataOutputStream out ;
    private DataInputStream in ;    
    
    //Metodos
    
    public void Conectar()
    {
        try
        {
            System.out.println("Escribe tu nickname(no más de 30 caracteres): ");
            Scanner teclado = new Scanner(System.in);
            do{
            nick = teclado.nextLine();
            }while(nick.length() <=0 &&  nick.length() >=30);
            socket = new Socket(ipn, Integer.parseInt(puerto));
            
            System.out.println("-------------------------------");;
            System.out.println("Estas Conectado con el Servidor");
            System.out.println("-------------------------------");

            out = new DataOutputStream(socket.getOutputStream());
            byte[] n = nick.getBytes(Charset.forName("US-ASCII"));
            out.write(n);
            in = new DataInputStream(socket.getInputStream());            
            
            Thread env = new Thread(new EnviarMensaje(out));
            Thread rec = new Thread(new RecibirMensaje(in));
            
            env.start();    
            rec.start();
            
           
            
            do{
                
            }while (env.isAlive() && rec.isAlive());
            
            socket.close();
        }
        
        catch (IOException e)
        {
            System.out.println("No se pudo establecer la conexion \n"+e );                    
        }
        
    }
       
}

class EnviarMensaje implements Runnable
{
    private DataOutputStream out;

    public EnviarMensaje(DataOutputStream out) {
        this.out = out;
    }
    
    public void run()
    {
        Scanner teclado = new Scanner(System.in);
        try
        {
            String mensaje = "";
            do
            {
                mensaje = teclado.nextLine();
                byte[] m = mensaje.getBytes(Charset.forName("US-ASCII"));
                out.write(m);
                System.out.println("Tu : "+ mensaje);
            }while(!mensaje.equals("fin"));
        }
        catch(Exception e)
        {
            System.out.println("Error al enviar mensaje \n"+e);                    
        }
    }
}

class RecibirMensaje implements Runnable 
{
    private DataInputStream in;

    public RecibirMensaje(DataInputStream in) {
        this.in = in;
    }
    
    public void run()
    {
        try
        {
            String mensaje="";
            do
            {
                
                byte[] m;
                m = new byte[128];
                in.read(m);
                mensaje = new String(m, Charset.forName("US-ASCII") );
                
                System.out.println("Servidor: "+mensaje);
            }while(!mensaje.equals("fin"));
        }
        
        catch(Exception e)
        {   
            System.out.println("Error al recibir un mensaje "+e);
        }        
    }
}