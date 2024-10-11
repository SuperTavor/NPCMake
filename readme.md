# npcmake
npcmake is a modding tool for **Yo-kai Watch 2** that allows you to easily create, place and code NPCs.


## How does it work?
First, you create your **NPC TOML.** An NPC TOML is a configuration file that defines many things about your NPC. It looks like this:

```toml
NpcX = 0
# Y here is how it acts in 2D games, not the height.
NpcY = 0
# This, however, is the height.
NpcZ = 0
# Use degrees
NpcRotation = 0

# On which chapter would your NPC be talkable? (write c01 for chapter 1, c02 for chapter 2, etc. C11 is post game)
ChapterCode = ""c11""

# On which map ID are you adding your NPC to?
MapID = ""t101i01""

etc.....
```
then, you feed it into npcmake along with your mapres folder. You can get your mapres folder from your game FA using this path:
`data/res/map/[MapID]`.

Now you're almost done! Just make sure [XtractQuery](https://github.com/onepiecefreak3/XtractQuery) is usable from the location you installed npcmake to, and install the outputted files from npcmake.

Enjoy!

## Special thanks
Tinifan - CfgBin and XPCK logic

Onepiecefreak3 - Help with fixing a bug in the CfgBin logic