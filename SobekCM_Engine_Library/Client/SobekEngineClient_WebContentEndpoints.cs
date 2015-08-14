﻿#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SobekCM.Core.MemoryMgmt;
using SobekCM.Core.MicroservicesClient;
using SobekCM.Core.WebContent;
using SobekCM.Core.WebContent.Admin;
using SobekCM.Core.WebContent.Hierarchy;
using SobekCM.Core.WebContent.Single;
using SobekCM.Engine_Library.Endpoints;
using SobekCM.Tools;

#endregion

namespace SobekCM.Core.Client
{
    /// <summary> Gateway to all the web content-related endpoints exposed by the SobekCM engine </summary>
    public class SobekEngineClient_WebContentEndpoints : MicroservicesClientBase
    {
        /// <summary> Constructor for a new instance of the SobekEngineClient_WebContentEndpoints class </summary>
        /// <param name="ConfigObj"> Fully constructed microservices client configuration </param>
        public SobekEngineClient_WebContentEndpoints(MicroservicesClient_Configuration ConfigObj) : base(ConfigObj)
        {
            // All work done in the base constructor
        }

        /// <summary> Get the information for a single non-aggregational web content page </summary>
        /// <param name="WebContentID"> Primary key for this non-aggregational web content page </param>
        /// <param name="Tracer"></param>
        /// <returns> Object with all the information and source text for the top-level web content page </returns>
        public HTML_Based_Content Get_HTML_Based_Content(int WebContentID, Custom_Tracer Tracer)
        {
            // Add a beginning trace
            Tracer.Add_Trace("SobekEngineClient_WebContentServices.Get_HTML_Based_Content", "Get by primary key");

            // Look in the cache
            HTML_Based_Content fromCache = CachedDataManager.WebContent.Retrieve_Page_Details(WebContentID, Tracer);
            if (fromCache != null)
            {
                Tracer.Add_Trace("SobekEngineClient_WebContentServices.Get_HTML_Based_Content", "Found page in the local cache");
                return fromCache;
            }

            // Get the endpoint
            MicroservicesClient_Endpoint endpoint = GetEndpointConfig("WebContent.Get_HTML_Based_Content_By_ID", Tracer);

            // Format the URL
            string url = String.Format(endpoint.URL, WebContentID);

            // Call out to the endpoint and deserialize the object
            HTML_Based_Content returnValue = Deserialize<HTML_Based_Content>(url, endpoint.Protocol, Tracer);

            // Add to the local cache
            if (returnValue != null)
            {
                Tracer.Add_Trace("SobekEngineClient_WebContentServices.Get_HTML_Based_Content", "Store page in the local cache");
                CachedDataManager.WebContent.Store_Page_Details(returnValue, Tracer);
            }

            // Return the object
            return returnValue;
        }

        /// <summary> Get the information for a single top-level web content page </summary>
        /// <param name="InfoBrowseMode"> Path for the requested web content page ( i.e., software/download/.. ) </param>
        /// <param name="Tracer"></param>
        /// <returns> Object with all the information and source text for the top-level web content page </returns>
        public HTML_Based_Content Get_HTML_Based_Content( string InfoBrowseMode, Custom_Tracer Tracer )
        {
            // Add a beginning trace
            Tracer.Add_Trace("SobekEngineClient_WebContentServices.Get_HTML_Based_Content", "Get by URL, or info/browse code");

            // Get the endpoint
            MicroservicesClient_Endpoint endpoint = GetEndpointConfig("WebContent.Get_HTML_Based_Content_By_URL", Tracer);

            // Format the URL
            string url = String.Format(endpoint.URL, InfoBrowseMode);

            // Call out to the endpoint and return the deserialized object
            return Deserialize<HTML_Based_Content>(url, endpoint.Protocol, Tracer);
        }

        /// <summary> Gets the special missing web content page, used when a requested resource is missing </summary>
        /// <param name="Tracer"></param>
        /// <returns> Object with all the information and source text for the special top-level missing web content page </returns>
        public HTML_Based_Content Get_Special_Missing_Page(Custom_Tracer Tracer)
        {
            // Add a beginning trace
            Tracer.Add_Trace("SobekEngineClient_WebContentServices.Get_Special_Missing_Page", "Get by primary key");

            // Look in the cache
            HTML_Based_Content fromCache = CachedDataManager.WebContent.Retrieve_Special_Missing_Page(Tracer);
            if (fromCache != null)
            {
                Tracer.Add_Trace("SobekEngineClient_WebContentServices.Get_Special_Missing_Page", "Found page in the local cache");
                return fromCache;
            }

            // Get the endpoint
            MicroservicesClient_Endpoint endpoint = GetEndpointConfig("WebContent.Get_Special_Missing_Page", Tracer);

            // Call out to the endpoint and deserialize the object
            HTML_Based_Content returnValue = Deserialize<HTML_Based_Content>(endpoint.URL, endpoint.Protocol, Tracer);

            // Add to the local cache
            if (returnValue != null)
            {
                Tracer.Add_Trace("SobekEngineClient_WebContentServices.Get_Special_Missing_Page", "Store page in the local cache");
                CachedDataManager.WebContent.Store_Special_Missing_Page(returnValue, Tracer);
            }

            // Return the object
            return returnValue;
        }

