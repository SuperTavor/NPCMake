using NPCMake.Core.RequiredFilesManagement;
using NPCMake.Core.Utils.Tinifan.Archive.XPCK;
using NPCMake.Core.Utils.Tinifan.Binary;
using NPCMake.Core.Utils.Tinifan.Tools;
using System.Numerics;
namespace NPCMake.Core.NPCLogic;

class NPCBinManager
{
    private string _npcName;
    private Vector3 _npcPosition;
    private int _npcRot;
    public NPCBinManager(string npcName, Vector3 npcPos, int npcRotation)
    {
        _npcName = npcName;
        _npcPosition = npcPos;
        _npcRot = npcRotation;
    }

    public byte[] CreateNpcBin()
    {
        var npcbin = new CfgBin();
        npcbin.Open(DefaultNpcBin.DATA);
        //Get POINT entry
        var point = npcbin.Entries.Last();
        point.Variables = [
            new(Core.Utils.Tinifan.Binary.Logic.Type.Float, _npcPosition.X),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Float, _npcPosition.Z),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Float, _npcPosition.Y),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, _npcRot),
        ];
        var finishedNpcBin = npcbin.Save();
        return finishedNpcBin;
    }
}
