﻿#region Using directives

using System.IO;
using System.Linq;

#endregion

namespace SobekCM.Builder_Library.Modules.Items
{
    public class SaveToDatabaseModule : abstractSubmissionPackageModule
    {
        public override bool DoWork(Incoming_Digital_Resource Resource)
        {
            // Determine total size on the disk
            string[] all_files_final = Directory.GetFiles(Resource.Resource_Folder);
            double size = all_files_final.Sum(ThisFile => (double)(((new FileInfo(ThisFile)).Length) / 1024));
            Resource.DiskSpaceMb = size;

            // Also, set the TextSearchable flag correctly
            string[] text_files = Directory.GetFiles(Resource.Resource_Folder, "*.txt");
            bool page_image_text_found = false;
            foreach (string thisFile in text_files)
            {
                // Is this text from a PAGE IMAGE (jpeg or jp2) file?
                string filename_sans_extension = Path.GetFileNameWithoutExtension(thisFile);
                string possible_jpeg = Path.Combine(Resource.Resource_Folder, filename_sans_extension + ".jpg");
                string possible_jp2 = Path.Combine(Resource.Resource_Folder, filename_sans_extension + ".jpg");
                if ((File.Exists(possible_jp2)) || (File.Exists(possible_jpeg)))
                {
                    page_image_text_found = true;
                    break;
                }
            }
            Resource.Metadata.Behaviors.Text_Searchable = page_image_text_found;

            // Save this package to the database
            if (!Resource.Save_to_Database(Resource.NewPackage))
            {
                OnError("Error saving data to SobekCM database.  The database may not reflect the most recent data in the METS.", Resource.BibID + ":" + Resource.VID, Resource.METS_Type_String, Resource.BuilderLogId);
                return true;
            }

            return true;
        }
    }
}