        /// <summary> Get the list of milestones affecting a single (non aggregation affiliated) static web content page </summary>
        /// <param name="WebContentID"> Primary key to the web page in question </param>
        /// <param name="Tracer"></param>
        /// <returns> Single page milestone report wrapper </returns>
        public Single_WebContent_Change_Report Get_Single_Milestones(int WebContentID, Custom_Tracer Tracer)
        {
            // Add a beginning trace
            Tracer.Add_Trace("SobekEngineClient_WebContentServices.Get_Single_Milestones");

            // Get the endpoint
            MicroservicesClient_Endpoint endpoint = GetEndpointConfig("WebContent.Get_Single_Milestones", Tracer);

            // Format the URL
            string url = String.Format(endpoint.URL, WebContentID);

            // Call out to the endpoint and return the deserialized object
            return Deserialize<Single_WebContent_Change_Report>(url, endpoint.Protocol, Tracer);
        }

        /// <summary> Get the complete monthly usage for a single web content page </summary>
        /// <param name="WebContentID"> Primary key of the web content page for which this the usage report applies </param>
        /// <param name="Tracer"></param>
        /// <returns> Single web content usage report wrapper around the list of monthly usage hits </returns>
        public Single_WebContent_Usage_Report Get_Single_Usage_Report(int WebContentID, Custom_Tracer Tracer)
        {
            // Add a beginning trace
            Tracer.Add_Trace("SobekEngineClient_WebContentServices.Get_Single_Usage_Report");

            // Get the endpoint
            MicroservicesClient_Endpoint endpoint = GetEndpointConfig("WebContent.Get_Single_Usage_Report", Tracer);

            // Format the URL
            string url = String.Format(endpoint.URL, WebContentID);

            // Call out to the endpoint and return the deserialized object
            return Deserialize<Single_WebContent_Usage_Report>(url, endpoint.Protocol, Tracer);
        }

        /// <summary> Get the complete hierarchy of web content pages and redirects, used for navigation </summary>
        /// <param name="UseCache"> Flag indicates whether this should look in the cache and store in the cache </param>
        /// <param name="Tracer"></param>
        /// <returns> Complete hierarchy of non-aggregational web content pages and redirects, used for navigation </returns>
        public WebContent_Hierarchy Get_Hierarchy(bool UseCache, Custom_Tracer Tracer)
        {
            // Add a beginning trace
            if ( Tracer != null )
                Tracer.Add_Trace("SobekEngineClient_WebContentServices.Get_Hierarchy");

            // Look in the cache if that is included here
            if ((Config.UseCache) && (UseCache))
            {
                WebContent_Hierarchy cacheValue = CachedDataManager.WebContent.Retrieve_Hierarchy(Tracer);
                if (cacheValue != null)
                    return cacheValue;
            }

            // Get the endpoint
            MicroservicesClient_Endpoint endpoint = GetEndpointConfig("WebContent.Get_Hierarchy", Tracer);

            // Call out to the endpoint and return the deserialized object
            WebContent_Hierarchy returnValue = Deserialize<WebContent_Hierarchy>(endpoint.URL, endpoint.Protocol, Tracer);

            // If there was a result and cache should be used, cache if 
            if ((returnValue != null) && (UseCache) && ( Config.UseCache ))
            {
                CachedDataManager.WebContent.Store_Hierarchy(returnValue, Tracer);
            }

            return returnValue;
        }



        #region Endpoints related to global recent updates

        /// <summary> Returns a flag indicating if there are any global recent updates to the web content entities (pages and redirects) </summary>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        /// <returns> Flag indicating if there are recent updates to web content entities </returns>
        public bool Has_Global_Recent_Updates(Custom_Tracer Tracer)
        {
            // Add a beginning trace
            Tracer.Add_Trace("SobekEngineClient_WebContentServices.Has_Global_Recent_Updates");

            // Look in the the cache a recently stored value
            if (Config.UseCache)
            {
                bool? cacheFlag = CachedDataManager.WebContent.Retrieve_Has_Global_Recent_Updates_Flag(Tracer);
                if (cacheFlag.HasValue)
                    return cacheFlag.Value;
            }

            // Get the endpoint
            MicroservicesClient_Endpoint endpoint = GetEndpointConfig("WebContent.Has_Global_Recent_Updates", Tracer);

            // Format the URL
            string url = endpoint.URL;

            // Call out to the endpoint and return the deserialized object
            bool returnValue = Deserialize<bool>(url, endpoint.Protocol, Tracer);

            // Store in the cache
            if (Config.UseCache)
                CachedDataManager.WebContent.Store_Has_Global_Recent_Updates_Flag(returnValue, Tracer);

            return returnValue;
        }

