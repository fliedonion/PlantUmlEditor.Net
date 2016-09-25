import net.case_of_t.lib.win.ProcessCollector;
import net.case_of_t.lib.win.ProcessCommandLineInfo;

import java.io.*;
import java.nio.file.DirectoryStream;
import java.nio.file.FileSystems;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.ByteBuffer;
import java.nio.charset.Charset;
import java.util.List;
import java.util.concurrent.TimeoutException;

public class NamedPipeBackgroundRenderer {

    // VSDebug=true / false : true, if NamedPipeServer run under VSDebugger.
    // processId=<pid of namedpipe server>  : process id of NamedPipeServer.
    // processName=<ProcessName> : process name of NamedPipeServer.  do not include spaces.
    // `VSDebug = true`, `VSDebug= true` and `VSDebug =true` are wrong. do not include space between equal sign("=") and operand.
    //
    // ~.jar VSDebug=true
    // ~.jar ProcessId=<pid> processName=CaseOfT.Net.PlantUMLClient.exe

    public static void main(String[] args){
        if(args.length == 0){
            System.out.println("there are no arguments.");
        }else{

            ParentInfo pi = new ParentInfo();
            for(String arg : args){
                String[] splitted = arg.split("=" ,2);

                if(splitted.length == 2){
                    if(!pi.applyField(splitted[0], splitted[1])){
                        System.err.println("unknown param or invalid value :" + splitted[0]);
                    }
                }else{
                    System.err.println("unknown param" + arg);
                }
            }

            if(!pi.isVsDebug() && pi.getProcessId() == -1){
                System.err.println("processId is not specified.");
                return;
            }
            if(!pi.isVsDebug() && pi.getProcessName().equals("")){
                System.err.println("processName is not specified.");
                return;
            }

            new NamedPipeBackgroundRenderer().communicate(pi);
        }
    }
    // CaseOfT.Net.PlantUMLClient

    private final String serverNameBase = "PlantUmlNPServer";
    private final String debugProcessName = "CaseOfT.Net.PlantUMLClient.vshost.exe";

    boolean isParentAlive(ParentInfo pi){
        ProcessCollector collector = new ProcessCollector();
        List<ProcessCommandLineInfo> list;
        if(pi.isVsDebug()){
            // When server running with visual studio Debugging, only one process can run.
            // Because second process' NamedPipeServer name will conflict with first process.
            list = collector.collectTargetProcessWhichNameIs(debugProcessName);
        }
        else{
            list = collector.collectTargetProcessWhichProcessIdIs(pi.getProcessId());
        }
        if(list.size() == 0) return false;

        return true;
    }

    String getServerName(ParentInfo pi){
        if(pi.isVsDebug() && pi.getProcessId() == -1){
            return serverNameBase;
        }
        else {
            return serverNameBase + "-" + pi.getProcessId();
        }
    }

    private final int retryWaitMsForNamedPipeServerSearch = 5000;
    private final int retryWaitMsForWaitingServerStart = 10000;

    boolean namedPipeServerExists(String name){
        return namedPipeServerExists(name, 1);
    }

    boolean namedPipeServerExists(String name, int retry){
        Path dir = FileSystems.getDefault().getPath("\\\\.\\pipe\\");
        while(retry > 0){
            retry--;
            try(DirectoryStream<Path> ds = Files.newDirectoryStream(dir, name)) {
                for (Path path : ds) {
                    return true;
                }
                if (retry > 0){
                    try {
                        Thread.sleep(retryWaitMsForNamedPipeServerSearch);
                    } catch (InterruptedException e) {
                        Thread.currentThread().interrupt();
                    }
                }
            }catch(IOException iox){
                // TODO: implement.
                return false;
            }
        }
        return false;
    }

    ProcessCommandLineInfo getServerProcess(String processName, int processId){
        ProcessCollector collector = new ProcessCollector();
        List<ProcessCommandLineInfo> list =  collector.collectTargetProcessWhichNameIsAndProcessIdIs(processName, processId);
        if(list.size()==1) return list.get(0);
        return null;
    }

    ProcessCommandLineInfo getServerProcess(String processName)
        throws RuntimeException{

        ProcessCollector collector = new ProcessCollector();
        List<ProcessCommandLineInfo> list =  collector.collectTargetProcessWhichNameIs(processName);
        if(list.size() > 1){
            for(ProcessCommandLineInfo pci : list){
                System.err.println(pci.getName() + ", " + pci.getProcessId() + "," + pci.getCommandLine());
            }
            throw new RuntimeException("Too Many Server Processes found.");
        }
        if(list.size()==1) return list.get(0);
        return null;
    }

    ProcessCommandLineInfo getServerProcess(ParentInfo pi){
        String procName = pi.isVsDebug() && pi.getProcessName().equals("") ? debugProcessName : pi.getProcessName();
        if(pi.isVsDebug() && pi.getProcessId() == -1){
            return getServerProcess(procName);
        }else{
            return getServerProcess(procName, pi.getProcessId());
        }
    }

