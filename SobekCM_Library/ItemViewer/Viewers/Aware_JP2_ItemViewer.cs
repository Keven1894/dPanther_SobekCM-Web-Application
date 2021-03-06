#region Using directives

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SobekCM.Core.Configuration;
using SobekCM.Core.Navigation;
using SobekCM.Library.HTML;
using SobekCM.Library.UI;
using SobekCM.Tools;
using Image = System.Web.UI.WebControls.Image;

#endregion

namespace SobekCM.Library.ItemViewer.Viewers
{
	/// <summary> Item viewer displays a JPEG2000 file related to the digital resource, allowing zoom and pan. </summary>
	/// <remarks> This class extends the abstract class <see cref="abstractItemViewer"/> and implements the 
	/// <see cref="iItemViewer" /> interface. <br /><br />
	/// This class was written specifically for the Aware JPEG2000 server, and will need modifications to work
	/// with any other JPEG2000 server software. </remarks>
	public class Aware_JP2_ItemViewer : abstractItemViewer
	{
	    private int actualZoomLevel;
		private string featureColor;
	    private int featureHeight;
	    private string featureType;
	    private int featureWidth;
	    private int featureX;
		private int featureY;
	    private int height;
	    private readonly string resourceType;
	    private int width;
	    private ushort zoomlevels;

	    private int thumbnailClickX;
	    private int thumbnailClickY;

		/// <summary> Constructor for a new instance of the Aware_JP2_ItemViewer class </summary>
        public Aware_JP2_ItemViewer(Navigation_Object Current_Mode)
		{
			width = 0;
			height = 0;
			zoomlevels = 0;
			resourceType = String.Empty;

		    CurrentMode = Current_Mode;

            handle_postback();
		}

		/// <summary> Constructor for a new instance of the Aware_JP2_ItemViewer class </summary>
		/// <param name="Attributes"> Attributes for the JPEG2000 file to display, including width and height</param>
		/// <param name="Resource_Type"> Resource type for the item being displayed; this affects the overall rendering style </param>
		public Aware_JP2_ItemViewer( string Resource_Type, string Attributes, Navigation_Object Current_Mode )
		{
			resourceType = Resource_Type;
			width = 0;
			height = 0;
			zoomlevels = 0;

            // Parse if there were attributes
		    if (Attributes.Length <= 0) return;

		    string[] splitter = Attributes.Split(";".ToCharArray());
		    foreach (string thisSplitter in splitter)
		    {
		        if (thisSplitter.ToUpper().IndexOf("WIDTH") >= 0)
		        {
		            Int32.TryParse(thisSplitter.Substring(thisSplitter.IndexOf("=") + 1), out width);
		        }

		        if (thisSplitter.ToUpper().IndexOf("HEIGHT") >= 0)
		        {
		            Int32.TryParse(thisSplitter.Substring(thisSplitter.IndexOf("=") + 1), out height);
		        }
		    }

            CurrentMode = Current_Mode;
            handle_postback();
		}

        private void handle_postback()
        {
            thumbnailClickX = -100;
            thumbnailClickY = -100;

            if (CurrentMode.isPostBack)
            {
                string x_click = HttpContext.Current.Request.Form["thumbnail_click_x"];
                string y_click = HttpContext.Current.Request.Form["thumbnail_click_y"];

                if ((x_click != "-100") && (y_click != "-100"))
                {
                    Int32.TryParse(x_click, out thumbnailClickX);
                    Int32.TryParse(y_click, out thumbnailClickY);
                }
            }
        }

	    /// <summary> Gets the type of item viewer this object represents </summary>
		/// <value> This property always returns the enumerational value <see cref="ItemViewer_Type_Enum.JPEG2000"/>. </value>
		public override ItemViewer_Type_Enum ItemViewer_Type
		{
			get { return ItemViewer_Type_Enum.JPEG2000; }
		}


		/// <summary> Flag indicates if this view should be overriden if the item is checked out by another user </summary>
		/// <remarks> This always returns the value TRUE for this viewer </remarks>
		public override bool Override_On_Checked_Out
		{
			get
			{
				return true;
			}
		}

		/// <summary> Sets the attributes for the JPEG2000 file to display, including width and height  </summary>
		public override string Attributes
		{
			set
			{
				// Parse if there were attributes
			    if (value.Length <= 0) return;

			    string[] splitter = value.Split(";".ToCharArray() );
			    foreach( string thisSplitter in splitter )
			    {
			        if (thisSplitter.ToUpper().IndexOf("WIDTH") >= 0)
			        {
			            Int32.TryParse(thisSplitter.Substring(thisSplitter.IndexOf("=") + 1), out width);
			        }

			        if (thisSplitter.ToUpper().IndexOf("HEIGHT") >= 0)
			        {
			            Int32.TryParse(thisSplitter.Substring(thisSplitter.IndexOf("=") + 1), out height);
			        }
			    }
			}
		}

		/// <summary> Get the height for the current image, from the attributes </summary>
		public int Height
		{
			get { return height; }
		}

		/// <summary> Get the width for the current image, from the attributes </summary>
		public int Width
		{
			get { return width; }
		}

