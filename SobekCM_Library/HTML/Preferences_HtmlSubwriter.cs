#region Using directives

using System;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using SobekCM.Core.Configuration;
using SobekCM.Core.Navigation;
using SobekCM.Engine_Library.Navigation;
using SobekCM.Tools;

#endregion

namespace SobekCM.Library.HTML
{
    /// <summary> [TO BE DEPRECATED] Prefernces html subwriter allows the user to set some basic preferences </summary>
    /// <remarks> This class extends the <see cref="abstractHtmlSubwriter"/> abstract class.<br /><br />
    /// This class is planned to be deprecated and treat each user (whether authenticated or not) as a mySobek
    /// user.  Preferences would then be served from the mySobek account/preferences writer </remarks>
    public class Preferences_HtmlSubwriter : abstractHtmlSubwriter
    {
        /// <summary> Constructor for a new instance of the Preferences_HtmlSubwriter class </summary>
        /// <param name="RequestSpecificValues"> All the necessary, non-global data specific to the current request </param>
        public Preferences_HtmlSubwriter(RequestCache RequestSpecificValues) : base(RequestSpecificValues) 
        {
            // See if there was a hidden request
            string hidden_request = HttpContext.Current.Request.Form["hidden_request"] ?? String.Empty;

            if (hidden_request == "submit")
            {
                NameValueCollection form = HttpContext.Current.Request.Form;

                string language_option = form["languageDropDown"];
                switch (language_option)
                {
                    case "en":
                        RequestSpecificValues.Current_Mode.Language = Web_Language_Enum.English;
                        break;

                    case "fr":
                        RequestSpecificValues.Current_Mode.Language = Web_Language_Enum.French;
                        break;

                    case "es":
                        RequestSpecificValues.Current_Mode.Language = Web_Language_Enum.Spanish;
                        break;

                }

                string defaultViewDropDown = form["defaultViewDropDown"];
                HttpContext.Current.Session["User_Default_View"] = defaultViewDropDown;

                int user_sort = Convert.ToInt32(form["defaultSortDropDown"]);
                HttpContext.Current.Session["User_Default_Sort"] = user_sort;

                RequestSpecificValues.Current_Mode.Mode = Display_Mode_Enum.Aggregation;
				RequestSpecificValues.Current_Mode.Aggregation_Type = Aggregation_Type_Enum.Home;
                UrlWriterHelper.Redirect(RequestSpecificValues.Current_Mode);

            }
        }

