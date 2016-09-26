package net.case_of_t.lib.puml;

import net.sourceforge.plantuml.FileFormat;
import net.sourceforge.plantuml.FileFormatOption;
import net.sourceforge.plantuml.SourceStringReader;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.UnsupportedEncodingException;
import java.nio.charset.Charset;

public class PlantUmlWrapper {

    public static String generateSvg(String data){
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

    public enum OutPUmlFormats{
        SVG,
        PNG,
        ANIMATED_GIF,
        PDF,
        ATXT,
        UTXT,
        EPS,
        HTML,
        HTML5,
        LATEX,
        MJPEG,
        BRAILLE_PNG,
        SCXML,
        VDX
    }

    private static FileFormat ToFileFormat(OutPUmlFormats format)
        throws IllegalArgumentException{
        switch(format){
            case SVG:
                return FileFormat.SVG;
            case PNG:
                return FileFormat.PNG;
            case ANIMATED_GIF:
                return FileFormat.ANIMATED_GIF;
            case PDF:
                return FileFormat.PDF;
            case ATXT:
                return FileFormat.ATXT;
            case UTXT:
                return FileFormat.UTXT;
            case EPS:
                return FileFormat.EPS;
            case HTML:
                return FileFormat.HTML;
            case HTML5:
                return FileFormat.HTML5;
            case LATEX:
                return FileFormat.LATEX;
            case MJPEG:
                return FileFormat.MJPEG;
            case BRAILLE_PNG:
                return FileFormat.BRAILLE_PNG;
            case SCXML:
                return FileFormat.SCXML;
            case VDX:
                return FileFormat.VDX;
            default:
                throw new IllegalArgumentException("Invalid Format");
        }
    }

    public static byte[] generate(String data, OutPUmlFormats format){
        try{
            FileFormat fmt = ToFileFormat(format);

            SourceStringReader reader = new SourceStringReader(data);
            final ByteArrayOutputStream os = new ByteArrayOutputStream();
            String desc = reader.generateImage(os, new FileFormatOption(fmt));
            os.close();
            return os.toByteArray();

        }catch(IOException ex){
            ex.printStackTrace();
            try{
                return ex.toString().getBytes("utf-8");
            }catch(UnsupportedEncodingException uex){
                return ex.toString().getBytes();
            }
        }catch(Exception ex){
            ex.printStackTrace();
            try{
                return ex.toString().getBytes("utf-8");
            }catch(UnsupportedEncodingException uex){
                return ex.toString().getBytes();
            }
        }
    }


}
