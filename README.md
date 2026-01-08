# Saveyard
Save manager for speedrunners, mainly focused towards GTA 3D era games (III & VC & SA)

<img width="463" height="475" alt="image" src="https://github.com/user-attachments/assets/9942f17a-cb6c-4b41-9db3-6f8822dc6249" />
<img width="287" height="325" alt="image" src="https://github.com/user-attachments/assets/45983006-86b7-47cb-8d78-12222913b7d8" /><br>

# How to use:
1. Open settings and write info about config that you're going to be working with. Typically you would want one config for a category, e.g. one for Any%, one for 100% etc. Example:

`Game - GTA San Andreas`  
_The game you're running, duh_

`Category - Any%`  
_Category_

`Config file name (.cfg) - no ajs`  
_The name for your config, `.cfg` is added automatically, which results in `no ajs.cfg` here. You can press `Generate` to generate the name for it using `Game - Category` template. Usually you would want exactly that but who knows, maybe some people would love to have several configs for one category. People sometimes name their splits in some bizzare ways. I don't know why._

`Save files directory - D:\Libraries\Documents\GTA San Andreas User Files`  
_The path for where savefiles for the game are stored at. And yes, I moved libraries away from the C drive._

`File prefix - GTASAsf`  
_The part of the file name that comes before the number. In GTA San Andreas savefiles are named in "GTASAsf1.b" format_

`File postfix - .b`  
_Whatever comes after the number. This field can be empty, as for example Bully: Scholarship Edition doesn't give it's saves any extension whatsoever. God knows what you'll be using, so yeah_

2. Add saves that you will be using. First textbox is for a name, second one is for path - you can either copypaste it, or browse by pressing `...` button. The save you're selecting a source file can have any name and any extension - program will rename it using `{prefix}+{i}+{postfix}` formula. You can add and remove pages by using `+` and `-` buttons
   
3. Program is aimed towards GTA games, as it was mentioned, so it's 8 saves per page, since that's the maximum amount of simultaneously available saves the game can see. As I personally imagined it - you would want to have one or couple of pages per segment, so considering the nature of SA Any% run that is divided on 6 segments (Los Santos, Badlands, San Fierro, Desert, Las Venturas, Return to Los Santos) my `GTA San Andreas - Any%.cfg` would have, let's say:
   - 8+6 saves for Los Santos on page 1 and 2
   - 5 saves for Badlands on page 3
   - 8+8+3 saves for San Fierro on pages 4-6 and etc.
4. Save your config. You can open configs you currently have by pressing `File` and then hovering over at `Open` at tool strip menu and then selecting the config you want to use. They're all are stored at program's root folder, over at `Configs`. You can also browse open a config file, however after you press `Save` it will save it in your program folder, the `Saves` one
   
5. Now you can load your saves, either by full pages or individually.
   If you load them individually (`Load`) they just get put in directory to corresponding slot
   If you load them all at once (`Set all`), empty slots will remove such numbers from the directory. For example, if you have saves 1-5 and 8 on page, saves 6 and 7 will get deleted from the saves directory
   
6. To prevent your needed saves from disappearing, you can back them up using `Backup current saves` button. It will copy them over at program's root folder, `Saves/Game name/Category name/Config name/Date and time of the backup` directory
   
7. You can also clear your saves folder from them by pressing `Clear folder` button. It will only delete files that follow the `{prefix}+{i}+{postfix}` formula, so no replays and/or other things will get deleted
   
8. Button `User Files` opens the directory for your savefiles, `Program folder` opens the folder of where program is stored if you need to take a look at saves you've backed up or configs you have/want to delete

# Functions to add:
- Expand amount of saves up to 20 (optional)
- Consider saves being numbered not just `1` `2` `3` `4` but also `01` `02` `03` `04` and `000` `001` `002` `003` (Max Payne 2)
- Consider saves being numbered from `0` and not `1`
- `Clear` button for each save
- Add replay.rep support (one replay for the whole page & for each save)
- Names for each page
- Add sounds for indication purposes (optional)
- Add tray icon for quicker usage
- Add hotkeys for flawless usage (optional & bindable)
- Change savefile name (as data of it) according to the one in the config (optional)

# Known bugs & things that need changing:
- ` - .cfg` config name when empty in settings
- `Game - .cfg` if no category written
- When opening config open on the page it was saved at
- For new config either fill with just American dad info, or no info at all when creating
