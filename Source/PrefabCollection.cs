using MVR.FileManagementSecure;
using System.Text.RegularExpressions;

namespace VAM_Decal_Maker
{
    public class PrefabCollection
    {
        public string Author { get; private set; }
        public string ItemType { get; private set; }
        public string ItemName { get; private set; }
        public string Path { get; private set; }
        public string Package { get; private set; }

        public string ImagePath { get; private set; }

        public PrefabCollection(string path, string package = null)
        {
            string fileName = GetFileName(path);

            Path = path;
            Package = package;
            //*?)(\.dsgn)?  lowest posible after underscore and optionally match dsgn since early versions had that in the name
            MatchCollection mc = Regex.Matches(fileName, @"^(.*)_(.*)_(.*?)(\.dsgn)?.DecalMakerPreset.json");

            foreach (Match m in mc)
            {
                Author = m.Groups[1].Value;
                ItemType = m.Groups[2].Value;
                ItemName = m.Groups[3].Value;
                string imagePath = path.Replace(".json", ".jpg");
                if (FileManagerSecure.FileExists(imagePath))
                {
                    ImagePath = imagePath;
                }
            }
        }


        //from C# PATH
        private char[] sep = new char[] { '/', '\\' };
        private string GetFileName(string path)
        {
            int num = path.LastIndexOfAny(sep);
            if (num >= 0)
            {
                return path.Substring(num + 1);
            }
            return path;
        }
    }


}

