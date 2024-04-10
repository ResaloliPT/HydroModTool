namespace HydroToolChain.App;

public static class Constants
{
    private static readonly string _paksFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mining", "Saved", "Paks");
    public static string PaksFolder {
        get
        {
            if (!Directory.Exists(_paksFolder))
            {
                Directory.CreateDirectory(_paksFolder);
            }

            return _paksFolder;
        }
    }

    public static readonly int GameId = 1106840;
}