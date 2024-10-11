using NPCMake.Core.RequiredFilesManagement;
using NPCMake.Core.Utils.Tinifan.Archive.XPCK;
using NPCMake.Core.Utils.Tinifan.Binary;
using NPCMake.Core.Utils.Tinifan.Tools;
using System.Numerics;
namespace NPCMake.Core.NPCLogic;

public class NPCPCKManager
{
    private string _npcName;
    private XPCK _pck;
    private Vector3 _npcPosition;
    private int _npcRot;
    public NPCPCKManager(byte[] pckData, string npcName, Vector3 npcPos, int npcRotation)
    {
        _npcName = npcName;
        _pck = new XPCK(pckData);
        _npcPosition = npcPos;
        _npcRot = npcRotation;
    }

    public byte[] SavePCK() { return _pck.Save(); }

    public void CreateNPCAppearanceData()
    {
        var submem = new SubMemoryStream(CreateNpcBin());
        //Add to xpck
        var npcbinpath = $"{_npcName}.npcbin";
        _pck.Directory.Files.Add(npcbinpath, submem);
    }

    private byte[] CreateNpcBin()
    {
        var defNpcBinData = DefaultNpcBin.DATA;
        var npcbin = new CfgBin();
        npcbin.Open(defNpcBinData);
        //Get POINT entry
        var point = npcbin.Entries.Last();
        point.Variables = [
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, _npcPosition.X),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, _npcPosition.Z),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, _npcPosition.Y),
            new(Core.Utils.Tinifan.Binary.Logic.Type.Int, _npcRot),
        ];
        var finishedNpcBin = npcbin.Save();
        return finishedNpcBin;
    }
}
