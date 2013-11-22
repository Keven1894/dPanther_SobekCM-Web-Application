﻿#region Using directives
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SobekCM.Library.Application_State;
using SobekCM.Library.Navigation;
using SobekCM.Library.Users;
using SobekCM.Resource_Object;
using System.Collections.Generic;
#endregion

namespace SobekCM.Library.MySobekViewer
{
    class Track_Item_MySobekViewer : abstract_MySobekViewer
    {
       private Dictionary<string, User_Object> user_list;
        private List<string> scanners_list;
        private  string barcodeString;
        private  int itemID;
        private string encodedItemID;
        private string checksum;
        private string BibID;
        private string VID;
        private string error_message = String.Empty;
        private int stage=1;
        private string hidden_request ;
        private string hidden_value;
        private string title;
        private string equipment;

        private string start_Time;
        private string end_Time;
        
        private DateTime this_workflow_date;

        private DataTable tracking_users;
//        private DataTable workflow_entries_from_DB;
  //      private Dictionary<string, DataRow> current_entries;
        private DataTable open_workflows_from_DB;
       
        private Dictionary<string,Tracking_Workflow> current_workflows;
        private User_Object current_selected_user;

        /// <summary> Constructor for a new instance of the Track_Item_MySobekViewer class </summary>
        /// <param name="User"> Authenticated user information </param>
        /// <param name="Current_Mode"> Mode / navigation information for the current request</param>
        /// <param name="Tracer">Trace object keeps a list of each method executed and important milestones in rendering</param>
        public Track_Item_MySobekViewer(User_Object User, SobekCM_Navigation_Object Current_Mode, Custom_Tracer Tracer) 
            :  base(User)
          {
                    Tracer.Add_Trace("Track_Item_MySobekViewer.Constructor", String.Empty);

                    currentMode = Current_Mode;
                    

                     //If there is no user, go back
                    if (user == null)
                    {
                        currentMode.My_Sobek_Type = My_Sobek_Type_Enum.Home;
                        HttpContext.Current.Response.Redirect(currentMode.Redirect_URL());
                    }

     
            //Initialize variables
            tracking_users = new DataTable();
            user_list = new Dictionary<string, User_Object>();
            scanners_list = new List<string>();

 
            //Get the list of users who are possible Scanning/Processing technicians from the DB
            tracking_users = Database.SobekCM_Database.Tracking_Get_Users_Scanning_Processing();
            
            foreach (DataRow row in tracking_users.Rows)
            {
                User_Object temp_user = new User_Object();
                temp_user.UserName = row["UserName"].ToString();
                temp_user.Given_Name = row["FirstName"].ToString();
                temp_user.Family_Name = row["LastName"].ToString();
                temp_user.Email = row["EmailAddress"].ToString();
                user_list.Add(temp_user.UserName, temp_user);
            }
           
            if(!user_list.ContainsKey(User.UserName))
                user_list.Add(User.UserName, User);

            //Get the list of scanning equipment
            DataTable scanners = new DataTable();
            scanners = Database.SobekCM_Database.Tracking_Get_Scanners_List();
            foreach (DataRow row in scanners.Rows)
            {
                scanners_list.Add(row["ScanningEquipment"].ToString());
            }

            //See if there were any hidden requests
            hidden_request = HttpContext.Current.Request.Form["Track_Item_behaviors_request"] ?? String.Empty;
            hidden_value = HttpContext.Current.Request.Form["Track_Item_hidden_value"] ?? String.Empty;


            //Get the equipment value
            if (HttpContext.Current.Session["Equipment"]!=null && !String.IsNullOrEmpty(HttpContext.Current.Session["Equipment"].ToString()))
                equipment = HttpContext.Current.Session["Equipment"].ToString();

            else
            {
                equipment = scanners_list[0];
                HttpContext.Current.Session["Equipment"] = equipment;
            }
            //Check the hidden value if equipment was previously changed
            if (!String.IsNullOrEmpty(HttpContext.Current.Request.Form["hidden_equipment"]))
            {
                equipment = HttpContext.Current.Request.Form["hidden_equipment"];
                HttpContext.Current.Session["equipment"] = equipment;
            }


            //Also get the currently selected user
            if (HttpContext.Current.Session["Selected_User"] != null)
                current_selected_user = (User_Object)HttpContext.Current.Session["Selected_User"];

            else
            {
                current_selected_user = User;
                HttpContext.Current.Session["Selected_User"] = current_selected_user;
            }
            //Check the hidden value if equipment was previously changed
            if (!String.IsNullOrEmpty(HttpContext.Current.Request.Form["hidden_selected_username"]) )
            {
               current_selected_user = new User_Object();
                string temp = HttpContext.Current.Request.Form["hidden_selected_username"];
                current_selected_user = user_list[temp];

                HttpContext.Current.Session["Selected_User"] = current_selected_user;
            }


                
            //If there is a valid item currently selected
            if (!String.IsNullOrEmpty(BibID) && !String.IsNullOrEmpty(VID))
            {
                //Get the the form field values
                start_Time = Convert.ToDateTime(HttpContext.Current.Request.Form["txtStartTime"]).ToString("HH:mm");
                end_Time = Convert.ToDateTime(HttpContext.Current.Request.Form["txtEndTime"]).ToString("HH:mm");
                this_workflow_date = Convert.ToDateTime(HttpContext.Current.Request.Form["txtStartDate"]);
            }

            //         hidden_entry_type = HttpContext.Current.Request.Form[""]
            switch (hidden_request.ToLower())
            {
                case "decode_barcode":
                    barcodeString = hidden_value;
                    //Decode the scanned barcode 
                    if (!String.IsNullOrEmpty(barcodeString))
                    {
                        //Find a match for a number within the string, which corresponds to the event
                        Match val = Regex.Match(barcodeString, @"\d");
                        if (val.Success)
                        {
                            int len = barcodeString.IndexOf(val.Value);
                            Int32.TryParse(val.Value, out stage);

                            //Extract the item ID & checksum from the barcode string
                            encodedItemID = barcodeString.Substring(0, len);
                            checksum = barcodeString.Substring(len + 1, (barcodeString.Length - len - 1));

                            //Verify that the checksum is valid
                            bool isValidChecksum = Is_Valid_Checksum(encodedItemID, val.Value, checksum);
                            if (!isValidChecksum)
                                error_message = "Barcode error- checksum error";

                            //Save the item information for this itemID
                            bool IsValidItem = Get_Item_Info_from_Barcode(encodedItemID);
                            if (!IsValidItem)
                                error_message = "Barcode error - invalid ItemID referenced";
                            else
                            {
                                Get_Bib_VID_from_ItemID(itemID);
                            }

                        }
                    }
                    break;

                case "read_manual_entry":
                    //Get the related hidden values for the selected manual entry fields
                    string hidden_bibID = HttpContext.Current.Request.Form["hidden_BibID"] ?? String.Empty;
                    string hidden_VID =  HttpContext.Current.Request.Form["hidden_VID"] ?? String.Empty;
                    string hidden_event_num = HttpContext.Current.Request.Form["hidden_event_num"] ?? String.Empty;
                    if (String.IsNullOrEmpty(hidden_bibID) || String.IsNullOrEmpty(hidden_VID) || String.IsNullOrEmpty(hidden_event_num))
                    {
                        error_message = "You must enter a BibID and a VID!";
                    }
                    else
                    {
                        Int32.TryParse(hidden_event_num, out stage);
                        BibID = hidden_bibID;
                        VID = hidden_VID;
                        try
                        {
                            itemID = Resource_Object.Database.SobekCM_Database.Get_ItemID(BibID, VID);
                            Get_Bib_VID_from_ItemID(itemID);
                        }
                        catch (Exception ee)
                        {
                            error_message = "Invalid BibID or VID!";
                        }


                    }
                    break;

                default:
                    break;
               }

            //If this is the start of a workflow, check if there is an already openend workflow for the same user, item
            if (!String.IsNullOrEmpty(itemID.ToString()) && itemID != 0)
            {
                //If this is the start of a workflow
   //             if (stage == 1 || stage == 3)
  //              {
                    DataView temp_open_workflows_all_users = new DataView(Database.SobekCM_Database.Tracking_Get_Open_Workflows(itemID,stage));
                    //string rowFilter = "WorkPerformedBy=" + User.Email;
    //                temp_open_workflows_all_users.RowFilter = rowFilter;

                    //Filter the open workflows associated with the currently selected user
                    open_workflows_from_DB = temp_open_workflows_all_users.ToTable().Clone();

                    foreach (DataRowView rowView in temp_open_workflows_all_users)
                    {
                        DataRow newRow = open_workflows_from_DB.NewRow();
                        newRow.ItemArray = rowView.Row.ItemArray;
                        string username_column = rowView["WorkPerformedBy"].ToString();
                        if (username_column == current_selected_user.Email)
                            open_workflows_from_DB.Rows.Add(newRow);
                    }
                    
     
    //            }

    //            int row_count = open_workflows_from_DB.Rows.Count;
 
            }

            //If a valid Bib, VID workflow was entered, add this to the current session
            if (String.IsNullOrEmpty(error_message) && !String.IsNullOrEmpty(BibID) && !String.IsNullOrEmpty(VID))
            {
                Add_New_Workflow();
            }

          }


