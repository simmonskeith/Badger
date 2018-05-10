using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Badger.Core.Interfaces
{
    public interface IFileService
    {
        bool IsDirectory(string path);
        bool FileExists(string path);

        void CreateFolder(string path);
        void CopyFile(string fromPath, string toPath);
        void DeleteFile(string path);
        void DeleteFolder(string path, bool recursive);

        List<string> GetFiles(string path, string pattern, System.IO.SearchOption searchOption);
        List<string> GetLines(string path);
        XDocument LoadHtmlFile(string path);

        void WriteLine(string path, string output);
        void WriteLines(string path, string[] lines, bool append);
        void WriteFile(string path, string output, bool append);
        void WriteConsole(string output);
        void SaveImage(string path, Bitmap image, ImageFormat format);
        void SaveHtmlDocument(string path, XDocument doc);

        void StartProcess(string filename);
    }
}
