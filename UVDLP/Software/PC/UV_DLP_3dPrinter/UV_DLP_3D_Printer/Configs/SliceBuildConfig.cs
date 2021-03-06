﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
namespace UV_DLP_3D_Printer
{
    /*
     * This class holds some information about the 
     * slicing and building parameters
     */
    public class SliceBuildConfig
    {
        public static int FILE_VERSION = 1;
        public string m_filename; // for housekeeping
        public enum eBuildDirection 
        {
            Top_Down,
            Bottom_Up
        }
        public double dpmmX; // dots per mm x
        public double dpmmY; // dots per mm y
        public int xres, yres; // the resolution of the output image in pixels
        public double ZThick; // thickness of the z layer - slicing height
        public int layertime_ms; // time to project image per layer in milliseconds
        public int firstlayertime_ms; // first layer exposure time 
        public int numfirstlayers;
        public int blanktime_ms; // blanking time between layers
       // public int raise_time_ms; // time delay for the z axis to raise on a per-layer basis
        public int plat_temp; // desired platform temperature in celsius 
        public bool exportgcode; // export the gcode file when slicing
        public bool exportsvg; // export the svg slices when building
        public bool exportimages; // export image slices when building
        public eBuildDirection direction;
        public double liftdistance; // distance to lift and retract
        public double slidetiltval; // a value used for slide / tilt 
        public bool antialiasing; // should we use anti-aliasing
        public bool usemainliftgcode; // should we use mainliftgcode-tab instead of generating the gcode
        public bool autocalcdelay; // should we calculate the delay automatically
        public double aaval; // anti-aliasing scaler value - How much to upsample the image values between 1.0 - 3.0 should be fine
        public double liftfeedrate; // initial lift may cause a lot of suction. To maximize lift power, we slow the steppers down to maximize stepper motor torque.
        public double liftretractrate; // the feedrate that this lowers(for bottom-up) or raises(top-down) the build platform, this is the retraction rate of the lift.
        private String m_headercode; // inserted at beginning of file
        private String m_footercode; // inserted at end of file
        private String m_preliftcode; // inserted before each slice
        private String m_postliftcode; // inserted after each slice
        private String m_preslicecode; // inserted before each slice
        private String m_mainliftcode; // inserted before each slice
        public int XOffset, YOffset; // the X/Y pixel offset used 

        private String[] m_defheader = 
        {
            "(********** Header Start ********)\r\n", //
            "Here you can set any G or M-Code\r\n",
            "which should be executed BEFORE\r\n",
            "the Printing-Process starts.\r\n",
            "Here is a good place for the\r\n",
            "Homing Prozedure.\r\n",
            "\r\n",
            "(Generated by UV - DLP Slicer)\r\n",
            "G21 (Set units to be mm)\r\n", 
            "G91 (Relative Positioning)\r\n",
            "M17 (Enable motors)\r\n",
            "(********** Header End **********)\r\n", // 
            //"()\r\n"
        };
        private String[] m_deffooter = 
        {
            "(********** Footer Start ********)\r\n", //
            "Here you can set any G or M-Code\r\n",
            "which should be executed after the\r\n",
            "last Layer is Printed\r\n",
            "\r\n",
            "E.g. G90 (Absolute Positioning)\r\n",
            "then G1 Z 200 (Move Z to Z-Max)\r\n",
            "\r\n",
            "(Generated by UV - DLP Slicer)\r\n",
            "M18 (Disable Motors)\r\n",
            "(<Completed>)\r\n", // a marker for completed            
            "(********** Footer End ********)\r\n", // 
        };

        private String[] m_defprelift = 
        {
            "(********** Pre-Lift Start ********)\r\n", //
            "Here you can set any G or M-Code\r\n",
            "which should be executed directly\r\n",
            "AFTER a Slice is shown.\r\n",
            "\r\n",
            "E.g. G1 Y -20 F5000 to close the Shutter\r\n",
            "(********** Pre-Lift End **********)\r\n",
        };