       /// <summary>
        /// Add new workflow to the dictionary and the session of all current workflows
       /// </summary>
        private void Add_New_Workflow()
        {
           //Fetch this dictionary from the session if present
            current_workflows = (HttpContext.Current.Session["Tracking_Current_Workflows"]) as Dictionary<string, Tracking_Workflow>;
            
            //else create a new one
            if (current_workflows == null)
            {
                current_workflows = new Dictionary<string, Tracking_Workflow>();
            }

            Tracking_Workflow this_workflow = new Tracking_Workflow();
            this_workflow.BibID = BibID;
            this_workflow.VID = VID;
            this_workflow.Equipment = equipment;
            this_workflow.thisUser = current_selected_user;
           

            //Add the date and time
            string currentTime = DateTime.Now.ToString("HH:mm");


            if (stage == 1 || stage == 3)
            {
                start_Time = currentTime;
                end_Time = null;
                if (open_workflows_from_DB == null || open_workflows_from_DB.Rows.Count > 0)
                    return;
            }
            else if (stage == 2 || stage == 4)
            {
               
                    start_Time = null;
                    end_Time = currentTime;
                    if (open_workflows_from_DB != null && open_workflows_from_DB.Rows.Count > 0)
                    {
                        int count = 0;
                        foreach (DataRow row in open_workflows_from_DB.Rows)
                        {
                            if (Convert.ToDateTime(row["DateStarted"]).ToString("yyyy-MM-dd") == DateTime.Now.ToString("yyyy-MM-dd"))
                            {
                                count++;
                                if (count > 1)
                                {
                                    error_message = "More than one unclosed workflow entries for today!";
                                    return;
                                }
                                start_Time = Convert.ToDateTime(row["DateStarted"]).ToString("HH:mm");
                            }
                        }
                    }
                    else
                    {
                        return;
                    }

              }

            this_workflow.StartTime = start_Time;
            this_workflow.EndTime = end_Time;
            this_workflow.Date = DateTime.Now.ToString("yyyy-MM-dd");

            this_workflow.itemID = itemID;
            this_workflow.Saved = false;
            this_workflow.Title = title;
            this_workflow.Workflow_type = stage;
           int WorkflowID;

            string key = itemID + stage + current_selected_user.Email;
            if (!current_workflows.ContainsKey(key))
            {
   //             WorkflowID = Database.SobekCM_Database.Tracking_Save_New_Workflow(this_workflow.itemID, this_workflow.Date, this_workflow.)
                current_workflows.Add(key,this_workflow);

            }

            //Save the dictionary back to the session
            HttpContext.Current.Session["Tracking_Current_Workflows"] = current_workflows;

        }


