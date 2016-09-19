import java.io.*;
import java.nio.ByteBuffer;
import java.nio.charset.Charset;

public class NamedPipeBackgroundRenderer {

    public static void main(String[] args){
        if(args.length == 0){
            System.out.println("there are no arguments.");
        }else{
            communicate(args[0]);
        }
    }

    static void communicate(String serverName){

        RandomAccessFile pipe = null;
        try {
            // if check `new File("\\\\.\\pipe\\" + serverName).exists()`, pipe instance become busy.

            pipe = new RandomAccessFile("\\\\.\\pipe\\" + serverName, "rw");

            while(pipe.length() == 0){
                // TODO: use readFully instead of readLine, after Test server closed behavior.
                // byte[] sizeInfo = new byte[4];
                // pipe.readFully(sizeInfo);

                // ByteBuffer wrapped = ByteBuffer.wrap(sizeInfo);
                // int size = wrapped.getInt();
                // byte[] resBytes = new byte[size];
                // pipe.readFully(resBytes);
                // final String data = new String(resBytes, Charset.forName("UTF-8"));

                String echoResponse = pipe.readLine();
                while(pipe.length() != 0){
                    echoResponse += "\n" + pipe.readLine();
                }
                if(echoResponse == null){
                    System.out.println("Server is closed.");
                    break;
                }
                System.out.println("Response: " + echoResponse);
                if(echoResponse.equals("<EXIT>")){
                    break;
                }
                String s = "I Accepted : " + PlantUmlWrapper.generateSvg(echoResponse);
                pipe.write(s.getBytes("utf-8"));
            }
            pipe.close();

        } catch( EOFException eof){
            System.out.println("EOF detected.");
            eof.printStackTrace();
        } catch( IOException e){
            System.out.println("IOError.");
            e.printStackTrace();
        } catch (Exception e){
            e.printStackTrace();
        }
        finally{
            try{
                if(pipe !=null) pipe.close();
            }catch(Exception e) {}
        }
    }


}