	    /// <summary> Gets any HTML for a Navigation Row above the image or text </summary>
		/// <value> This returns the HTML including buttons for zooming in and changing the viewport size </value>
		private string NavigationRow
		{
			get
			{
				string zoom_out_text = "Zoom Out";
				string zoom_to_level_text = "Zoom to Level ";
				string current_zoom_text = "Current Zoom";
				string zoom_in_text = "Zoom In";
				string rotate_clockwise_text = "Rotate Clockwise";
				string rotate_counter_text = "Rotate Counter Clockwise";
				string pan_up_text = "Pan Up";
				string pan_down_text = "Pan Down";
				string pan_left_text = "Pan Left";
				string pan_right_text = "Pan Right";

				if (CurrentMode.Language == Web_Language_Enum.French)
				{
					zoom_out_text = "Zoom Arri�re";
					zoom_to_level_text = "Zoom a Niveau ";
					current_zoom_text = "Zoom Courant";
					zoom_in_text = "Zoom Avant";
					rotate_clockwise_text = "Tournez a Droit";
					rotate_counter_text = "Tournez a Gauche";
					pan_up_text = "Panoramique vertical vers le haut";
					pan_down_text = "Panoramique vertical vers le bas";
					pan_left_text = "Panoramique horizontal droit";
					pan_right_text = "Panoramique horizontal gauche";
				}

				if (CurrentMode.Language == Web_Language_Enum.Spanish)
				{
					zoom_out_text = "Zoom Hacia Afuera";
					zoom_to_level_text = "Ampliar al Nivel ";
					current_zoom_text = "Ampliaci�n Presente";
					zoom_in_text = "Zoom Hacia Dentro";
					rotate_clockwise_text = "Rotar Hacia la Derecha";
					rotate_counter_text = "Rotar Hacia la Izquierda";
					pan_up_text = "Mirar Hacia Arriba";
					pan_down_text = "Mirar Hacia Abajo";
					pan_left_text = "Mirar Hacia la Izquierda";
					pan_right_text = "Mirar Hacia la Derecha";
				}


				string image_location = CurrentMode.Base_URL + "design/skins/" + CurrentMode.Base_Skin_Or_Skin + "/zoom_controls/";

				StringBuilder navRow = new StringBuilder(1000);
				navRow.Append("<table>" + Environment.NewLine );
				navRow.Append("<tr>" + Environment.NewLine );
				navRow.Append("<td style=\"width:30px;\">&nbsp;</td>" + Environment.NewLine );
				navRow.Append("<td>" + Environment.NewLine );

				// Calculate the number of zooms
				zoomlevels = zoom_levels();
                ushort zoom = CurrentMode.Viewport_Zoom.HasValue ? CurrentMode.Viewport_Zoom.Value : (ushort)1;

				// Calculate the size, in pixels, of the viewport
			    int currViewportSize = CurrentMode.Viewport_Size.HasValue ? CurrentMode.Viewport_Size.Value : 1;
                int size_pixels = get_jp2_viewport_size(currViewportSize, zoom);

				// Save the current x and y
                int x = CurrentMode.Viewport_Point_X.HasValue ? CurrentMode.Viewport_Point_X.Value : 0;
                int y = CurrentMode.Viewport_Point_Y.HasValue ? CurrentMode.Viewport_Point_Y.Value : 0;
                

				// Do Zoom out button
				if (zoom > 1)
					CurrentMode.Viewport_Zoom = (ushort)(zoom - 1);
                int zoomed_pixel_size = get_jp2_viewport_size(currViewportSize, zoom);
				CurrentMode.Viewport_Point_X = adjust_x(x, y, size_pixels, zoomed_pixel_size);
				CurrentMode.Viewport_Point_Y = adjust_y(x, y, size_pixels, zoomed_pixel_size);
				navRow.Append("<a href=\"" + UrlWriterHelper.Redirect_URL(CurrentMode) + "\"><img alt=\"" + zoom_out_text + "\" src=\"" + image_location + "zoom_out.gif\" /></a>" + Environment.NewLine );

				// Add the buttons for each zoom level in this image
				for (ushort i = 1; i <= zoomlevels; i++)
				{
					// Direct access to zoom
					CurrentMode.Viewport_Zoom = i;
                    zoomed_pixel_size = get_jp2_viewport_size(currViewportSize, i);
					if (zoom != i)
					{
						CurrentMode.Viewport_Point_X = adjust_x(x, y, size_pixels, zoomed_pixel_size);
						CurrentMode.Viewport_Point_Y = adjust_y(x, y, size_pixels, zoomed_pixel_size);
						navRow.Append("<a href=\"" + UrlWriterHelper.Redirect_URL(CurrentMode) + "\"><img alt=\"" + zoom_to_level_text + "&ldquo;" + (zoomlevels - i + 1) + "&rdquo;\" src=\"" + image_location + "znotselected.gif\" /></a>" + Environment.NewLine );
					}
					else
					{
						navRow.Append("<img alt=\"" + current_zoom_text + "\" src=\"" + image_location + "zselected.gif\"/>" + Environment.NewLine );
					}
				}

				// Do Zoom in button
				CurrentMode.Viewport_Zoom = (ushort)(zoom + 1);
				if (CurrentMode.Viewport_Zoom > zoomlevels)
					CurrentMode.Viewport_Zoom = zoomlevels;
                zoomed_pixel_size = get_jp2_viewport_size(currViewportSize, CurrentMode.Viewport_Zoom.Value);
				CurrentMode.Viewport_Point_X = adjust_x(x, y, size_pixels, zoomed_pixel_size);
				CurrentMode.Viewport_Point_Y = adjust_y(x, y, size_pixels, zoomed_pixel_size);
				navRow.Append("<a href=\"" + UrlWriterHelper.Redirect_URL(CurrentMode) + "\"><img alt=\"" + zoom_in_text + "\" src=\"" + image_location + "zoom_in.gif\" /></a>" + Environment.NewLine );

				// Restore the x and y
				CurrentMode.Viewport_Point_X = x;
				CurrentMode.Viewport_Point_Y = y;

				// Return to regular zoom
				CurrentMode.Viewport_Zoom = zoom;
				navRow.Append("</td>" + Environment.NewLine );
                navRow.Append("<td style=\"width:30px;\">&nbsp;</td>" + Environment.NewLine);
				navRow.Append("<td>" + Environment.NewLine );

				// Only show this if there is an X or Y
				if (CurrentMode.Viewport_Zoom != 1)
				{
					navRow.Append("<img alt=\"Pan\" src=\"" + image_location + "pantools.gif\" usemap=\"#Map\" />" + Environment.NewLine );
				}
				else
				{
					navRow.Append("&nbsp;");
				}
				navRow.Append("</td>" + Environment.NewLine );
                navRow.Append("<td style=\"width:30px;\">&nbsp;</td>" + Environment.NewLine);
				navRow.Append("<td>" + Environment.NewLine );

				// Do the rotate buttons
			    ushort rotate = CurrentMode.Viewport_Rotation.HasValue ? CurrentMode.Viewport_Rotation.Value : (ushort) 0;
				CurrentMode.Viewport_Point_Y = x;
				if (CurrentMode.Viewport_Point_X < 0)
					CurrentMode.Viewport_Point_X = 0;
				if ((CurrentMode.Viewport_Rotation % 2) == 0)
					CurrentMode.Viewport_Point_X = height - y - size_pixels;
				else
					CurrentMode.Viewport_Point_X = width - y - size_pixels;
				CurrentMode.Viewport_Rotation = (ushort)(rotate + 1);
				if (CurrentMode.Viewport_Rotation > 3)
					CurrentMode.Viewport_Rotation = 0;
				navRow.Append("<a href=\"" + UrlWriterHelper.Redirect_URL(CurrentMode) + "\"><img alt=\"" + rotate_clockwise_text + "\" src=\"" + image_location + "cw.gif\" /></a>" + Environment.NewLine );
				CurrentMode.Viewport_Point_X = x;
				CurrentMode.Viewport_Point_Y = y;

				CurrentMode.Viewport_Rotation = (rotate);
				CurrentMode.Viewport_Point_X = y;
				if ((CurrentMode.Viewport_Rotation % 2) == 0)
					CurrentMode.Viewport_Point_Y = width - x - size_pixels;
				else
					CurrentMode.Viewport_Point_Y = height - x - size_pixels;
				if (CurrentMode.Viewport_Point_Y < 0)
					CurrentMode.Viewport_Point_Y = 0;
				CurrentMode.Viewport_Rotation = (ushort)(rotate - 1);
				if (CurrentMode.Viewport_Rotation < 0)
					CurrentMode.Viewport_Rotation = 3;
				navRow.Append("<a href=\"" + UrlWriterHelper.Redirect_URL(CurrentMode) + "\"><img alt=\"" + rotate_counter_text + "\" src=\"" + image_location + "ccw.gif\" /></a>" + Environment.NewLine );
				CurrentMode.Viewport_Rotation = rotate;
				CurrentMode.Viewport_Point_X = x;
				CurrentMode.Viewport_Point_Y = y;

				navRow.Append("</td>" + Environment.NewLine );
                navRow.Append("<td style=\"width:30px;\">&nbsp;</td>" + Environment.NewLine);
				navRow.Append("<td>" + Environment.NewLine );

				// Smallest screen button (512 x 512)
                ushort size = CurrentMode.Viewport_Size.HasValue ? CurrentMode.Viewport_Size.Value : (ushort) 1;
				if (size == 0)
					navRow.Append("<img src=\"" + image_location + "sizetools1.gif\" alt=\"Small size view\" />" + Environment.NewLine );
				else
				{
					CurrentMode.Viewport_Size = 0;
					navRow.Append("<a href=\"" + UrlWriterHelper.Redirect_URL(CurrentMode) + "\"><img src=\"" + image_location + "sizetools1_not.gif\" alt=\"Small size view\" /></a>" + Environment.NewLine );
				}


				// Medium small screen button (768x768)
				ushort temp_zoom_levels;
				if (size == 1)
					navRow.Append("<img src=\"" + image_location + "sizetools2.gif\" alt=\"Medium size view\" />" + Environment.NewLine );
				else
				{
					CurrentMode.Viewport_Size = 1;
					temp_zoom_levels = zoom_levels();
					if (zoom > temp_zoom_levels)
						CurrentMode.Viewport_Zoom = temp_zoom_levels;
					navRow.Append("<a href=\"" + UrlWriterHelper.Redirect_URL(CurrentMode) + "\"><img src=\"" + image_location + "sizetools2_not.gif\" alt=\"Medium size view\" /></a>" + Environment.NewLine );
				}

				// Medium large screen button (1024 x 1024)
				if (size == 2)
					navRow.Append("<img src=\"" + image_location + "sizetools3.gif\" alt=\"Medium-large size view\" />" + Environment.NewLine );
				else
				{
					CurrentMode.Viewport_Size = 2;
					temp_zoom_levels = zoom_levels();
					if (zoom > temp_zoom_levels)
						CurrentMode.Viewport_Zoom = temp_zoom_levels;
					navRow.Append("<a href=\"" + UrlWriterHelper.Redirect_URL(CurrentMode) + "\"><img src=\"" + image_location + "sizetools3_not.gif\" alt=\"Medium-large size view\"/></a>" + Environment.NewLine );
				}

				// Large screen button (1536x1536)
				if (size == 3)
					navRow.Append("<img src=\"" + image_location + "sizetools4.gif\" alt=\"Large size view\" />" + Environment.NewLine );
				else
				{
					CurrentMode.Viewport_Size = 3;
					temp_zoom_levels = zoom_levels();
					if (zoom > temp_zoom_levels)
						CurrentMode.Viewport_Zoom = temp_zoom_levels;
					navRow.Append("<a href=\"" + UrlWriterHelper.Redirect_URL(CurrentMode) + "\"><img src=\"" + image_location + "sizetools4_not.gif\" alt=\"Large size view\" /></a>" + Environment.NewLine );
				}
				CurrentMode.Viewport_Size = size;
				CurrentMode.Viewport_Zoom = zoom;

				navRow.Append("</td>" + Environment.NewLine );


				// If this is an aerial, add a download link
				if (resourceType.IndexOf("AERIAL") >= 0)
				{
                    navRow.Append("<td style=\"width:50px;\">&nbsp;</td>" + Environment.NewLine);
					navRow.Append("<td>" + Environment.NewLine );
					navRow.Append("<a title=\"To download, right click here, select 'Save Target As...' and save the JPEG2000 to your local computer.\" href=\"" + CurrentItem.Web.Source_URL + "/" + FileName + "\" target=\"jp2_downlad\" ><img border=\"0px\" alt=\"To download, right click here, select 'Save Target As...' and save the JPEG2000 to your local computer.\" src=\"" + CurrentMode.Base_URL + "design/skins/" + CurrentMode.Base_Skin_Or_Skin + "/zoom_controls/save.gif\"></a></td>");
					navRow.Append("<td><a title=\"To download, right click here, select 'Save Target As...' and save the JPEG2000 to your local computer.\" href=\"" + CurrentItem.Web.Source_URL + "/" + FileName + "\" target=\"jp2_downlad\" >Download<br />this tile</a>");
					navRow.Append("</td>" + Environment.NewLine );
				}
				navRow.Append("</tr>" + Environment.NewLine );
				navRow.Append("</table>" + Environment.NewLine );

				// Add the pan map, if the pan tool is displayed
				if (CurrentMode.Viewport_Zoom != 1)
				{
				    x = CurrentMode.Viewport_Point_X.HasValue ? CurrentMode.Viewport_Point_X.Value : 0;
                    y = CurrentMode.Viewport_Point_Y.HasValue ? CurrentMode.Viewport_Point_Y.Value : 0;

					navRow.Append("<map name=\"Map\">" + Environment.NewLine );
					CurrentMode.Viewport_Point_X = x - size_pixels;
					if (CurrentMode.Viewport_Point_X < 0)
						CurrentMode.Viewport_Point_X = 0;
					navRow.Append("\t<area shape=\"RECT\" coords=\"0,12,12,24\" href=\"" + UrlWriterHelper.Redirect_URL(CurrentMode) + "\" alt=\"" + pan_left_text +"\" >" + Environment.NewLine );

					CurrentMode.Viewport_Point_X = x;
					CurrentMode.Viewport_Point_Y = y - size_pixels;
					if (CurrentMode.Viewport_Point_Y < 0)
						CurrentMode.Viewport_Point_Y = 0;
					navRow.Append("\t<area shape=\"RECT\" coords=\"12,0,24,12\" href=\"" + UrlWriterHelper.Redirect_URL(CurrentMode) + "\" alt=\"" + pan_up_text + "\" >" + Environment.NewLine );

					CurrentMode.Viewport_Point_Y = y;
					CurrentMode.Viewport_Point_X = x + size_pixels;
					if ((CurrentMode.Viewport_Point_X + size_pixels) > width)
						CurrentMode.Viewport_Point_X = width - size_pixels;
					navRow.Append("\t<area shape=\"RECT\" coords=\"24,12,36,24\" href=\"" + UrlWriterHelper.Redirect_URL(CurrentMode) + "\" alt=\"" + pan_right_text + "\" >" + Environment.NewLine );

					CurrentMode.Viewport_Point_X = x;
					CurrentMode.Viewport_Point_Y = y + size_pixels;
					if ((CurrentMode.Viewport_Point_Y + size_pixels) > height)
						CurrentMode.Viewport_Point_Y = height - size_pixels;
					navRow.Append("\t<area shape=\"RECT\" coords=\"12,24,24,36\" href=\"" + UrlWriterHelper.Redirect_URL(CurrentMode) + "\" alt=\"" + pan_down_text + "\" >" + Environment.NewLine );

					CurrentMode.Viewport_Point_Y = y;
					CurrentMode.Viewport_Point_X = x;
					navRow.Append("</map>" + Environment.NewLine );
				}


			    if (CurrentMode.Viewport_Zoom != 1)
			    {
                    navRow.AppendLine("<input type=\"hidden\" id=\"thumbnail_click_x\" name=\"thumbnail_click_x\" value=\"-100\"/>");
                    navRow.AppendLine("<input type=\"hidden\" id=\"thumbnail_click_y\" name=\"thumbnail_click_y\" value=\"-100\"/>");

			        navRow.AppendLine("<script type=\"text/javascript\">");
			        navRow.AppendLine("  $('#sbkAj2_ThumbZoomedIn').click(function(e){");
			        navRow.AppendLine("    var x = e.pageX - e.target.offsetLeft;");
			        navRow.AppendLine("    var y = e.pageY - e.target.offsetTop;");
			        navRow.AppendLine("    $('#thumbnail_click_x').val(x);");
			        navRow.AppendLine("    $('#thumbnail_click_y').val(y);");
			        navRow.AppendLine("    document.itemNavForm.submit();");
			        navRow.AppendLine("  });");
			        navRow.AppendLine("</script>");
			    }
			    return navRow.ToString();
			}
		}

