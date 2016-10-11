using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace ImageCompressor_v1
{
    class ImageCompressor
    {
        private static readonly List<byte[]> ImageTypes = new List<byte[]>
        {
            new byte[] {0xFF, 0xD8}, //jpg
            new byte[] {0x42, 0x4D}, //bmp
            new byte[] {0x47, 0x49, 0x46}, //gif
            new byte[] {0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A} //png
        };

        private readonly string _path;

        public ImageCompressor(string path)
        {

            // if it is not a directory, neither a file
            if (!Directory.Exists(path) && !File.Exists(path))
            {
                throw new ArgumentException(@"Not an image file or a directory.", path);
            }

            // if it is a file but not an image file
            if (File.Exists(path) && !IsImageFile(path))
            {
                throw new ArgumentException(@"Not an image file or a directory.", path);
            }

            _path = path;

        }

        public void Compress(string destinationDir, bool recursive)
        {
            Compress(_path, destinationDir, recursive);
        }

        public void Compress(string sourcePath, string destinationDir, bool recursive)
        {
            if (!Directory.Exists(destinationDir))
            {
                throw new ArgumentException(@"Output directory does not exist.", destinationDir);
            }

            if (File.Exists(sourcePath))
            {
                ProcessImageFile(sourcePath, destinationDir);
            }
            else
            {
                foreach (var file in Directory.GetFiles(sourcePath))
                {
                    if (IsImageFile(file))
                    {
                        ProcessImageFile(file, destinationDir);
                    }
                }

                if (!recursive) return;

                Parallel.ForEach(Directory.GetDirectories(sourcePath), directory =>
                //foreach (var directory in Directory.GetDirectories(sourcePath))
                {
                    var innerDestinationDir = Path.Combine(destinationDir, new DirectoryInfo(directory).Name);

                    if (!Directory.Exists(innerDestinationDir))
                    {
                        Directory.CreateDirectory(innerDestinationDir);
                    }

                    Compress(directory, innerDestinationDir, recursive);
                }
                );
            }
        }

        public static bool IsImageFile(string path)
        {
            var position = 0;
            var possibilities = ImageTypes.Count;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var reversedMask = new bool[ImageTypes.Count];

                int singleByte;
                while ((singleByte = fs.ReadByte()) != -1)
                {
                    for (var i = 0; i < ImageTypes.Count; i++)
                    {
                        if (reversedMask[i] || ImageTypes[i][position] == singleByte) continue;

                        reversedMask[i] = true;
                        if (--possibilities == 0)
                        {
                            return false;
                        }
                    }

                    position++;
                    for (var i = 0; i < ImageTypes.Count; i++)
                    {
                        // there is a possible byte start
                        // and
                        // byte start is exhausted (position index is at the next possible position)
                        if (!reversedMask[i] && position == ImageTypes[i].Length)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        private void ProcessImageFile(string sourceFilePath, string destinationDir)
        {
            var destinationFilePath = Path.Combine(destinationDir, Path.GetFileName(sourceFilePath));
            string tempFileName;

            using (var bm = new Bitmap(sourceFilePath))
            {
                bm.SetPixel(0, 0, bm.GetPixel(0, 0));


                if (sourceFilePath != destinationFilePath)
                {
                    bm.Save(destinationFilePath);
                    return;
                }

                tempFileName = Path.GetTempFileName();
                bm.Save(tempFileName);
            }
         
            File.Delete(destinationFilePath);
            File.Move(tempFileName, destinationFilePath);
        }
    }
}
