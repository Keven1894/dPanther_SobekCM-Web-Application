#region Using directives

using System;
using System.IO;
using System.Text;

#endregion

namespace SobekCM.Resource_Object.Bib_Info
{
    /// <summary>Stores the information about any 'affiliations' associated with this digital resource. </summary>
    /// <remarks>Object created by Mark V Sullivan (2006) for University of Florida's Digital Library Center.  </remarks>
    [Serializable]
    public class Affiliation_Info : XML_Writing_Base_Type
    {
        private string campus;
        private string center;
        private string college;
        private string department, institute;
        private string nameref;
        private string section, subsection;
        private string term;
        private string unit;
        private string university;

        /// <summary> Constructor for a new empty instance of the Affilation_Info class </summary>
        public Affiliation_Info()
        {
            // DO nothing
        }

        /// <summary> Gets or sets the reference to a name id within the same resource </summary>
        public string Name_Reference
        {
            get { return nameref ?? String.Empty; }
            set { nameref = value; }
        }

        /// <summary> Gets or sets the affiliation term associated with this resource </summary>
        /// <remarks>This can be used rather than the entire hierarchy of the other members of this class.</remarks>
        public string Term
        {
            get { return term ?? String.Empty; }
            set { term = value; }
        }

        /// <summary> Gets or sets the university listed in the affililation hierarchy for this resource </summary>
        public string University
        {
            get { return university ?? String.Empty; }
            set { university = value; }
        }

        /// <summary> Gets or sets the campus listed in the affililation hierarchy for this resource </summary>
        public string Campus
        {
            get { return campus ?? String.Empty; }
            set { campus = value; }
        }

        /// <summary> Gets or sets the college listed in the affililation hierarchy for this resource </summary>
        public string College
        {
            get { return college ?? String.Empty; }
            set { college = value; }
        }

        /// <summary> Gets or sets the unit listed in the affililation hierarchy for this resource </summary>
        public string Unit
        {
            get { return unit; }
            set { unit = value; }
        }

        /// <summary> Gets or sets the department listed in the affililation hierarchy for this resource </summary>
        public string Department
        {
            get { return department ?? String.Empty; }
            set { department = value; }
        }

        /// <summary> Gets or sets the institute listed in the affililation hierarchy for this resource </summary>
        public string Institute
        {
            get { return institute ?? String.Empty; }
            set { institute = value; }
        }

        /// <summary> Gets or sets the center listed in the affililation hierarchy for this resource </summary>
        public string Center
        {
            get { return center ?? String.Empty; }
            set { center = value; }
        }

        /// <summary> Gets or sets the section listed in the affililation hierarchy for this resource </summary>
        public string Section
        {
            get { return section ?? String.Empty; }
            set { section = value; }
        }

        /// <summary> Gets or sets the subsection listed in the affililation hierarchy for this resource </summary>
        public string SubSection
        {
            get { return subsection ?? String.Empty; }
            set { subsection = value; }
        }

        /// <summary> Flag indicates if this affiliation hierarchy contains any data </summary>
        public bool hasData
        {
            get
            {
                return (!String.IsNullOrEmpty(term)) || (!String.IsNullOrEmpty(university)) || (!String.IsNullOrEmpty(campus)) || (!String.IsNullOrEmpty(college)) ||
                       (!String.IsNullOrEmpty(unit)) || (!String.IsNullOrEmpty(department)) || (!String.IsNullOrEmpty(institute)) || (!String.IsNullOrEmpty(center)) ||
                       (!String.IsNullOrEmpty(section)) || (!String.IsNullOrEmpty(subsection));
            }
        }

        /// <summary> Gets or sets the entire XML-safe for this resource </summary>
        internal string Affiliation_XML
        {
            get
            {
                if (!String.IsNullOrEmpty(term))
                    return Convert_String_To_XML_Safe(term);

                StringBuilder returnVal = new StringBuilder();

                if (!String.IsNullOrEmpty(university))
                {
                    returnVal.Append(" -- " + university);
                }

                if (!String.IsNullOrEmpty(campus))
                {
                    returnVal.Append(" -- " + campus);
                }

                if (!String.IsNullOrEmpty(college))
                {
                    returnVal.Append(" -- " + college);
                }

                if (!String.IsNullOrEmpty(unit))
                {
                    returnVal.Append(" -- " + unit);
                }

                if (!String.IsNullOrEmpty(department))
                {
                    returnVal.Append(" -- " + department);
                }

                if (!String.IsNullOrEmpty(institute))
                {
                    returnVal.Append(" -- " + institute);
                }

                if (!String.IsNullOrEmpty(center))
                {
                    returnVal.Append(" -- " + center);
                }

                if (!String.IsNullOrEmpty(section))
                {
                    returnVal.Append(" -- " + section);
                }

                if (!String.IsNullOrEmpty(subsection))
                {
                    returnVal.Append(" -- " + subsection);
                }

                if (returnVal.ToString().Length == 0)
                {
                    return String.Empty;
                }
                
                string returnString = returnVal.ToString();
                returnString = returnString.Substring(3).Trim();
                return Convert_String_To_XML_Safe(returnString);
            }
        }

