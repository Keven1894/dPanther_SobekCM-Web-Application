-----------------------------------------------------------------------

         SobekCM Web Repository and Builder Solutions Readme

-----------------------------------------------------------------------


This package includes two Visual Studio 2012 solutions at the top level.

1) SobekCM_Web.sln, which builds the SobekCM Web Repository.  

2) SobekCM_Builder.sln, which builds the SobekCM Builder Application.


In addition, a third Visual Studio 2012 solution for the installer appears
under the Installer folder. TREAESTASETAWET


-----------------------------------------------------------------------

Design folders:

To run the web repository successfully, you will need to copy the design
files from your instance of SobekCM.


-----------------------------------------------------------------------

More information:

For more information about this system, please see the help pages linked
off of sobek.ufl.edu


-----------------------------------------------------------------------

WiX Toolset:

 The SobekCM_WiX_Installer project will cause warnings and not open in your Visual 
Studio unless you install the appropriate WiX Toolset.  This currently utilizes Wix 3.8 .

You could also choose to ignore the warning if you are not going to be making a MSI file 
to install a custom version of the Builder or the web.

-----------------------------------------------------------------------

Pre-compile Batch Files and Microsoft Ajax Minifier:

To use the precompiler batch files, you will need to install the Microsoft Ajax 
Minifier.  This is used to minify the css and javascript before pre-compilation.

-----------------------------------------------------------------------

Mark Sullivan ( 12/15/2013 )