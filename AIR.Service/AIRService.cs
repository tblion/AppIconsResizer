using System.Collections.Generic;
using AIR.Commons;
using AIR.Lib;

namespace AIR.Service
{
    public class AIRService : IAIRService
    {      

        public List<PictureContainer> ConvertBatch(List<string> filesIn, List<PresetSize> sizes,
            List<string> formats)
        {
            return AIRCore.ConvertBatch(filesIn, sizes, formats);
        }

        public string CreateZipFile(List<PictureContainer> files,string tempFolderPath, string fileName)
        {
            return AIRCore.CreateZipFile(files, tempFolderPath, fileName);
        }

        public string CreateZipFile(List<string> fileList, string folderPath, string fileName)
        {
            return AIRCore.CreateZipFile(fileList, folderPath, fileName);
        }
    }
}