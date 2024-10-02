using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace PlasticsApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiCommunityToolkit()
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Nunito-Bold.ttf", "NunitoB");
                    fonts.AddFont("Nunito-Medium.ttf", "NunitoM");
                });

            return builder.Build();
        }
    }
}