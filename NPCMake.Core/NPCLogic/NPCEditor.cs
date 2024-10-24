using Tomlyn.Model;
using System.Text;
using NPCMake.Core.RequiredFilesManagement;
using NPCMake.Core.Utils.Tinifan.Tools;
using NPCMake.Core.Utils.Tinifan.Archive.XPCK;

namespace NPCMake.Core.NPCLogic;
public class NPCEditor
{
    private RequiredFilesManager _reqFilesManager;
    private int _npcId = 0x0;
    private TomlTable _tomlTable;
    private string _npcName = "";
    private int _triggerFunctionId = 0;
    private int _baseId;
    private string _appearCond;
    private string _outdir = "";
    private bool _editedXq = false;
    private MapXQManager? _xqManager;

    public NPCEditor(RequiredFilesManager manager, TomlTable table)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _tomlTable = table;
        _reqFilesManager = manager;
        _npcName = (string)_tomlTable["NpcName"];
        //Create NPC id
        _npcId = (int)Crc32.Compute(Encoding.UTF8.GetBytes(_npcName));
        _baseId = (int)(long)_tomlTable["BaseId"];
        _appearCond = (string)_tomlTable["AppearCond"];
    }

    public void ApplyChanges()
    {
        var npcDataEditor = new NPCDataManager(_reqFilesManager, _npcName, _npcId, _baseId, _appearCond, GetNpcTypeAsInt());
        var keys = _reqFilesManager.RequiredFileData.Keys.ToList();
        for (int i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            if (key.Contains("npc_set_0.01") || key.Contains("npc_set_0.02"))
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
            else if(key.Contains($"{_reqFilesManager.MapID}_trigger"))
            {
                _reqFilesManager.RequiredFileData[key] = EditTriggerDirectly(_reqFilesManager.RequiredFileData[key]);
            }
            else if(key.Contains($"{_reqFilesManager.MapID}.xq") && !_editedXq)
            {
                _reqFilesManager.RequiredFileData[key] = EditXqDirectly();
                _editedXq = true;
            }
        }
        //for yw1, npc.pck doesn't exist - all the npc files are out in the open, so if it's yw1, we should add a npcbin now.
        var npcBinPath = $"data/res/map/{_reqFilesManager.MapID}/{_npcName}.npcbin";
        _reqFilesManager.RequiredFileData[npcBinPath] = CreateNpcBinDirectly();
    }
    private byte[] EditTriggerDirectly(byte[] trigger)
    {
        //b4 editing the trigger u have to edit the xq so let's edit the xq first if not edited
        if(!_editedXq)
        {
            var xqPath = $"seq/map/{_reqFilesManager.MapID}.xq";
            _reqFilesManager.RequiredFileData[xqPath] = EditXqDirectly();
        }
        var triggerPath = $"data/res/map/{_reqFilesManager.MapID}/{_reqFilesManager.MapID}_trigger.cfg.bin";
        var manager = new NPCXQAndTrigger(trigger, _npcId, _xqManager!);
        return manager.LinkTrigger();
    }

    private byte[] EditXqDirectly()
    {
        var xqPath = $"seq/map/{_reqFilesManager.MapID}.xq";
        var requiredXqPath = Path.Combine(_reqFilesManager.RequiredFilesSourceDir, xqPath);
        _xqManager = new MapXQManager(_reqFilesManager, (string)_tomlTable["OnTalk"], Path.Combine(_reqFilesManager.RequiredFilesSourceDir, requiredXqPath));
        _editedXq = true;
        return _xqManager.AddNewTriggerFunctionToXQ();
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
        editor.EditFilesInMapPck();
        return editor.PackPCK();
    }
    public void ExportFiles()
    {
        _outdir = $"{_npcName}_output";

        if (Directory.Exists(_outdir))
        {
            Directory.Delete(_outdir, true);
        }

        Directory.CreateDirectory(_outdir);

        foreach (var file in _reqFilesManager.RequiredFileData)
        {
            Console.WriteLine($"Exporting {file.Key.Replace("\\", "/")}");
            var exportPath = Path.Combine(_outdir, file.Key);
            Directory.CreateDirectory(Path.GetDirectoryName(exportPath)!);
            File.WriteAllBytes(exportPath, file.Value);
        }
    }

    private byte[] EditNPCPck(byte[] pckData)
    {
        var npcbinData = CreateNpcBinDirectly();
        var pck = new XPCK(pckData);
        var submem = new SubMemoryStream(npcbinData);
        //Add to xpck
        var npcbinpath = $"{_npcName}.npcbin";
        if (pck.Directory.Files.ContainsKey(npcbinpath))
        {
            pck.Directory.Files[npcbinpath] = submem;
        }
        else pck.Directory.Files.Add(npcbinpath, submem);
        return pck.Save();
    }

    private int GetNpcTypeAsInt()
    {
        var npcTypeObj = _tomlTable["NpcType"];
        if(npcTypeObj is not long and not string)
        {
            Console.WriteLine("WARNING: NpcType has an invalid value. Defaulted to 2 (HUMAN)");
            return 2;
        }
        else
        {
            if(npcTypeObj is long)
            {
                return (int)(long)npcTypeObj;
            }
            else if(npcTypeObj is string)
            {
                if(Enum.TryParse((string)npcTypeObj, true, out NPCType npcType))
                {
                    return (int)npcType;
                }
                else
                {
                    Console.WriteLine("WARNING: NpcType has an invalid value. Defaulted to 2 (HUMAN)");
                    return 2;
                }
            }
        }

        return -1;
    }

    private byte[] CreateNpcBinDirectly()
    {
        //Check if positions are ints or floats
        List<object> objectPositions = [_tomlTable["NpcX"], _tomlTable["NpcY"], _tomlTable["NpcZ"]];
        List<float> floatPositions = [];
        for (int i = 0; i < objectPositions.Count; i++)
        {
            if (objectPositions[i] is long)
            {
                floatPositions.Add((long)objectPositions[i]);
            }
            else
            {
                floatPositions.Add((float)(double)objectPositions[i]);
            }
        }
        var npcbin = new NPCBinManager(
            _npcName,
            new
            (
                 floatPositions[0],
                 floatPositions[1],
                 floatPositions[2]
            ),

            (int)(long)_tomlTable["NpcRotation"]
        );

        return npcbin.CreateNpcBin();
    }


}