	    /// <summary> Gets the url to go to the first page </summary>
		/// <remarks> This code is specific to this viewer since the viewport and zoom is reset while going to the requested page </remarks>
		public override string First_Page_URL
		{
			get
			{
                int x = CurrentMode.Viewport_Point_X.HasValue ? CurrentMode.Viewport_Point_X.Value : 0;
                int y = CurrentMode.Viewport_Point_Y.HasValue ? CurrentMode.Viewport_Point_Y.Value : 0;
			    ushort zoom = CurrentMode.Viewport_Zoom.HasValue ? CurrentMode.Viewport_Zoom.Value : (ushort) 1;
                ushort rotation = CurrentMode.Viewport_Zoom.HasValue ? CurrentMode.Viewport_Zoom.Value : (ushort) 0;
				CurrentMode.Viewport_Point_X = 0;
				CurrentMode.Viewport_Point_Y = 0;
				CurrentMode.Viewport_Zoom = 1;
				CurrentMode.Viewport_Rotation = 0;
				string returnValue = base.First_Page_URL;
				CurrentMode.Viewport_Point_X = x;
				CurrentMode.Viewport_Point_Y = y;
				CurrentMode.Viewport_Zoom = zoom;
				CurrentMode.Viewport_Rotation = rotation;
				return returnValue;
			}
		}

