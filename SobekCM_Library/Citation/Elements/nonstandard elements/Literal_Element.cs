﻿#region Using directives

using System;
using System.IO;
using System.Text;
using System.Xml;
using SobekCM.Core.ApplicationState;
using SobekCM.Core.Configuration;
using SobekCM.Core.Users;
using SobekCM.Resource_Object;

#endregion

namespace SobekCM.Library.Citation.Elements
{
	/// <summary> Element is used to write a html literal into the form </summary>
	/// <remarks> This class implements the <see cref="iElement"/> interface and extends the <see cref="abstract_Element"/> class. </remarks>
	public class Literal_Element : abstract_Element
	{
		private string html_text;

		/// <summary> Constructor for a new instance of the Literal_Element class </summary>
		public Literal_Element()
		{
			html_text = String.Empty;
		}

		/// <summary> Prepares the bib object for the save, by clearing any existing data in this element's related field(s) </summary>
		/// <param name="Bib"> Existing digital resource object which may already have values for this element's data field(s) </param>
		/// <param name="Current_User"> Current user, who's rights may impact the way an element is rendered </param>
		/// <remarks> This does nothing </remarks>
		public override void Prepare_For_Save(SobekCM_Item Bib, User_Object Current_User)
		{
			// do nothing
		}

		/// <summary> Saves the data rendered by this element to the provided bibliographic object during postback </summary>
		/// <param name="Bib"> Object into which to save the user's data, entered into the html rendered by this element </param>
		public override void Save_To_Bib(SobekCM_Item Bib)
		{
			// do nothing
		}

		public override void Render_Template_HTML(TextWriter Output, SobekCM_Item Bib, string Skin_Code, bool IsMozilla, StringBuilder PopupFormBuilder, User_Object Current_User, Web_Language_Enum CurrentLanguage, Language_Support_Info Translator, string Base_URL)
		{
			Output.WriteLine("  <!-- Literal Element -->");
			Output.WriteLine("  <tr>");
			Output.WriteLine("    <td style=\"width:" + LEFT_MARGIN + "px\">&nbsp;</td>");
			Output.WriteLine("    <td colspan=\"2\">" + html_text + "</td>");
			Output.WriteLine("  </tr>");
		}

		protected override void Inner_Read_Data(XmlTextReader XMLReader)
		{
			while (XMLReader.Read())
			{
				if ((XMLReader.NodeType == XmlNodeType.Element) && (XMLReader.Name.ToLower() == "value"))
				{
					XMLReader.Read();
					html_text = XMLReader.Value.Trim();
				}
			}
		}
	}
}