        /// <summary> Get the list of all the recent updates to all (non aggregation affiliated) static web content pages </summary>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        /// <param name="Page"> Page number of recent updates ( starting with one and counting up ) </param>
        /// <param name="RowsPerPage"> (Optional) Number of rows of updates to include in each page of results </param>
        /// <param name="UserFilter"> (Optional) Filter to only return items updated by one user </param>
        /// <returns> List of requested recent udpates </returns>
        public WebContent_Recent_Changes Get_Global_Recent_Updates(Custom_Tracer Tracer, int Page, int? RowsPerPage = null, string UserFilter = null )
        {
            // Add a beginning trace
            Tracer.Add_Trace("SobekEngineClient_WebContentServices.Get_Global_Recent_Updates");

            // Get the endpoint
            MicroservicesClient_Endpoint endpoint = GetEndpointConfig("WebContent.Get_Global_Recent_Updates", Tracer );

            // Format the URL
            string url = endpoint.URL + "?page=" + Page;
            if ((RowsPerPage.HasValue) && ( RowsPerPage.Value > 0 ))
                url = url + "&rowsPerPage=" + RowsPerPage.Value;
            if (!String.IsNullOrEmpty(UserFilter))
                url = url + "&user=" + UserFilter;

            // Call out to the endpoint and return the deserialized object
            return Deserialize<WebContent_Recent_Changes>(url, endpoint.Protocol, Tracer);
        }

        /// <summary> Get the URL for the list of all recent updates to (non aggregation affiliated) static web pages 
        /// for consumption by the jQuery DataTable.net plug-in </summary>
        /// <remarks> This URL is not an endpoint used by the user interface library, but rather employed by the 
        /// user's browser in concert with the jQuery DataTable.net plug-in.  </remarks>
        public string Get_Global_Recent_Updates_JDataTable_URL
        {
            get
            {
                return Config["Get_Global_Recent_Updates_JDataTable"] == null ? null : Config["Get_Global_Recent_Updates_JDataTable"].URL;
            }
        }

        /// <summary> Gets the list of possible next level from an existing page in the recent updates, used for filtering </summary>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        /// <param name="Level1"> (Optional) First level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level2"> (Optional) Second level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level3"> (Optional) Third level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level4"> (Optional) Fourth level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level5"> (Optional) Fifth level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level6"> (Optional) Sixth level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level7"> (Optional) Seventh level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level8"> (Optional) Eighth level of URL for the updated web content entity, if looking for children of a page </param>
        /// <returns> List of the values present in the next level of the recent updates list, used for filtering </returns>
        public List<string> Get_Global_Recent_Updates_NextLevel(Custom_Tracer Tracer, string Level1 = null, string Level2 = null, string Level3 = null, string Level4 = null, string Level5 = null, string Level6 = null, string Level7 = null, string Level8 = null)
        {
            // Add a beginning trace
            Tracer.Add_Trace("SobekEngineClient_WebContentServices.Get_Global_Recent_Updates_NextLevel");

            // Look in the the cache a recently stored value
            if (Config.UseCache)
            {
                List<string> cacheValue = CachedDataManager.WebContent.Retrieve_Global_Recent_Updates_NextLevel(Tracer, Level1, Level2, Level3, Level4, Level5, Level6, Level7, Level8 );
                if (cacheValue != null)
                    return cacheValue;
            }

            // Get the endpoint
            MicroservicesClient_Endpoint endpoint = GetEndpointConfig("WebContent.Get_Global_Recent_Updates_NextLevel", Tracer);

            // Format the URL
            string url = endpoint.URL;
            if (!String.IsNullOrEmpty(Level1))
            {
                if (String.IsNullOrEmpty(Level2))
                    url = endpoint.URL + "/" + Level1;
                else if ((!String.IsNullOrEmpty(Level2)) && (String.IsNullOrEmpty(Level3)))
                    url = endpoint.URL + "/" + Level1 + "/" + Level2;
                else
                {
                    StringBuilder urlBuilder = new StringBuilder(endpoint.URL + "/" + Level1 + "/" + Level2 + "/" + Level3);
                    if (!String.IsNullOrEmpty(Level4))
                    {
                        urlBuilder.Append("/" + Level4);
                        if (!String.IsNullOrEmpty(Level5))
                        {
                            urlBuilder.Append("/" + Level5);
                            if (!String.IsNullOrEmpty(Level6))
                            {
                                urlBuilder.Append("/" + Level6);
                                if (!String.IsNullOrEmpty(Level7))
                                {
                                    urlBuilder.Append("/" + Level7);
                                    if (!String.IsNullOrEmpty(Level8))
                                    {
                                        urlBuilder.Append("/" + Level8);
                                    }
                                }
                            }
                        }
                    }

                    url = urlBuilder.ToString();
                }
            }


            // Call out to the endpoint and return the deserialized object
            List<string> returnValue = Deserialize<List<string>>(url, endpoint.Protocol, Tracer);

            // Store in the cache
            if ((Config.UseCache) && (returnValue != null))
                CachedDataManager.WebContent.Store_Global_Recent_Updates_NextLevel(returnValue, Tracer);

            return returnValue;
        }

        /// <summary> Get the list of all users that have participated in the recent updates to all top-level static web content pages </summary>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        /// <returns> List of users that made recent updates </returns>
        public List<string> Get_Global_Recent_Updates_Users(Custom_Tracer Tracer)
        {
            // Add a beginning trace
            Tracer.Add_Trace("SobekEngineClient_WebContentServices.Get_Global_Recent_Updates_Users");

            // Look in the the cache a recently stored value
            if (Config.UseCache)
            {
                List<string> cacheValue = CachedDataManager.WebContent.Retrieve_Global_Recent_Updates_Users( Tracer );
                if (cacheValue != null)
                    return cacheValue;
            }

            // Get the endpoint
            MicroservicesClient_Endpoint endpoint = GetEndpointConfig("WebContent.Get_Global_Recent_Updates_Users", Tracer);

            // Format the URL
            string url = endpoint.URL;

            // Call out to the endpoint and return the deserialized object
            List<string> returnValue = Deserialize<List<string>>(url, endpoint.Protocol, Tracer);

            // Store in the cache
            if ((Config.UseCache) && (returnValue != null))
                CachedDataManager.WebContent.Store_Global_Recent_Updates_Users(returnValue, Tracer );

            return returnValue;
        }


