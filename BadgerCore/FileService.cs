using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Badger.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Badger.Core
{
    public class FileService : IFileService
    {
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public void DeleteFolder(string path, bool recursive)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive);
            }
            while (Directory.Exists(path))
            {
                System.Threading.Thread.Sleep(100);
            }
        }

        public void CreateFolder(string path)
        {
            Directory.CreateDirectory(path);
            while(!Directory.Exists(path))
            {
                System.Threading.Thread.Sleep(100);
            }
        }

        public void WriteLine(string path, string output)
        {
            using (var f = new StreamWriter(path, true))
            {
                f.WriteLine(output);
            }
        }

        public void WriteLines(string path, string[] lines, bool append)
        {
            using (var f = new StreamWriter(path, append))
            {
                foreach (var line in lines)
                {
                    f.WriteLine(line);
                }
            }
        }

        public void WriteFile(string path, string output, bool append)
        {
            using (var f = new StreamWriter(path, append))
            {
                f.Write(output);
            }
        }


        public void WriteConsole(string output)
        {
            Console.Write(output);
        }

        public void SaveImage(string path, Bitmap image, ImageFormat format)
        {
            image.Save(path, format);
        }

        public void SaveHtmlDocument(string path, XDocument doc)
        {
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true,
                IndentChars = "  "
            };
            using (var writer = XmlWriter.Create(path, settings))
            {
                doc.WriteTo(writer);
            }
        }

        public void CopyFile(string fromPath, string toPath)
        {
            try
            {
                File.Copy(fromPath, toPath);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public bool IsDirectory(string path)
        {
            return System.IO.Directory.Exists(path);
        }

        public List<string> GetFiles(string path, string pattern, SearchOption searchOption)
        {
            return System.IO.Directory.GetFiles(path, pattern, searchOption).ToList();
        }

        public List<string> GetLines(string path)
        {
            var lines = new List<string>();
            try
            {
                lines = File.ReadAllLines(path).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while reading ${path}:  {e.Message}");
            }
            return lines;
        }

        public XDocument LoadHtmlFile(string path)
        {
            return XDocument.Load(path);
        }

        public void StartProcess(string filename)
        {
            System.Diagnostics.Process.Start(filename);
        }
    }
}
