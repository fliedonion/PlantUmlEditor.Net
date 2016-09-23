package net.case_of_t.lib.win;

import java.lang.reflect.Field;

import static java.lang.Integer.parseInt;

public class ProcessCommandLineInfo {

    public String getCommandLine() {
        return commandLine;
    }

    public void setCommandLine(String commandLine) {
        this.commandLine = commandLine;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public int getProcessId() {
        return processId;
    }

    public void setProcessId(int processId) {
        this.processId = processId;
    }

    private String commandLine;
    private String name;
    private int processId;

    public boolean applyField(String fieldName, String value){
        try{
            return applyField(fieldName, value, false);
        }catch(NoSuchFieldException ex){
            return false;
        }
    }

    public boolean applyField(String fieldName, String value, boolean throwIfNotFoundSuchField)
        throws NoSuchFieldException{
        fieldName = fieldName.toUpperCase();
        Field field;
        switch(fieldName){
            case "NAME":
                setName(value);
                break;
            case "COMMANDLINE":
                setCommandLine(value);
                break;
            case "PROCESSID":
                if(value.trim().equals(""))
                    setProcessId(-1);
                else
                    setProcessId(parseInt(value.trim()));
                break;
            default:
                if(throwIfNotFoundSuchField){
                    throw new NoSuchFieldException(fieldName);
                }
                return false;
        }
        return true;

    }
}
