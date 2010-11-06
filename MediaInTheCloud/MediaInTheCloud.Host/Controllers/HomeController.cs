using System.Web.Mvc;
using System.IO;
using System.Collections.Generic;

namespace MediaInTheCloud.Host.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            return View();
        }

        public ActionResult Upload()
        {
            string uploadPath = this.HttpContext.Server.MapPath("~/Files");
            FileUploadProcess fileUpload = new FileUploadProcess();
            fileUpload.ProcessRequest(this.HttpContext, uploadPath);
            return View();
        }

        public ActionResult GetServerItems()
        {
            var path = this.HttpContext.Server.MapPath("~/Files");
            var files = Directory.GetFiles(path);
            var items = new List<MediaItem>();
            foreach (var file in files)
            {
                var mediaItem = new MediaItem();
                mediaItem.Name = Path.GetFileName(file);
                mediaItem.Url = "http://localhost/MediaInTheCloud.Host/Files/" + mediaItem.Name;
                if (file.EndsWith(".wav"))
                {
                    mediaItem.IsAudio = true;
                }
                items.Add(mediaItem);
            }

            return Json(items, JsonRequestBehavior.AllowGet);
        }
    }

    public class MediaItem
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public bool IsAudio { get; set; }
    }
}
