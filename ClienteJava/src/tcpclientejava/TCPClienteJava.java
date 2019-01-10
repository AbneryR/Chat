/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package tcpclientejava;
import java.util.Scanner;
/**
 *
 * @author Johnatan Santiago
 */
public class TCPClienteJava {

    /**
     * @param args the command line arguments
     */    
    
    
    public static void main(String[] args) {
        // TODO code application logic here
        Scanner teclado = new Scanner (System.in);
        Cliente cliente = new Cliente();
        System.out.println("Escribe la direccion IP del servidor");
        String ip = teclado.next();
        cliente.ipn = ip;
        System.out.println("Escribe el puerto del servidor: ");
        ip = teclado.next();
        cliente.puerto = ip;
        cliente.Conectar();

        
        /*
        System.out.println("*** Escribele al servidor ***");
        for (;;)
        {
            String x = teclado.nextLine();
            cliente.EnviarMensaje(x);
        }
        */
    }
    
}
