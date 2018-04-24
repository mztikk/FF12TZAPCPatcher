# FF12TZAPCPatcher
App to apply and remove patches to FFXII_TZA.exe.  
Patches get loaded from .dif files(can be created by ida pro or the create patch tool in this app) numbers have to be in hex for the .dif files.  
And .json files of BytePatch class  
```
{ "Name": "example", "Offset": 10, "BytesToPatch": [144, 144, 144, 144, 144], "OriginalBytes": [232, 213, 166, 0, 0] }
```
```
{ "Name": "example", "Offset": 10, "BytesToPatch": [144, 144, 144, 144, 144], "OriginalBytes": [232, 213, 166, 0, 0], "Description": "This is a description" }
```
Descriptions are optional  

Patches included so far are:  
Disable Auto Pause  
Disable MiniMap render  
Disable World render