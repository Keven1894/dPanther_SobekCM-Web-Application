﻿#region Using directives

using System;
using System.IO;
using SobekCM.Library.Aggregations;
using SobekCM.Library.Application_State;
using SobekCM.Library.Configuration;
using SobekCM.Library.HTML;
using SobekCM.Library.MainWriters;
using SobekCM.Library.Navigation;

#endregion

namespace SobekCM.Library.AggregationViewer.Viewers
{
    /// <summary> Renders the full text search page for a given item aggregation </summary>
    /// <remarks> This class implements the <see cref="iAggregationViewer"/> interface and extends the <see cref="abstractAggregationViewer"/> class.<br /><br />
    /// Aggregation viewers are used when displaying aggregation home pages, searches, browses, and information pages.<br /><br />
    /// During a valid html request to display the home page with basic search, the following steps occur:
    /// <ul>
    /// <li>Application state is built/verified by the <see cref="Application_State.Application_State_Builder"/> </li>
    /// <li>Request is analyzed by the <see cref="Navigation.SobekCM_QueryString_Analyzer"/> and output as a <see cref="SobekCM_Navigation_Object"/> </li>
    /// <li>Main writer is created for rendering the output, in this case the <see cref="Html_MainWriter"/> </li>
    /// <li>The HTML writer will create the necessary subwriter.  For a collection-level request, an instance of the  <see cref="Aggregation_HtmlSubwriter"/> class is created. </li>
    /// <li>To display the requested collection view, the collection subwriter will creates an instance of this class </li>
    /// </ul></remarks>
    public class Full_Text_Search_AggregationViewer : abstractAggregationViewer
    {
        private readonly string arg1;
        private readonly string arg2;
        private readonly string browse_url;
        private readonly string textBoxValue;

        /// <summary> Constructor for a new instance of the Full_Text_Search_AggregationViewer class </summary>
        /// <param name="Current_Aggregation"> Current item aggregation object </param>
        /// <param name="Current_Mode"> Mode / navigation information for the current request</param>
        public Full_Text_Search_AggregationViewer(Item_Aggregation Current_Aggregation, SobekCM_Navigation_Object Current_Mode): base(Current_Aggregation, Current_Mode)
        {
            // Determine the sub text to use
            const string subCode = "s=";

            // Save the search term
            if (currentMode.Search_String.Length > 0)
            {
                textBoxValue = currentMode.Search_String;
            }

            // Determine the complete script action name
            Display_Mode_Enum displayMode = currentMode.Mode;
            Search_Type_Enum searchType = currentMode.Search_Type;
            currentMode.Mode = Display_Mode_Enum.Results;
            currentMode.Search_Type = Search_Type_Enum.Full_Text;
            string search_string = currentMode.Search_String;
            currentMode.Search_String = String.Empty;
            currentMode.Search_Fields = String.Empty;
            currentMode.Search_Precision = Search_Precision_Type_Enum.Inflectional_Form;
            arg2 = String.Empty;
            arg1 = currentMode.Redirect_URL();
            currentMode.Mode = Display_Mode_Enum.Aggregation_Browse_Info;
            currentMode.Info_Browse_Mode = "all";
            browse_url = currentMode.Redirect_URL();
            currentMode.Mode = Display_Mode_Enum.Search;
            if ((!currentMode.Show_Selection_Panel) || (Current_Aggregation.Children_Count == 0))
            {
                scriptActionName = "fulltext_search_sobekcm('" + arg1 + "', '" + browse_url + "');";
                scriptIncludeName = "<script src=\"" + currentMode.Base_URL + "default/scripts/sobekcm_search.js\" type=\"text/javascript\"></script>";
            }
            else
            {
                scriptActionName = "fulltext_select_search_sobekcm('" + arg1 + "', '" + subCode + "')";
                arg2 = subCode;
                scriptIncludeName = "<script src=\"" + currentMode.Base_URL + "default/scripts/sobekcm_search.js\" type=\"text/javascript\"></script>";
            }
            currentMode.Mode = displayMode;
            currentMode.Search_Type = searchType;
            currentMode.Search_String = search_string;
            currentMode.Info_Browse_Mode = String.Empty;
        }

        /// <summary> Gets the type of collection view or search supported by this collection viewer </summary>
        /// <value> This returns the <see cref="Item_Aggregation.CollectionViewsAndSearchesEnum.Basic_Search"/> enumerational value </value>
        public override Item_Aggregation.CollectionViewsAndSearchesEnum Type
        {
            get { return Item_Aggregation.CollectionViewsAndSearchesEnum.FullText_Search; }
        }

