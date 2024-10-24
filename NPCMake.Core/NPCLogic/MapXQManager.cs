using NPCMake.Core.RequiredFilesManagement;
using NPCMake.Core.Utils.Tinifan.Archive.XPCK;
using NPCMake.Core.Utils.Tinifan.Tools;
using System.Diagnostics;
using System.Text;

namespace NPCMake.Core.NPCLogic;

class MapXQManager
{
    private RequiredFilesManager _reqFilesManager;

    public int TriggerFunctionID = 0;

    private string _onNpcTalkCode;

    private const string TEMP_ORIGINAL_EXTRACTED_XQ_PATH = "tmp/originalXq.xq";

    private readonly string TEMP_EDITED_DECOMPILED_XQ_PATH = $"{TEMP_ORIGINAL_EXTRACTED_XQ_PATH}.txt";

    private const string TEMP_DIR_PATH = "tmp";

    private const string TEMP_EDITED_COMPILED_XQ_PATH = $"{TEMP_ORIGINAL_EXTRACTED_XQ_PATH}.txt.xq";


    public MapXQManager(RequiredFilesManager reqFilesManager, string onNPCTalkCode, string? xqPath = null)
    {
        _onNpcTalkCode = onNPCTalkCode;
        _reqFilesManager = reqFilesManager;
        if (xqPath != null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(TEMP_ORIGINAL_EXTRACTED_XQ_PATH));
            File.Copy(xqPath, TEMP_ORIGINAL_EXTRACTED_XQ_PATH, true);
        }
    }

    public byte[] AddNewTriggerFunctionToXQ()
    {
        Console.WriteLine("Compiling NPC XQ...");
        //decompile it
        DecompileVanillaTrigger();

        var xqContent = File.ReadAllText(TEMP_EDITED_DECOMPILED_XQ_PATH);
        TriggerFunctionID = GetNewTriggerFunctionID(xqContent);
        var updatedXq = AddOnTalkFuncToXq(xqContent, TriggerFunctionID);
        //Write back to decompiled file
        File.WriteAllText(TEMP_EDITED_DECOMPILED_XQ_PATH, updatedXq);
        //compile
        CompileEditedTrigger();

        var newXqbytes = File.ReadAllBytes(TEMP_EDITED_COMPILED_XQ_PATH);
        DeleteTempDirectory();

        return newXqbytes;
    }

    private void DecompileVanillaTrigger()
    {
        PerformXtractQueryOperation($"-o e -f {TEMP_ORIGINAL_EXTRACTED_XQ_PATH}", "Initial decomp error");
    }

    private void PerformXtractQueryOperation(string arguments, string errorText)
    {
        var proc = Process.Start(new ProcessStartInfo("xtractquery", arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        });
        proc!.WaitForExit();
        if (proc.ExitCode != 0)
        {
            Console.WriteLine($"Failed to compile XQ: {errorText}:\n{proc.StandardError.ReadToEnd()}");
            DeleteTempDirectory();
            Environment.Exit(1);
        }
    }

    private void DeleteTempDirectory()
    {
        Directory.Delete(TEMP_DIR_PATH, true);
    }
    private void CompileEditedTrigger()
    {
        PerformXtractQueryOperation($"-o c -t xq32 -f {TEMP_EDITED_DECOMPILED_XQ_PATH}", "Final compilation error");
    }
    private string AddOnTalkFuncToXq(string ogXq, int triggerFunctiojnId)
    {
        StringBuilder funcSb = new(ogXq);
        funcSb.AppendLine($"RunCmd_Map{triggerFunctiojnId}() {{");
        funcSb.Append(_onNpcTalkCode);
        funcSb.AppendLine("}");
        return funcSb.ToString();
    }
    private int GetNewTriggerFunctionID(string xq)
    {
        int lastNum = 0;
        //Reverse to get to the last definition faster
        foreach (var line in xq.Split('\n').Reverse())
        {
            //To make sure it's not a call to RunCmd_Map. but a definition, we check if it contains $
            //(Variable definition nominator, and as there are no void calls in XQ, a $ is enough to determine a call of any sort)
            if (line.Contains("RunCmd_Map") && !line.Contains("$"))
            {
                //Get number from there
                var num = int.Parse(line.Replace("RunCmd_Map", "").Replace("()", "").Trim());
                if (num > lastNum) lastNum = num;
            }
        }
        return lastNum + 1;
    }
}
