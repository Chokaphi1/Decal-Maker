using MVR.FileManagementSecure;
using System;
using System.Collections.Generic;


namespace VAM_Decal_Maker
{
    //from system PATH since it is unusable in VAM
    public static class PathHelper
    {
        //unity compatible file list https://docs.unity3d.com/Manual/BuiltInImporters.html
        public static List<string> imageExtensions = new List<string>() { ".jpg", ".jpeg", ".tif", "tiff", ".tga", ".gif", ".png", ".psd", ".bmp", ".iff", ".pict", ".pic", ".pct", ".exr", ".hdr" };
        public static List<string> videoExtensions = new List<string>() { ".asf", ".avi", ".dv", ".m4v", ".mov", ".mp4", ".mpg", ".mpeg", ".ogv", ".vp8", ".webm", ".wmv" };

        private static char[] PathSeparatorChars = new char[] { '/', '\\' };

        //MacGruber Discord 03/26/2020
        //Get directory path where the plugin is located. Based on Alazi's & VAMDeluxe's method.
        public static string GetPluginPath(MVRScript self)
        {
            string id = self.name.Substring(0, self.name.IndexOf('_'));
            string filename = self.manager.GetJSON()["plugins"][id].Value;
            return filename.Substring(0, filename.LastIndexOfAny(PathSeparatorChars));
        }
        // Get path prefix of the package that contains our plugin.
        public static string GetPackagePath(MVRScript self)
        {
            string filename = GetPluginPath(self);
            int idx = filename.IndexOf(":/");
            if (idx >= 0)
                return filename.Substring(0, idx + 2);
            else
                return string.Empty;
        }

        //By: VAMDeluxe Discord
        public static List<string> GetFilesAtPathRecursive(string path, string pattern)
        {
            List<string> combined = new List<string>();
            string[] files = FileManagerSecure.GetFiles(path, pattern);
            string[] directories = FileManagerSecure.GetDirectories(path);

            List<ShortCut> shortCuts = FileManagerSecure.GetShortCutsForDirectory(path);

            files.ToList().ForEach(file =>
            {
                combined.Add(file);
            });

            directories.ToList().ForEach(directory =>
            {
                combined.AddRange(GetFilesAtPathRecursive(directory, pattern));
            });

            return combined;
        }

        //from C# PATH
        public static string GetFileName(string path)
        {
            int num = path.LastIndexOfAny(PathSeparatorChars);
            if (num >= 0)
            {
                return path.Substring(num + 1);
            }
            return path;
        }

        public static string GetExtension(string path)
        {
            if (path == null)
            {
                return null;
            }

            int num = findExtension(path);
            if (num > -1 && num < path.Length - 1)
            {
                return path.Substring(num);
            }
            return string.Empty;
        }

        private static int findExtension(string path)
        {
            if (path != null)
            {
                int num = path.LastIndexOf('.');
                int num2 = path.LastIndexOfAny(PathSeparatorChars);
                if (num > num2)
                {
                    return num;
                }
            }
            return -1;
        }

        /// <summary>Gets an array containing the characters that are not allowed in path names.</summary>
        /// <returns>An array containing the characters that are not allowed in path names.</returns>
        public static char[] GetInvalidPathChars = new char[]
        {
            '"',
            '<',
            '>',
            '|',
            '\0',
            '\u0001',
            '\u0002',
            '\u0003',
            '\u0004',
            '\u0005',
            '\u0006',
            '\a',
            '\b',
            '\t',
            '\n',
            '\v',
            '\f',
            '\r',
            '\u000e',
            '\u000f',
            '\u0010',
            '\u0011',
            '\u0012',
            '\u0013',
            '\u0014',
            '\u0015',
            '\u0016',
            '\u0017',
            '\u0018',
            '\u0019',
            '\u001a',
            '\u001b',
            '\u001c',
            '\u001d',
            '\u001e',
            '\u001f'
        };

        public static string GetFileNameWithoutExtension(string path)
        {
            return ChangeExtension(GetFileName(path), null);
        }

        public static string ChangeExtension(string path, string extension)
        {
            if (path == null)
            {
                return null;
            }
            if (path.IndexOfAny(GetInvalidPathChars) != -1)
            {
                throw new ArgumentException("Illegal characters in path.");
            }
            int num = findExtension(path);
            if (extension == null)
            {
                return (num >= 0) ? path.Substring(0, num) : path;
            }
            if (extension.Length == 0)
            {
                return (num >= 0) ? path.Substring(0, num + 1) : (path + '.');
            }
            if (path.Length != 0)
            {
                if (extension.Length > 0 && extension[0] != '.')
                {
                    extension = "." + extension;
                }
            }
            else
            {
                extension = string.Empty;
            }
            if (num < 0)
            {
                return path + extension;
            }
            if (num > 0)
            {
                string str = path.Substring(0, num);
                return str + extension;
            }
            return extension;
        }

    }

}