    ProcessCommandLineInfo waitForServerProcessStart(ParentInfo pi)
        throws TimeoutException {

        int retry = 30;
        while (retry > 0){
            retry--;
            ProcessCommandLineInfo info = getServerProcess(pi);
            if(info != null) return info;

            if(retry > 0){
                try{
                    Thread.sleep(retryWaitMsForWaitingServerStart);
                }catch(InterruptedException e){
                    Thread.currentThread().interrupt();
                }

            }
        }
        throw new TimeoutException("can't detect server process");
    }

    boolean isServerProcessAlive(ProcessCommandLineInfo serverInfo){
        return getServerProcess(serverInfo.getName(), serverInfo.getProcessId()) != null;
    }

    void communicate(ParentInfo pi){
        Logger logger = new Logger(1);
        ProcessCommandLineInfo serverInfo = null;
        try {
            logger.infoLog("Wait server start.");
            serverInfo = waitForServerProcessStart(pi);

        }catch(TimeoutException ex){
            logger.infoLog(ex.getMessage());
            System.err.println("Server not found.");
            return;
        }
        logger.infoLog(String.format("server process detected %s : %d.", serverInfo.getName(), serverInfo.getProcessId()));

        logger.infoLog("NamedPipeServer search start.");
        String namedPipeServerName = getServerName(pi);
        if(!namedPipeServerExists(namedPipeServerName, 20)){
            if(!isServerProcessAlive(serverInfo)){
                logger.infoLog("Server process was gone, While waiting NamedPipeServer Starts. Program will exit.");
                return;
            }

            // todo: retry waiting namedpipeserver, if need.
            logger.infoLog("Can't detect NamedPipeServer Started. Program will exit.");
            return;
        }
        logger.infoLog("NamedPipeServer Detected.");

        boolean firstTime = true;
        while(true){
            if(isServerProcessAlive(serverInfo)){
                logger.infoLog("Server process was gone. Program will exit.");
                break;
            }
            if(!firstTime){
                boolean found = false;
                logger.infoLog("Current process was closed by unknown reason. Waiting restart NamedPipeServer.");
                for(int i = 0; i< 10; i++){
                    if(namedPipeServerExists(namedPipeServerName, 12 * 2)){
                        found = true;
                        break;
                    }
                    if(!isServerProcessAlive(serverInfo)){
                        found = false;
                        break;
                    }
                }
                if(!found){
                    logger.infoLog("Server process closed or NamedPipeServer missing long time. Program will exit.");
                    break;
                }
            }

            try{
                waitRequest(pi);
            } catch( FileNotFoundException nex){
                // Namedpipe detected but FileNotFoundException occurred.
                logger.infoLog("error: NamedPipe Not Found.");
                break;
            } catch( IOException e){
                logger.infoLog("error: IOException :" + e.getMessage());

            } catch (Exception e){
                e.printStackTrace();
                logger.infoLog("error: Exception :" + e.getMessage());
                break;
            }
            firstTime = false;
        }
        logger.infoLog("Read loop end.");
    }


    private void waitRequest(ParentInfo pi)
        throws IOException{
        try(RandomAccessFile pipe = new RandomAccessFile("\\\\.\\pipe\\" + getServerName(pi), "rw")){
            byte[] sizeInfo = new byte[4];
            String data = "";
            try{
                while(true) {
                    pipe.readFully(sizeInfo);
                    ByteBuffer wrapped = ByteBuffer.wrap(sizeInfo);
                    int size = wrapped.getInt();
                    if(size > 0) {
                        byte[] reqBytes = new byte[size];
                        pipe.readFully(reqBytes);
                        data = new String(reqBytes, Charset.forName("UTF-8"));

                        if(data.equals("+OK Accepted.\n")){
                            System.out.println("Receive communicate message.");
                        }else{
                            byte[] resBytes = PlantUmlWrapper.generateSvg(data).getBytes("utf-8");
                            byte[] sendSizeInfo = ByteBuffer.allocate(4).putInt(resBytes.length).array();
                            pipe.write(sendSizeInfo);
                            pipe.write(resBytes);
                        }
                    }else{
                        System.out.println("size is 0. skip.");
                    }
                }
            }catch(EOFException eofex){
                System.out.println("Server may closed.");
            }catch(IOException ioex){
                ioex.printStackTrace();
            }

        } finally{

        }
    }



    private void obsoleteCode() throws Exception{
        Logger logger = new Logger(1);
        RandomAccessFile pipe = null;
        String namedPipeServerName = serverNameBase;
        try {
            logger.infoLog("NamedPipeServer Detected.");
            pipe = new RandomAccessFile("\\\\.\\pipe\\" + namedPipeServerName, "rw");
            logger.infoLog("Pipe Open Succeed.");

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
            // }catch(EOFException eofex){
            //     // ioex.printStackTrace(); // may be connection closed.
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
                    // todo: if serverProcess alive, waiting namedpipeserver again and try reconnect.
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
        } catch( FileNotFoundException nex){
            // Namedpipe detected but FileNotFoundException occurred.
            System.out.println("NamedPipe Not Found. Program will exit.");
            return;

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