        private String[] m_defpostlift = 
        {
            "(********** Post-Lift Start ********)\r\n", //
            "Here you can set any G or M-Code\r\n",
            "which should be executed directly\r\n",
            "AFTER the Main-Lift Sequence.\r\n",
            "\r\n",
            "E.g. G1 E 5 F200 to pump some resign\r\n",
            "or M117 'text to be shown on Display'\r\n",
            "(********** Post-Lift End **********)\r\n",
        };

        private String[] m_defpreslice = 
        {
            "(********** Pre-Slice Start ********)\r\n", //
            "Here you can set any G or M-Code\r\n",
            "which should be executed directly\r\n",
            "BEFORE a Slice is shown.\r\n",
            "\r\n",
            "E.g. G1 Y 20 F5000 to open the Shutter\r\n",
            "(********** Pre-Slice End **********)\r\n",
        };
        private String[] m_defmainlift = 
        {
            "(********** Main-Lift Sequence ********)\r\n",// 
            "Here you can set any G or M-Code, which \r\n",
            "should be executed as the Main-Lift, if\r\n",
            "the generated G-Code does not fit your needs.\r\n",
            "The below GCode will only be executed if you\r\n",
            "checked the Checkbox 'Use Main-Lift Gcode'\r\n",
            "\r\n",
            "(Example by UV - DLP Slicer)\r\n",
            "G1 Z 1.5 F21\r\n", 
            "G1 Z 1.5 F210\r\n",
            "G1 Z -2.9 F210\r\n",
            "(********** Main-Lift Sequence **********)\r\n", // 
            //"()\r\n"
        };

            

        private void SetDefaultCodes()
        {
            StringBuilder sb = new StringBuilder();
            foreach (String s in m_defheader)
                sb.Append(s);
            HeaderCode = sb.ToString();

            sb = new StringBuilder(); // clear
            foreach (String s in m_deffooter)
                sb.Append(s);
            FooterCode = sb.ToString();

            sb = new StringBuilder();
            foreach (String s in m_defprelift)
                sb.Append(s);
            PreLiftCode = sb.ToString();

            sb = new StringBuilder();
            foreach (String s in m_defpostlift)
                sb.Append(s);
            PostLiftCode = sb.ToString();

            sb = new StringBuilder();
            foreach (String s in m_defpreslice)
                sb.Append(s);
            PreSliceCode = sb.ToString();

            sb = new StringBuilder();
            foreach (String s in m_defmainlift)
                sb.Append(s);
            MainLiftCode = sb.ToString();
        }

        public String HeaderCode
        {
            get { return m_headercode; }
            set { m_headercode = value; }
        }
        public String FooterCode
        {
            get { return m_footercode; }
            set { m_footercode = value; }
        }
        public String PreLiftCode
        {
            get { return m_preliftcode; }
            set { m_preliftcode = value; }
        }

        public String PostLiftCode
        {
            get { return m_postliftcode; }
            set { m_postliftcode = value; }
        }

        public String PreSliceCode
        {
            get { return m_preslicecode; }
            set { m_preslicecode = value; }
        }

        public String MainLiftCode
        {
            get { return m_mainliftcode; }
            set { m_mainliftcode = value; }
        }

        /*
         Copy constructor
         */
        public SliceBuildConfig(SliceBuildConfig source) 
        {
            dpmmX = source.dpmmX; // dots per mm x
            dpmmY = source.dpmmY; // dots per mm y
            xres = source.xres;
            yres = source.yres; // the resolution of the output image
            ZThick = source.ZThick; // thickness of the z layer - slicing height
            layertime_ms = source.layertime_ms; // time to project image per layer in milliseconds
            firstlayertime_ms = source.firstlayertime_ms;
            blanktime_ms = source.blanktime_ms;
            plat_temp = source.plat_temp; // desired platform temperature in celsius 
            exportgcode = source.exportgcode; // export the gcode file when slicing
            exportsvg = source.exportsvg; // export the svg slices when building
            exportimages = source.exportimages; // export image slices when building
            m_headercode = source.m_headercode; // inserted at beginning of file
            m_footercode = source.m_footercode; // inserted at end of file
            m_preliftcode = source.m_preliftcode; // inserted between each slice            
            m_postliftcode = source.m_postliftcode; // inserted between each slice    
            m_preslicecode = source.m_preslicecode; // inserted before each slice
            m_mainliftcode = source.m_mainliftcode; // inserted before postlift and after prelift. its the main lift code

            liftdistance = source.liftdistance;
            direction = source.direction;
            numfirstlayers = source.numfirstlayers;
            XOffset = source.XOffset;
            YOffset = source.YOffset;
            slidetiltval = source.slidetiltval;
            antialiasing = source.antialiasing;
            usemainliftgcode = source.usemainliftgcode;
            autocalcdelay = source.autocalcdelay;
            liftfeedrate = source.liftfeedrate;
            liftretractrate = source.liftretractrate;
            aaval = source.aaval;//
            //raise_time_ms = source.raise_time_ms;
        }

