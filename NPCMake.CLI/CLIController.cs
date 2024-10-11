using NPCMake.Core.NPCLogic;
using System.Diagnostics;
using Tomlyn;
using NPCMake.Core.RequiredFilesManagement;
namespace NPCMake.CLI;

public class CLIController
{
    #region Consts
    private readonly string USAGE =
    @$"npcmake USAGE:
            Make sure XtractQuery is available from the CMD in this location. You can get it here: https://github.com/onepiecefreak3/XtractQuery/releases
            Modes:
                template - Writes a NPCAdder toml template to a file. Specify the file path like this:
                ""NPCAdder template <File path>""

                make - Modifies map files based on the provided NPCAdder toml. You also need to specify a folder with all the required files (Copying data/res/map/[YourMapID] is a valid folder) like this:
                ""NPCAdder make <toml with npc settings> <folder with required files>""

            Required files:
                NOTE: All of these files can be found in data/res/map/<Your map ID>.
                npc.pck
                <Map ID>.pck
                <Map ID>_npc_base_talk_<Chapter>_0.01b.cfg.bin (Use C99 for the NPC to be talkable no matter what)
                <Map ID>_npc_set_0.01b.cfg
            ";
   

    #endregion
    private string[] _args { get; set; }

    public CLIController(string[] args)
    {
        _args = args;
    }


    public void Run()
    {
        if (_args.Length == 0)
        {
            Console.WriteLine(USAGE);
        }
        else
        {
            if (_args[0] == "template")
            {
                OptionTemplate();
            }
            else if (_args[0] == "make")
            {
                OptionMake();
            }
        }
    }

    private void OptionMake()
    {
        if (_args.Length != 3)
        {
            Console.WriteLine(USAGE);
        }
        else
        {
            var tomlPath = _args[1];
            if (!File.Exists(tomlPath))
            {
                Console.WriteLine("Cannot find info TOML file");
                Environment.Exit(0);
            }
            var folderWithImportantFiles = _args[2];
            var tomlTable = Toml.Parse(File.ReadAllText(tomlPath)).ToModel();
            var mapid = (string)tomlTable["MapID"];
            var chapterCode = (string)tomlTable["ChapterCode"];
            //check if folder exists
            if (!Directory.Exists(folderWithImportantFiles))
            {
                Console.WriteLine("Required files directory does not exist.");
            }
            else
            {
                var requiredFilesManager = new RequiredFilesManager(folderWithImportantFiles, mapid);
                if(!requiredFilesManager.IsXtractQueryAvailable())
                {
                    Console.WriteLine("XtractQuery is not accessible from this location.");
                    return;
                }
                if (!requiredFilesManager.DirHasFiles(chapterCode))
                {
                    Console.WriteLine("Directory does not have required files. Please review the usage menu.");
                }
                else
                {
                    var npcEditor = new NPCEditor(requiredFilesManager, tomlTable);
                    npcEditor.ApplyChanges();
                    npcEditor.ExportFiles();
                    npcEditor.PrintImportantInfo();
                }
            }
        }
    }

    private void OptionTemplate()
    {
        if (_args.Length < 2)
        {
            Console.WriteLine(USAGE);
        }
        else
        {
            var file = _args[1];
            try
            {
                File.WriteAllText(file, RequiredFilesManager.TOML_TEMPLATE);
            }
            catch
            {
                Console.WriteLine("Could not write to " + file);
            }
        }
    }
}
