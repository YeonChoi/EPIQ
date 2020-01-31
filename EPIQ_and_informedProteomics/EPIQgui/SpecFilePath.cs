using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EPIQgui
{
    public class SpecFilePath
    {
        public string RawPath = "";
        public string RawFileName = "";
        public string IdPath = "";
        public string IdFileName = "";

        public SpecFilePath()
        {
            
        }

        public SpecFilePath(string path, string type)
        {
            if (type.ToUpper() == "RAW")
            {
                RawPath = path;
                RawFileName = Path.GetFileName(path);
            }
            else if (type.ToUpper() == "ID")
            {
                IdPath = path;
                IdFileName = Path.GetFileName(path);
            }
            else
            {
                throw new Exception("Invalid type option in SpecFilePath class initiator");
            }
        }

        public void AddRawPath(string path)
        {
            RawPath = path;
            RawFileName = Path.GetFileName(path);
        }

        public void AddIdPath(string path)
        {
            IdPath = path;
            IdFileName = Path.GetFileName(path);
        }
    }
}
