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
        Logger logger = new Logger(1);
        int retryConnectionLimit = 50;

        RandomAccessFile pipe = null;
        try {
            logger.infoLog("Wait server start.");
            System.out.println("Waiting server stands up ");

            // if check `new File("\\\\.\\pipe\\" + serverName).exists()`, pipe instance become busy.
            while(retryConnectionLimit > 0){
                try{
                    pipe = new RandomAccessFile("\\\\.\\pipe\\" + serverName, "rw");
                    break;
                }
                catch( FileNotFoundException ex){
                    System.out.print(".");
                    retryConnectionLimit--;
                    try{
                        Thread.sleep( 1000 );
                    }
                    catch( InterruptedException e) {
                    }
                }
            }
            if(retryConnectionLimit == 0){
                System.out.println();
                System.out.println("Error: Connection Timeout.");
                logger.infoLog("Wait server timeout.");
                return;
            }
            logger.infoLog("Wait server end.");

            logger.infoLog("Read loop start.");

            // byte[] sizeInfo = new byte[4];
            // try{
            //     while(true) {
            //         pipe.readFully(sizeInfo);
            //         ByteBuffer wrapped = ByteBuffer.wrap(sizeInfo);
            //         int size = wrapped.getInt();
            //         byte[] resBytes = new byte[size];
            //         pipe.readFully(resBytes);
            //         final String data = new String(resBytes, Charset.forName("UTF-8"));
            //         pipe.readFully(sizeInfo);
            //     }
            // }catch(IOException ioex){
            //     ioex.printStackTrace();
            // }
            //
            // if(sizeInfo==null) return;


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
                    logger.infoLog("Server is closed.");
                    break;
                }
                logger.debugLog("Response: " + echoResponse);
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
        logger.infoLog("Read loop end.");

    }
}
