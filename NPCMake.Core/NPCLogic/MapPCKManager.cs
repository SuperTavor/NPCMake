using NPCMake.Core.RequiredFilesManagement;
using NPCMake.Core.Utils.Tinifan.Archive.XPCK;
using NPCMake.Core.Utils.Tinifan.Binary;
namespace NPCMake.Core.NPCLogic
{
    public class MapPCKManager
    {
        private RequiredFilesManager _reqFilesManager;
        private MapXQManager _mapXQManager;
        private XPCK _mapPck;
        private const int NPC_TRIGGER_TYPE = 11;
        private int _npcId;
        public MapPCKManager(RequiredFilesManager reqFilesManager, byte[] mapPckData, string OnNpcTalkCode, int npcId)
        {
            _npcId = npcId;
            _reqFilesManager = reqFilesManager;
            _mapPck = new XPCK(mapPckData);
            _mapXQManager = new(_reqFilesManager, _mapPck, OnNpcTalkCode);
        }
        public void CompileXQAndLinkTrigger()
        {
            Console.WriteLine("Compiling NPC XQ...");
            //Add code to XQ
            _mapXQManager.AddNewTriggerFunctionToXQ();
            //Link trigger
            Console.WriteLine("Linking trigger to XQ...");
            var triggerData = _mapPck.Directory.Files[$"{_reqFilesManager.MapID}_trigger.cfg.bin"].ByteContent;
            _mapPck.Directory.Files[$"{_reqFilesManager.MapID}_trigger.cfg.bin"] = new(LinkTrigger(triggerData));

        }

        public byte[] PackPCK()
        {
            return _mapPck.Save();
        }
        private byte[] LinkTrigger(byte[] triggerData)
        {
            var trigger = new CfgBin();
            trigger.Open(triggerData);
            //Increment entry count
            trigger.Entries[0].Variables[0].Value = (int)trigger.Entries[0].Variables[0].Value + 1;
            //Create new entry
            var newEntry = trigger.Entries.Last().Clone();
            newEntry.Variables = [
                new(Core.Utils.Tinifan.Binary.Logic.Type.Int, NPC_TRIGGER_TYPE),
                new(Core.Utils.Tinifan.Binary.Logic.Type.Int, _npcId),
                new(Core.Utils.Tinifan.Binary.Logic.Type.Int, 0),
                new(Core.Utils.Tinifan.Binary.Logic.Type.Int, 0),
                new(Core.Utils.Tinifan.Binary.Logic.Type.Int, 0),
                new(Core.Utils.Tinifan.Binary.Logic.Type.Int, 0),
                new(Core.Utils.Tinifan.Binary.Logic.Type.Int, _mapXQManager.TriggerFunctionID),
            ];
            trigger.Entries.Add(newEntry);

            var updatedtriggerdata = trigger.Save();
            return updatedtriggerdata;
        }
    }
}
