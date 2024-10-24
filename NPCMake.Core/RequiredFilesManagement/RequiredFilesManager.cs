using System.Diagnostics;

namespace NPCMake.Core.RequiredFilesManagement
{
    public class RequiredFilesManager
    {
        public List<string> RequiredFiles;

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

# Specifies if you are adding your NPC to Yo-kai Watch 1. Leave as false if you are working with YW2 or 3.
IsYw1 = false

# How will your NPC appear on the map?
# Available NPCTypes:
# HUMAN
# YOKAI
# (Make sure to spell your NPCType exactly like in the last comments, or replace the NPCType with an int value representing any NPCType.)
NpcType = ""HUMAN""
    ";

        public Dictionary<string, byte[]> RequiredFileData = new Dictionary<string, byte[]>();
        public string RequiredFilesSourceDir;
        public string MapID = "";
        public RequiredFilesManager(string dir, string mapid, bool isYw1)
        {
            RequiredFilesSourceDir = dir;
            MapID = mapid;
            RequiredFiles = GetProperRequiredFiles(isYw1);
        }

        private List<string> GetProperRequiredFiles(bool isYw1) => isYw1 switch
        {
            false => 
            [
                "data/res/map/<MAPID>/npc.pck",
                "data/res/map/<MAPID>/<MAPID>.pck",
                "data/res/map/<MAPID>/<MAPID>_npc_base_talk_<CHAPTER>",
                "data/res/map/<MAPID>/<MAPID>_npc_set_0.01",
            ],
            true =>
            [
                "data/res/map/<MAPID>/<MAPID>_trigger",
                "data/res/map/<MAPID>/<MAPID>_npc_set_0.02",
                "data/res/map/<MAPID>/<MAPID>_npc_base_talk_<CHAPTER>_0.02",
                "seq/map/<MAPID>.xq"
            ]
        };

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
            var dirFiles = Directory.GetFiles(RequiredFilesSourceDir, "*.*", SearchOption.AllDirectories)
                                .Select(x => x.Replace("\\","/"));
            int foundFiles = 0;
            foreach (var file in dirFiles)
            {
                foreach (var requiredFile in RequiredFiles)
                {
                    var modifiedRequiredFile = requiredFile.Replace("<MAPID>", MapID).Replace("<CHAPTER>", chapterCode);
                    if (file.Contains(modifiedRequiredFile))
                    {
                        RequiredFileData[Path.GetRelativePath(RequiredFilesSourceDir,file)] = File.ReadAllBytes(file);
                        foundFiles++;
                    }
                }
            }
            return foundFiles >= RequiredFiles.Count;
        }
    }
}
