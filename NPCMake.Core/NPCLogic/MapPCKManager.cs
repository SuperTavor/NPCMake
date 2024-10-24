using NPCMake.Core.RequiredFilesManagement;
using NPCMake.Core.Utils.Tinifan.Archive.XPCK;
namespace NPCMake.Core.NPCLogic
{
    class MapPCKManager
    {
        private RequiredFilesManager _reqFilesManager;
        private XPCK _mapPck;
        private int _npcId;
        private NPCXQAndTrigger _xqAndTriggerEditor;
        public MapPCKManager(RequiredFilesManager reqFilesManager, byte[] mapPckData, string OnNpcTalkCode, int npcId)
        {
            _npcId = npcId;
            _reqFilesManager = reqFilesManager;
            _mapPck = new XPCK(mapPckData);
            ExtractXQ();
            _xqAndTriggerEditor = new(_mapPck.Directory.Files[$"{reqFilesManager.MapID}_trigger.cfg.bin"].ByteContent, _npcId, new(_reqFilesManager, OnNpcTalkCode));
        }
        
        private void ExtractXQ()
        {
            if (!Directory.Exists("tmp")) Directory.CreateDirectory("tmp");
            File.WriteAllBytes(NPCXQAndTrigger.TEMP_EXTRACT_XQ_PATH, _mapPck.Directory.Files[$"{_reqFilesManager.MapID}.xq"].ByteContent!);
        }

        public void EditFilesInMapPck()
        {
            _mapPck.Directory.Files[$"{_reqFilesManager.MapID}.xq"] = new(_xqAndTriggerEditor.AddActionToXq());
            _mapPck.Directory.Files[$"{_reqFilesManager.MapID}_trigger.cfg.bin"] = new(_xqAndTriggerEditor.LinkTrigger());
        }
        public byte[] PackPCK()
        {
            return _mapPck.Save();
        }
       
    }
}
