using NPCMake.Core.RequiredFilesManagement;
using NPCMake.Core.Utils.Tinifan.Binary;
namespace NPCMake.Core.NPCLogic;

public class NPCDataManager
{
    private RequiredFilesManager _reqFilesManager;
    private string _npcName;
    private int _npcId;
    private string _appearCond;
    private int _baseId;
    public NPCDataManager(RequiredFilesManager reqFilesManager, string npcName, int npcId, int baseId, string appearCond)
    {
        _reqFilesManager = reqFilesManager;
        _npcName = npcName;
        _npcId = npcId;
        _baseId = baseId;
        _appearCond = appearCond;
    }

    public byte[] MakeNPCTalkable(byte[] npcBaseTalkData)
    {
        var cfgbin = new CfgBin();
        cfgbin.Open(npcBaseTalkData);
        //Increment entry count
        cfgbin.Entries[0].Variables[0].Value = (int)cfgbin.Entries[0].Variables[0].Value + 1;
        //Clone last entry and populate it
        var newEntry = cfgbin.Entries[0].Children[0].Clone();
        newEntry.Variables = [
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, _npcId),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, 0),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, 1),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, 1),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, 1),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, 2),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, 1),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, 3),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, 1),
        ];
        cfgbin.Entries[0].Children.Add(newEntry);

        return cfgbin.Save();
    }


    private void AddNpcSetBaseEntry(CfgBin cfgbin)
    {
        //Add NPC_BASE entry
        var npcBase = cfgbin.Entries[0];
        //Increment entry count
        npcBase.Variables[0].Value = (int)npcBase.Variables[0].Value + 1;
        //Dupe an entry and populate it with proven OK values
        var newNpcBaseEntry = npcBase.Children[0].Clone();
        newNpcBaseEntry.Variables =
        [
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, _npcId),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, 0),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, _baseId),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, 0),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, 2),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, 0),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, 0),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, 0),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, 0),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, 0),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, 1),
        ];

        //add new entry
        npcBase.Children.Add(newNpcBaseEntry);
    }

    private int AddNpcAppearEntryAndGetOffset(CfgBin cfgbin)
    {
        //Add NPC_APPEAR entry
        var npcAppear = cfgbin.Entries[2];
        //increment entry count
        npcAppear.Variables[0].Value = (int)npcAppear.Variables[0].Value + 1;
        var newNpcAppearEntry = npcAppear.Children[0].Clone();
        newNpcAppearEntry.Variables =
        [
            //npc name
            new(Core.Utils.Tinifan.Binary.Logic.Type.String, _npcName),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, -1),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, -1),
            new(Core.Utils.Tinifan.Binary.Logic.Type.String, _appearCond),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, -1),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, 0),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, -1),
        ];
        npcAppear.Children.Add(newNpcAppearEntry);
        //get new entry's offset to use later
        var npcAppearOffset = (int)npcAppear.Variables[0].Value - 1;

        return npcAppearOffset;
    }

    private void AddNpcPresentEntry(CfgBin cfgbin, int npcAppearOffset)
    {
        //Add NPC_PRESENT entry
        var npcPresent = cfgbin.Entries[1];
        //increment variable count
        npcPresent.Variables[0].Value = (int)npcPresent.Variables[0].Value + 1;
        //clone entry
        var newNpcPresentEntry = npcPresent.Children[0].Clone();
        //Set variables
        newNpcPresentEntry.Variables =
        [
           new(Core.Utils.Tinifan.Binary.Logic.Type.Int, _npcId),
           //offset of our NPC_APPEAR entry
           new(Core.Utils.Tinifan.Binary.Logic.Type.Int, npcAppearOffset),
           new(Core.Utils.Tinifan.Binary.Logic.Type.Int, 1),
        ];
        npcPresent.Children.Add(newNpcPresentEntry);
    }
    public byte[] SetNPCInMap(byte[] npcSetCfgBinData)
    {
        var cfgbin = new CfgBin();
        cfgbin.Open(npcSetCfgBinData);
        AddNpcSetBaseEntry(cfgbin);
        var appearOffset = AddNpcAppearEntryAndGetOffset(cfgbin);
        AddNpcPresentEntry(cfgbin, appearOffset);
        return cfgbin.Save();
    }
}
