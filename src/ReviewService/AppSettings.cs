using Microsoft.Extensions.Options;

namespace ReviewService
{
    public class AppSettings
    {
        public string MONGO_URI { get; set; }
    }

    public class AppSettingsResolver
    {
        private IOptions<AppSettings> _appSettings;

        public AppSettingsResolver(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }
    }
}
