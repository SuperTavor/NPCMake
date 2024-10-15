using System.Diagnostics;

namespace NPCMake.Core.RequiredFilesManagement
{
    public class RequiredFilesManager
    {
        public readonly List<string> REQUIRED_FILES = [
            "npc.pck",
            "<MAPID>.pck",
            "npc_base_talk_<CHAPTER>",
            "npc_set_0.01",
        ];

        public const string TOML_TEMPLATE =
       @"
# the name of your NPC. Can be anything!
NpcName = ""MyNPC""
# What BaseID will your NPC be using?
BaseId = 0x0

# The next few properties dictate your NPC's position. You can find a desired position using Tinifan's tool, GetNPCPos.  Download it through this link:
# https://mega.nz/file/JhQhCSgb#3vKYomcbHN6BwLe8SNGshtbAB63jAep9YDrTOVTSnT8

NpcX = 0
# Y here is how it acts in 2D games, not the height.
NpcY = 0
# This, however, is the height.
NpcZ = 0
# Use degrees
NpcRotation = 0

# On which chapter would your NPC be talkable? (write c01 for chapter 1, c02 for chapter 2, etc. C11 is post game)
ChapterCode = ""c11""

# On which map ID are you adding your NPC to?
MapID = ""t101i01""

# NPC code, written in XQ.
OnTalk = """"""
$local1 = log(""Hello, world!"");
""""""
# Appear condition in the Level5 COND format. You can copy it from another NPC or generate one using the Level5Condition tool by me. Download it here:
# https://mega.nz/file/09xHySDJ#7SkW5yDiS3Ccw3r1naLXqlp03pXP1c9a7VrS67HOQGc
# NOTE: You can leave the cond as is for the NPC to appear at all times.
AppearCond = ""0""
    ";

        public Dictionary<string, byte[]> RequiredFileData = new Dictionary<string, byte[]>();
        private string _dir;
        public string MapID = "";
        public RequiredFilesManager(string dir, string mapid)
        {
            _dir = dir;
            MapID = mapid;
        }

        public bool IsXtractQueryAvailable()
        {
            var process = Process.Start(new ProcessStartInfo("xtractquery", "-h")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            });
            process!.WaitForExit();
            return process.ExitCode == 0;
        }

        public bool DirHasFiles(string chapterCode)
        {
            //Get files from directory
            var dirFiles = Directory.GetFiles(_dir);
            int foundFiles = 0;
            foreach (var file in dirFiles)
            {
                foreach (var requiredFile in REQUIRED_FILES)
                {
                    var modifiedRequiredFile = requiredFile.Replace("<MAPID>", MapID).Replace("<CHAPTER>", chapterCode);
                    if (file.Contains(modifiedRequiredFile))
                    {
                        RequiredFileData[Path.GetFileName(file)] = File.ReadAllBytes(file);
                        foundFiles++;
                    }
                }
            }
            return foundFiles >= REQUIRED_FILES.Count;
        }
    }
}
