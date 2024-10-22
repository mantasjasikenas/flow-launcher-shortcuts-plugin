using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Controls;

public static class Pickers
{
    public static FileOpenPicker CreateFilePicker(List<string>? fileTypeFilter = null)
    {
        var filePicker = new FileOpenPicker();

        var window = App.MainWindow;

        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hWnd);

        filePicker.ViewMode = PickerViewMode.Thumbnail;

        if (fileTypeFilter is null)
        {
            filePicker.FileTypeFilter.Add("*");
        }
        else
        {
            foreach (var filter in fileTypeFilter)
            {
                filePicker.FileTypeFilter.Add(filter);
            }
        }

        return filePicker;
    }

    public static FolderPicker CreateFolderPicker()
    {
        var folderPicker = new FolderPicker();

        var window = App.MainWindow;

        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hWnd);

        folderPicker.ViewMode = PickerViewMode.Thumbnail;

        return folderPicker;
    }
}
