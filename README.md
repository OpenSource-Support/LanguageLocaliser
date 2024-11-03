## Language Localiser

A small Command-Line based tool which allows for automatic generation of Localisation files via Google Translate in Json format primarily built for the Ryujinx project. To run, provide the full path as first command-line argument pointing to the location where the "{language}_{region}.json" files are, similar to Ryujinx's implementation here:

<a href="https://github.com/GreemDev/Ryujinx/tree/master/src/Ryujinx/Assets/Locales">Greemdev Ryujinx Locale Files</a>

Any localisation tokens which are in some languages but not all, will automatically be translated via Google Translate to the missing languages, and the necessary modifications will be made to the Json files in place.
