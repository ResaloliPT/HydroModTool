using System.Reflection;
using System.Text;
using HydroToolChain.App.Configuration;
using HydroToolChain.App.Configuration.Models;
using HydroToolChain.App.WindowsHelpers;

namespace HydroToolChain.App.Business;

internal class AppActions : IAppActions
{
    private const string AboutMeUrl = "https://raw.githubusercontent.com/ResaloliPT/HydroModTool/master/Readme.md";
    private const string DefaultReadme = @"
# Hydroneer Modding Toolchain [![Donate](https://www.paypalobjects.com/en_US/i/btn/btn_donate_SM.gif)](https://paypal.me/ResaloliPT)

                Check the application repository [Here](https://github.com/ResaloliPT/HydroModTool)";


    private readonly AppOptions _appOptions;
    private readonly IAppConfiguration _appConfiguration;
    private readonly IWindowsHelpers _windowsHelpers;
    private readonly HttpClient _httpClient;

    public AppActions(
        AppOptions appOptions,
        IAppConfiguration appConfiguration,
        IWindowsHelpers windowsHelpers)
    {
        _appOptions = appOptions;
        _appConfiguration = appConfiguration;
        _windowsHelpers = windowsHelpers;
        _httpClient = new HttpClient();
    }
    
    public async Task<bool> ImportConfig(CancellationToken ct)
    {
        var selectedFilePath = _appOptions.SelectConfigDialog();

        if (selectedFilePath is null)
        {
            return false;
        }

        return await _appConfiguration.TryImport(selectedFilePath);
    }

    public async Task<bool> ExportConfig(ConfigPartials? partial, CancellationToken ct)
    {
         var result = await _appConfiguration.ExportConfig(partial, ct);
         if (!result.success)
         {
             return false;
         }

         var destination = _appOptions.SaveConfigDialog(partial);
         if (destination is null)
         {
             return false;
         }
         
         await File.WriteAllTextAsync(destination, result.content, ct);
         
         return true;
    }

    public async Task<(bool success, string readme)> GetReadmeAsync(CancellationToken ct)
    {
        try
        {
            var readMe = await _httpClient.GetStringAsync(AboutMeUrl, ct);
            
            return (true, readMe);
        }
        catch (Exception)
        {
            try
            {
                var aboutFile = Path.Combine(Assembly.GetExecutingAssembly().Location, "About.md");

                return (false, await File.ReadAllTextAsync(aboutFile, ct));
            }
            catch (Exception)
            {
                return (false, DefaultReadme);
            }
        }
    }

    public void LaunchGame()
    {
        _windowsHelpers.StartGame();
    }

    public bool ClearModsFolder()
    { 
        try
        {
            Directory.Delete(Constants.PaksFolder, true);
            Directory.CreateDirectory(Constants.PaksFolder);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    public bool OpenModsFolder()
    {
        try
        {
            _windowsHelpers.OpenFolder(Constants.PaksFolder);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }
}