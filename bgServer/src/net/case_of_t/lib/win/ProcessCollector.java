package net.case_of_t.lib.win;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.LinkedList;
import java.util.List;


public class ProcessCollector {

    private boolean runUnderWindows = false;

    public ProcessCollector(){
        runUnderWindows = System.getProperty("os.name").toLowerCase().startsWith("wind");
    }

    private String createWmiExecutionString(boolean useLikeOperator, String paramName){
        return String.format("wmic.exe process where (name %s '%s') get name,ProcessId,CommandLine /format:LIST",
                useLikeOperator ? "like" : "=" ,
                paramName);
    }

    private String createWmiExecutionString(int paramProcessId){
        return String.format("wmic.exe process where (ProcessId = %d) get name,ProcessId,CommandLine /format:LIST",
                paramProcessId);
    }

    private String createWmiExecutionString(boolean useLikeOperator, String paramName, int paramProcessId){
        return String.format("wmic.exe process where (name %s '%s'and ProcessId = %d) get name,ProcessId,CommandLine /format:LIST",
                useLikeOperator ? "like" : "=" ,
                paramName,
                paramProcessId);
    }

    public List<ProcessCommandLineInfo> collectTargetProcessWhichNameIsAndProcessIdIs(String name, int processId){
        return collectTargetProcess(createWmiExecutionString(false, name, processId));
    }

    public List<ProcessCommandLineInfo> collectTargetProcessWhichNameLikeAndProcessIdIs(String name, int processId){
        return collectTargetProcess(createWmiExecutionString(true, name, processId));
    }

    public List<ProcessCommandLineInfo> collectTargetProcessWhichProcessIdIs(int processId){
        return collectTargetProcess(createWmiExecutionString(processId));
    }

    public List<ProcessCommandLineInfo> collectTargetProcessWhichNameIs(String name){
        return collectTargetProcess(createWmiExecutionString(false, name));
    }

    public List<ProcessCommandLineInfo> collectTargetProcessWhichNameLike(String name){
        return collectTargetProcess(createWmiExecutionString(true, name));
    }

    public List<ProcessCommandLineInfo> collectTargetProcessWhichNameContains(String name){
        return collectTargetProcess(createWmiExecutionString(true, String.format("%%%%%s%%%%", name)));
    }


    protected List<ProcessCommandLineInfo> collectTargetProcess(String wmiExecString){
        List<ProcessCommandLineInfo> list = new LinkedList<>();

        String line;
        try {
            Process proc = Runtime.getRuntime().exec(wmiExecString);
            BufferedReader input = new BufferedReader(new InputStreamReader(proc.getInputStream()));

            boolean itemIsEmpty = false;
            ProcessCommandLineInfo pi = null;
            while ((line = input.readLine()) != null) {
                if(line.trim().equals("")){
                    if(!itemIsEmpty){
                        if(pi != null){
                            list.add(pi);
                        }
                        pi = new ProcessCommandLineInfo();
                        itemIsEmpty = true;
                    }
                }
                else{
                    String[] splitted = line.split("=" ,2);
                    if(pi.applyField(splitted[0], splitted[1])){
                        itemIsEmpty = false;
                    }
                    if(runUnderWindows){
                        input.readLine(); // skip empty line for windows \r\n.
                        // When we can use JDK1.8, use scanner with delimiter "\r\n" instead of BufferedReader is easy way to solve this.
                    }
                }
            }
            if(!itemIsEmpty && pi != null){
                list.add(pi);
            }

            input.close();
            proc.getOutputStream().close();
            proc.getErrorStream().close();

        } catch (IOException ioe) {
            ioe.printStackTrace();
        }
        return list;
    }
}