        public SliceBuildConfig() 
        {           
            // every new config will be set to default settings,
            // if the load fails for some reason (such as a previous version)
            // then whatever information that has already been loaded will stay
            // the rest will have been set up to the default values
            CreateDefault(); 
        }
        public void UpdateFrom(MachineConfig mf)
        {
            dpmmX = mf.PixPerMMX; //10 dots per mm
            dpmmY = mf.PixPerMMY;// 10;
            xres = mf.XRes;
            yres = mf.YRes;            
        }
        public void CreateDefault() 
        {
            numfirstlayers = 3;
            layertime_ms = 1000;// 1 second default
            firstlayertime_ms = 5000;
            blanktime_ms = 2000; // 2 seconds blank
            xres = 1024;
            yres = 768;
            ZThick = .05;
            plat_temp = 75;
            dpmmX = 102.4;
            dpmmY = 76.8;
            XOffset = 0;
            YOffset = 0;
            numfirstlayers = 3;
            exportgcode = true;
            exportsvg = false;
            exportimages = false;
            direction = eBuildDirection.Bottom_Up;
            liftdistance = 5.0;
            //raise_time_ms = 750;
            slidetiltval = 0.0;
            antialiasing = false;
            usemainliftgcode = false;
            autocalcdelay = false;
            aaval = 1.5;
            liftfeedrate = 50.0;// 50mm/s
            liftretractrate = 100.0;// 100mm/s
            SetDefaultCodes(); // set up default gcodes
        }

