namespace HydroToolChain.App.UE4Pak
{
    public class UE4PakerOptions
    {
        public UE4PakerOptions(UE4PakVersion pakVersion)
        {
            PakVersion = pakVersion;
        }
        
        public UE4PakVersion PakVersion { get; set; }
    }
}