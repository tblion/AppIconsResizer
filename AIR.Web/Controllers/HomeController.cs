using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Web;
using System.Web.Mvc;
using AIR.Commons;
using AIR.Service;
using Newtonsoft.Json;

namespace AIR.Web.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        ///     Get Size list from databse or json file
        /// </summary>
        /// <param name="preset"></param>
        /// <returns></returns>
        private List<PresetSize> GetSizeListFromJson(Enums.PresetEnum preset)
        {
            var list = new List<PresetSize>();
            var pp = new PlatformPreset();
            try
            {
                switch (preset)
                {
                    case Enums.PresetEnum.iOS:
                    {
                        using (var sr = new StreamReader(Server.MapPath("~/json/iOS.json")))
                        {
                            pp = JsonConvert.DeserializeObject<PlatformPreset>(sr.ReadToEnd());
                        }
                        break;
                    }

                    case Enums.PresetEnum.WindowsPhone:
                        {
                            using (var sr = new StreamReader(Server.MapPath("~/json/Windows.json")))
                            {
                                pp = JsonConvert.DeserializeObject<PlatformPreset>(sr.ReadToEnd());
                            }
                            break;
                        }

                    case Enums.PresetEnum.Android:
                        {
                            using (var sr = new StreamReader(Server.MapPath("~/json/Android.json")))
                            {
                                pp = JsonConvert.DeserializeObject<PlatformPreset>(sr.ReadToEnd());
                            }
                            break;
                        }

                    default:
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (pp?.Preset != null)
            {
                foreach (var size in pp.Values)
                {
                    var ps = new PresetSize
                    {
                        Size = new Size(size[0], size[1]),
                        PresetEnum = preset
                    };
                    list.Add(ps);
                }
            }

            return list;
        }

        /// <summary>
        ///     Set session variable
        /// </summary>
        /// <param name="isIosExport"></param>
        /// <param name="isAndroidExport"></param>
        /// <param name="isWindowsExport"></param>
        /// <returns></returns>
        public ActionResult SetSession(bool isIosExport, bool isAndroidExport, bool isWindowsExport)
        {
            Session[AIRResources.iOs] = isIosExport;
            Session[AIRResources.Android] = isAndroidExport;
            Session[AIRResources.Windows] = isWindowsExport;

            var arrayToReturn = new[]
            {
                isIosExport,
                isAndroidExport,
                isWindowsExport
            };
            return Json(arrayToReturn, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Download the zip file
        /// </summary>
        /// <returns></returns>
        public ActionResult GetZipFile()
        {

            var service = new AIRService();
            var zipPathList = (List<string>) Session[AIRResources.ZipPathList];

            if (zipPathList.Count == 0)
            {
                return null;
            }

            if (zipPathList.Count == 1)
            {
                return File(zipPathList[0], @"application/octet-stream","Zip.zip");
            }


            var foldername = Session[AIRResources.SessionGuid].ToString();
            var folderPath = Path.Combine(Server.MapPath("~/App_Data"), foldername);

            var zipFile = service.CreateZipFile(zipPathList,folderPath,foldername);
            return File(zipFile, @"application/octet-stream", "Zip.zip");

        }

        /// <summary>
        ///     Save the file on the server
        /// </summary>
        /// <param name="UploadDefault"></param>
        /// <returns></returns>
        public ActionResult SaveDefault(IEnumerable<HttpPostedFileBase> UploadDefault)

        {
            var listPath = new List<string>();

            var foldername = Session[AIRResources.SessionGuid].ToString();
            var folderPath = Path.Combine(Server.MapPath("~/App_Data"), foldername);

            if (Directory.Exists(folderPath))
            {
               // Directory.Delete(folderPath, true);
            }
            else
            {
                Directory.CreateDirectory(folderPath);
            }

            

            try
            {

                var fileName = string.Empty;
                var filenameWOExtension = string.Empty;
                // Technicaly, there is only one file
                foreach (var file in UploadDefault)

                {
                     fileName = Path.GetFileName(file.FileName);
                    filenameWOExtension = Path.GetFileNameWithoutExtension(fileName);
                    if (string.IsNullOrEmpty(fileName))
                    {
                        continue;
                    }

                    var destinationPath = Path.Combine(folderPath, fileName);

                    file.SaveAs(destinationPath);

                    if (System.IO.File.Exists(destinationPath))
                    {
                        listPath.Add(destinationPath);
                    }
                }

                var presetListToDo = new List<PresetSize>();

                if ((bool) Session[AIRResources.iOs])
                {
                    presetListToDo.AddRange(GetSizeListFromJson(Enums.PresetEnum.iOS));
                }
                if ((bool) Session[AIRResources.Android])
                {
                    presetListToDo.AddRange(GetSizeListFromJson(Enums.PresetEnum.Android));
                }
                if ((bool) Session[AIRResources.Windows])
                {
                    presetListToDo.AddRange(GetSizeListFromJson(Enums.PresetEnum.WindowsPhone));
                }

                var service = new AIRService();

                // TODO Choose the file format
                var formatList = new List<string>
                {
                    "png"
                };
                var pictureContainerList = service.ConvertBatch(listPath, presetListToDo, formatList);

                var zipFilePath = service.CreateZipFile(pictureContainerList, folderPath, filenameWOExtension);

                // We put the file in the Session List
                var listSessionFilePath = Session[AIRResources.ZipPathList] as List<string>;
                if (listSessionFilePath != null)
                {
                    listSessionFilePath.Add(zipFilePath);
                }

                return Json("Ok");
            }
            finally
            {
                // Delete
                foreach (var path in listPath)
                {
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }
            }
        }

        /// <summary>
        /// Remove files
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns></returns>
        public ActionResult RemoveDefault(string[] fileNames)

        {
            foreach (var fullName in fileNames)

            {
                var fileName = Path.GetFileName(fullName);

                var physicalPath = Path.Combine(Server.MapPath("~/App_Data"), fileName);

                if (System.IO.File.Exists(physicalPath))

                {
                    System.IO.File.Delete(physicalPath);
                }
            }

            return Content("");
        }
    }
}