        /*This is used to serialize to the GCode post-header info*/
        public bool Load(String filename) 
        {
            try
            {
                m_filename = filename;
                LoadGCodes();
                XmlReader xr = (XmlReader)XmlReader.Create(filename);
                xr.ReadStartElement("SliceBuildConfig");
                int ver = int.Parse(xr.ReadElementString("FileVersion"));
                if (ver != FILE_VERSION) 
                {
                    return false; // I may try to implement some backward compatibility here...
                }
                dpmmX = double.Parse(xr.ReadElementString("DotsPermmX"));
                dpmmY = double.Parse(xr.ReadElementString("DotsPermmY"));
                xres = int.Parse(xr.ReadElementString("XResolution"));
                yres = int.Parse(xr.ReadElementString("YResolution"));
                ZThick = double.Parse(xr.ReadElementString("SliceHeight"));
                layertime_ms = int.Parse(xr.ReadElementString("LayerTime"));
                firstlayertime_ms = int.Parse(xr.ReadElementString("FirstLayerTime"));
                blanktime_ms = int.Parse(xr.ReadElementString("BlankTime"));
                plat_temp = int.Parse(xr.ReadElementString("PlatformTemp"));
                exportgcode = bool.Parse(xr.ReadElementString("ExportGCode"));
                exportsvg = bool.Parse(xr.ReadElementString("ExportSVG"));
                exportimages = bool.Parse(xr.ReadElementString("ExportImages")); ;
                XOffset = int.Parse(xr.ReadElementString("XOffset"));
                YOffset = int.Parse(xr.ReadElementString("YOffset"));
                numfirstlayers = int.Parse(xr.ReadElementString("NumberofBottomLayers"));
                direction = (eBuildDirection)Enum.Parse(typeof(eBuildDirection), xr.ReadElementString("Direction"));
                liftdistance = double.Parse(xr.ReadElementString("LiftDistance"));
                slidetiltval = double.Parse(xr.ReadElementString("SlideTiltValue"));
                antialiasing = bool.Parse(xr.ReadElementString("AntiAliasing"));
                autocalcdelay = bool.Parse(xr.ReadElementString("AutoCalcDelay"));
                usemainliftgcode = bool.Parse(xr.ReadElementString("UseMainLiftGCode"));
                aaval = double.Parse(xr.ReadElementString("AntiAliasingValue"));
                liftfeedrate = double.Parse(xr.ReadElementString("LiftFeedRate"));
                liftretractrate = double.Parse(xr.ReadElementString("LiftRetractRate"));

                xr.ReadEndElement();
                xr.Close();
                
                return true;
            }
            catch (Exception ex)
            {
                DebugLogger.Instance().LogRecord(ex.Message);
                return false;
            }       
        }
        public bool Save(String filename)
        {
            try 
            {
                m_filename = filename;
                XmlWriter xw =XmlWriter.Create(filename);
                xw.WriteStartElement("SliceBuildConfig");
                xw.WriteElementString("FileVersion",FILE_VERSION.ToString());
                xw.WriteElementString("DotsPermmX", dpmmX.ToString());
                xw.WriteElementString("DotsPermmY", dpmmY.ToString());
                xw.WriteElementString("XResolution", xres.ToString());
                xw.WriteElementString("YResolution", yres.ToString());
                xw.WriteElementString("SliceHeight", ZThick.ToString());
                xw.WriteElementString("LayerTime", layertime_ms.ToString());
                xw.WriteElementString("FirstLayerTime", firstlayertime_ms.ToString());
                xw.WriteElementString("BlankTime", blanktime_ms.ToString());                
                xw.WriteElementString("PlatformTemp", plat_temp.ToString());
                xw.WriteElementString("ExportGCode", exportgcode.ToString());
                xw.WriteElementString("ExportSVG", exportsvg.ToString());
                xw.WriteElementString("ExportImages", exportimages.ToString());
                xw.WriteElementString("XOffset", XOffset.ToString());
                xw.WriteElementString("YOffset", YOffset.ToString());
                xw.WriteElementString("NumberofBottomLayers", numfirstlayers.ToString());
                xw.WriteElementString("Direction", direction.ToString());
                xw.WriteElementString("LiftDistance", liftdistance.ToString());
                xw.WriteElementString("SlideTiltValue", slidetiltval.ToString());
                xw.WriteElementString("AntiAliasing", antialiasing.ToString());
                xw.WriteElementString("AutoCalcDelay", autocalcdelay.ToString());
                xw.WriteElementString("UseMainLiftGCode", usemainliftgcode.ToString());
                xw.WriteElementString("AntiAliasingValue", aaval.ToString());
                xw.WriteElementString("LiftFeedRate", liftfeedrate.ToString());
                xw.WriteElementString("LiftRetractRate", liftretractrate.ToString());

               // xw.WriteElementString("Raise_Time_Delay",raise_time_ms.ToString());
                xw.WriteEndElement();
                xw.Close();
                SaveGCodes();
                return true;
            }
            catch (Exception ex) 
            {
                DebugLogger.Instance().LogRecord(ex.Message);
                return false;
            }            
        }

