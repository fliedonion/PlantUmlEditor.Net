package net.case_of_t.puml_bgrender;

import net.case_of_t.lib.puml.PlantUmlWrapper;

import java.io.PrintStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.DataInputStream;
import java.io.EOFException;
import java.net.ServerSocket;
import java.net.Socket;
import java.nio.ByteBuffer;
import java.nio.charset.Charset;

/**
 * BackgroundRenderer render PlantUml text to SVG through TCP.
 *
 * this is obsolete.
 */
class BackgroundRenderer {

    // please run under Command prompt.
    public static void main(String[] args) {
        ServerSocket sv = null;
        String line;

        PrintStream os;
        Socket recSocket = null;

        try{
            sv = new ServerSocket(3000);
        }catch(IOException ex){
            System.out.println(ex);
            System.out.println("Server terminate.");
            return;
        }

        try{
            byte[] sizeInfo = new byte[4];

            while(true){
                recSocket = sv.accept();

                try {
                    InputStream is = recSocket.getInputStream();
                    DataInputStream dis = new DataInputStream(is);

                    dis.readFully(sizeInfo);

                    ByteBuffer wrapped = ByteBuffer.wrap(sizeInfo);
                    int size = wrapped.getInt();
                    byte[] resBytes = new byte[size];

                    dis.readFully(resBytes);
                    final String data = new String(resBytes, Charset.forName("UTF-8"));

                    os = new PrintStream(recSocket.getOutputStream());
                    os.println(PlantUmlWrapper.generateSvg(data));
                    if (!recSocket.isConnected()) recSocket.close();
                }catch(EOFException ex){
                    System.out.println("EOF:");
                    System.out.println(ex);
                }catch(IOException ex){
                    System.out.println("IO:");
                    System.out.println(ex);
                }
            }

        }catch(IOException ex){
            System.out.println(ex);
        }
    }
}

