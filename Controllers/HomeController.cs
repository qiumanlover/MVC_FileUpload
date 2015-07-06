using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAppMVC.Models;

namespace WebAppMVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        /// <summary>
        /// Uploads the specified files.
        /// </summary>
        /// <param name="fileToUpload">The files.</param>
        /// <returns>ActionResult</returns>

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase[] fileToUpload)
        {
            foreach (HttpPostedFileBase file in fileToUpload)
            {
                string path = System.IO.Path.Combine(Server.MapPath("~/App_Data"), System.IO.Path.GetFileName(file.FileName));
                file.SaveAs(path);
            }
            ViewBag.Message = "File(s) uploaded successfully";
            return RedirectToAction("Index");
        }

        public ActionResult UploadFile()
        {
            return View();
        }

        [HttpPost]
        public ActionResult MyUpload()
        {
            foreach (string filekey in Request.Files)
            {
                HttpPostedFileBase file = Request.Files[filekey];
                string path = System.IO.Path.Combine(Server.MapPath("~/App_Data"), System.IO.Path.GetFileName(file.FileName));
                file.SaveAs(path);
            }
            ViewBag.Message = "File(s) uploaded successfully";
            return RedirectToAction("Index");
        }

        public ActionResult MyFileUploader()
        {
            return View();
        }

        [HttpPost]
        public void SetFileInfo()
        {
            byte[] buffer = new byte[Request.InputStream.Length];
            int count = Request.InputStream.Read(buffer, 0, buffer.Length);
            string json = System.Text.Encoding.UTF8.GetString(buffer, 0, count);
            JObject jobject = JObject.Parse(json);
            MyFileInfo fileinfo = new MyFileInfo();
            fileinfo.FileData = new List<byte>();
            fileinfo.FileName = jobject["filename"].ToString();
            fileinfo.FileSize = Convert.ToInt64(jobject["filesize"].ToString());
            Session.Add("fileinfo", fileinfo);
            //if (!System.IO.File.Exists("D:\\" + fileinfo.FileName))
            //{16955776997
            //    System.IO.File.Create("D:\\" + fileinfo.FileName);
            //}
            using (FileStream fs = new FileStream("D:\\" + fileinfo.FileName, FileMode.Create))
            {
                fs.Flush();
            }
            var obj = new JObject(new JProperty("index", fileinfo.CurrentSize));
            Response.Write(obj);
        }
        [HttpPost]
        public void TransferData()
        {
            MyFileInfo fileinfo = (MyFileInfo)Session["fileinfo"];
            byte[] buffer = new byte[Request.InputStream.Length];
            int count = Request.InputStream.Read(buffer, 0, buffer.Length);
            fileinfo.FileData.AddRange(buffer);
            if (fileinfo.FileData.Count >= 1024 * 1024 * 8 || (fileinfo.CurrentSize + buffer.Length) == fileinfo.FileSize)
            {
                WriteIOSync("D:\\" + fileinfo.FileName, fileinfo.FileData.ToArray());
                fileinfo.FileData.Clear();
            }
            fileinfo.CurrentSize += buffer.Length;
            var obj = new JObject(new JProperty("index", fileinfo.CurrentSize));
            Response.Write(obj);
        }

        private async void WriteIOAsync(string filename, byte[] buffer)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Append, FileAccess.Write))
            {
                await fs.WriteAsync(buffer, 0, buffer.Length);
            }
        }

        private void WriteIOSync(string filename, byte[] buffer)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Append, FileAccess.Write))
            {
                fs.Write(buffer, 0, buffer.Length);
            }
        }

        public static byte[] charToByte(char c)
        {
            byte[] b = new byte[2];
            b[0] = (byte)((c & 0xFF00) >> 8);
            b[1] = (byte)(c & 0xFF);
            return b;
        }
    }
}