        #endregion

        #region Endpoint related to the usage statistics reports of all web content pages

        /// <summary> Returns a flag indicating if any usage has been reported for this instance's web content entities (pages and redirects) </summary>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        /// <returns> Flag indicating if any usage has been reported for this instance's web content entities (pages and redirects) </returns>
        public bool Has_Global_Usage(Custom_Tracer Tracer)
        {
            // Add a beginning trace
            Tracer.Add_Trace("SobekEngineClient_WebContentServices.Has_Global_Usage");

            // Look in the the cache a recently stored value
            if (Config.UseCache)
            {
                bool? cacheFlag = CachedDataManager.WebContent.Retrieve_Has_Global_Usage_Flag(Tracer);
                if (cacheFlag.HasValue)
                    return cacheFlag.Value;
            }

            // Get the endpoint
            MicroservicesClient_Endpoint endpoint = GetEndpointConfig("WebContent.Has_Global_Usage", Tracer);

            // Format the URL
            string url = endpoint.URL;

            // Call out to the endpoint and return the deserialized object
            bool returnValue = Deserialize<bool>(url, endpoint.Protocol, Tracer);

            // Store in the cache
            if (Config.UseCache)
                CachedDataManager.WebContent.Store_Has_Global_Usage_Flag(returnValue, Tracer);

            return returnValue;
        }

        /// <summary> Get usage of all web content pages between two dates </summary>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        /// <param name="Year1"> Start year of the year/month range for these usage stats </param>
        /// <param name="Month1"> Start month of the year/month range for these usage stats </param>
        /// <param name="Year2"> End year of the year/month range for these usage stats </param>
        /// <param name="Month2"> End month of the year/month range for these usage stats </param>
        /// <param name="Page"> Page number of used pages ( starting with one and counting up ) </param>
        /// <param name="RowsPerPage"> (Optional) Number of rows of used pages to include in each page of results </param>
        /// <returns> Web content usage report </returns>
        public WebContent_Usage_Report Get_Global_Usage_Report(Custom_Tracer Tracer, int Year1, int Month1, int Year2, int Month2, int Page, int? RowsPerPage = null )
        {
            // Add a beginning trace
            Tracer.Add_Trace("SobekEngineClient_WebContentServices.Get_Global_Usage_Report");

            // Get the endpoint
            MicroservicesClient_Endpoint endpoint = GetEndpointConfig("WebContent.Get_Global_Usage_Report", Tracer);

            // Format the URL
            string url = endpoint.URL + "?year1=" + Year1 + "&month1=" + Month1 + "&year2=" + Year2 + "&month2=" + Month2 + "&page=" + Page;
            if ((RowsPerPage.HasValue) && (RowsPerPage.Value > 0))
            {
                url = url + "&rowsPerPage=" + RowsPerPage.Value;
            }

            // Call out to the endpoint and return the deserialized object
            return Deserialize<WebContent_Usage_Report>(url, endpoint.Protocol, Tracer);
        }

        /// <summary> Get the URL for the list of usage for a global usage report for consumption by the jQuery DataTable.net plug-in </summary>
        /// <remarks> This URL is not an endpoint used by the user interface library, but rather employed by the 
        /// user's browser in concert with the jQuery DataTable.net plug-in.  </remarks>
        public string Get_Global_Usage_Report_JDataTable_URL
        {
            get
            {
                return Config["Get_Global_Usage_Report_JDataTable"] == null ? null : Config["Get_Global_Usage_Report_JDataTable"].URL;
            }
        }

