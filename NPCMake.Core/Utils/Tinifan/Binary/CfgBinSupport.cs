﻿using System.Runtime.InteropServices;

namespace NPCMake.Core.Utils.Tinifan.Binary
{
    public class CfgBinSupport
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header
        {
            public int EntriesCount;
            public int StringTableOffset;
            public int StringTableLength;
            public int StringTableCount;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct KeyHeader
        {
            public int KeyLength;
            public int KeyCount;
            public int KeyStringOffset;
            public int keyStringLength;
        }
    }
}