		/// <summary> Gets the url to go to the previous page </summary>
		/// <remarks> This code is specific to this viewer since the viewport and zoom is reset while going to the requested page </remarks>
		public override string Previous_Page_URL
		{
			get
			{
                int x = CurrentMode.Viewport_Point_X.HasValue ? CurrentMode.Viewport_Point_X.Value : 0;
                int y = CurrentMode.Viewport_Point_Y.HasValue ? CurrentMode.Viewport_Point_Y.Value : 0;
                ushort zoom = CurrentMode.Viewport_Zoom.HasValue ? CurrentMode.Viewport_Zoom.Value : (ushort)1;
                ushort rotation = CurrentMode.Viewport_Zoom.HasValue ? CurrentMode.Viewport_Zoom.Value : (ushort)0;
				CurrentMode.Viewport_Point_X = 0;
				CurrentMode.Viewport_Point_Y = 0;
				CurrentMode.Viewport_Zoom = 1;
				CurrentMode.Viewport_Rotation = 0;
				string returnValue = base.Previous_Page_URL;
				CurrentMode.Viewport_Point_X = x;
				CurrentMode.Viewport_Point_Y = y;
				CurrentMode.Viewport_Zoom = zoom;
				CurrentMode.Viewport_Rotation = rotation;
				return returnValue;
			}
		}

		/// <summary> Gets the url to go to the next page </summary>
		/// <remarks> This code is specific to this viewer since the viewport and zoom is reset while going to the requested page </remarks>
		public override string Next_Page_URL
		{
			get
			{
                int x = CurrentMode.Viewport_Point_X.HasValue ? CurrentMode.Viewport_Point_X.Value : 0;
                int y = CurrentMode.Viewport_Point_Y.HasValue ? CurrentMode.Viewport_Point_Y.Value : 0;
                ushort zoom = CurrentMode.Viewport_Zoom.HasValue ? CurrentMode.Viewport_Zoom.Value : (ushort)1;
                ushort rotation = CurrentMode.Viewport_Zoom.HasValue ? CurrentMode.Viewport_Zoom.Value : (ushort)0;
				CurrentMode.Viewport_Point_X = 0;
				CurrentMode.Viewport_Point_Y = 0;
				CurrentMode.Viewport_Zoom = 1;
				CurrentMode.Viewport_Rotation = 0;
				string returnValue = base.Next_Page_URL;
				CurrentMode.Viewport_Point_X = x;
				CurrentMode.Viewport_Point_Y = y;
				CurrentMode.Viewport_Zoom = zoom;
				CurrentMode.Viewport_Rotation = rotation;
				return returnValue;
			}
		}

