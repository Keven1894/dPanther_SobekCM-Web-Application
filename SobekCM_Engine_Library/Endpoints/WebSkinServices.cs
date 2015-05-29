﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Web;
using Jil;
using ProtoBuf;
using SobekCM.Core.Configuration;
using SobekCM.Core.Skins;
using SobekCM.Engine_Library.ApplicationState;
using SobekCM.Engine_Library.JSON_Client_Helpers;
using SobekCM.Engine_Library.Microservices;
using SobekCM.Engine_Library.Skins;
using SobekCM.Tools;

namespace SobekCM.Engine_Library.Endpoints
{
    /// <summary> Class supports all the web skin-level services provided by the SobekCM engine </summary>
    public class WebSkinServices
    {
        /// <summary> Gets the complete (language agnostic) web skin, by web skin code </summary>
        /// <param name="Response"></param>
        /// <param name="UrlSegments"></param>
        /// <param name="Protocol"></param>
        public void GetCompleteWebSkin(HttpResponse Response, List<string> UrlSegments, NameValueCollection QueryString, Microservice_Endpoint_Protocol_Enum Protocol)
        {
            if (UrlSegments.Count > 0)
            {
                Custom_Tracer tracer = new Custom_Tracer();

                string skinCode = UrlSegments[0];

                Complete_Web_Skin_Object returnValue = get_complete_web_skin(skinCode, tracer);


                if (Protocol == Microservice_Endpoint_Protocol_Enum.JSON)
                {
                    JSON.Serialize(returnValue, Response.Output, Options.ISO8601ExcludeNulls);
                }
                else
                {
                    Serializer.Serialize(Response.OutputStream, returnValue);
                }
            }
        }

        /// <summary> Gets the language-specific web skin, by web skin code and language code </summary>
        /// <param name="Response"></param>
        /// <param name="UrlSegments"></param>
        /// <param name="Protocol"></param>
        public void GetWebSkin(HttpResponse Response, List<string> UrlSegments, NameValueCollection QueryString, Microservice_Endpoint_Protocol_Enum Protocol)
        {
            if (UrlSegments.Count > 1)
            {
                Custom_Tracer tracer = new Custom_Tracer();

                // Get the code and language from the URL
                string skinCode = UrlSegments[0];
                string language = UrlSegments[1];
                Web_Language_Enum languageEnum = Web_Language_Enum_Converter.Code_To_Enum(language);

                Web_Skin_Object returnValue = get_web_skin(skinCode, languageEnum, Engine_ApplicationCache_Gateway.Settings.Default_UI_Language, tracer);


                if (Protocol == Microservice_Endpoint_Protocol_Enum.JSON)
                {
                    JSON.Serialize(returnValue, Response.Output, Options.ISO8601ExcludeNulls);
                }
                else
                {
                    Serializer.Serialize(Response.OutputStream, returnValue);
                }
            }
        }

        /// <summary> Get the list of ordered web skin codes </summary>
        /// <param name="Response"></param>
        /// <param name="UrlSegments"></param>
        /// <param name="Protocol"></param>
        public void GetOrderedCodes(HttpResponse Response, List<string> UrlSegments, NameValueCollection QueryString, Microservice_Endpoint_Protocol_Enum Protocol)
        {
            if (Protocol == Microservice_Endpoint_Protocol_Enum.JSON)
            {
                JSON.Serialize(Engine_ApplicationCache_Gateway.Web_Skin_Collection.Ordered_Skin_Codes, Response.Output, Options.ISO8601ExcludeNulls);
            }
            else
            {
                Serializer.Serialize(Response.OutputStream, Engine_ApplicationCache_Gateway.Web_Skin_Collection.Ordered_Skin_Codes);
            }
        }

        /// <summary> [PUBLIC] Get the list of uploaded images for a particular web skin </summary>
        /// <param name="Response"></param>
        /// <param name="UrlSegments"></param>
        /// <param name="Protocol"></param>
        /// <remarks> This REST API should be publicly available for users that are performing administrative work </remarks>
        public void GetWebSkinUploadedImages(HttpResponse Response, List<string> UrlSegments, NameValueCollection QueryString, Microservice_Endpoint_Protocol_Enum Protocol)
        {
            if (UrlSegments.Count > 0)
            {
                string webSkin = UrlSegments[0];

                // Ensure a valid aggregation
                Complete_Web_Skin_Object skinObject = get_complete_web_skin(webSkin, null);
                if (skinObject != null)
                {
                    List<UploadedFileFolderInfo> serverFiles = new List<UploadedFileFolderInfo>();

                    string design_folder = Engine_ApplicationCache_Gateway.Settings.Base_Design_Location + "skins\\" + webSkin + "\\uploads";
                    if (Directory.Exists(design_folder))
                    {
                        string foldername = webSkin;

                        string[] files = Directory.GetFiles(design_folder);
                        foreach (string thisFile in files)
                        {
                            string filename = Path.GetFileName(thisFile);
                            string extension = Path.GetExtension(thisFile);

                            // Exclude some files
                            if ((!String.IsNullOrEmpty(extension)) && (extension.ToLower().IndexOf(".db") < 0) && (extension.ToLower().IndexOf("bridge") < 0) && (extension.ToLower().IndexOf("cache") < 0))
                            {
                                string url = Engine_ApplicationCache_Gateway.Settings.System_Base_URL + "design/skins/" + webSkin + "/uploads/" + filename;
                                serverFiles.Add(new UploadedFileFolderInfo(url, foldername));
                            }
                        }
                    }

                    JSON.Serialize(serverFiles, Response.Output, Options.ISO8601ExcludeNulls);
                }
            }
        }

        #region Helper methods, destined to be private

        /// <summary> [HELPER] Gets the complete (language agnostic) web skin, by web skin code </summary>
        /// <param name="SkinCode"> Web skin code </param>
        /// <param name="Tracer"></param>
        /// <returns> A built complete web skin </returns>
        /// <remarks> This may be public now, but this will be converted into a private helped class with 
        /// the release of SobekCM 5.0 </remarks>
        public static Complete_Web_Skin_Object get_complete_web_skin(string SkinCode, Custom_Tracer Tracer)
        {
            DataRow thisRow = Engine_ApplicationCache_Gateway.Web_Skin_Collection.Skin_Row(SkinCode);
            if ( thisRow == null )
                return null;

            Complete_Web_Skin_Object returnObject = Web_Skin_Utilities.Build_Skin_Complete(thisRow);
            return returnObject;
        }

        /// <summary> [HELPER] Gets the language-specific web skin, by web skin code and language code </summary>
        /// <param name="SkinCode"> Web skin code </param>
        /// <param name="RequestedLanguage"> Web language </param>
        /// <param name="DefaultLanguage"> Default language, in case the requested web language does nto exist </param>
        /// <param name="Tracer"></param>
        /// <returns> A build language-specific web skin </returns>
        /// <remarks> This may be public now, but this will be converted into a private helped class with 
        /// the release of SobekCM 5.0 </remarks>
        public static Web_Skin_Object get_web_skin(string SkinCode, Web_Language_Enum RequestedLanguage, Web_Language_Enum DefaultLanguage, Custom_Tracer Tracer)
        {
            Complete_Web_Skin_Object completeSkin = get_complete_web_skin(SkinCode, Tracer);

            if (completeSkin == null)
                return null;

            return Web_Skin_Utilities.Build_Skin(completeSkin, Web_Language_Enum_Converter.Enum_To_Code(RequestedLanguage));
        }

        #endregion
    }
}