        /// <summary> Gets the list of possible next level from an existing used page in a global usage report, used for filtering  </summary>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        /// <param name="Year1"> Start year of the year/month range for these usage stats </param>
        /// <param name="Month1"> Start month of the year/month range for these usage stats </param>
        /// <param name="Year2"> End year of the year/month range for these usage stats </param>
        /// <param name="Month2"> End month of the year/month range for these usage stats </param>
        /// <param name="Level1"> (Optional) First level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level2"> (Optional) Second level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level3"> (Optional) Third level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level4"> (Optional) Fourth level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level5"> (Optional) Fifth level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level6"> (Optional) Sixth level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level7"> (Optional) Seventh level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level8"> (Optional) Eighth level of URL for the updated web content entity, if looking for children of a page </param>
        /// <returns> List of the values present in the next level of the requested usage report list, used for filtering </returns>
        public List<string> Get_Global_Usage_Report_NextLevel(Custom_Tracer Tracer, int Year1, int Month1, int Year2, int Month2, string Level1 = null, string Level2 = null, string Level3 = null, string Level4 = null, string Level5 = null, string Level6 = null, string Level7 = null, string Level8 = null)
        {
            // Add a beginning trace
            Tracer.Add_Trace("SobekEngineClient_WebContentServices.Get_Global_Usage_Report_NextLevel");

            // Look in the the cache a recently stored value
            if (Config.UseCache)
            {
                List<string> cacheValue = CachedDataManager.WebContent.Retrieve_Global_Usage_Report_NextLevel(Tracer, Year1, Month1, Year2, Month2, Level1, Level2, Level3, Level4, Level5, Level6, Level7, Level8);
                if (cacheValue != null)
                    return cacheValue;
            }

            // Get the endpoint
            MicroservicesClient_Endpoint endpoint = GetEndpointConfig("WebContent.Get_Global_Usage_Report_NextLevel", Tracer);

            // Format the URL
            string url = endpoint.URL;
            if (!String.IsNullOrEmpty(Level1))
            {
                if (String.IsNullOrEmpty(Level2))
                    url = endpoint.URL + "/" + Level1;
                else if ((!String.IsNullOrEmpty(Level2)) && (String.IsNullOrEmpty(Level3)))
                    url = endpoint.URL + "/" + Level1 + "/" + Level2;
                else
                {
                    StringBuilder urlBuilder = new StringBuilder(endpoint.URL + "/" + Level1 + "/" + Level2 + "/" + Level3);
                    if (!String.IsNullOrEmpty(Level4))
                    {
                        urlBuilder.Append("/" + Level4);
                        if (!String.IsNullOrEmpty(Level5))
                        {
                            urlBuilder.Append("/" + Level5);
                            if (!String.IsNullOrEmpty(Level6))
                            {
                                urlBuilder.Append("/" + Level6);
                                if (!String.IsNullOrEmpty(Level7))
                                {
                                    urlBuilder.Append("/" + Level7);
                                    if (!String.IsNullOrEmpty(Level8))
                                    {
                                        urlBuilder.Append("/" + Level8);
                                    }
                                }
                            }
                        }
                    }

                    url = urlBuilder.ToString();
                }
            }
            url = url + "?year1=" + Year1 + "&month1=" + Month1 + "&year2=" + Year2 + "&month2=" + Month2;

            // Call out to the endpoint and return the deserialized object
            List<string> returnValue = Deserialize<List<string>>(url, endpoint.Protocol, Tracer);

            // Store in the cache
            if ((Config.UseCache) && (returnValue != null))
                CachedDataManager.WebContent.Store_Global_Usage_Report_NextLevel(returnValue, Tracer, Year1, Month1, Year2, Month2, Level1, Level2, Level3, Level4, Level5, Level6, Level7, Level8);

            return returnValue;
        }


        #endregion

        #region Endpoints related to the complete list of global redirects

        /// <summary> Returns a flag indicating if there are any global redirects within the web content system </summary>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        /// <returns> Flag indicating if there are any global redirects within the web content system </returns>
        public bool Has_Redirects(Custom_Tracer Tracer)
        {
            // Add a beginning trace
            Tracer.Add_Trace("SobekEngineClient_WebContentServices.Has_Redirects");

            // Look in the the cache a recently stored value
            if (Config.UseCache)
            {
                bool? cacheFlag = CachedDataManager.WebContent.Retrieve_Has_Redirects_Flag(Tracer);
                if (cacheFlag.HasValue)
                    return cacheFlag.Value;
            }

            // Get the endpoint
            MicroservicesClient_Endpoint endpoint = GetEndpointConfig("WebContent.Has_Redirects", Tracer);

            // Format the URL
            string url = endpoint.URL; 

            // Call out to the endpoint and return the deserialized object
            bool returnValue = Deserialize<bool>(url, endpoint.Protocol, Tracer);

            // Store in the cache
            if (Config.UseCache)
                CachedDataManager.WebContent.Store_Has_Redirects_Flag(returnValue, Tracer);

            return returnValue;
        }

        /// <summary> Get the list of all the global redirects </summary>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        /// <param name="Page"> Page number of used pages ( starting with one and counting up ) </param>
        /// <param name="RowsPerPage"> (Optional) Number of rows of used pages to include in each page of results </param>
        /// <returns> Reqeusted list of web content redirect entities </returns>
        public WebContent_Basic_Pages Get_All_Redirects(Custom_Tracer Tracer, int Page, int? RowsPerPage = null)
        {
            // Add a beginning trace
            Tracer.Add_Trace("SobekEngineClient_WebContentServices.Get_All_Redirects");

            // Get the endpoint
            MicroservicesClient_Endpoint endpoint = GetEndpointConfig("WebContent.Get_All_Redirects", Tracer);

            // Format the URL
            string url = endpoint.URL + "?page=" + Page;
            if ((RowsPerPage.HasValue) && (RowsPerPage.Value > 0))
                url = url + "&rowsPerPage=" + RowsPerPage.Value;

            // Call out to the endpoint and return the deserialized object
            return Deserialize<WebContent_Basic_Pages>(url, endpoint.Protocol, Tracer);
        }

        /// <summary> Get the URL for the list of all the global redirects for consumption by the jQuery DataTable.net plug-in </summary>
        /// <remarks> This URL is not an endpoint used by the user interface library, but rather employed by the 
        /// user's browser in concert with the jQuery DataTable.net plug-in.  </remarks>
        public string Get_All_Redirects_JDataTable_URL
        {
            get
            {
                return Config["Get_All_Redirects_JDataTable"] == null ? null : Config["Get_All_Redirects_JDataTable"].URL;
            }
        }

