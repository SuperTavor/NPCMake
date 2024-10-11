using ImGui.Forms;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Factories;
using ImGui.Forms.Modals;
using ImGui.Forms.Modals.IO.Windows;
using NPCMake.Core.RequiredFilesManagement;
using NPCMake.GUI.Modals;

namespace NPCMake.GUI.Forms
{
    public class MainForm : Form
    {
        public MainForm() : base()
        {
            this.Title = "npcmake";
            this.Content = GetMainContent();
        }

        private async void MakeFromToml(object s, EventArgs e)
        {
            await new MakeFromTomlModal().ShowAsync();
        }
        private StackLayout GetMainContent()
        {
            var makeFromTomlBtn = new Button("Make from npcmake TOML")
            {
                Padding = new(20, 10)
            };
            makeFromTomlBtn.Clicked += MakeFromToml;
            var genTemplateBtn = new Button("Generate a template for npcmake TOML")
            {
                Padding = new(20, 10)
            };
            genTemplateBtn.Clicked += async (s, e) =>
            {
                var sfd = new WindowsSaveFileDialog();
                sfd.Filters = [new("TOML", ".toml")];
                sfd.Title = "Save your toml template";
                if(await sfd.ShowAsync() == ImGui.Forms.Modals.DialogResult.Ok)
                {
                    File.WriteAllText(sfd.Files[0], RequiredFilesManager.TOML_TEMPLATE);
                    await MessageBox.ShowInformationAsync("Info", "Saved template!");
                }
            };
            return new StackLayout
            {
                Alignment = Alignment.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                ItemSpacing = 10,
                Items =
                {
                    new Label("npcmake.")
                    {
                        Font = FontFactory.Get("ProggyClean",30)
                    },
                    makeFromTomlBtn,
                    genTemplateBtn
                },
            };
        }
    }
}
