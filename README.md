## Language Localiser

A small tool which allows for automatic generation of Localisation files via Google Translate in Json format primarily built for the Ryujinx project. To run simply provide full path as first argument to where the "{language}_{region}.json" files are, similar to Ryujinx's implementation here:

<a href="https://github.com/GreemDev/Ryujinx/tree/master/src/Ryujinx/Assets/Locales">Greemdev Ryujinx Locale Files</a>

Any localisation tokens which are in one language but not in the others will automatically be translated by Google Translate and the necessary modifications will be made to the Json files in place.