        /// <summary> Writes this affiliation information as SobekCM-formatted XML </summary>
        /// <param name="SobekcmNamespace"> Namespace to use for the SobekCM custom schema ( usually 'sobekcm' )</param>
        /// <param name="Results"> Stream to write this affiliation information as SobekCM-formatted XML</param>
        internal void Add_SobekCM_Metadata(string SobekcmNamespace, TextWriter Results)
        {
            if (!hasData)
                return;

            if (!String.IsNullOrEmpty(nameref))
            {
                Results.Write("<" + SobekcmNamespace + ":Affiliation nameid=\"" + nameref + "\">\r\n");
            }
            else
            {
                Results.Write("<" + SobekcmNamespace + ":Affiliation>\r\n");
            }
            if (!String.IsNullOrEmpty(term))
            {
                Results.Write("<" + SobekcmNamespace + ":AffiliationTerm>" + Convert_String_To_XML_Safe(term) + "</" + SobekcmNamespace + ":AffiliationTerm>\r\n");
            }
            else
            {
                Results.Write("<" + SobekcmNamespace + ":HierarchicalAffiliation>\r\n");

                if (!String.IsNullOrEmpty(university))
                {
                    Results.Write("<" + SobekcmNamespace + ":University>" + Convert_String_To_XML_Safe(university) + "</" + SobekcmNamespace + ":University>\r\n");
                }

                if (!String.IsNullOrEmpty(campus))
                {
                    Results.Write("<" + SobekcmNamespace + ":Campus>" + Convert_String_To_XML_Safe(campus) + "</" + SobekcmNamespace + ":Campus>\r\n");
                }

                if (!String.IsNullOrEmpty(college))
                {
                    Results.Write("<" + SobekcmNamespace + ":College>" + Convert_String_To_XML_Safe(college) + "</" + SobekcmNamespace + ":College>\r\n");
                }

                if (!String.IsNullOrEmpty(unit))
                {
                    Results.Write("<" + SobekcmNamespace + ":Unit>" + Convert_String_To_XML_Safe(unit) + "</" + SobekcmNamespace + ":Unit>\r\n");
                }

                if (!String.IsNullOrEmpty(department))
                {
                    Results.Write("<" + SobekcmNamespace + ":Department>" + Convert_String_To_XML_Safe(department) + "</" + SobekcmNamespace + ":Department>\r\n");
                }

                if (!String.IsNullOrEmpty(institute))
                {
                    Results.Write("<" + SobekcmNamespace + ":Institute>" + Convert_String_To_XML_Safe(institute) + "</" + SobekcmNamespace + ":Institute>\r\n");
                }

                if (!String.IsNullOrEmpty(center))
                {
                    Results.Write("<" + SobekcmNamespace + ":Center>" + Convert_String_To_XML_Safe(center) + "</" + SobekcmNamespace + ":Center>\r\n");
                }

                if (!String.IsNullOrEmpty(section))
                {
                    Results.Write("<" + SobekcmNamespace + ":Section>" + Convert_String_To_XML_Safe(section) + "</" + SobekcmNamespace + ":Section>\r\n");
                }

                if (!String.IsNullOrEmpty(subsection))
                {
                    Results.Write("<" + SobekcmNamespace + ":Subsection>" + Convert_String_To_XML_Safe(subsection) + "</" + SobekcmNamespace + ":Subsection>\r\n");
                }

                Results.Write("</" + SobekcmNamespace + ":HierarchicalAffiliation>\r\n");
            }
            Results.Write("</" + SobekcmNamespace + ":Affiliation>\r\n");
        }

        /// <summary> Writes this affiliation as a XML-safe string </summary>
        /// <returns> The affiliation hierarchy as a single string </returns>
        public override string ToString()
        {
            return Affiliation_XML;
        }
    }
}