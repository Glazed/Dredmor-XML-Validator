When a new expansion or game version is released the Mod Validator may need to be updated to support new features that are allowed in mods. This document lists the steps to take to update the validator.

1. Add new XML elements and attributes to DredmorSchema.xsd in the DredmorXmlValidation project. For example, for expansion 3 we had to add encrustDB as a root element to support validating encrustDB.xml, and a bunch of other new elements and attributes. Make sure to use shared types where appropriate.

2. Regenerate the resource manifest. This is accomplished by executing the DredmorResourceManifestGenerator project in Release mode. Make sure the first command line argument in the project properties is set to the path where the new version of DoD is located on your machine. For expansion three we had to modify the GameResources class to load Powers because they are now referenced by crusts. We need to add them to the resource manifest so that mods which reference them can be validated. This also required creating a new ContentResourceFilter property for Powers on the GameResource class.

3. Make sure the XmlFilesToValidate array in the DredmorInfo class in the DredmorUtilities projet is up to date. In expansion 3 a new XML file type was added: encrustDB.xml. This was added to the array so that these files are validated. New files names must also be added to the AllContentXmlFiles array.

4. Modify XmlResourceValidator to validate any new resource references that were introduced in the game. For expansion 3 support for a new file had to be added to this: encrustDB.xml. A new concrete class, EncrustXmlResourceValidator, was added in order to validate the powers with those files, and limitations on the number of certain child elements within an encrust element. A new method to the abstract base class XmlResourceValidator was added named AddMissingPowerErrors.

5. Regenerate words.txt in the DredmorUtilities project. This file supports the case-insensitive validation of XML files. It must contain all element, attribute, and enumerated value names from DredmorSchema.xsd. To regenerate the file, run the DredmorXsdWordExtractor project in Release mode. The first command line argument must be the path to the DredmorSchema.xsd file.

6. Update the assembly version numbers in the shared AssemblyVersion.cs file in the Refrences solution folder.