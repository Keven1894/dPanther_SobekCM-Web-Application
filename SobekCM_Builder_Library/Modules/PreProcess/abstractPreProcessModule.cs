﻿using System.Collections.Generic;
using SobekCM.Core.Settings;

namespace SobekCM.Builder_Library.Modules.PreProcess
{
    public abstract class abstractPreProcessModule : iPreProcessModule
    {
        public List<string> Arguments { get; set; }

        public abstract void DoWork( InstanceWide_Settings Settings );

        public event ModuleErrorLoggingDelegate Error;
        public event ModuleStandardLoggingDelegate Process;

        protected long OnError(string LogStatement, string BibID_VID, string MetsType, long RelatedLogID)
        {
            if (Error != null)
                return Error(LogStatement, BibID_VID, MetsType, RelatedLogID);

            return -1;
        }

        protected long OnProcess(string LogStatement, string DbLogType, string BibID_VID, string MetsType, long RelatedLogID)
        {
            if (Process != null)
                return Process(LogStatement, DbLogType, BibID_VID, MetsType, RelatedLogID);

            return -1;
        }
    }
}
