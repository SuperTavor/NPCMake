using Tomlyn.Model;
using System.Text;
using NPCMake.Core.RequiredFilesManagement;
using NPCMake.Core.Utils.Tinifan.Tools;
using System.Runtime.CompilerServices;
namespace NPCMake.Core.NPCLogic;

public class NPCEditor
{
    private RequiredFilesManager _reqFilesManager;
    private uint _npcId = 0x0;
    private TomlTable _tomlTable;
    private string _npcName = "";
    private int _triggerFunctionId = 0;
    private int _baseId;
    private string _appearCond;
    private string _outdir = "";
    public NPCEditor(RequiredFilesManager manager, TomlTable table)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _tomlTable = table;
        _reqFilesManager = manager;
        _npcName = (string)_tomlTable["NpcName"];
        //Create NPC id
        _npcId = Crc32.Compute(Encoding.UTF8.GetBytes(_npcName));
        _baseId = (int)(long)_tomlTable["BaseId"];
        _appearCond = (string)_tomlTable["AppearCond"];
    }

    public void ApplyChanges()
    {
        var npcDataEditor = new NPCDataManager(_reqFilesManager, _npcName, _npcId, _baseId, _appearCond);
        var keys = _reqFilesManager.RequiredFileData.Keys.ToList();
        for (int i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            if (key.Contains("npc_set_0.01b"))
            {
                _reqFilesManager.RequiredFileData[key] = npcDataEditor.SetNPCInMap(_reqFilesManager.RequiredFileData[key]);
            }
            else if (key.Contains("npc.pck"))
            {
                _reqFilesManager.RequiredFileData[key] = EditNPCPck(_reqFilesManager.RequiredFileData[key]);
            }
            else if (key.Contains("npc_base_talk"))
            {
                _reqFilesManager.RequiredFileData[key] = npcDataEditor.MakeNPCTalkable(_reqFilesManager.RequiredFileData[key]);
            }
            else if (key.Contains($"{_reqFilesManager.MapID}.pck"))
            {
                _reqFilesManager.RequiredFileData[key] = EditMapPck(_reqFilesManager.RequiredFileData[key]);
            }
        }
    }

    public void PrintImportantInfo(List<string> toOutput = null!)
    {
        List<string> infos = [$"NPC ID For future use: {_npcId.ToString("X")}", $"Saved to {_outdir}"];
        //if it's null we just print to the console, else we modify the list
        if(toOutput != null)
        {
            foreach (var info in infos) toOutput.Add(info);
        }
        else
        {
            foreach(var info in infos) Console.WriteLine(info);
        }
    }
    private byte[] EditMapPck(byte[] pckData)
    {
        var editor = new MapPCKManager(_reqFilesManager, pckData, (string)_tomlTable["OnTalk"], _npcId);
        editor.CompileXQAndLinkTrigger();
        return editor.PackPCK();
    }
    public void ExportFiles()
    {
        _outdir = Path.GetFullPath($"{_npcName}_output");

        if (Directory.Exists(_outdir))
        {
            Directory.Delete(_outdir, true);
        }

        Directory.CreateDirectory(_outdir);

        foreach (var file in _reqFilesManager.RequiredFileData)
        {
            Console.WriteLine($"Exporting {file.Key}");
            File.WriteAllBytes(Path.Combine(_outdir, file.Key), file.Value);
        }
    }

    private byte[] EditNPCPck(byte[] pckData)
    {
        var editor = new NPCPCKManager(
            pckData,
            _npcName,
            new(
                (int)(long)_tomlTable["NpcX"],
                (int)(long)_tomlTable["NpcY"],
                (int)(long)_tomlTable["NpcZ"]),
                (int)(long)_tomlTable["NpcRotation"]
            );

        editor.CreateNPCAppearanceData();
        return editor.SavePCK();
    }



}
