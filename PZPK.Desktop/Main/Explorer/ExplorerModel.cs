using PZPK.Desktop.Global;
using System;

namespace PZPK.Desktop.Main.Explorer;

public class ExplorerModel
{
    public PZPKPackageModel? Model => PZPKPackageModel.Current;

    public event Action? OnPackageOpened;
    public event Action? OnPackageClosed;

    public void OpenPackage(string path, string password)
    {
        if (!string.IsNullOrWhiteSpace(path) && !string.IsNullOrWhiteSpace(password))
        {
            try
            {
                PZPKPackageModel.Open(path, password);
                OnPackageOpened?.Invoke();
            }
            catch (Exception ex)
            {
                Toast.Error(ex.Message);
                Logger.Instance.Log(ex.Message);
            }
        }
    }
    public void ClosePackage()
    {
        Model?.Close();
        OnPackageClosed?.Invoke();
    }
}
