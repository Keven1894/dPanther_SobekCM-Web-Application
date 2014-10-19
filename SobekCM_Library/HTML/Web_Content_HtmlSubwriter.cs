﻿#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using SobekCM.Core.Navigation;
using SobekCM.Core.SiteMap;
using SobekCM.Engine_Library.Navigation;
using SobekCM.Tools;

#endregion

namespace SobekCM.Library.HTML
{
    /// <summary> Simple web content html subwriter acts like a simple content management system, reading in source
    /// html files and displaying them within the framework of this digital library and after applying
    /// the indicated web skin. </summary>
    /// <remarks> This class extends the <see cref="abstractHtmlSubwriter"/> abstract class. </remarks>
    public class Web_Content_HtmlSubwriter : abstractHtmlSubwriter
    {
        private string breadcrumbs;

        /// <summary> Constructor for a new instance of the Web_Content_HtmlSubwriter class </summary>
        /// <param name="RequestSpecificValues"> All the necessary, non-global data specific to the current request </param>
        public Web_Content_HtmlSubwriter(RequestCache RequestSpecificValues) : base(RequestSpecificValues) 
        {
            // If there is a sitemap, check if this is a robot request and then if the URL
            // for the sitemap pages is URL restricted
            if ((RequestSpecificValues.Site_Map != null) && (RequestSpecificValues.Site_Map.Is_URL_Restricted_For_Robots) && (RequestSpecificValues.Current_Mode.Is_Robot))
            {
                if (RequestSpecificValues.Current_Mode.Base_URL != RequestSpecificValues.Site_Map.Restricted_Robot_URL)
                {
                    RequestSpecificValues.Current_Mode.Base_URL = RequestSpecificValues.Site_Map.Restricted_Robot_URL;
                    string redirect_url = UrlWriterHelper.Redirect_URL(RequestSpecificValues.Current_Mode);

                    HttpContext.Current.Response.Status = "301 Moved Permanently";
                    HttpContext.Current.Response.AddHeader("Location", redirect_url);
                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                    RequestSpecificValues.Current_Mode.Request_Completed = true;
                    return;
                }
            }
        }

        /// <summary> Gets the collection of special behaviors which this subwriter
        /// requests from the main HTML subwriter. </summary>
        /// <remarks> By default, this returns an empty list </remarks>
        public override List<HtmlSubwriter_Behaviors_Enum> Subwriter_Behaviors
        {
            get
            {
                return new List<HtmlSubwriter_Behaviors_Enum>
                    {
                        HtmlSubwriter_Behaviors_Enum.Suppress_Banner
                    };
            }
        }

        /// <summary> Add the sitemap tree-view control, if there is a site map included in this object </summary>
        /// <param name="placeHolder"> Main place holder ( &quot;mainPlaceHolder&quot; ) in the itemNavForm form, widely used throughout the application</param>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        public void Add_Controls(PlaceHolder placeHolder, Custom_Tracer Tracer)
        {
            if ((RequestSpecificValues.Site_Map == null) || (RequestSpecificValues.Current_Mode.Is_Robot))
                return;

            Tracer.Add_Trace("Web_Content_HtmlSubwriter.Add_Controls", "Adding site map tree nav view");

            // Create the treeview
            TreeView treeView1 = new TreeView
                {
                    CssClass = "SobekSiteMapTreeView",
                    ExpandDepth = 0,
                    NodeIndent = 15,
                    ShowLines = true,
                    EnableClientScript = true,
                    PopulateNodesFromClient = true
                };

            // Set some tree view properties
            treeView1.TreeNodePopulate += treeView1_TreeNodePopulate;


            // Determine the base URL
            string base_url = RequestSpecificValues.Current_Mode.Base_URL;
            if (RequestSpecificValues.Current_Mode.Writer_Type == Writer_Type_Enum.HTML_LoggedIn)
            {
                base_url = base_url + "l/";
            }

            // Find the selected node
            int selected_node = RequestSpecificValues.Site_Map.Selected_NodeValue(RequestSpecificValues.Current_Mode.Info_Browse_Mode);

            foreach (SobekCM_SiteMap_Node rootSiteMapNode in RequestSpecificValues.Site_Map.RootNodes)
            {
                // Add the sitemaps root node first
                TreeNode rootNode = new TreeNode
                    {
                        SelectAction = TreeNodeSelectAction.None,
                        Text = string.Format("<a href='{0}{1}' title='{2}'>{3}</a>", base_url, rootSiteMapNode.URL, rootSiteMapNode.Description, rootSiteMapNode.Title)
                    };
                treeView1.Nodes.Add(rootNode);

                // Was this node currently selected?
                if (rootSiteMapNode.URL == RequestSpecificValues.Current_Mode.Info_Browse_Mode)
                {
                    rootNode.Text = string.Format("<span Title='{0}'>{1}</span>", rootSiteMapNode.Description, rootSiteMapNode.Title);
                    rootNode.Expand();
                }

                // Now add all the children recursively
                add_child_nodes(rootNode, rootSiteMapNode, base_url, selected_node);
            }

            // Always expand the top node
            //rootNode.Expand();

            // Add the tree to the view
            placeHolder.Controls.Add(treeView1);

        }

