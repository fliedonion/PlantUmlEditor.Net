import java.lang.reflect.Field;

import static java.lang.Integer.parseInt;

public class ParentInfo {

    public String getProcessName() {
        return processName;
    }

    private void setProcessName(String processName) {
        this.processName = processName;
    }

    public int getProcessId() {
        return processId;
    }

    private void setProcessId(int processId) {
        this.processId = processId;
    }

    private String processName = "";
    private int processId = -1;
    private boolean vsDebug = false;

    public boolean isVsDebug() {
        return vsDebug;
    }

    private void setVsDebug(boolean vsDebug) {
        this.vsDebug = vsDebug;
    }

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
                setProcessName(value.trim());
                break;
            case "PROCESSID":
                if(value.trim().equals(""))
                    setProcessId(-1);
                else
                    setProcessId(parseInt(value.trim()));
                break;
            case "VSDEBUG":
                if(value.trim().toUpperCase().equals("TRUE") || value.trim().toUpperCase().equals("1"))
                    setVsDebug(true);
                else
                    setVsDebug(false);
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
