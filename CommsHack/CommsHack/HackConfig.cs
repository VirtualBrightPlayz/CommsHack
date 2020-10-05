using Exiled.API.Interfaces;

namespace CommsHack
{
    public class HackConfig : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public string CommsFile { get; set; } = "D:/SteamLibrary/steamapps/common/SCP Secret Laboratory Dedicated Server/file.raw";
    }
}