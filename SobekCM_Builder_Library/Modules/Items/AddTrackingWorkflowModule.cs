﻿#region Using directives

using System;
using SobekCM.Library.Database;

#endregion

namespace SobekCM.Builder_Library.Modules.Items
{
    public class AddTrackingWorkflowModule : abstractSubmissionPackageModule
    {
        public override bool DoWork(Incoming_Digital_Resource Resource)
        {
            // Clear the flag for additional work
            SobekCM_Database.Update_Additional_Work_Needed_Flag(Resource.Metadata.Web.ItemID, false, null);

            // Mark a log in the database that this was handled as well
            Resource_Object.Database.SobekCM_Database.Add_Workflow(Resource.Metadata.Web.ItemID, "Bulk Loaded", String.Empty, "SobekCM Bulk Loader", String.Empty);

            // If the item is born digital, has files, and is currently public, close out the digitization milestones completely
            if ((!Resource.Metadata.Tracking.Born_Digital_Is_Null) && (Resource.Metadata.Tracking.Born_Digital) && (Resource.Metadata.Behaviors.IP_Restriction_Membership >= 0) && (Resource.Metadata.Divisions.Download_Tree.Has_Files))
            {
                Resource_Object.Database.SobekCM_Database.Update_Digitization_Milestone(Resource.Metadata.Web.ItemID, 4, DateTime.Now);
            }

            return true;
        }
    }
}
