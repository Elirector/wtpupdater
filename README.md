# What is it?
A tool to download and install [We The People mod](https://github.com/We-the-People-civ4col-mod/Mod/releases) for Civilization IV Colonization game


# How do I use it?
1. Press "Refresh" to get availible versions
1. Select required version (last is selected by default)
1. Press "Save" to start downlading, press "Cancel" to stop it
1. Press "FindDir" to find Civ4Col installation dir or select other dir if not found or autoselect %documents%\My Games\Covilization IV Colonization (Install subdir will always be "\Mods\WeThePeople", if exists it will be cleared!)
1. Change subdir name for mod in a text field if needed
1. Press "Unzip" when file downlading completed and install dir selected

# What should it do?
Parse mod release page on github, find all releases, download chosen one, unpack it in game folder

# Roadmap / todo or something

1. Some configuration
1. Need a way to distinguish current mod version
1. Remake as wizard with auto/silent option

# Release history
## 0.0.2.0
 - Remade download for the new github releases page
 - Runs setup.bat if it exists
 - Blacklisted unsupported and/or buggy versions (4.0 and 4.0.1 for now)

## 0.0.1.0 
- First proper version number
- Temp file is now located in user's temp dir
- Temp files now should be removed on util close (even after errors)
- Added more logging and some basic error handling