		/// <summary> Gets the url to go to the last page </summary>
		/// <remarks> This code is specific to this viewer since the viewport and zoom is reset while going to the requested page </remarks>
		public override string Last_Page_URL
		{
			get
			{
                int x = CurrentMode.Viewport_Point_X.HasValue ? CurrentMode.Viewport_Point_X.Value : 0;
                int y = CurrentMode.Viewport_Point_Y.HasValue ? CurrentMode.Viewport_Point_Y.Value : 0;
                ushort zoom = CurrentMode.Viewport_Zoom.HasValue ? CurrentMode.Viewport_Zoom.Value : (ushort)1;
                ushort rotation = CurrentMode.Viewport_Zoom.HasValue ? CurrentMode.Viewport_Zoom.Value : (ushort)0;
				CurrentMode.Viewport_Point_X = 0;
				CurrentMode.Viewport_Point_Y = 0;
				CurrentMode.Viewport_Zoom = 1;
				CurrentMode.Viewport_Rotation = 0;
				string returnValue = base.Last_Page_URL;
				CurrentMode.Viewport_Point_X = x;
				CurrentMode.Viewport_Point_Y = y;
				CurrentMode.Viewport_Zoom = zoom;
				CurrentMode.Viewport_Rotation = rotation;
				return returnValue;
			}
		}

		/// <summary> Width for the main viewer section to adjusted to accomodate this viewer</summary>
		/// <value> If the user has not zoomed into the image, 650 is returned, otherwise -1 </value>
		public override int Viewer_Width
		{
			get
			{
			    if (CurrentMode.Viewport_Size <= 1 )
				{
					return 650;
				}
			    return -1;
			}
		}

	    public override void Perform_PreDisplay_Work(Custom_Tracer Tracer)
	    {
	        Tracer.Add_Trace("Aware_JP2_ItemViewer.Perform_PreDisplay_Work", "Check for all the image specification data from the JPEG2000 file");

	        // If no attributes exist, read it from the JPEG2000 file
	        if ((width <= 0) || (height <= 0))
	        {
	            string network_jpeg2000 = UI_ApplicationCache_Gateway.Settings.Image_Server_Network + CurrentItem.Web.AssocFilePath.Replace("/", "\\") + FileName;
	            if (File.Exists(network_jpeg2000))
	            {
	                Tracer.Add_Trace("Aware_JP2_ItemViewer.Add_Nav_Bar_Menu_Section", "Parsing JPEG2000 file for image attributes");
	                get_attributes_from_jpeg2000(network_jpeg2000);
	                Tracer.Add_Trace("Aware_JP2_ItemViewer.Add_Nav_Bar_Menu_Section", "Width=" + width + " and Height=" + height);
	            }
	        }

	        // Determine the zoom levels
	        zoomlevels = zoom_levels();
	        int currentZoom = CurrentMode.Viewport_Zoom.HasValue ? CurrentMode.Viewport_Zoom.Value : 1;
            actualZoomLevel = (zoomlevels - currentZoom + 1);

	        // Now, handle a post back from the thumbnail being clicked
            if ((thumbnailClickX != -100) && (thumbnailClickY != -100))
            {
                long x_value = thumbnailClickX;
                long y_value = thumbnailClickY;

                // Determine the "real" x and y (in terms of large image
                float image_scale = (height) / ((float)width);
                const float THUMBNAIL_SCALE = 300F / 200F;
                if (image_scale < THUMBNAIL_SCALE)
                {
                    // Width restricted
                    float width_scale = (float)width / 200;
                    x_value = (long)(x_value * width_scale);
                    y_value = (long)(y_value * width_scale);
                }
                else
                {
                    // Height restricted
                    float height_scale = (float)height / 300;
                    x_value = (long)(x_value * height_scale);
                    y_value = (long)(y_value * height_scale);
                }

                // Determine the size of the current portal
                int currViewportSize = CurrentMode.Viewport_Size.HasValue ? CurrentMode.Viewport_Size.Value : 1;
                currentZoom = CurrentMode.Viewport_Zoom.HasValue ? CurrentMode.Viewport_Zoom.Value : 1;
                long size_pixels = get_jp2_viewport_size(currViewportSize, currentZoom);


                // Subtract one half of that from the x and y value, so the image is centered
                // on the spot that was clicked upon
                x_value = (long)(x_value - ((0.5F) * (size_pixels)));
                y_value = (long)(y_value - ((0.5F) * (size_pixels)));
                if (x_value < 0)
                    x_value = 0;
                if (y_value < 0)
                    y_value = 0;
                if (x_value > (width) - size_pixels)
                    x_value = ((width) - size_pixels);
                if (y_value > (height) - size_pixels)
                    y_value = ((height) - size_pixels);


                // Assign the computed x and y
                CurrentMode.Viewport_Point_X = (int)x_value;
                CurrentMode.Viewport_Point_Y = (int)y_value;

                // Call the base method
                UrlWriterHelper.Redirect(CurrentMode);
            }
	    }


	    /// <summary> Adds a graphical feature above the image, used for highlighting a portion of the served image </summary>
	    /// <param name="Color"> Color to use for this feature (i.e., Red, Yellow, Blue, White, or Black ) </param>
	    /// <param name="Type"> Type of feature to draw ( i.e., DrawEllipse, FillEllipse, DrawRectangle, FillRectangle )</param>
	    /// <param name="X"> X location on the complete image where the feature should be drawn </param>
	    /// <param name="Y"> Y location on the complete image where the feature should be drawn </param>
	    /// <param name="FeatureWidth"> Width of the feature which should be drawn </param>
	    /// <param name="FeatureHeight"> Height of the feature which should be drawn </param>
	    public void Add_Feature(string Color, string Type, int X, int Y, int FeatureWidth, int FeatureHeight)
	    {
	        featureColor = Color;
	        featureType = Type;
	        featureX = X;
	        featureY = Y;
	        featureWidth = FeatureWidth;
	        featureHeight = FeatureHeight;
	    }

