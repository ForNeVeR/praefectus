using System;
using System.IO;
using System.Threading.Tasks;

namespace Taskomatic.Core
{
    public static class ConfigService
    {
        public static Task<Config> LoadConfig()
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (string.IsNullOrEmpty(home))
            {
                throw new Exception("Cannot determine user home directory");
            }

            var path = Path.Combine(home, Config.ConfigFileName);
            return Task.Run(async () =>
            {
                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return await Config.Read(stream);
                }
            });
        }
    }
}
