using System.Collections.Generic;
using AIR.Commons;

namespace AIR.Service
{
    public interface IAIRService
    {

        List<PictureContainer> ConvertBatch(List<string> filesIn, List<PresetSize> sizes,
            List<string> formats);

        string CreateZipFile(List<PictureContainer> files,string tempFolderPath, string fileName);

        string CreateZipFile(List<string> fileList, string folderPath, string fileName);
    }
}