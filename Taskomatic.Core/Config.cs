using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Taskomatic.Core
{
    public class Config
    {
        public const string ConfigFileName = ".taskomatic.json";

        public string GitHubProject { get; set; }
        public string TaskWarriorPath { get; set; } 

        public static Task<Config> Read(Stream input)
        {
            return Task.Run(() =>
            {
                var serializer = new JsonSerializer();
                using (var streamReader = new StreamReader(input))
                using (var reader = new JsonTextReader(streamReader))
                {
                    return serializer.Deserialize<Config>(reader);
                }
            });
        }
    }
}