        private void add_child_nodes(TreeNode treeNode, SobekCM_SiteMap_Node siteNode, string base_url, int selected_node )
        {
            // Only do anything if there are child nodes defined
            if (siteNode.Child_Nodes_Count > 0)
            {
                // Step through each child node
                ReadOnlyCollection<SobekCM_SiteMap_Node> childNodes = siteNode.Child_Nodes;
                int child_node_counter = 0;
                while ( child_node_counter < childNodes.Count )
                {
                    // Get this child node
                    SobekCM_SiteMap_Node childNode = childNodes[child_node_counter];

                    // Add this child node to the tree view
                    TreeNode childTreeNode = new TreeNode
                                                 {
                                                     SelectAction = TreeNodeSelectAction.None,
                                                     Value = childNode.NodeValue.ToString()
                                                 };

                    if (childNode.URL.Length > 0)
                    {
                        childTreeNode.Text = string.Format("<a href='{0}' title='{1}'>{2}</a>", base_url + childNode.URL, childNode.Description, childNode.Title);
                    }
                    else
                    {
                        childTreeNode.Text = string.Format("<span Title='{0}' class='SobekSiteMapNoLink' >{1}</span>", childNode.Description, childNode.Title);
                        childTreeNode.SelectAction = TreeNodeSelectAction.Expand;
                    }
                    treeNode.ChildNodes.Add(childTreeNode);

                    if (childNode.Child_Nodes_Count > 0)
                    {
                        // Determine if the selected node is in the child nodes...
                        if ((childNode.NodeValue < selected_node) && ((child_node_counter + 1 == childNodes.Count) || (childNodes[child_node_counter + 1].NodeValue > selected_node)))
                        {
                            // Recurse through any children
                            add_child_nodes(childTreeNode, childNode, base_url, selected_node);
                        }
                        else
                        {
                            childTreeNode.PopulateOnDemand = true;
                        }
                    }

                    // Was this node currently selected?
                    if (childNode.NodeValue == selected_node )
                    {
                        childTreeNode.Text = string.Format("<span Title='{0}'>{1}</span>", childNode.Description, childNode.Title);
                        childTreeNode.Expand();

                        // Create the breadcrumbs now
                        StringBuilder breadcrumbBuilder = new StringBuilder();
                        breadcrumbBuilder.Append(childTreeNode.Text);

                        // Add each parent next, if they have a URL and also expand each parent
                        TreeNode selectedNodeExpander = childTreeNode;
                        while (selectedNodeExpander.Parent != null) 
                        {
                            // expand this node
                            (selectedNodeExpander.Parent).Expand();

                            // add to breadcrumb, if a link
                            if (selectedNodeExpander.Parent.SelectAction == TreeNodeSelectAction.None)
                            {
                                string text = selectedNodeExpander.Parent.Text.Replace(" Namespace</a>","</a>").Replace(" Sub-Namespace</a>","</a>");

                                breadcrumbBuilder.Insert(0, text + " <img src=\"" + RequestSpecificValues.Current_Mode.Base_URL + "design/skins/" + RequestSpecificValues.Current_Mode.Base_Skin + "/breadcrumbimg.gif\" alt=\">\" /><img src=\"" + RequestSpecificValues.Current_Mode.Base_URL + "design/skins/" + RequestSpecificValues.Current_Mode.Base_Skin + "/breadcrumbimg.gif\" alt=\">\" /> ");
                            }

                            // step up another level
                            selectedNodeExpander = selectedNodeExpander.Parent;
                        }
                        breadcrumbs = breadcrumbBuilder.ToString();
                    }

                    child_node_counter++;
                }
            }
        }

