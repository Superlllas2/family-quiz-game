using System;
using System.Collections.Generic;
using System.IO;
using FamilyQuiz.Core.Content;
using Newtonsoft.Json.Linq;

namespace FamilyQuiz.Tests
{
    /// <summary>Loads the real files from the repo's content/ folder, so tests use the real numbers.</summary>
    public static class TestContent
    {
        private static string _folder;
        private static GameContent _content;

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

        /// <summary>All of content/ with a changed config.json (a test of a rule at another setting).</summary>
        public static GameContent WithConfig(Action<JObject> change)
        {
            var config = JObject.Parse(Read(ConfigLoader.FileName));
            config["timers"]["lastCallEnabled"] = true; // see Content()
            change(config);
            var text = config.ToString();
            return new ContentLoader(name => name == ConfigLoader.FileName ? text : Read(name)).Load();
        }

        /// <summary>
        /// All of content/ through the ContentLoader, loaded once (tests never change it), with last call on: the suite
        /// was written for last call (D74), which the shipped config switches off (decisions/0020, D119).
        /// LastCallOffTests play the shipped config itself (<see cref="Shipped"/>).
        /// </summary>
        public static GameContent Content()
        {
            return _content ?? (_content = WithConfig(c => { }));
        }

        /// <summary>All of content/ exactly as shipped.</summary>
        public static GameContent Shipped()
        {
            return new ContentLoader(Read).Load();
        }
    }
}
