using System.Collections.Generic;
using System.IO;
using FamilyQuiz.Core.Content;

namespace FamilyQuiz.Tests
{
    /// <summary>Loads the real files from the repo's content/ folder, so tests use the real numbers.</summary>
    public static class TestContent
    {
        private static string _folder;

        public static string Folder
        {
            get
            {
                if (_folder != null) return _folder;
                var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
                while (dir != null)
                {
                    var candidate = Path.Combine(dir.FullName, "content", "config.json");
                    if (File.Exists(candidate))
                    {
                        _folder = Path.Combine(dir.FullName, "content");
                        return _folder;
                    }

                    dir = dir.Parent;
                }

                throw new FileNotFoundException("content/config.json not found above " + Directory.GetCurrentDirectory());
            }
        }

        public static string Read(string file)
        {
            return File.ReadAllText(Path.Combine(Folder, file));
        }

        public static GameConfig Config()
        {
            return ConfigLoader.Parse(Read(ConfigLoader.FileName));
        }

        public static IReadOnlyList<CharacterDef> Characters(GameConfig config)
        {
            return CharacterLoader.Parse(Read(CharacterLoader.FileName), config);
        }
    }
}
