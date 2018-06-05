# RSDK reverse and tools

# Original ReadME Section

This is a repository where I am hosting tools and reverse engineering notes from RSDK (Retro-SDK) development kit developed by Christian Whitehead.

RSDK has 5 versions:
* RSDKv1: Retro Sonic
* RSDKv2: Sonic Nexus, Sonic XG
* RSDKv3: Sonic CD 2011
* RSDKv4: Sonic the Hedgehog 1 and Sonic the Hedgehog 2 remakes
* RSDKv5: Sonic Mania

The repository supports the Animation System used by RSDKv3 (unchanged in RSDKv4) and RSDKv5. The tool used to view and edit the animation of those versions is called RSDK Animation Editor.

![picture](http://www.lucianociccariello.com/Content/images/portfolio/projects/rsdk1.png)

During 2012 I created a program to disassembly the bytecode used by RSDKv3. It is old, buggy and released only because it was directly requested. It is no longer maintained and I am uploading it for research purpose only.

# RubberDuckyCooly's ReadME Section

# NOTES:

- At the moment the program doesn't load the .bmp files used by retro sonic animation files. A workaround for this is just to open the file in gimp (since programs like paint.net and MSpaint modify the pallete indexes when saving) and saving it as .gif!

- A freshly extracted sonic nexus data file is incompatible with this animation editor! 

- To Enable support for it download the program called "not_this" from the link below, when you open the animation file (or any file) with "not_this" it swaps all the bytes to their opposite (0 becomes 1 and 1 becomes 0), this is the "encryption" that many of the raw files in the extracted data folder including the animation file, this make it readable by this program.

http://unhaut.x10host.com/retrun/nexus.html

# Thanks:
Xeeynamo - For making the Sonic Mania Animation Editor and RSDKv3 and RSDKv5 Loading Functions

Nextvolume/tails92 - For Making TaxAni, which I used as a guide for RSDKv1 and RSDKv2
