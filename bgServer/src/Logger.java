public class Logger {
    private int logLevel = 0;

    Logger(){
    }
    Logger(int logLevel){
        this.logLevel = logLevel;
    }

    int getLogLevel(){
        return logLevel;
    }
    void setLogLevel(int value){
        logLevel = value;
    }
    void debugLog(String s){
        if(logLevel >= 4){
            System.out.println(s);
        }
    }
    void infoLog(String s){
        if(logLevel >= 1){
            System.out.println(s);
        }
    }
}