        /// <summary> Gets the list of possible next level from an existing point in the redirects hierarchy, used for filtering </summary>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        /// <param name="Level1"> (Optional) First level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level2"> (Optional) Second level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level3"> (Optional) Third level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level4"> (Optional) Fourth level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level5"> (Optional) Fifth level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level6"> (Optional) Sixth level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level7"> (Optional) Seventh level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level8"> (Optional) Eighth level of URL for the updated web content entity, if looking for children of a page </param>
        /// <returns> List of the values present in the next level of the redirects list, used for filtering </returns>
        public List<string> Get_All_Redirects_NextLevel(Custom_Tracer Tracer, string Level1 = null, string Level2 = null, string Level3 = null, string Level4 = null, string Level5 = null, string Level6 = null, string Level7 = null, string Level8 = null)
        {
            // Add a beginning trace
            Tracer.Add_Trace("SobekEngineClient_WebContentServices.Get_All_Redirects_NextLevel");

            // Look in the the cache a recently stored value
            if (Config.UseCache)
            {
                List<string> cacheValue = CachedDataManager.WebContent.Retrieve_All_Redirects_NextLevel(Tracer, Level1, Level2, Level3, Level4, Level5, Level6, Level7, Level8);
                if (cacheValue != null)
                    return cacheValue;
            }

            // Get the endpoint
            MicroservicesClient_Endpoint endpoint = GetEndpointConfig("WebContent.Get_All_Redirects_NextLevel", Tracer);

            // Format the URL
            string url = endpoint.URL;
            if (!String.IsNullOrEmpty(Level1))
            {
                if (String.IsNullOrEmpty(Level2))
                    url = endpoint.URL + "/" + Level1;
                else if ((!String.IsNullOrEmpty(Level2)) && (String.IsNullOrEmpty(Level3)))
                    url = endpoint.URL + "/" + Level1 + "/" + Level2;
                else
                {
                    StringBuilder urlBuilder = new StringBuilder(endpoint.URL + "/" + Level1 + "/" + Level2 + "/" + Level3);
                    if (!String.IsNullOrEmpty(Level4))
                    {
                        urlBuilder.Append("/" + Level4);
                        if (!String.IsNullOrEmpty(Level5))
                        {
                            urlBuilder.Append("/" + Level5);
                            if (!String.IsNullOrEmpty(Level6))
                            {
                                urlBuilder.Append("/" + Level6);
                                if (!String.IsNullOrEmpty(Level7))
                                {
                                    urlBuilder.Append("/" + Level7);
                                    if (!String.IsNullOrEmpty(Level8))
                                    {
                                        urlBuilder.Append("/" + Level8);
                                    }
                                }
                            }
                        }
                    }

                    url = urlBuilder.ToString();
                }
            }

            // Call out to the endpoint and return the deserialized object
            List<string> returnValue = Deserialize<List<string>>(url, endpoint.Protocol, Tracer);

            // Store in the cache
            if ((Config.UseCache) && (returnValue != null))
                CachedDataManager.WebContent.Store_All_Redirects_NextLevel(returnValue, Tracer, Level1, Level2, Level3, Level4, Level5, Level6, Level7, Level8 );

            return returnValue;
        }


        #endregion

        #region Endpoints related to the complete list of web content pages (excluding redirects)

        /// <summary> Returns a flag indicating if there are any web content pages (excluding redirects) </summary>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        /// <returns> Flag indicating if there are any web content pages (excluding redirects) </returns>
        public bool Has_Content_Pages(Custom_Tracer Tracer)
        {
            // Add a beginning trace
            Tracer.Add_Trace("SobekEngineClient_WebContentServices.Has_Content_Pages");

            // Look in the the cache a recently stored value
            if (Config.UseCache)
            {
                bool? cacheFlag = CachedDataManager.WebContent.Retrieve_Has_Content_Pages_Flag(Tracer);
                if (cacheFlag.HasValue)
                    return cacheFlag.Value;
            }

            // Get the endpoint
            MicroservicesClient_Endpoint endpoint = GetEndpointConfig("WebContent.Has_Content_Pages", Tracer);

            // Format the URL
            string url = endpoint.URL;

            // Call out to the endpoint and return the deserialized object
            bool returnValue = Deserialize<bool>(url, endpoint.Protocol, Tracer);

            // Store in the cache
            if (Config.UseCache)
                CachedDataManager.WebContent.Store_Has_Content_Pages_Flag(returnValue, Tracer);

            return returnValue;
        }

