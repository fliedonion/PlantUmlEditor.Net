import net.sourceforge.plantuml.FileFormat;
import net.sourceforge.plantuml.FileFormatOption;
import net.sourceforge.plantuml.SourceStringReader;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.nio.charset.Charset;

class PlantUmlWrapper {
    static String generateSvg(String data){
        SourceStringReader reader = new SourceStringReader(data);
        final ByteArrayOutputStream os = new ByteArrayOutputStream();
        try{
            String desc = reader.generateImage(os, new FileFormatOption(FileFormat.SVG));
            os.close();
            return new String(os.toByteArray(), Charset.forName("UTF-8"));
        }catch(IOException ex){
            return ex.toString();
        }
    }
}