        /// <summary> Get the item BibID, VID, title from the ItemID </summary>
        /// <param name="item_ID"></param>
        private void Get_Bib_VID_from_ItemID(int item_ID)
        {
        
            DataRow temp = Database.SobekCM_Database.Tracking_Get_Item_Info_from_ItemID(item_ID);
            BibID = temp["BibID"].ToString();
            VID = temp["VID"].ToString();
            title = temp["Title"].ToString();
            if (String.IsNullOrEmpty(BibID) || String.IsNullOrEmpty(VID))
                error_message = "No matching item found for this ItemID!";

        }


        /// <summary> Validate the checksum on the barcode value </summary>
        /// <param name="encoded_ItemID">The itemID in Base-26 format</param>
        /// <param name="Stage">Indicates the event boundary</param>
        /// <param name="checksum_string">The checksum value generated for this barcode</param>
        /// <returns>Returns TRUE if the checksum is valid, else FALSE</returns>
        private bool Is_Valid_Checksum(string encoded_ItemID, string Stage, string checksum_string)
        {
            bool is_valid_checksum = true;
            int event_num=0;
            int thisItemID = 0;
            int thisChecksumValue = 0;
            
            Int32.TryParse(Stage, out event_num);
            thisItemID = Int_from_Base26(encoded_ItemID);
            thisChecksumValue = Int_from_Base26(checksum_string);

            if (thisChecksumValue != (thisItemID + event_num)%26)
                is_valid_checksum = false;
 
            return is_valid_checksum;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encoded_ItemID"></param>
        /// <returns></returns>
        private bool Get_Item_Info_from_Barcode(string encoded_ItemID)
        {
            bool result = true;
            itemID = Int_from_Base26(encoded_ItemID);
            if (String.IsNullOrEmpty(itemID.ToString()))
                result = false;
            return result;
        }

       
        /// <summary> Converts a Base-26 value to the Base-10 equivalent </summary>
        /// <param name="number">The number in Base-26</param>
        /// <returns>The converted Base-10 equivalent</returns>
        private  int Int_from_Base26(String number)
        {
            int convertedNumber = 0;
            if (!String.IsNullOrEmpty(number))
            {
                convertedNumber = (number[0] - 'A');
                for (int i = 1; i < number.Length; i++)
                {
                    convertedNumber *= 26;
                    convertedNumber += (number[i] - 'A');
                }
            }
            return convertedNumber;
        }


        /// <summary> Title for the page that displays this viewer, this is shown in the search box at the top of the page, just below the banner </summary>
        /// <value> This returns the value 'Track Item'</value>
        public override string Web_Title
        {
            get
            {
                return "Item Tracking";
            }
        }

        
        /// <summary> Add the HTML to be displayed in the main SobekCM viewer area </summary>
        /// <param name="Output"> Textwriter to write the HTML for this viewer</param>
        /// <param name="Tracer">Trace object keeps a list of each method executed and important milestones in rendering</param>
        /// <remarks> This class does nothing, since the individual metadata elements are added as controls, not HTML </remarks>
        public override void Write_HTML(TextWriter Output, Custom_Tracer Tracer)
        {
            Tracer.Add_Trace("Track_Item_MySobekViewer.Write_HTML", "Do nothing");

            //Include the js files
			Output.WriteLine("<script type=\"text/javascript\" src=\"" + currentMode.Base_URL + "default/scripts/jquery/jquery-ui-1.10.3.custom.min.js\"></script>");
            Output.WriteLine("<script type=\"text/javascript\" src=\"" + currentMode.Base_URL + "default/scripts/jquery/jquery.color-2.1.1.js\"></script>");
            Output.WriteLine("<script type=\"text/javascript\" src=\"" + currentMode.Base_URL + "default/scripts/jquery/jquery.timers.min.js\"></script>");
            Output.WriteLine("<script type=\"text/javascript\" src=\"" + currentMode.Base_URL + "default/scripts/sobekcm_track_item.js\" ></script>");
            Output.WriteLine("  <link rel=\"stylesheet\" type=\"text/css\" href=\"" + currentMode.Base_URL + "default/jquery-ui.css\" />");
        }


        public override void Add_Controls(PlaceHolder MainPlaceHolder, Custom_Tracer Tracer)
        {
            Tracer.Add_Trace("Track_Item_MySobekViewer.Add_Controls", "");
         //   base.Add_Controls(MainPlaceHolder, Tracer);

            string barcode_row_style = String.Empty;
            string manual_row_style = String.Empty;

            StringBuilder builder = new StringBuilder(2000);
            builder.AppendLine("<!-- Track_Item_MySobekViewer.Add_Controls -->");
            builder.AppendLine("  <link rel=\"stylesheet\" type=\"text/css\" href=\"" + currentMode.Base_URL + "default/SobekCM_MySobek.css\" /> ");
            builder.AppendLine("  <link rel=\"stylesheet\" type=\"text/css\" href=\"" + currentMode.Base_URL + "default/SobekCM_Admin.css\" /> ");
            builder.AppendLine("<div class=\"SobekHomeText\">");

            //Add the hidden variables
            builder.AppendLine("<!-- Hidden field is used for postbacks to add new form elements (i.e., new page, etc..) -->");
            builder.AppendLine("<input type=\"hidden\" id=\"Track_Item_behaviors_request\" name=\"Track_Item_behaviors_request\" value=\"\"/>");
            builder.AppendLine("<input type=\"hidden\" id=\"Track_Item_hidden_value\" name=\"Track_Item_hidden_value\"value=\"\"/>");
            builder.AppendLine("<input type=\"hidden\" id=\"TI_entry_type\" name=\"TI_entry_type\" value=\"\"/>");
            builder.AppendLine("<input type=\"hidden\" id=\"hidden_BibID\" name=\"hidden_BibID\" value=\"\"/>");
            builder.AppendLine("<input type=\"hidden\" id=\"hidden_VID\" name=\"hidden_VID\" value=\"\" />");
            builder.AppendLine("<input type=\"hidden\" id=\"hidden_event_num\" name=\"hidden_event_num\" value=\"\" />");
            builder.AppendLine("<input type=\"hidden\" id=\"hidden_equipment\" name=\"hidden_equipment\" value=\"\"/>");
            builder.AppendLine("<input type=\"hidden\" id=\"hidden_selected_username\" name=\"hidden_selected_username\" value=\"\"/>");
  //          builder.AppendLine("<input type=\"hidden\" id=\"hidden_selected_userEmail\" name=\"hidden_selected_userEmail\" value=\"\"/>");

            //Start the User, Equipment info table
            builder.AppendLine("<span class=\"sbkTi_HomeText\"><h2>User and Equipment</h2></span>");
            builder.AppendLine("<table class=\"sbkTi_table\">");
            builder.AppendLine("<tr>");
            builder.AppendLine("          <td>Scanned/Processed by:</td>");
            builder.AppendLine("          <td><select id=\"ddlUserStart\" name=\"ddlUserStart\" onchange=\"ddlUser_Changed(this.id);\">");

            //Add the list of users to the dropdown list
            foreach (KeyValuePair<string, User_Object> thisUser in user_list)
            {
                if (thisUser.Key == current_selected_user.UserName)
                    builder.AppendLine("<option value=\"" + thisUser.Key + "\" selected>" + thisUser.Value.Full_Name + "</option>");
                else
                {
                    builder.AppendLine("<option value=\"" + thisUser.Key + "\">" + thisUser.Value.Full_Name + "</option>");
                }
            }
            builder.AppendLine("</td>");
            builder.AppendLine("           <td>Equipment used:</td>");
            builder.AppendLine("           <td><select name=\"ddlEquipmentStart\" id=\"ddlEquipmentStart\" onchange=\"ddlEquipment_Changed(this.id);\">");

  
            //Add the list of scanners to the dropdown list
            foreach (string thisScanner in scanners_list)
            {
                if(thisScanner==equipment)
                    builder.AppendLine("<option value=\"" + thisScanner + "\" selected>"+thisScanner+"</option>");
                else
                    builder.AppendLine("<option value=\"" + thisScanner + "\">" + thisScanner + "</option>");
            }
            builder.AppendLine("</select></td>");
            builder.AppendLine("</tr>");
            builder.AppendLine("</table>");


            //Start the Item Information Table
            string bibid = (String.IsNullOrEmpty(BibID)) ? String.Empty : BibID;
            string vid = (String.IsNullOrEmpty(VID)) ? String.Empty : VID;

            builder.AppendLine("<span class=\"sbkTi_HomeText\"><h2>Item Information</h2></span>");
            builder.AppendLine("<table class=\"sbkTi_table\">");
    
            //Add the item info section
            if (hidden_request == "read_manual_entry")
            {
                builder.AppendLine("<tr><td colspan=\"100%\"><input type=\"radio\" name=\"rbEntryType\" id=\"rb_barcode\" value=0 onclick=\"rbEntryTypeChanged(this.value);\">Barcode Entry</td></tr>");
                barcode_row_style = "style=\"margin-left:200px\";";

                builder.AppendLine("<tr id=\"tblrow_Barcode\" " + barcode_row_style + "><td></td><td>Scan barcode here:</td>");
                builder.AppendLine("         <td colspan=\"3\"><input type=\"text\" id=\"txtScannedString\" name=\"txtScannedString\" autofocus onchange=\"BarcodeStringTextbox_Changed(this.value);\"/></td>");
                builder.AppendLine("<td>");
                builder.AppendLine("<div id=\"divAddButton_barcode\" style=\"float:right;\">");
                builder.AppendLine("    <button title=\"Add new tracking entry\" class=\"sbkMySobek_RoundButton\" onclick=\"Add_new_entry_barcode(); return false;\">ADD</button>");
                builder.AppendLine("</div></td></tr>");

                builder.AppendLine("<td colspan=\"100%\"><input type=\"radio\" name=\"rbEntryType\" id=\"rb_manual\" value=1 checked onclick=\"rbEntryTypeChanged(this.value);\">Manual Entry</td></tr>");
                builder.AppendLine("<tr id=\"tblrow_Manual1\" " + manual_row_style + "><td></td><td>BibID:</td><td><input type=\"text\" id=\"txtBibID\" value=\"" + bibid + "\" /></td>");
                builder.AppendLine("         <td>VID:</td><td><input type=\"text\" id=\"txtVID\" value=\"" + vid + "\" /></td>");
                builder.AppendLine("</tr>");
                builder.AppendLine("<tr id=\"tblrow_Manual2\" " + manual_row_style + ">");
                builder.AppendLine("<td></td><td>Event:</td><td><select id=\"ddlManualEvent\" name=\"ddlManualEvent\">");
                builder.AppendLine("                                       <option value=\"1\" selected>Start Scanning</option>");
                builder.AppendLine("                                        <option value=\"2\">End Scanning</option>");
                builder.AppendLine("                                        <option value=\"3\">Start Processing</option>");
                builder.AppendLine("                                        <option value=\"4\">End Processing</option></select>");
                builder.AppendLine("</td>");

                //Call the JavaScript functions to apply the appropriate CSS class for the disabled row(s)
                builder.AppendLine("<script type=\"text/javascript\">DisableRow_SetCSSClass('tblrow_Barcode');</script>");
                builder.AppendLine("<script type=\"text/javascript\">DisableRow_RemoveCSSClass('tblrow_Manual1');</script>");
                builder.AppendLine("<script type=\"text/javascript\">DisableRow_RemoveCSSClass('tblrow_Manual2');</script>");

            }
            else
            {
                builder.AppendLine("<tr><td colspan=\"100%\"><input type=\"radio\" name=\"rbEntryType\" id=\"rb_barcode\" value=0 checked onclick=\"rbEntryTypeChanged(this.value);\">Barcode Entry</td></tr>");
                manual_row_style = "style=\"margin-left:200px\";";
               
                builder.AppendLine("<tr id=\"tblrow_Barcode\" " + barcode_row_style + "><td></td><td>Scan barcode here:</td>");
                builder.AppendLine("         <td colspan=\"3\"><input type=\"text\" id=\"txtScannedString\" name=\"txtScannedString\" autofocus onchange=\"BarcodeStringTextbox_Changed(this.value);\"/></td>");
                builder.AppendLine("<td>");
                builder.AppendLine("<div id=\"divAddButton_barcode\" style=\"float:right;\">");
                builder.AppendLine("    <button title=\"Add new tracking entry\" class=\"sbkMySobek_RoundButton\" onclick=\"Add_new_entry_barcode(); return false;\">ADD</button>");
                builder.AppendLine("</div></td></tr>");

                builder.AppendLine("<tr><td colspan=\"100%\"><input type=\"radio\" name=\"rbEntryType\" id=\"rb_manual\" value=1 onclick=\"rbEntryTypeChanged(this.value);\">Manual Entry</td></tr>");
                builder.AppendLine("<tr id=\"tblrow_Manual1\" " + manual_row_style + "><td></td><td>BibID:</td><td><input type=\"text\" id=\"txtBibID\" value=\"" + bibid + "\" /></td>");
                builder.AppendLine("         <td>VID:</td><td><input type=\"text\" id=\"txtVID\" value=\"" + vid + "\" /></td>");
                builder.AppendLine("</tr>");
                builder.AppendLine("<tr id=\"tblrow_Manual2\" " + manual_row_style + ">");
                builder.AppendLine("<td></td><td>Event:</td><td><select id=\"ddlManualEvent\" name=\"ddlManualEvent\">");
                builder.AppendLine("                                       <option value=\"1\" selected>Start Scanning</option>");
                builder.AppendLine("                                        <option value=\"2\">End Scanning</option>");
                builder.AppendLine("                                        <option value=\"3\">Start Processing</option>");
                builder.AppendLine("                                        <option value=\"4\">End Processing</option></select>");
                builder.AppendLine("</td>");

                //Call the JavaScript functions to apply the appropriate CSS class for the disabled row(s)
                builder.AppendLine("<script type=\"text/javascript\">DisableRow_SetCSSClass('tblrow_Manual1');</script>");
                builder.AppendLine("<script type=\"text/javascript\">DisableRow_SetCSSClass('tblrow_Manual2');</script>");

                builder.AppendLine("<script type=\"text/javascript\">DisableRow_RemoveCSSClass('tblrow_Barcode');</script>");

            }

            //Set the appropriate value in the workflow dropdown 
            if (stage >= 1)
            {
                builder.AppendLine("<script type=\"text/javascript\">SetDropdown_Selected(" + stage + ");</script>");
            }

            builder.AppendLine("<td>");
            builder.AppendLine("<div id=\"divAddButton\" style=\"float:right;\">");
            builder.AppendLine("    <button title=\"Add new tracking entry\" class=\"sbkMySobek_RoundButton\" onclick=\"Add_new_entry(); return false;\">ADD</button>");
            builder.AppendLine("</div></td></tr>");

            builder.AppendLine("</table>");


            //If a new event has been scanned/entered, then display this table
            if (!String.IsNullOrEmpty(bibid) && !String.IsNullOrEmpty(vid))
            {
                string selected_text_scanning = String.Empty;
                string selected_text_processing = String.Empty;
                string currentTime = DateTime.Now.ToString("");


                //if (stage == 1 || stage == 2)
                //{
                //    selected_text_scanning = " selected";
                //    if (stage == 1)
                //        start_Time = DateTime.Now.ToString("HH:mm");
                //    else
                //    {
                //        end_Time = currentTime;
                //    }
                //}
                //else if (stage == 3 || stage == 4)
                //{
                //    selected_text_processing = " selected";
                //    if (stage == 3)
                //        start_Time = currentTime;
                //    else
                //    {
                //        end_Time = currentTime;
                //    }
                // }
                //Start the Tracking Info section
                builder.AppendLine("<span class=\"sbkTi_HomeText\"><h2>Add Tracking Information</h2></span>");
                builder.AppendLine("<span id = \"TI_NewEntrySpan\" class=\"sbkTi_TrackingEntrySpanMouseOut\" onmouseover=\"return entry_span_mouseover(this.id);\" onmouseout=\"return entry_span_mouseout(this.id);\">");
                builder.AppendLine("<table class=\"sbkTi_table\">");
                builder.AppendLine("<tr >");
                builder.AppendLine("<td>Item: </td><td>"+ bibid + ":"+vid+"</td>");
        
                builder.AppendLine("<td>Title: </td><td>" + title + "</td>");
                builder.AppendLine("</tr>");
        
                builder.AppendLine("<tr><td>Workflow:</td>");
                builder.AppendLine("         <td><select id=\"ddlEvent\" name=\"ddlEvent\"> disabled");
                builder.AppendLine("                  <option value=\"1\" "+selected_text_scanning+">Scanning</option>");
                builder.AppendLine("                  <option value=\"2\""+selected_text_processing+">Processing</option></select>");
                builder.AppendLine("         </td>");
                builder.AppendLine("         <td>Date:</td>");
                string currentDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
                builder.AppendLine("         <td><input type=\"date\" name=\"txtStartDate\" id=\"txtStartDate\" value=\"" + currentDate + "\" /> </td>");
                builder.AppendLine("</tr>");
                
                //Add the Start and End Times
                builder.AppendLine("<tr>");
                builder.AppendLine("<td>Start Time:</td>");
                builder.AppendLine("<td><input type=\"time\" name=\"txtStartTime\" id=\"txtStartTime\" value = \""+start_Time+"\"/></td>");
                builder.AppendLine("<td>End Time:</td>");
                builder.AppendLine("<td><input type=\"time\" name=\"txtEndTime\" id=\"txtEndTime\" value = \""+end_Time+"\"/></td></tr>");
                builder.AppendLine("<tr><td colspan=\"4\"><span style=\"float:right;\">");
                builder.AppendLine("    <button title=\"Save changes\" class=\"sbkMySobek_RoundButton\" onclick=\"save_workflow(); return false;\">SAVE</button>");
                builder.AppendLine("    <button title=\"Delete this workflow\" class=\"sbkMySobek_RoundButton\" onclick=\"delete_workflow(); return false;\">DELETE</button>");
                builder.AppendLine("</span></td></tr>");

                //End this table
                builder.AppendLine("</table>");
                
                builder.AppendLine("</span>");

              
                //If there are any previously opened and unclosed workflows for this item
                if (open_workflows_from_DB != null && open_workflows_from_DB.Rows.Count > 0 && (stage==1 || stage==3))
                {

                    builder.AppendLine("<table width=\"75%\"><tr style=\"background:#333333\"><td ></td></tr></table>");

                    builder.AppendLine("<span id=\"TI_NewEntry_duplicate_Span\"  class=\"sbkTi_TrackingEntrySpanMouseOut\"  onmouseover=\"return entry_span_mouseover(this.id);\" onmouseout=\"return entry_span_mouseout(this.id);\">"); 
                    builder.AppendLine("<table class=\"sbkTi_table\" >");

                    
                    //builder.AppendLine("<tr><th>Item</th><th>Workflow</th><th>Date</th><th>Start Time</th><th>End Time</th><th>User</th><th>Equipment</th></tr>");
                    foreach (DataRow row in open_workflows_from_DB.Rows)
                    {

                        builder.AppendLine("<tr><td colspan=\"4\">");
                        builder.AppendLine("<span style=\"color:red;\">You already have an open workflow from " + Convert.ToDateTime(row["DateStarted"]).ToString("MM/dd/yyyy") + "! </span>");

                        builder.AppendLine("</td></tr>");

                        
                        //Start the table
                        builder.AppendLine("<tr >");
                        builder.AppendLine("<td>Item: </td><td>" + BibID + ":" + VID + "</td>");

                        builder.AppendLine("<td>Title: </td><td>" + title + "</td>");
                        builder.AppendLine("</tr>");

                        string this_workflow = row["WorkFlowName"].ToString();
                        if (this_workflow == "Scanning")
                        {
                            selected_text_scanning = "selected";
                            selected_text_processing = "";
                        }
                        else if (this_workflow == "Processing")
                        {
                            selected_text_scanning = "";
                            selected_text_processing = "selected";
                        }
                        builder.AppendLine("<tr><td>Workflow:</td>");
                        builder.AppendLine("         <td><select id=\"ddlEvent\" name=\"ddlEvent\"> disabled");
                        builder.AppendLine("                  <option value=\"1\" " + selected_text_scanning + ">Scanning</option>");
                        builder.AppendLine("                  <option value=\"2\"" + selected_text_processing + ">Processing</option></select>");
                        builder.AppendLine("         </td>");
                        builder.AppendLine("         <td>Date:</td>");

                        builder.AppendLine("         <td><input type=\"date\" name=\"txtStartDate\" id=\"txtStartDate\" value=\"" + Convert.ToDateTime(row["DateStarted"]).ToString("yyyy-MM-dd") + "\" /> </td>");
                        builder.AppendLine("</tr>");

                        //Add the Start and End Times
                        builder.AppendLine("<tr>");
                        builder.AppendLine("<td>Start Time:</td>");
                        builder.AppendLine("<td><input type=\"time\" name=\"txtStartTime\" id=\"txtStartTime\" value = \"" + Convert.ToDateTime(row["DateStarted"]).ToString("HH:mm") + "\"/></td>");
                        builder.AppendLine("<td>End Time:</td>");
                        builder.AppendLine("<td><input type=\"time\" name=\"txtEndTime\" id=\"txtEndTime\" class=\"sbkTi_ErrorBox\" value = \"" + end_Time + "\"/></td></tr>");

                        builder.AppendLine("<tr><td colspan=\"4\"><span style=\"float:right;\">");
                        builder.AppendLine("    <button title=\"Save changes\" class=\"sbkMySobek_RoundButton\" onclick=\"save_workflow(); return false;\">SAVE</button>");
                        builder.AppendLine("    <button title=\"Delete this workflow\" class=\"sbkMySobek_RoundButton\" onclick=\"delete_workflow(); return false;\">DELETE</button>");
                        builder.AppendLine("</span></td></tr>");

                        //End this table
                    //    builder.AppendLine("</table>");
                


                        //End the table
                    }

    //                builder.AppendLine("<tr><td colspan=\"4\">&nbsp;</td></tr>");
       //             builder.AppendLine("<tr style=\"background:#333333\"><td colspan=\"4\"></td></tr>");
                    
                    builder.AppendLine("</table>");
                    builder.AppendLine("</span>");
          //          builder.AppendLine("<table width=\"75%\"><tr style=\"background:#333333\"><td></td></tr></table>");
                }

                //Add the current History table
                if (current_workflows != null)
                {
                    builder.AppendLine("<span class=\"sbkTi_HomeText\"><h2>Current Work History</h2></span>");
                    builder.AppendLine("<table id=\"sbkTi_tblCurrentTracking\" class=\"sbkSaav_Table\">");
                    builder.AppendLine("<tr><th>Item</th><th>Workflow</th><th>Date</th><th>Start Time</th><th>End Time</th><th>User</th><th>Equipment</th></tr>");
                    foreach (KeyValuePair<string, Tracking_Workflow> thisPair in current_workflows)
                    {
                       Tracking_Workflow this_workflow = thisPair.Value;
                       string workflow_text = String.Empty;
                        if (this_workflow.Workflow_type == 1 || this_workflow.Workflow_type == 2)
                            workflow_text = "Scanning";
                        else
                        {
                            workflow_text = "Processing";
                        }
                        builder.AppendLine("<tr>");
                        builder.AppendLine("<td title=\"" + this_workflow.Title + "\">" + this_workflow.BibID + ":" + this_workflow.VID + "</td>");
                        builder.AppendLine("<td>" + workflow_text + "</td>");
                        builder.AppendLine("<td>" + Convert.ToDateTime(this_workflow.Date).ToString("MM/dd/yyyy") + "</td>");
                        builder.AppendLine("<td>" + (String.IsNullOrEmpty(this_workflow.StartTime)?"-":Convert.ToDateTime(this_workflow.StartTime).ToString("hh:mm tt")) + "</td>");
                        builder.AppendLine("<td>" + (String.IsNullOrEmpty(this_workflow.EndTime) ? "-" : Convert.ToDateTime(this_workflow.EndTime).ToString("hh:mm tt")) + "</td>");
                        //     builder.AppendLine("<td>" + Convert.ToDateTime(row["DateStarted"]).ToShortTimeString() + "</td>");
                        //       builder.AppendLine("<td>" + Convert.ToDateTime(row["DateCompleted"]).ToShortTimeString() + "</td>");
                        builder.AppendLine("<td>" + this_workflow.thisUser.UserName + "</td>");
                        builder.AppendLine("<td>" + this_workflow.Equipment + "</td>");
                        builder.AppendLine("</tr>");
                    }

                    builder.AppendLine("</table>");
                }

        //        builder.AppendLine("</span>");
            }

          

            //Add the Save and Done buttons
            builder.AppendLine("<div id=\"divButtons\" style=\"float:right;\">");
            builder.AppendLine("    <button title=\"Save changes\" class=\"sbkMySobek_RoundButton\" onclick=\"save(); return false;\">SAVE</button>");
            builder.AppendLine("    <button title=\"Save all changes and exit\" class=\"sbkMySobek_RoundButton\" onclick=\"save(); return false;\">DONE</button>");
            builder.AppendLine("</div");
            builder.AppendLine("<br/><br/>");
            //Close the main div
            builder.AppendLine("</div>");

            
            LiteralControl control1 = new LiteralControl(builder.ToString());
          
            MainPlaceHolder.Controls.Add(control1);


     }




//A tracking workflow object which holds all the details of the current workflow 
  protected class Tracking_Workflow
  {
      public int WorkflowID { get; set; }

      public int Workflow_type { get; set; }

      public string Date { get; set; }

      public string StartTime { get; set; }

      public User_Object thisUser { get; set; }

      public int itemID { get; set; }

      public string EndTime { get; set; }

      public string BibID { get; set; }

      public string VID { get; set; }

      public string Title { get; set; }

      public string Equipment { get; set; }

      public bool Saved { get; set; }

      public Tracking_Workflow()
      {
          
      }
  }

    }
}
