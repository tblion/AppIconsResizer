using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using AIR.Commons;
using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using Ionic.Zip;
using Microsoft.SqlServer.Server;

namespace AIR.Lib
{
    public static class AIRCore
    {
        /// <summary>
        /// Convert a file to PictureContainer
        /// </summary>
        /// <param name="fileIn"></param>
        /// <param name="size"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private static PictureContainer Convert(string fileIn, PresetSize size, ISupportedImageFormat format)
        {
            var filename = Path.GetFileNameWithoutExtension(fileIn);
            // Generate Url
            var id = Guid.NewGuid();
            var path = string.Format(@"/Temp/{0}/{1}_{2}x{3}.{4}", id, filename, size.Size.Width, size.Size.Height,
                format.DefaultExtension);
            using (var imageFactory = new ImageFactory(true))
            {
                try
                {
                    // Load, resize, set the format and save an image.
                    imageFactory.Load(fileIn)
                        .Resize(size.Size)
                        .Format(format);
                    imageFactory.Save(path);

                    if (!File.Exists(path)) throw new FileNotFoundException("File not found");
                    var pc = new PictureContainer
                    {
                        PresetSize = size,
                        FilePath = path
                    };
                    return pc;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return null;
                }
            }
        }

        /// <summary>
        /// Batch convert filelist to List of PictureContainer
        /// </summary>
        /// <param name="filesIn"></param>
        /// <param name="sizes"></param>
        /// <param name="formats"></param>
        /// <returns></returns>
        public static List<PictureContainer> ConvertBatch(List<string> filesIn, List<PresetSize> sizes,
            List<string> formats)
        {
            var formatList = ConvertFormatStringToImageFormat(formats);
            return
                (from file in filesIn from size in sizes from format in formatList select Convert(file, size, format))
                    .ToList();
        }

        /// <summary>
        /// Convert imageformat string to ISupportedImageFormat
        /// </summary>
        /// <param name="formatStringList"></param>
        /// <returns></returns>
        private static List<ISupportedImageFormat> ConvertFormatStringToImageFormat(IEnumerable<string> formatStringList)
        {
            var listFormat = new List<ISupportedImageFormat>();

            foreach (var formatString in formatStringList)
            {
                switch (formatString)
                {
                    case "jpg":
                        {
                            listFormat.Add(new JpegFormat()
                            {
                                Quality = 100
                            });
                            break;
                        }
                    case "png":
                        {
                            listFormat.Add(new PngFormat()
                            {
                                Quality = 100
                            });
                            break;
                        }
                }
            }

            return listFormat;
        }

        /// <summary>
        /// Create zip from PictureContainer
        /// </summary>
        /// <param name="files"></param>
        /// <param name="tempFolderPath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string CreateZipFile(List<PictureContainer> files, string tempFolderPath, string fileName)
        {
            var path = Path.Combine(tempFolderPath, string.Format(@"{0}.zip", fileName));
            using (var zip = new ZipFile(path))
            {
                foreach (var pictureContainer in files)
                {
                    if (pictureContainer == null) continue;
                    var folder = string.Empty;


                    switch (pictureContainer.PresetSize.PresetEnum)
                    {
                        case Enums.PresetEnum.Android:
                            {
                                folder = "Android";
                                break;
                            }
                        case Enums.PresetEnum.iOS:
                            {
                                folder = "iOS";
                                break;
                            }

                        case Enums.PresetEnum.WindowsPhone:
                            {
                                folder = "WP";
                                break;
                            }
                        case Enums.PresetEnum.None:
                        default:
                            {
                                break;
                            }
                    }
                    zip.AddFile(pictureContainer.FilePath, folder);
                }
                zip.Save();
            }
            return !File.Exists(path) ? string.Empty : path;
        }

        /// <summary>
        /// Create zip from filelist
        /// </summary>
        /// <param name="fileList"></param>
        /// <param name="folderPath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string CreateZipFile(List<string> fileList, string folderPath, string fileName)
        {

            var filepath = Path.Combine(folderPath, string.Format(@"{0}.zip", fileName));

            try
            {
                using (ZipFile zip = new ZipFile(filepath))
                {
                    zip.AddFiles(fileList,"");
                    zip.Save();
                    return filepath;
                }
            }
            catch (Exception ex)
            {
                //TODO log exception
                return null;
            }
           
           
        }
    }
}