        /// <summary> Flag indicates whether the secondary text requires controls </summary>
        /// <value> This property always returns the value FALSE </value>
        public override bool Always_Display_Home_Text
        {
            get
            {
                return false;
            }
        }

        /// <summary>Flag indicates whether the subaggregation selection panel is displayed for this collection viewer</summary>
        /// <value> This property always returns the <see cref="Selection_Panel_Display_Enum.Selectable"/> enumerational value </value>
        public override Selection_Panel_Display_Enum Selection_Panel_Display
        {
            get
            {
                return Selection_Panel_Display_Enum.Never;
            }
        }

        /// <summary> Add the HTML to be displayed in the search box </summary>
        /// <param name="Output"> Textwriter to write the HTML for this viewer</param>
        /// <param name="Tracer">Trace object keeps a list of each method executed and important milestones in rendering</param>
        public override void Add_Search_Box_HTML(TextWriter Output, Custom_Tracer Tracer)
        {
            if (Tracer != null)
            {
                Tracer.Add_Trace("Full_Text_Search_AggregationViewer.Add_Search_Box_HTML", "Adding html for search box");
            }

            string search_collection = "Search full text";
            if (currentMode.Language == Web_Language_Enum.Spanish)
            {
                search_collection = "Buscar en la colección";
            }

            if (currentMode.Language == Web_Language_Enum.French)
            {
                search_collection = "Recherche dans la collection";
            }

            Output.WriteLine("  <br />");
            Output.WriteLine("  <table width=\"100%\" id=\"FullTextSearchPanel\" >");
            Output.WriteLine("    <tr align=\"right\">");
            Output.WriteLine("      <td align=\"right\" width=\"28%\"><b><label for=\"SobekHomeSearchBox\">" + search_collection + ":</label></b></td>");
            Output.WriteLine("      <td width=\"3%\">&nbsp;</td>");
            Output.Write("      <td align=\"left\" width=\"60%\"><input name=\"u_search\" type=\"text\" class=\"SobekHomeSearchBox\" id=\"SobekHomeSearchBox\" value=\"" + textBoxValue + "\" onfocus=\"textbox_enter('SobekHomeSearchBox', 'SobekHomeSearchBox_focused');\" onblur=\"textbox_leave('SobekHomeSearchBox', 'SobekHomeSearchBox');\" ");

            if (currentMode.Browser_Type.IndexOf("IE") >= 0)
                Output.Write(" onkeydown=\"fnTrapKD(event, 'text', '" + arg1 + "', '" + arg2 + "','" + browse_url + "');\"");
            else
                Output.Write(" onkeydown=\"return fnTrapKD(event, 'text', '" + arg1 + "', '" + arg2 + "','" + browse_url + "');\"");

            Output.WriteLine(" /></td>");

            Output.WriteLine("      <td align=\"left\" width=\"10%\"><a onclick=\"" + scriptActionName + "\"><img name=\"jsbutton\" id=\"jsbutton\" src=\"" + currentMode.Base_URL + "design/skins/" + currentMode.Base_Skin + "/buttons/go_button.gif\" border=\"0\" alt=\"" + search_collection + "\" /></a></td>");
            Output.WriteLine("    </tr>");
            Output.WriteLine("    <tr valign=\"top\">");
            Output.WriteLine("      <td colspan=\"2\">&nbsp;</td>");
            Output.WriteLine("      <td align=\"left\"><div id=\"circular_progress\" name=\"circular_progress\" class=\"hidden_progress\">&nbsp;</div></td>");
            Output.WriteLine("    </tr>");
            Output.WriteLine("  </table>");

            Output.WriteLine();
            Output.WriteLine("<!-- Focus on search box -->");
            Output.WriteLine("<script type=\"text/javascript\">focus_element('SobekHomeSearchBox');</script>");
            Output.WriteLine();
        }

        /// <summary> Add the HTML to be displayed below the search box </summary>
        /// <param name="Output"> Textwriter to write the HTML for this viewer</param>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
        /// <remarks> This adds the search tips by calling the base method <see cref="abstractAggregationViewer.Add_Simple_Search_Tips"/> </remarks>
        public override void Add_Secondary_HTML(TextWriter Output, Custom_Tracer Tracer)
        {
            if (Tracer != null)
            {
                Tracer.Add_Trace("Advanced_Search_AggregationViewer.Add_Secondary_HTML", "Adding simple search tips");
            }

            Add_Simple_Search_Tips(Output, Tracer);
        }
    }
}