﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SobekCM.Builder_Library.Settings
{
    public static class MultiInstance_Builder_Settings
    {
        static MultiInstance_Builder_Settings()
        {
            Instance_Package_Limit = -1;
        }

        /// <summary> Maximum number of packages to process for each instance, before moving onto the 
        /// instance  </summary>
        /// <remarks> -1 is the default value and indicates no limit </remarks>
        public static int Instance_Package_Limit { get; set; }

        /// <summary> ImageMagick executable file </summary>
        public static string ImageMagick_Executable { get; set; }

        /// <summary> Ghostscript executable file </summary>
        public static string Ghostscript_Executable { get; set; }

    }
}