        // these get stored to the gcode file as a reference
        public override String ToString() 
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(****Build and Slicing Parameters****)\r\n");
            sb.Append("(Pix per mm X            = " + String.Format("{0:0.00000}", dpmmX) + " px/mm )\r\n");
            sb.Append("(Pix per mm Y            = " + String.Format("{0:0.00000}", dpmmY) + " px/mm )\r\n");
            sb.Append("(X Resolution            = " + xres + " px )\r\n");
            sb.Append("(Y Resolution            = " + yres + " px )\r\n");
            sb.Append("(X Pixel Offset          = " + XOffset + " px )\r\n");
            sb.Append("(Y Pixel Offset          = " + YOffset + " px )\r\n");
            sb.Append("(Layer Thickness         = " + String.Format("{0:0.00000}", ZThick) + " mm )\r\n");
            sb.Append("(Layer Time              = " + layertime_ms + " ms )\r\n");
            sb.Append("(Bottom Layers Time        = " + firstlayertime_ms + " ms )\r\n");
            sb.Append("(Number of Bottom Layers = " + numfirstlayers + " )\r\n");
            sb.Append("(Blanking Layer Time     = " + blanktime_ms + " ms )\r\n");
            sb.Append("(Build Direction         = " + direction.ToString() + ")\r\n");
            sb.Append("(Lift Distance           = " + liftdistance.ToString() + " mm )\r\n");
            sb.Append("(Slide/Tilt Value        = " + slidetiltval.ToString() + ")\r\n");
            sb.Append("(Anti Aliasing           = " + antialiasing.ToString() + ")\r\n");
            sb.Append("(Auto Calculate Delay    = " + autocalcdelay.ToString() + ")\r\n");
            sb.Append("(Use Mainlift GCode Tab  = " + usemainliftgcode.ToString() + ")\r\n");
            sb.Append("(Anti Aliasing Value     = " + aaval.ToString() + " )\r\n");
            sb.Append("(Z Lift Feed Rate        = " + String.Format("{0:0.00000}", liftfeedrate) + " mm/s )\r\n");
            sb.Append("(Z Lift Retract Rate     = " + String.Format("{0:0.00000}", liftretractrate) + " mm/s )\r\n");
            return sb.ToString();
        }

        public void LoadGCodes() 
        {
            try
            {

                String profilepath = Path.GetDirectoryName(m_filename);                
                profilepath += UVDLPApp.m_pathsep;
                profilepath += Path.GetFileNameWithoutExtension(m_filename);
                if (!Directory.Exists(profilepath))
                {
                    Directory.CreateDirectory(profilepath);
                    SetDefaultCodes();
                    SaveGCodes();// save the default gcode files for this machine
                }
                else
                {
                    //load the files
                    m_headercode = LoadFile(profilepath + UVDLPApp.m_pathsep + "start.gcode");
                    m_footercode = LoadFile(profilepath + UVDLPApp.m_pathsep + "end.gcode");
                    m_preliftcode = LoadFile(profilepath + UVDLPApp.m_pathsep + "prelift.gcode");
                    m_postliftcode = LoadFile(profilepath + UVDLPApp.m_pathsep + "postlift.gcode");
                    m_preslicecode = LoadFile(profilepath + UVDLPApp.m_pathsep + "preslice.gcode");
                    m_mainliftcode = LoadFile(profilepath + UVDLPApp.m_pathsep + "mainlift.gcode");
                }
            }
            catch (Exception ex) 
            {
                DebugLogger.Instance().LogRecord(ex.Message);
            }
        }

        public String LoadFile(String filename) 
        {
            try
            {
                return File.ReadAllText(filename);
            }
            catch (Exception ex) 
            {
                DebugLogger.Instance().LogRecord(ex.Message);
                return "";
            }
        }

        public bool SaveFile(String filename, String contents) 
        {
            try
            {
                File.WriteAllText(filename, contents);
                return true;
            }
            catch (Exception ex) 
            {
                DebugLogger.Instance().LogRecord(ex.Message);
                return false;
            }
        }
        public void SaveGCodes() 
        {
            try
            {
                String profilepath = Path.GetDirectoryName(m_filename);
                profilepath += UVDLPApp.m_pathsep;
                profilepath += Path.GetFileNameWithoutExtension(m_filename);
                //create the directory if it doesn't exist
                if (!Directory.Exists(profilepath))
                {
                    Directory.CreateDirectory(profilepath);
                }

                SaveFile(profilepath + UVDLPApp.m_pathsep + "start.gcode", m_headercode);
                SaveFile(profilepath + UVDLPApp.m_pathsep + "end.gcode", m_footercode);
                SaveFile(profilepath + UVDLPApp.m_pathsep + "prelift.gcode", m_preliftcode);
                SaveFile(profilepath + UVDLPApp.m_pathsep + "postlift.gcode", m_postliftcode);
                SaveFile(profilepath + UVDLPApp.m_pathsep + "preslice.gcode", m_preslicecode);
                SaveFile(profilepath + UVDLPApp.m_pathsep + "mainlift.gcode", m_mainliftcode);
            }
            catch (Exception ex) 
            {
                DebugLogger.Instance().LogError(ex.Message);
            }
        }
    }
}
