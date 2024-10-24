using NPCMake.Core.Utils.Tinifan.Binary;
using NPCMake.Core.Utils.Tinifan.Tools;

namespace NPCMake.Core.NPCLogic
{
    class NPCXQAndTrigger
    {
        private const int NPC_TRIGGER_TYPE = 11;
        private byte[] _triggerData;
        private int _npcId;
        private MapXQManager _mapXQManager;
        public const string TEMP_EXTRACT_XQ_PATH = "tmp/originalXq.xq";
        public NPCXQAndTrigger(byte[] triggerData, int npcID, MapXQManager mapXqManager)
        {
            _triggerData = triggerData;
            _npcId = npcID;
            _mapXQManager = mapXqManager;
        }
        public byte[] AddActionToXq()
        {
            return _mapXQManager.AddNewTriggerFunctionToXQ();
        }


        public byte[] LinkTrigger()
        {
            var trigger = new CfgBin();
            trigger.Open(_triggerData);
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
