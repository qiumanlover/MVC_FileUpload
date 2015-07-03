using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace WebAppMVC.Models
{
    public class MyFileInfo
    {
        public string FileName { get; set; }
        public string FileNameWithoutExtension
        {
            get
            {
                return this.FileName.Substring(0, this.FileName.LastIndexOf('.'));
            }
        }
        public string FileExtension
        {
            get
            {
                return this.FileExtensionWithDot.Substring(1);
            }
        }
        public string FileExtensionWithDot
        {
            get
            {
                return this.FileName.Substring(this.FileName.LastIndexOf('.'));
            }
        }
        public int FileSize { get; set; }
        public string SavePath { get; set; }

        public List<byte> FileData { get; set; }
        public int CurrentSize { get; set; }
    }
}