	    public override void Write_Left_Nav_Menu_Section(TextWriter Output, Custom_Tracer Tracer)
        {
	        if (Tracer != null)
	        {
                Tracer.Add_Trace("Aware_JP2_ItemViewer.Write_Left_Nav_Menu_Section", "Adds small thumbnail for image navigation");
	        }

	        string thumnbnail_text = "THUMBNAIL";
	        string click_on_thumbnail_text = "Click on Thumbnail to Recenter Image";

	        if (CurrentMode.Language == Web_Language_Enum.French)
	        {
	            thumnbnail_text = "MINIATURE";
	            click_on_thumbnail_text = "Faites un clic sur la minature pour faire centrer l'image";
	        }

	        if (CurrentMode.Language == Web_Language_Enum.Spanish)
	        {
	            thumnbnail_text = "MINIATURA";
	            click_on_thumbnail_text = "Haga Clic en la Miniatura para centralizar la Imagen";
	        }

	        // Add the HTML to start this menu section
            Output.WriteLine("        <ul class=\"sbkIsw_NavBarMenu\">");
            Output.WriteLine("          <li class=\"sbkIsw_NavBarHeader\"> " + thumnbnail_text + " </li>");
            Output.WriteLine("          <li class=\"sbkIsw_NavBarMenuNonLink\">");

	        // Compute the values needed to create the thumbnail
            int currViewportSize = CurrentMode.Viewport_Size.HasValue ? CurrentMode.Viewport_Size.Value : 1;
	        int size_pixels = 512 + ( currViewportSize * 256 );
	        if ( CurrentMode.Viewport_Size == 3 )
	            size_pixels = 1536;
            ushort currViewRotation = CurrentMode.Viewport_Rotation.HasValue ? CurrentMode.Viewport_Rotation.Value : (ushort) 0;
	        int rotation = ( currViewRotation % 4 ) * 90;

	        // Build the filename
	        string jpeg2000_filename = FileName.Replace(" ", "%20");
	        if ((jpeg2000_filename.Length > 0) && (jpeg2000_filename[0] != '/'))
	        {
	            jpeg2000_filename = CurrentItem.Web.AssocFilePath + FileName.Replace(" ","%20");
	        }

	        // Build the source URL
	        StringBuilder url_builder = new StringBuilder(500);
	        if (string.IsNullOrEmpty(featureType))
	        {
	            url_builder.Append(UI_ApplicationCache_Gateway.Settings.JP2ServerUrl + "thumbnailserver?res=" + (zoom_levels() - CurrentMode.Viewport_Zoom + 1));
	            if (CurrentMode.Viewport_Zoom != 1)
	                url_builder.Append("&viewwidth=" + size_pixels + "&viewheight=" + size_pixels + "&x=" + CurrentMode.Viewport_Point_X + "&y=" + CurrentMode.Viewport_Point_Y);
	            url_builder.Append("&rotation=" + rotation + "&filename=" + jpeg2000_filename + "&maxthumbnailwidth=200&maxthumbnailheight=300");

	        }
	        else
	        {
	            // Determine the actual location on the viewport for the feature
	            url_builder.Append( UI_ApplicationCache_Gateway.Settings.SobekCM_ImageServer + "?z=" + actualZoomLevel + "&w=" + size_pixels + "&h=" + size_pixels);
	            url_builder.Append("&r=" + rotation + "&file=" + jpeg2000_filename);
	            if (CurrentMode.Viewport_Zoom != 1)
	                url_builder.Append("&x=" + CurrentMode.Viewport_Point_X + "&y=" + CurrentMode.Viewport_Point_Y);
	            url_builder.Append("&ax=" + featureX + "&ay=" + featureY + "&at=FillEllipse&ac=" + featureColor + "&aw=12&ah=12");
	            url_builder.Append("&tw=" + width + "&th=" + height + "&type=thumb");
	        }
            
	        // Create the image object
	        if ( CurrentMode.Viewport_Zoom == 1 )
            {
                Output.WriteLine("<img id=\"sbkAj2_Thumb\" src=\"" + url_builder.ToString().Replace("&","&amp;") + "\" alt=\"Navigational Thumbnail\" />");
            }
	        else
	        {
                Output.WriteLine("<img id=\"sbkAj2_ThumbZoomedIn\" src=\"" + url_builder.ToString().Replace("&", "&amp;") + "\" alt=\"Navigational Thumbnail\" />");
	        }
		
	        // Add the HTML to end this menu section
	        if ( CurrentMode.Viewport_Zoom != 1 )
	        {
                Output.WriteLine("          <br />");
	            Output.WriteLine("          " + click_on_thumbnail_text);

	        }

            Output.WriteLine("          </li>");
            Output.WriteLine("        </ul>");


	        //// If this is an aerial index and has latitude and longitude, add it now
	        //if (currentItem.Bib_Info.Type.Type.ToUpper().IndexOf("AERIAL") >= 0)
	        //{
	        //    // See if this page sequence has a polygon matching the page
	        //    matchingPolygon = null;
	        //    foreach (SobekCM.Resource_Object.Bib_Info.Coordinate_Polygon thisPolygon in currentItem.Bib_Info.Coordinates.Polygons)
	        //    {
	        //        if (thisPolygon.Page_Sequence == currentMode.Page)
	        //        {
	        //            matchingPolygon = thisPolygon;
	        //            break;
	        //        }
	        //    }

	        //    // Add the google map here
	        //    if (matchingPolygon != null)
	        //    {
	        //        string coverage_text = "SPATIAL COVERAGE";


	        //        StringBuilder map_builder = new StringBuilder();
	        //        map_builder.AppendLine("        <ul class=\"SobekNavBarMenu\">" + Environment.NewLine + "          <li class=\"SobekNavBarHeader\"> " + coverage_text + " </li>" + Environment.NewLine + "        </ul>");
	        //        map_builder.AppendLine("        <div id=\"map1\" name=\"map1\" style=\"width: 200px; height: 200px\" >&nbsp;</div>");
	        //        System.Web.UI.WebControls.Literal mapLiteral = new System.Web.UI.WebControls.Literal();
	        //        mapLiteral.Text = map_builder.ToString();
	        //        MainPlaceHolder.Controls.Add(mapLiteral);
	        //    }
				
	        //}
	    }