        /// <summary> Event handler loads the nodes on request to the serial hierarchy trees when the user requests them
        /// by expanding a node </summary>
        /// <param name="sender"> TreeView object that fired this event </param>
        /// <param name="e"> Event arguments includes the tree node which was expanded </param>
        void treeView1_TreeNodePopulate(object sender, TreeNodeEventArgs e)
        {
            SobekCM_SiteMap_Node retrieved_node = RequestSpecificValues.Site_Map.Node_By_Value(Convert.ToInt32(e.Node.Value));

            // Determine the base URL
            string base_url = RequestSpecificValues.Current_Mode.Base_URL;
            if (RequestSpecificValues.Current_Mode.Writer_Type == Writer_Type_Enum.HTML_LoggedIn)
            {
                base_url = base_url + "l/";
            }

            if ((retrieved_node != null) && (retrieved_node.Child_Nodes_Count > 0))
            {
                foreach (SobekCM_SiteMap_Node childNode in retrieved_node.Child_Nodes)
                {
                    // Add this child node to the tree view
                    TreeNode childTreeNode = new TreeNode
                                                 {
                                                     SelectAction = TreeNodeSelectAction.None,
                                                     Value = childNode.NodeValue.ToString()
                                                 };
                    if (childNode.Child_Nodes_Count > 0)
                    {
                        childTreeNode.PopulateOnDemand = true;
                    }
                    if (childNode.URL.Length > 0)
                    {
                        childTreeNode.Text = string.Format("<a href='{0}' title='{1}'>{2}</a>", base_url + childNode.URL, childNode.Description, childNode.Title);
                    }
                    else
                    {
                        childTreeNode.Text = string.Format("<span Title='{0}' class='SobekSiteMapNoLink' >{1}</span>", childNode.Description, childNode.Title);
                        childTreeNode.SelectAction = TreeNodeSelectAction.Expand;
                    }
                    e.Node.ChildNodes.Add(childTreeNode);
                }
            }
        }


        /// <summary> Writes the HTML generated by this simple text / CMS html subwriter directly to the response stream </summary>
        /// <param name="Output"> Stream to which to write the HTML for this subwriter </param>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        /// <returns> FALSE -- Value indicating if html writer should finish the page immediately after this, or if there are other controls or routines which need to be called first </returns>
        /// <remarks> This just begins the page and gets ready for site map tree view to possibly be added </remarks>
        public override bool Write_HTML(TextWriter Output, Custom_Tracer Tracer)
        {
            Tracer.Add_Trace("Web_Content_HtmlSubwriter.Write_HTML", "Rendering HTML");

            // The header is already drawn, so just start the main table here
            if (RequestSpecificValues.Site_Map != null)
            {
                Output.WriteLine("<table width=\"100%\">");
                Output.WriteLine("<tr>");
                if (RequestSpecificValues.Site_Map.Width > 0)
                {
                    Output.WriteLine("<td valign=\"top\" width=\"" + RequestSpecificValues.Site_Map.Width + "px\">");
                }
                else
                {
                    Output.WriteLine("<td valign=\"top\">");
                }

                // If this is a robot, just draw the links
                if (RequestSpecificValues.Current_Mode.Is_Robot)
                {
                    Output.WriteLine("<div class=\"SobekSiteMapTreeView\">");
                    foreach (SobekCM_SiteMap_Node rootNode in RequestSpecificValues.Site_Map.RootNodes)
                    {
                        recursively_draw_sitemap_for_robots(Output, rootNode, String.Empty);
                    }
                    Output.WriteLine("</div>");
                }
            }

            return false;
        }

        private void recursively_draw_sitemap_for_robots(TextWriter Output, SobekCM_SiteMap_Node Node, string Indent)
        {

            // Add this text
            if ((Node.URL.Length > 0) && (Node.URL != RequestSpecificValues.Current_Mode.Info_Browse_Mode))
            {
                Output.WriteLine(Indent + "<a href='" + RequestSpecificValues.Current_Mode.Base_URL + Node.URL + "' title='" + Node.Description + "'>" + Node.Title + "</a><br />");
            }
            else
            {
                Output.WriteLine(Indent + "<span Title='" + Node.Description + "' class='SobekSiteMapNoLink' >" + Node.Title + "</span><br />");
            }

            // Add all the children
            if (Node.Child_Nodes_Count > 0)
            {
                foreach (SobekCM_SiteMap_Node childNode in Node.Child_Nodes)
                {
                    recursively_draw_sitemap_for_robots(Output, childNode, Indent + " &nbsp; &nbsp; &nbsp; ");
                }
            }
        }


