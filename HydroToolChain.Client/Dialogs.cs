using HydroToolChain.App.Configuration.Models;
using HydroToolChain.App.Models;
using MudBlazor;
using DialogResult = System.Windows.Forms.DialogResult;

namespace HydroToolChain.Client;

internal class Dialogs
{
    private readonly IDialogService _dialogService;

    public Dialogs(
        IDialogService dialogService)
    {
        _dialogService = dialogService;
    }
    
    public IReadOnlyCollection<string> OpenAssetsDialog(string title, string rootPath)
    {
        var files = new List<string>();
        var thread = new Thread(() =>
        {
            var mbResult = MessageBox.Show(@"Do you want to select a file? ('No' to select folder)", @"Select Files?",
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (mbResult == DialogResult.Cancel)
            {
                return;
            }
            
            if (mbResult == DialogResult.Yes)
            {
                using var dialog = new OpenFileDialog();
                dialog.Title = title;
                dialog.Multiselect = true;
                dialog.Filter = @"UE Assets|*.uasset;*.uexp;*.ubulk;*.ini;*.bin;*.umap;*.uplugin;*.uproject";
                dialog.InitialDirectory = rootPath;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    files.AddRange(dialog.FileNames);
                }

                return;
            }

            using (var dialog = new FolderBrowserDialog())
            {
                dialog.UseDescriptionForTitle = true;
                dialog.Description = title;
                dialog.InitialDirectory = rootPath;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    files.AddRange(Directory.GetFiles(dialog.SelectedPath, "*.uasset", SearchOption.AllDirectories)
                        .ToList());
                    files.AddRange(Directory.GetFiles(dialog.SelectedPath, "*.uexp", SearchOption.AllDirectories)
                        .ToList());
                    files.AddRange(Directory.GetFiles(dialog.SelectedPath, "*.ubulk", SearchOption.AllDirectories)
                        .ToList());
                    files.AddRange(Directory.GetFiles(dialog.SelectedPath, "*.ini", SearchOption.AllDirectories)
                        .ToList());
                    files.AddRange(Directory.GetFiles(dialog.SelectedPath, "*.bin", SearchOption.AllDirectories)
                        .ToList());
                    files.AddRange(Directory.GetFiles(dialog.SelectedPath, "*.umap", SearchOption.AllDirectories)
                        .ToList());
                    files.AddRange(Directory.GetFiles(dialog.SelectedPath, "*.uplugin", SearchOption.AllDirectories)
                        .ToList());
                    files.AddRange(Directory.GetFiles(dialog.SelectedPath, "*.uproject", SearchOption.AllDirectories)
                        .ToList());
                }
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Join();

        return files;
    }

    public string? SelectConfigDialog()
    {
        string? result = null;
        var staThread = new Thread(() =>
        {
            using var dialog = new OpenFileDialog();
            dialog.Title = @"Config to import";
            dialog.Multiselect = false;
            dialog.Filter =
                @"HydroModdingToolchain Configs|config.json;guids.json;uids.json;appData.json;*-HMT.json";

            result = dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
        });
        staThread.SetApartmentState(ApartmentState.STA);
        staThread.Start();
        staThread.Join();

        return result;
    }

    public string? SaveConfigDialog(ConfigPartials? partial)
    {
        string? fileDestination = null;
        var staThread = new Thread(() =>
        {
            var fileName = partial switch
            {
                ConfigPartials.Guids => "Guids",
                ConfigPartials.Uids => "Uids",
                _ => "Backup"
            };
            
            using var dialog = new SaveFileDialog();
            dialog.Title = @$"Save HMT Config [{fileName}]";
            dialog.DefaultExt = ".json";
            dialog.FileName = $"{fileName}-HMT";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                fileDestination = dialog.FileName;
            }
        });
        staThread.SetApartmentState(ApartmentState.STA);
        staThread.Start();
        staThread.Join();

        return fileDestination;
    }

    public void ShowMessage(MessageType messageType, string message)
    {
        switch (messageType)
        {
            case MessageType.Error:
                _dialogService.ShowMessageBox("Error", message);
                break;
            case MessageType.Warning:
                _dialogService.ShowMessageBox("Warning", message);
                break;
            case MessageType.Info:
                _dialogService.ShowMessageBox("Info", message);
                break;
        }
    }

    public string? SelectFolderDialog(string title)
    {
        using var dialog = new FolderBrowserDialog();
        dialog.UseDescriptionForTitle = true;
        dialog.Description = title;

        return dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : null;
    }
}