	    private int get_jp2_viewport_size( int size_scale,  int zoom_scale )
	    {
	        int viewport_size_pixels = 512 + ( size_scale * 256 );
	        if ( size_scale == 3 )
	            viewport_size_pixels = 1536;
	        return viewport_size_pixels * (int)Math.Pow(2, (zoomlevels - zoom_scale));
	    }

	    private int adjust_x( int current_x, int current_y, int current_jp2_viewport_size, int new_jp2_viewport_size )
	    {
	        // If this is currently 0 (and y is zero) return 0
	        if (( current_x == 0 ) && ( current_y == 0 ))
	            return 0;

	        // Determine the center x value
	        int new_x = current_x + (int) ( 0.5F * current_jp2_viewport_size );
	        new_x = new_x - (int) ( 0.5F * new_jp2_viewport_size );
	        if (( CurrentMode.Viewport_Rotation % 2 ) == 0 )
	        {
	            if ( new_x +  new_jp2_viewport_size > width )
	                new_x = width - new_jp2_viewport_size;
	        }
	        else
	        {
	            if ( new_x +  new_jp2_viewport_size > height )
	                new_x = height - new_jp2_viewport_size;
	        }
	        if ( new_x < 0 )
	            new_x = 0;
	        return new_x;
	    }

	    private int adjust_y( int current_x, int current_y, int current_jp2_viewport_size, int new_jp2_viewport_size )
	    {
	        // If this is currently 0 (and x is zero) return 0
	        if (( current_x == 0 ) && ( current_y == 0 ))
	            return 0;

	        // Determine the center x value
	        int new_y = current_y + (int) ( 0.5F * current_jp2_viewport_size );
	        new_y = new_y - (int) ( 0.5F * new_jp2_viewport_size );

	        if (( CurrentMode.Viewport_Rotation % 2 ) == 0 )
	        {
	            if ( new_y +  new_jp2_viewport_size > height )
	                new_y = height - new_jp2_viewport_size;
	        }
	        else
	        {
	            if ( new_y +  new_jp2_viewport_size > width )
	                new_y = width - new_jp2_viewport_size;
	        }

	        if ( new_y < 0 )
	            new_y = 0;
	        return new_y;
	    }

	    /// <summary> Adds the main view section to the page turner </summary>
	    /// <param name="MainPlaceHolder"> Main place holder ( &quot;mainPlaceHolder&quot; ) in the itemNavForm form into which the bulk of the item viewer's output is displayed</param>
	    /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
	    public override void Add_Main_Viewer_Section(PlaceHolder MainPlaceHolder, Custom_Tracer Tracer)
	    {
	        if (Tracer != null)
	        {
	            Tracer.Add_Trace("Aware_JP2_ItemViewer.Add_Main_Viewer_Section", "Adds controls to allow detection of clicks in JPEG2000 image");
	        }

			

	        // Build the value
	        //StringBuilder builder = new StringBuilder();
            int currViewportSize = CurrentMode.Viewport_Size.HasValue ? CurrentMode.Viewport_Size.Value : 1;
	        int size_pixels = 512 + ( currViewportSize * 256 );
	        if ( CurrentMode.Viewport_Size == 3 )
	            size_pixels = 1536;
            ushort currViewRotation = CurrentMode.Viewport_Rotation.HasValue ? CurrentMode.Viewport_Rotation.Value : (ushort)0;
	        int rotation = ( currViewRotation % 4 ) * 90;
	        //builder.Append( "\t\t<td align=\"center\" colspan=\"3\">" + Environment.NewLine );

	        if ( FileName.Length == 0 )
	        {
	            // Add the HTML for the error
	            Literal errorLiteral = new Literal
	                                       { Text ="\t\t<td style=\"text-align:center;\">" + Environment.NewLine +"<strong>JPEG2000 IMAGE NOT FOUND IN DATABASE!</strong>" +Environment.NewLine + "\t\t</td>" + Environment.NewLine };
	            MainPlaceHolder.Controls.Add( errorLiteral );
	        }
	        else
	        {			
	            // Add the HTML to start this
                Literal startLiteral = new Literal { Text = "<td>" + NavigationRow + "</td></tr><tr>\t\t<td style=\"text-align:center;\" id=\"sbkAj2_ImageTd\">" + Environment.NewLine };
	            MainPlaceHolder.Controls.Add( startLiteral );

                // Build the filename
	            string jpeg2000_filename = FileName.Replace(" ","%20");
	            if ((jpeg2000_filename.Length > 0) && (jpeg2000_filename[0] != '/'))
	            {
	                jpeg2000_filename = CurrentItem.Web.AssocFilePath + FileName.Replace(" ", "%20");
	            }
				
	            // Build the source URL
	            StringBuilder url_builder = new StringBuilder(500);
                int currentZoom = CurrentMode.Viewport_Zoom.HasValue ? CurrentMode.Viewport_Zoom.Value : 1;
	            int actual_zoom_level = (zoomlevels - currentZoom + 1);
	            if ((string.IsNullOrEmpty(featureType) || ( actual_zoom_level == 1 )))
	            {
	                url_builder.Append(UI_ApplicationCache_Gateway.Settings.JP2ServerUrl + "imageserver?res=" + actual_zoom_level + "&viewwidth=" + size_pixels + "&viewheight=" + size_pixels);
	                if (CurrentMode.Viewport_Zoom != 1)
	                    url_builder.Append("&x=" + CurrentMode.Viewport_Point_X + "&y=" + CurrentMode.Viewport_Point_Y);
	                url_builder.Append("&rotation=" + rotation + "&filename=" + jpeg2000_filename);
	            }
	            else
	            {
	                // Determine the actual location on the viewport for the feature
	                url_builder.Append(UI_ApplicationCache_Gateway.Settings.SobekCM_ImageServer + "?z=" + actual_zoom_level + "&w=" + size_pixels + "&h=" + size_pixels);
	                url_builder.Append("&r=" + rotation + "&file=" + jpeg2000_filename);
	                if (CurrentMode.Viewport_Zoom != 1)
	                    url_builder.Append("&x=" + CurrentMode.Viewport_Point_X + "&y=" + CurrentMode.Viewport_Point_Y);
	                url_builder.Append("&ax=" + featureX + "&ay=" + featureY + "&at=" + featureType + "&ac=" + featureColor + "&aw=" + featureWidth + "&ah=" + featureHeight);
	                url_builder.Append("&tw=" + width + "&th=" + height + "&op=0.5");
	            }


	            if ( CurrentMode.Viewport_Zoom != 1 )
	            {
	                // Create the image object
                    ImageButton mainImage = new ImageButton { CssClass = "sbkAj2_Image", ImageUrl = url_builder.ToString() };
	                mainImage.Height = size_pixels;
	                mainImage.BackColor = Color.White;
	                mainImage.Click +=mainImage_Click;
	                mainImage.AlternateText = "Main Image";
	                switch( CurrentMode.Language )
	                {
	                    case Web_Language_Enum.French:
	                        mainImage.ToolTip = "Faites un clic sur l'image pour la centrer";
	                        break;

	                    case Web_Language_Enum.Spanish:
	                        mainImage.ToolTip = "Haga Clic en la imagen para Centralizar";
	                        break;

	                    default:
	                        mainImage.ToolTip = "Click on image to center";
	                        break;
	                }
	                MainPlaceHolder.Controls.Add( mainImage );
	            }
	            else
	            {
	                // Create the image object
	                Image mainImage2 = new Image
	                                       { CssClass="sbkAj2_Image", ImageUrl = url_builder.ToString(), AlternateText = "Main Image" };
	                MainPlaceHolder.Controls.Add( mainImage2 );
	            }


	            // Add the HTML to end this
	            Literal endLiteral = new Literal {Text = "\t\t</td>" + Environment.NewLine};
	            MainPlaceHolder.Controls.Add( endLiteral );
	        }
	    }