        /// <summary> Writes the HTML generated by this preferences html subwriter directly to the response stream </summary>
        /// <param name="Output"> Stream to which to write the HTML for this subwriter </param>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        /// <returns> TRUE -- Value indicating if html writer should finish the page immediately after this, or if there are other controls or routines which need to be called first </returns>
        public override bool Write_HTML(TextWriter Output, Custom_Tracer Tracer)
        {
            Tracer.Add_Trace("Preferences_HtmlSubwriter.Write_HTML", "Rendering HTML");

            string preferences = "Preferences";
            string language = "Language:";
            string button = "Return";
            const string defaultView = "Default View:";
            const string defaultSort = "Default Sort:";

            if (RequestSpecificValues.Current_Mode.Language == Web_Language_Enum.Spanish)
            {
                preferences = "Preferencias";
                language = "Idioma:";
                button = "Regresar";
            }

            if (RequestSpecificValues.Current_Mode.Language == Web_Language_Enum.French)
            {
                preferences = "Pr�f�rences";
                language = "Langue:";
                button = "Retour";
            }

            // Start the page container
            Output.WriteLine("<div id=\"pagecontainer\">");
            Output.WriteLine("<br />");

            Output.WriteLine("<br /><br />");
            Output.WriteLine("<div class=\"SobekSearchPanel\" align=\"center\">");
            Output.WriteLine("  <h1>" + preferences + "</h1>");
            Output.WriteLine("  <form name=\"preferences_form\" id=\"addedForm\"  method=\"post\" action=\"" + UrlWriterHelper.Redirect_URL(RequestSpecificValues.Current_Mode) + "\" >");

            Output.WriteLine("<!-- Hidden field is used for postbacks to add new form elements (i.e., new name, new other titles, etc..) -->");
            Output.WriteLine("<input type=\"hidden\" id=\"hidden_request\" name=\"hidden_request\" value=\"\" />");


            Output.WriteLine("    <table class=\"ContactPrefInputPanel\" align=\"center\" >");
            Output.WriteLine("      <tr>");
            Output.WriteLine("        <td align=\"left\" width=\"100px\">" + language + "</td>");
            Output.WriteLine("        <td align=\"left\">");
            Output.WriteLine("          <select name=\"languageDropDown\" id=\"languageDropDown\">");
            Output.WriteLine(RequestSpecificValues.Current_Mode.Language == Web_Language_Enum.English ? "            <option selected=\"selected\" value=\"en\">English</option>" : "            <option value=\"en\">English</option>");
            Output.WriteLine(RequestSpecificValues.Current_Mode.Language == Web_Language_Enum.French ? "            <option selected=\"selected\" value=\"fr\">Fran�ais</option>" : "            <option value=\"fr\">Fran�ais</option>");
            Output.WriteLine(RequestSpecificValues.Current_Mode.Language == Web_Language_Enum.Spanish ? "            <option selected=\"selected\" value=\"es\">Espa�ol</option>" : "            <option value=\"es\">Espa�ol</option>");
            Output.WriteLine("          </select>");
            Output.WriteLine("        </td>");
            Output.WriteLine("      </tr>");
            Output.WriteLine("      <tr><td colspan=\"2\">&nbsp;</td></tr>");


            string user_view = "default";
            if (HttpContext.Current.Session["User_Default_View"] != null)
                user_view = HttpContext.Current.Session["User_Default_View"].ToString();

            Output.WriteLine("      <tr>");
            Output.WriteLine("        <td align=\"left\" width=\"100px\">" + defaultView + "</td>");
            Output.WriteLine("        <td align=\"left\">");
            Output.WriteLine("          <select name=\"defaultViewDropDown\" id=\"defaultViewDropDown\">");
            Output.WriteLine(user_view == "default" ? "            <option selected=\"selected\" value=\"default\">Collection Default</option>" : "            <option value=\"default\">Collection Default</option>");
            Output.WriteLine(user_view == "brief" ? "            <option selected=\"selected\" value=\"brief\">Brief View</option>" : "            <option value=\"brief\">Brief View</option>");
            Output.WriteLine(user_view == "table" ? "            <option selected=\"selected\" value=\"table\">Table View</option>" : "            <option value=\"table\">Table View</option>");
            Output.WriteLine(user_view == "thumb" ? "            <option selected=\"selected\" value=\"thumb\">Thumbnail View</option>" : "            <option value=\"thumb\">Thumbnail View</option>");
            Output.WriteLine("          </select>");
            Output.WriteLine("        </td>");
            Output.WriteLine("      </tr>");

            Output.WriteLine("      <tr><td colspan=\"2\">&nbsp;</td></tr>");

            int user_sort = -1;
            if (HttpContext.Current.Session["User_Default_Sort"] != null)
                user_sort = Convert.ToInt32(HttpContext.Current.Session["User_Default_Sort"]);

            Output.WriteLine("      <tr>");
            Output.WriteLine("        <td align=\"left\" width=\"100px\">" + defaultSort + "</td>");
            Output.WriteLine("        <td align=\"left\">");
            Output.WriteLine("          <select name=\"defaultSortDropDown\" id=\"defaultSortDropDown\">");

            Output.WriteLine(user_sort == -1 ? "            <option selected=\"selected\" value=\"-1\">Default</option>" : "            <option value=\"-1\">Default</option>");
            Output.WriteLine(user_sort == 1 ? "            <option selected=\"selected\" value=\"0\">Relevance</option>" : "            <option value=\"0\">Relevance</option>");
            Output.WriteLine(user_sort == 0 ? "            <option selected=\"selected\" value=\"1\">Title</option>" : "            <option value=\"1\">Title</option>");

            // Add the bibid sorts if this is an internal user
            if (RequestSpecificValues.Current_Mode.Internal_User)
            {
                Output.WriteLine(user_sort == 2 ? "            <option selected=\"selected\" value=\"2\">BibID Ascending</option>" : "            <option value=\"2\">BibID Ascending</option>");
                Output.WriteLine(user_sort == 3 ? "            <option selected=\"selected\" value=\"3\">BibID Descending</option>" : "            <option value=\"3\">BibID Descending</option>");
            }
            Output.WriteLine(user_sort == 10 ? "            <option selected=\"selected\" value=\"10\">Date Ascending</option>" : "            <option value=\"10\">Date Ascending</option>");
            Output.WriteLine(user_sort == 11 ? "            <option selected=\"selected\" value=\"11\">Date Descending</option>" : "            <option value=\"11\">Date Descending</option>");
            Output.WriteLine("          </select>");
            Output.WriteLine("        </td>");
            Output.WriteLine("      </tr>");

            Output.WriteLine("      <tr><td colspan=\"2\">&nbsp;</td></tr>");
            Output.WriteLine("      <tr>");
            Output.WriteLine("        <td colspan=\"2\" align=\"center\">");
            Output.WriteLine("          <input type=\"button\" onclick=\"return preferences();\" value=\"" + button + "\" class=\"SobekSearchButton\" />");
            Output.WriteLine("        </td>");
            Output.WriteLine("      </tr>");
            Output.WriteLine("      <tr><td colspan=\"2\">&nbsp;</td></tr>");
            Output.WriteLine("    </table>");
            Output.WriteLine("  </form>");
            Output.WriteLine("</div>");
            Output.WriteLine("<br />");
            Output.WriteLine();

            Output.WriteLine("<!-- Close the pagecontainer div -->");
            Output.WriteLine("</div>");
            Output.WriteLine();

            return true;
        }

        /// <summary> Title for this web page </summary>
        public override string WebPage_Title
        {
            get { return "{0} Preferences"; }
        }

        /// <summary> Write any additional values within the HTML Head of the
        /// final served page </summary>
        /// <param name="Output"> Output stream currently within the HTML head tags </param>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        public override void Write_Within_HTML_Head(TextWriter Output, Custom_Tracer Tracer)
        {
            Output.WriteLine("  <meta name=\"robots\" content=\"noindex, nofollow\" />");
        }
    }
}
