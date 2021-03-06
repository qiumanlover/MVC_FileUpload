using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using WebAppMVC.Models;

namespace WebAppMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly string defaultPath = "D:\\";
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
        public void MyUpload()
        {
            foreach (string filekey in Request.Files)
            {
                HttpPostedFileBase file = Request.Files[filekey];
                string path = System.IO.Path.Combine(defaultPath, System.IO.Path.GetFileName(file.FileName));
                file.SaveAs(path);
            }
            Response.Write("success");
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

        // 单文件，连续追加数据
        [HttpPost]
        public void UploadByPiece()
        {
            int stepSize = 1024 * 1024 * 1;
            string filePath = Path.Combine(defaultPath, Request["filename"].ToString());
            string tempPath = Path.Combine(defaultPath, string.Format("{0}_temp.data", Path.GetFileNameWithoutExtension(filePath)));
            int pieceCount = Convert.ToInt32(Request["pieceCount"]);
            int nxtPiece = 1;
            // 首次上传
            if (!System.IO.File.Exists(filePath) && !System.IO.File.Exists(tempPath))
            {
                using (FileStream tempStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
                {
                    byte[] curPiece = System.Text.Encoding.UTF8.GetBytes("1");
                    tempStream.Write(curPiece, 0, curPiece.Length);
                }
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    fileStream.Flush();
                }
            }
            // 传输过程中
            if (System.IO.File.Exists(filePath) && System.IO.File.Exists(tempPath))
            {
                byte[] dataBuffer = new byte[stepSize];
                int dataCount = 0;
                using (FileStream fs = new FileStream(tempPath, FileMode.Open, FileAccess.Read))
                {
                    dataCount = fs.Read(dataBuffer, 0, dataBuffer.Length);
                    nxtPiece = Convert.ToInt32(System.Text.Encoding.UTF8.GetString(dataBuffer, 0, dataCount));
                }
                int sendPiece = Convert.ToInt32(Request["curPiece"]);
                if (sendPiece == nxtPiece)
                {
                    var data = Request.Files["data"];
                    dataCount = data.InputStream.Read(dataBuffer, 0, dataBuffer.Length);
                    data.InputStream.Close();
                    // 对dataBuffer进行校验
                    using (FileStream fss = new FileStream(filePath, FileMode.Open, FileAccess.Write))
                    {// 在指定位置插入传输的数据
                        fss.Seek((sendPiece - 1) * stepSize, SeekOrigin.Begin);
                        fss.Write(dataBuffer, 0, dataCount);
                        fss.Flush();
                    }
                    if (nxtPiece == pieceCount)
                    {
                        System.IO.File.Delete(tempPath);
                        nxtPiece++;
                    }
                    else
                    {
                        using (FileStream fsn = new FileStream(tempPath, FileMode.Open, FileAccess.Write))
                        {// 更新文件片标志
                            dataBuffer = System.Text.Encoding.UTF8.GetBytes((++nxtPiece).ToString());
                            fsn.Write(dataBuffer, 0, dataBuffer.Length);
                        }
                    }
                }
                dataBuffer = null;
            }
            else
            {// 原文件已传输完或临时文件丢失
                System.IO.FileInfo file = new FileInfo(filePath);
                int filePieceCount = Convert.ToInt32(Math.Ceiling((double)((file.Length * 1.0) / (stepSize * 1.0))));
                if (filePieceCount == pieceCount)
                {
                    nxtPiece = filePieceCount + 1;
                }
                else
                {
                    using (FileStream tempStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
                    {
                        byte[] curPiece = System.Text.Encoding.UTF8.GetBytes("1");
                        tempStream.Write(curPiece, 0, curPiece.Length);
                    }
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        fileStream.Flush();
                    }
                }
            }
            JObject obj = new JObject(new JProperty("nxtPiece", nxtPiece));
            Response.Write(obj);
        }


    }
}
