//--------------------------------------------------------------------------------------------
//File:   Options.cs
//Desc:   This program defines a class Options which contains logic for commandline parsing.
//               
//---------------------------------------------------------------------------------------------

//This is where the file verification and part goes

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

//Thoughts: when writing error handling, maybe make the error message specific
namespace armsim
{
    public class Options
    {
        List<string> allowed = new List<string>() { "--mem"};

        List<string> errMsg = new List<string>()
        {
            null,
            "Usage: armsim [ --mem memory-size ] [elf-file]",
            "Invalid option",
            "Invalid memory size",
            "Invalid file"
        };

        public int Memsize { get; set; }
        public string Filename { get; set; }
        public string Valid { get; set; }
        public bool Exec { get; set; }
        public bool Traceall { get; set; }
        
        public Options() { Memsize = 32768; Filename = ""; Valid = null; Traceall = false; Exec = false; }

        //Validates that options are correct and allowed
        public void VldOpt(string[] args)
        {
            string s;
            int valid = 0;
            
            for(int i = 0; i < args.Length; ++i)
            {
                s = args[i];
                if(s.Substring(0, 2) == "--")
                {
                    switch (s)
                    {
                        case "--mem":
                            try { Memsize = Convert.ToInt32(args[i + 1]);}
                            catch (Exception){ Memsize = 6000000; }
                            finally { ; }
                            valid = VldMem(Memsize) ? 0 : 3;
                            ++i;
                            break;
                        case "--exec":
                            Exec = true;
                            break;
                        case "--traceall":
                            Traceall = true;
                            break;
                        default:
                            valid = 2;
                            break;
                    } 
                    
                } else if ((i == args.Length - 1) && valid == 0)
                {
                    Filename = s;
                    valid = Vldfile(Filename) ? 0 : 4;
                }
                else { valid = 1; }

            }
            
            Valid = errMsg[valid];
        }

        //Utility functions for validations 
        public bool VldMem(int size) { int mb = 1048576;  return ((size > 0) && (size < mb + 1)); }

        public bool Vldfile(string file) {

            bool vld = false;
            int msg = 0;

            byte[] ftp = new byte[4];
            byte[] aftp = new byte[4] { 0x7F, 0x45, 0x4c, 0x46 };

            if (File.Exists(file))
            {
                try
                {
                    using (FileStream fstr = new FileStream(file, FileMode.Open))
                    {
                        fstr.Read(ftp, 0, ftp.Length);
                        vld = ftp.SequenceEqual(aftp);
                    }
                }
                catch (Exception) {; }
                finally { ; }                
            }

            //One of the two places that have the same logic
            msg =  vld ? 0 : 4;
            Valid = errMsg[msg];
            return vld;
        }

    }
}