        /// <summary> Get the list of all the web content pages ( excluding redirects ) </summary>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        /// <param name="Page"> Page number of used pages ( starting with one and counting up ) </param>
        /// <param name="RowsPerPage"> (Optional) Number of rows of used pages to include in each page of results </param>
        /// <returns> Requested list of web content pages ( excluding redirects ) </returns>
        public WebContent_Basic_Pages Get_All_Pages(Custom_Tracer Tracer, int Page, int? RowsPerPage = null)
        {
            // Add a beginning trace
            Tracer.Add_Trace("SobekEngineClient_WebContentServices.Get_All_Pages");

            // Get the endpoint
            MicroservicesClient_Endpoint endpoint = GetEndpointConfig("WebContent.Get_All_Pages", Tracer);

            // Format the URL
            string url = endpoint.URL + "?page=" + Page;
            if ((RowsPerPage.HasValue) && (RowsPerPage.Value > 0))
                url = url + "&rowsPerPage=" + RowsPerPage.Value;

            // Call out to the endpoint and return the deserialized object
            return Deserialize<WebContent_Basic_Pages>(url, endpoint.Protocol, Tracer);
        }


        /// <summary> Get the URL for the list of all the web content pages ( excluding redirects ) for
        /// consumption by the jQuery DataTable.net plug-in </summary>
        /// <remarks> This URL is not an endpoint used by the user interface library, but rather employed by the 
        /// user's browser in concert with the jQuery DataTable.net plug-in.  </remarks>
        public string Get_All_Pages_JDataTable_URL
        {
            get
            {
                return Config["Get_All_Pages_JDataTable"] == null ? null : Config["Get_All_Pages_JDataTable"].URL;
            }
        }

        /// <summary> Gets the list of possible next level from an existing point in the page hierarchy, used for filtering </summary>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        /// <param name="Level1"> (Optional) First level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level2"> (Optional) Second level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level3"> (Optional) Third level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level4"> (Optional) Fourth level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level5"> (Optional) Fifth level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level6"> (Optional) Sixth level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level7"> (Optional) Seventh level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level8"> (Optional) Eighth level of URL for the updated web content entity, if looking for children of a page </param>
        /// <returns> List of the values present in the next level of the pages list, used for filtering </returns>
        public List<string> Get_All_Pages_NextLevel(Custom_Tracer Tracer, string Level1 = null, string Level2 = null, string Level3 = null, string Level4 = null, string Level5 = null, string Level6 = null, string Level7 = null, string Level8 = null)
        {
            // Add a beginning trace
            Tracer.Add_Trace("SobekEngineClient_WebContentServices.Get_All_Pages_NextLevel");

            // Look in the the cache a recently stored value
            if (Config.UseCache)
            {
                List<string> cacheValue = CachedDataManager.WebContent.Retrieve_All_Pages_NextLevel(Tracer, Level1, Level2, Level3, Level4, Level5, Level6, Level7, Level8);
                if (cacheValue != null)
                    return cacheValue;
            }

            // Get the endpoint
            MicroservicesClient_Endpoint endpoint = GetEndpointConfig("WebContent.Get_All_Pages_NextLevel", Tracer);

            // Format the URL
            string url = endpoint.URL;
            if (!String.IsNullOrEmpty(Level1))
            {
                if (String.IsNullOrEmpty(Level2))
                    url = endpoint.URL + "/" + Level1;
                else if ((!String.IsNullOrEmpty(Level2)) && (String.IsNullOrEmpty(Level3)))
                    url = endpoint.URL + "/" + Level1 + "/" + Level2;
                else
                {
                    StringBuilder urlBuilder = new StringBuilder(endpoint.URL + "/" + Level1 + "/" + Level2 + "/" + Level3);
                    if (!String.IsNullOrEmpty(Level4))
                    {
                        urlBuilder.Append("/" + Level4);
                        if (!String.IsNullOrEmpty(Level5))
                        {
                            urlBuilder.Append("/" + Level5);
                            if (!String.IsNullOrEmpty(Level6))
                            {
                                urlBuilder.Append("/" + Level6);
                                if (!String.IsNullOrEmpty(Level7))
                                {
                                    urlBuilder.Append("/" + Level7);
                                    if (!String.IsNullOrEmpty(Level8))
                                    {
                                        urlBuilder.Append("/" + Level8);
                                    }
                                }
                            }
                        }
                    }

                    url = urlBuilder.ToString();
                }
            }

            // Call out to the endpoint and return the deserialized object
            List<string> returnValue = Deserialize<List<string>>(url, endpoint.Protocol, Tracer);

            // Store in the cache
            if ((Config.UseCache) && (returnValue != null))
                CachedDataManager.WebContent.Store_All_Pages_NextLevel(returnValue, Tracer, Level1, Level2, Level3, Level4, Level5, Level6, Level7, Level8);

            return returnValue;
        }


        #endregion

        #region Endpoints related to the complete list of web content entities, including pages and redirects

        /// <summary> Returns a flag indicating if there are any web content entities, including pages and redirects </summary>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        /// <returns> Flag indicating if there are any web content entities, including pages and redirects </returns>
        public bool Has_Pages_Or_Redirects(Custom_Tracer Tracer)
        {
            // Add a beginning trace
            Tracer.Add_Trace("SobekEngineClient_WebContentServices.Has_Pages_Or_Redirects");

            // Look in the the cache a recently stored value
            if (Config.UseCache)
            {
                bool? cacheFlag = CachedDataManager.WebContent.Retrieve_Has_Content_Flag(Tracer);
                if (cacheFlag.HasValue)
                    return cacheFlag.Value;
            }


            // Get the endpoint
            MicroservicesClient_Endpoint endpoint = GetEndpointConfig("WebContent.Has_Pages_Or_Redirects", Tracer);

            // Format the URL
            string url = endpoint.URL;

            // Call out to the endpoint and return the deserialized object
            bool returnValue = Deserialize<bool>(url, endpoint.Protocol, Tracer);

            // Store in the cache
            if (Config.UseCache)
                CachedDataManager.WebContent.Store_Has_Content_Flag(returnValue, Tracer);

            return returnValue;
        }