		private ushort zoom_levels()
		{
			// Get the current portal size in pixels
            int currViewportSize = CurrentMode.Viewport_Size.HasValue ? CurrentMode.Viewport_Size.Value : 1;
            float size_pixels = 512 + (currViewportSize * 256);
			if ( CurrentMode.Viewport_Size == 3 )
				size_pixels = 1536;

			// Get the factor 
			float width_factor = (width) / size_pixels;
			float height_factor = (height) / size_pixels;
			float max_factor = Math.Max( width_factor, height_factor );

			// Return the zoom level
			if (( max_factor > 1 ) && ( max_factor <= 2 ))
				return 2;
			if (( max_factor > 2 ) && ( max_factor <= 4 ))
				return 3;
			if (( max_factor > 4 ) && ( max_factor <= 8 ))
				return 4;
			if ( max_factor > 8 )
				return 5;

			// If it made it here, image must be very small!
			return 1;
		}

		private void mainImage_Click(object sender, ImageClickEventArgs e)
		{
			// Determine the size of the current portal
            int currViewportSize = CurrentMode.Viewport_Size.HasValue ? CurrentMode.Viewport_Size.Value : 1;
		    int currentZoom = CurrentMode.Viewport_Zoom.HasValue ? CurrentMode.Viewport_Zoom.Value : 1;
            int size_pixels = get_jp2_viewport_size(currViewportSize, currentZoom);

			// Get the image dimensions
            int image_size = 512 + (currViewportSize * 256);
			if ( CurrentMode.Viewport_Size == 3 )
				image_size = 1536;

			// Get the click values
			int x = (int) (((float) e.X / image_size ) * size_pixels);
			int y = (int) (((float) e.Y / image_size ) * size_pixels);

			// Re-center
            int x_current = CurrentMode.Viewport_Point_X.HasValue ? CurrentMode.Viewport_Point_X.Value : 0;
            int y_current = CurrentMode.Viewport_Point_Y.HasValue ? CurrentMode.Viewport_Point_Y.Value : 0;
            int x_value = x_current + (x - (size_pixels / 2));
            int y_value = y_current + (y - (size_pixels / 2));

			// Make sure this doesn't violate anything
			if ( x_value < 0 )
				x_value = 0;
			if ( y_value < 0 )
				y_value = 0;
			if ( x_value > width - size_pixels )
				x_value = width - size_pixels;
			if ( y_value > height - size_pixels )
				y_value = height - size_pixels;

			// Assign the computed x and y
			CurrentMode.Viewport_Point_X = x_value;
			CurrentMode.Viewport_Point_Y = y_value;

			// Call the base method
            UrlWriterHelper.Redirect(CurrentMode);
		}

	    #region Method to actually compute the JPEG2000 attributes by reading the file 

	    private void get_attributes_from_jpeg2000(string file)
	    {
	        try
	        {
	            // Get the height and width of this JPEG file
	            FileStream reader = new FileStream(file, FileMode.Open, FileAccess.Read);
	            int[] previousValues = new[] { 0, 0, 0, 0 };
	            int bytevalue = reader.ReadByte();
	            int count = 1;
	            while (bytevalue != -1)
	            {
	                // Move this value into the array
	                previousValues[0] = previousValues[1];
	                previousValues[1] = previousValues[2];
	                previousValues[2] = previousValues[3];
	                previousValues[3] = bytevalue;

	                // Is this IHDR?
	                if ((previousValues[0] == 105) && (previousValues[1] == 104) &&
	                    (previousValues[2] == 100) && (previousValues[3] == 114))
	                {
	                    break;
	                }

	                // Is this the first four bytes and does it match the output from Kakadu 3-2?
	                if ((count == 4) && (previousValues[0] == 255) && (previousValues[1] == 79) && (previousValues[2] == 255) && (previousValues[3] == 81))
	                {
	                    reader.ReadByte();
	                    reader.ReadByte();
	                    reader.ReadByte();
	                    reader.ReadByte();
	                    break;
	                }
	                
                    // Read the next byte
	                bytevalue = reader.ReadByte();
	                count++;
	            }

	            // Now, read ahead for the height and width
	            height = (ushort)((((((reader.ReadByte() * 256) + reader.ReadByte()) * 256) + reader.ReadByte()) * 256) + reader.ReadByte());
	            width = (ushort)((((((reader.ReadByte() * 256) + reader.ReadByte()) * 256) + reader.ReadByte()) * 256) + reader.ReadByte());
	            reader.Close();

	            return;
	        }
	        catch
	        {
	            return;
	        }
	    }

	    #endregion

        public override List<HtmlSubwriter_Behaviors_Enum> ItemViewer_Behaviors
        {
            get
            {
                return new List<HtmlSubwriter_Behaviors_Enum>() { HtmlSubwriter_Behaviors_Enum.Item_Subwriter_Requires_Left_Navigation_Bar };
            }
        }
	}
}
