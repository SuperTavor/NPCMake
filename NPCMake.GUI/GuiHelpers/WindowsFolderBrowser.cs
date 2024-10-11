using static Vanara.PInvoke.Shell32;
namespace NPCMake.GUI.GuiHelpers
{
    public class WindowsFolderBrowser
    {
#pragma warning disable CA1416 // I only call this on Windows
        public static string? BrowseForFolder()
        {
            var dialog = (IFileDialog)new CFileOpenDialog();
            dialog.SetOptions(FILEOPENDIALOGOPTIONS.FOS_PICKFOLDERS);

            var result = dialog.Show();
            if (result == 0)  // S_OK
            {
                var selectedFile = dialog.GetResult().GetDisplayName(SIGDN.SIGDN_FILESYSPATH);
                return selectedFile;
            }
            return null;
        }
#pragma warning restore CA1416

    }
}
