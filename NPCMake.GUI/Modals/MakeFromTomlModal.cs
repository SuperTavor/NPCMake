using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Modals;
using ImGui.Forms.Modals.IO;
using ImGui.Forms.Modals.IO.Windows;
using NPCMake.Core.NPCLogic;
using NPCMake.Core.RequiredFilesManagement;
using NPCMake.GUI.GuiHelpers;
using Tomlyn;
namespace NPCMake.GUI.Modals
{
    public class MakeFromTomlModal : Modal
    {
        private string _tomlPath = "";
        private string _requiredFolderPath = "";
        private Label _tomlPathLabel = new("No TOML selected.");
        private Label _requiredFolderPathLabel = new("No map folder selected.");
        private bool _selectedToml = false;
        private bool _selectedRequiredFolderPath = false;
        private bool _isUnclosable = false;
        public MakeFromTomlModal() : base()
        {
            this.Size = new(400, 200);
            this.Caption = "Make NPC from TOML";
            this.Content = GetMainContent();
        }
        
        private async void ChooseToml(object s, EventArgs e)
        {
            var ofd = new WindowsOpenFileDialog
            {
                Title = "Choose a TOML file"
            };
            ofd.Filters.Add(new FileFilter("npcmake TOML configuration", "toml"));
            if (await ofd.ShowAsync() == DialogResult.Ok)
            {
                _tomlPath = ofd.Files[0];
                _tomlPathLabel.Text = Path.GetFileName(_tomlPath);
                _selectedToml = true;
            }
        }

        private async void ChooseRequiredFilesFolder(object s, EventArgs e)
        {
            //Check if we are on Windows so we can use the native browser
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                var path = WindowsFolderBrowser.BrowseForFolder();
                if(path != null)
                {
                    _selectedRequiredFolderPath = true;
                    _requiredFolderPath = path;
                    _requiredFolderPathLabel.Text = Path.GetFileName(path);
                }
            }
            else
            {
                var folderDialog = new SelectFolderDialog();
                if(await folderDialog.ShowAsync() == DialogResult.Ok)
                {
                    var path = folderDialog.Directory;
                    _selectedRequiredFolderPath = true;
                    _requiredFolderPath = path;
                    _requiredFolderPathLabel.Text = Path.GetFileName(path);
                }
            }
        }
        private async void Make(object s, EventArgs e)
        {
            if (!_selectedRequiredFolderPath || !_selectedToml)
            {
                await MessageBox.ShowErrorAsync("Err", "Please fill out all the fields.");
                return;
            }
            var tomlTable = Toml.Parse(File.ReadAllText(_tomlPath)).ToModel();
            var mapid = (string)tomlTable["MapID"];
            var chapterCode = (string)tomlTable["ChapterCode"];

            var requiredFilesManager = new RequiredFilesManager(_requiredFolderPath, mapid);
            if (!requiredFilesManager.IsXtractQueryAvailable())
            {
                await MessageBox.ShowInformationAsync("Err", "XtractQuery is not accessible from this location.");
                return;
            }
            if (!requiredFilesManager.DirHasFiles(chapterCode))
            {
                await MessageBox.ShowInformationAsync("Err", "Directory does not have required files. Please select data/map/res/[mapid] from your FA.");
                return;
            }
            else
            {
                this.Content = new Label("Working...");
                List<string> infoList = new();
                _isUnclosable = true;
                await Task.Run(() =>
                {
                    var npcEditor = new NPCEditor(requiredFilesManager, tomlTable);
                    npcEditor.ApplyChanges();
                    npcEditor.ExportFiles();
                    npcEditor.PrintImportantInfo(infoList);
                });
                await MessageBox.ShowInformationAsync("Notice", "Finished!");

                foreach (var info in infoList)
                {
                    await MessageBox.ShowInformationAsync("Info", info);
                }
                _isUnclosable = false;
                this.Close();
            }
        }

        protected override Task<bool> ShouldCancelClose()
        {
            return Task.FromResult(_isUnclosable);
        }
        private StackLayout GetMainContent()
        {
            var chooseTomlbtn = new Button("Pick npcmake TOML") { Padding = new(7, 7) };
            chooseTomlbtn.Clicked += ChooseToml;
            var chooseMapFolderBtn = new Button("Pick map folder with required files") { Padding = new(7, 7) };
            chooseMapFolderBtn.Clicked += ChooseRequiredFilesFolder;
            var makeBtn = new Button("Make NPC") { Padding = new(40, 7) };
            makeBtn.Clicked += Make;
            return new StackLayout
            {
                Alignment = Alignment.Vertical,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                ItemSpacing = 10,
                Items =
                {
                    new(chooseTomlbtn) {HorizontalAlignment = HorizontalAlignment.Center},
                    new(_tomlPathLabel) {HorizontalAlignment = HorizontalAlignment.Center},
                    chooseMapFolderBtn,
                    new(_requiredFolderPathLabel) {HorizontalAlignment = HorizontalAlignment.Center},
                    new StackItem(makeBtn)
                    {
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                }
            };
        }
    }
}
