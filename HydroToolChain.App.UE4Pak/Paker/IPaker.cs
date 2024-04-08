namespace HydroToolChain.App.UE4Pak.Paker;

public interface IPaker
{
    Task PakMod(string outputFolder, string modName, int modIndex, CancellationToken ct);
}