        /// <summary> Get the list of all the web content entities, including pages and redirects </summary>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        /// <param name="Page"> Page number of used pages ( starting with one and counting up ) </param>
        /// <param name="RowsPerPage"> (Optional) Number of rows of used pages to include in each page of results </param>
        /// <returns> Requested list of web content entities, including pages and redirects </returns>
        public WebContent_Basic_Pages Get_All(Custom_Tracer Tracer, int Page, int? RowsPerPage = null)
        {
            // Add a beginning trace
            Tracer.Add_Trace("SobekEngineClient_WebContentServices.Get_All");

            // Get the endpoint
            MicroservicesClient_Endpoint endpoint = GetEndpointConfig("WebContent.Get_All", Tracer);

            // Format the URL
            string url = endpoint.URL + "?page=" + Page;
            if ((RowsPerPage.HasValue) && (RowsPerPage.Value > 0))
                url = url + "&rowsPerPage=" + RowsPerPage.Value;

            // Call out to the endpoint and return the deserialized object
            return Deserialize<WebContent_Basic_Pages>(url, endpoint.Protocol, Tracer);
        }


        /// <summary> Get the URL for the list of all the web content entities, including pages and redirects, for
        /// consumption by the jQuery DataTable.net plug-in </summary>
        /// <remarks> This URL is not an endpoint used by the user interface library, but rather employed by the 
        /// user's browser in concert with the jQuery DataTable.net plug-in.  </remarks>
        public string Get_All_JDataTable_URL
        {
            get
            {
                return Config["Get_All_JDataTable"] == null ? null : Config["Get_All_JDataTable"].URL;
            }
        }

        /// <summary> Gets the list of possible next level from an existing point in the page AND redirects hierarchy, used for filtering </summary>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        /// <param name="Level1"> (Optional) First level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level2"> (Optional) Second level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level3"> (Optional) Third level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level4"> (Optional) Fourth level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level5"> (Optional) Fifth level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level6"> (Optional) Sixth level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level7"> (Optional) Seventh level of URL for the updated web content entity, if looking for children of a page </param>
        /// <param name="Level8"> (Optional) Eighth level of URL for the updated web content entity, if looking for children of a page </param>
        /// <returns> List of the values present in the next level of the pages AND redirects list, used for filtering </returns>
        public List<string> Get_All_NextLevel(Custom_Tracer Tracer, string Level1 = null, string Level2 = null, string Level3 = null, string Level4 = null, string Level5 = null, string Level6 = null, string Level7 = null, string Level8 = null)
        {
            // Add a beginning trace
            Tracer.Add_Trace("SobekEngineClient_WebContentServices.Get_All_NextLevel");

            // Look in the the cache a recently stored value
            if (Config.UseCache)
            {
                List<string> cacheValue = CachedDataManager.WebContent.Retrieve_All_Content_NextLevel(Tracer, Level1, Level2, Level3, Level4, Level5, Level6, Level7, Level8);
                if (cacheValue != null)
                    return cacheValue;
            }

            // Get the endpoint
            MicroservicesClient_Endpoint endpoint = GetEndpointConfig("WebContent.Get_All_NextLevel", Tracer);

            // Format the URL
            string url = endpoint.URL;
            if (!String.IsNullOrEmpty(Level1))
            {
                if (String.IsNullOrEmpty(Level2))
                    url = endpoint.URL + "/" + Level1;
                else if ((!String.IsNullOrEmpty(Level2)) && (String.IsNullOrEmpty(Level3)))
                    url = endpoint.URL + "/" + Level1 + "/" + Level2;
                else
                {
                    StringBuilder urlBuilder = new StringBuilder(endpoint.URL + "/" + Level1 + "/" + Level2 + "/" + Level3);
                    if (!String.IsNullOrEmpty(Level4))
                    {
                        urlBuilder.Append("/" + Level4);
                        if (!String.IsNullOrEmpty(Level5))
                        {
                            urlBuilder.Append("/" + Level5);
                            if (!String.IsNullOrEmpty(Level6))
                            {
                                urlBuilder.Append("/" + Level6);
                                if (!String.IsNullOrEmpty(Level7))
                                {
                                    urlBuilder.Append("/" + Level7);
                                    if (!String.IsNullOrEmpty(Level8))
                                    {
                                        urlBuilder.Append("/" + Level8);
                                    }
                                }
                            }
                        }
                    }

                    url = urlBuilder.ToString();
                }
            }

            // Call out to the endpoint and return the deserialized object
            List<string> returnValue = Deserialize<List<string>>(url, endpoint.Protocol, Tracer);

            // Store in the cache
            if ((Config.UseCache) && ( returnValue != null ))
                CachedDataManager.WebContent.Store_All_Content_NextLevel(returnValue, Tracer, Level1, Level2, Level3, Level4, Level5, Level6, Level7, Level8);

            return returnValue;
        }

        #endregion
    }
}