        /// <summary> Writes the HTML generated by this simple text / CMS html subwriter directly to the response stream </summary>
        /// <param name="Output"> Stream to which to write the HTML for this subwriter </param>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        /// <remarks> This finishes out the page and includes all the static content in this static web content file </remarks>
        public override void Write_Final_HTML(TextWriter Output, Custom_Tracer Tracer)
        {
            Tracer.Add_Trace("Web_Content_HtmlSubwriter.Write_Final_HTML", "Rendering HTML");

            // If there is a sitemap, move to the second part of the table
            if (RequestSpecificValues.Site_Map != null)
            {
                Output.WriteLine("</td>");
                Output.WriteLine("<td valign=\"top\">");
            }

            // Save the current mode and browse
            Display_Mode_Enum thisMode = RequestSpecificValues.Current_Mode.Mode;

            if ((RequestSpecificValues.Static_Web_Content.Banner.Length > 0) && (RequestSpecificValues.Static_Web_Content.Banner.ToUpper().Trim() != "NONE"))
            {
                if (RequestSpecificValues.Static_Web_Content.Banner.ToUpper().Trim() == "DEFAULT")
                {
                    if ((RequestSpecificValues.HTML_Skin != null) && (RequestSpecificValues.HTML_Skin.Banner_HTML.Length > 0))
                    {
                        Output.WriteLine(RequestSpecificValues.HTML_Skin.Banner_HTML);
                    }
                    else
                    {
                        if (RequestSpecificValues.Hierarchy_Object != null)
                        {
                            Output.WriteLine("<img id=\"mainBanner\" src=\"" + RequestSpecificValues.Current_Mode.Base_URL + RequestSpecificValues.Hierarchy_Object.Banner_Image(RequestSpecificValues.Current_Mode.Language, RequestSpecificValues.HTML_Skin) + "\" alt=\"MISSING BANNER\" /><br />");
                        }
                    }
                }
                else
                {
                    Output.WriteLine("<img id=\"mainBanner\" src=\"" + RequestSpecificValues.Static_Web_Content.Banner.Replace("<%BASEURL%>", RequestSpecificValues.Current_Mode.Base_URL) + "\" alt=\"MISSING BANNER\" /><br />");
                }
            }

            // Add the breadcrumbs
            if (!String.IsNullOrEmpty(breadcrumbs))
            {
                Output.WriteLine( "<span class=\"breadcrumbs\">" + breadcrumbs + "</span><br />");
            }

            // Add the secondary HTML ot the home page
            Output.WriteLine("<div class=\"SobekResultsPanel\">");
            Output.WriteLine(RequestSpecificValues.Static_Web_Content.Static_Text);
            Output.WriteLine("</div>");
            Output.WriteLine("<br />");
            Output.WriteLine();

            // If there is a sitemap, finish the main table
            if (RequestSpecificValues.Site_Map != null)
            {
                Output.WriteLine("</td>");
                Output.WriteLine("</tr>");
                Output.WriteLine("</table>");
            }
        }

        /// <summary> Title for this web page </summary>
        public override string WebPage_Title
        {
            get {
                return RequestSpecificValues.Static_Web_Content != null ? RequestSpecificValues.Static_Web_Content.Title : "{0}";
            }
        }

        /// <summary> Write any additional values within the HTML Head of the
        /// final served page </summary>
        /// <param name="Output"> Output stream currently within the HTML head tags </param>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        public override void Write_Within_HTML_Head(TextWriter Output, Custom_Tracer Tracer)
        {
            Output.WriteLine("  <meta name=\"robots\" content=\"index, nofollow\" />");

            // Add any other meta tags here as well
            if (RequestSpecificValues.Static_Web_Content != null)
            {
                if (RequestSpecificValues.Static_Web_Content.Description.Length > 0)
                {
                    Output.WriteLine("  <meta name=\"description\" content=\"" + RequestSpecificValues.Static_Web_Content.Description + "\" />");
                }
                if (RequestSpecificValues.Static_Web_Content.Keywords.Length > 0)
                {
                    Output.WriteLine("  <meta name=\"keywords\" content=\"" + RequestSpecificValues.Static_Web_Content.Keywords + "\" />");
                }
                if (RequestSpecificValues.Static_Web_Content.Author.Length > 0)
                {
                    Output.WriteLine("  <meta name=\"author\" content=\"" + RequestSpecificValues.Static_Web_Content.Author + "\" />");
                }
            }


			// Write the style sheet to use 
#if DEBUG
            Output.WriteLine("  <link href=\"" + RequestSpecificValues.Current_Mode.Base_URL + "default/SobekCM_Metadata.css\" rel=\"stylesheet\" type=\"text/css\" />");
#else
			Output.WriteLine("  <link href=\"" + RequestSpecificValues.Current_Mode.Base_URL + "default/SobekCM_Metadata.min.css\" rel=\"stylesheet\" type=\"text/css\" />");
#endif

            // If this is the static html web content view, add any special text which came from the original
            // static html file which was already read, which can include style sheets, etc..
            if ((RequestSpecificValues.Static_Web_Content != null) && (RequestSpecificValues.Static_Web_Content.Extra_Head_Info.Length > 0))
            {
                Output.WriteLine("  " + RequestSpecificValues.Static_Web_Content.Extra_Head_Info.Trim());
            }
        }

		/// <summary> Gets the CSS class of the container that the page is wrapped within </summary>
		public override string Container_CssClass
		{
			get
			{
                return RequestSpecificValues.Site_Map != null ? String.Empty : base.Container_CssClass;
			}
		}
    }
}
