# Hydroneer Modding Toolchain [![Master Build Status](https://github.com/ResaloliPT/HydroModTool/actions/workflows/dotnet.yml/badge.svg?branch=master)](https://github.com/ResaloliPT/HydroModTool/actions/workflows/dotnet.yml) [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=ResaloliPT_HydroModTool&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=ResaloliPT_HydroModTool) [![Donate](https://www.paypalobjects.com/en_US/i/btn/btn_donate_SM.gif)](https://paypal.me/ResaloliPT)

Hydroneer Modding Toolchain is a simple multi propouse tool to be used in the 2 scenarios of modding, either it be a user wanting to use mods or a developer using as a toolchain to stage, package and test the mod fast.

> NOTE: This tool is still in developement expect bugs, and feel free to report them as necessary.

## Features

- User
  - Install mods -> Install mods listed from [Bridgepour wiki](https://bridgepour.com/legacy-mods) (Mods are direclty downloaded to game folder)
- Developer
  - Stage -> Select assets to be copied to another folder to work on, also replaces GUIDs
  - Package -> Packaged staged assets into a .pak based on the Project name (Ex: Project Name = MenuMod -> 500-MenuMod_P.pak)
  - Copy Mod -> Will copy the .pak file into the game folder (%LOCALAPPDATA%\Mining\Saved\Paks)
  - Launch Game -> Launch the game for fast testing (Works for steam only!)
  - Dev Express -> Will execute all mentioned actions in order for a one-click-deploy experience

## Documentation

> The repository is under construction. There will be a proper documentation at some point!

While theres is no integral documentation you can ask help to me direcly on discord [Foulball Hanghover Discord](https://discord.com/invite/foulballhangover) #hydro-modding channel or create a Issue ticket you have seen any bug

## Download

You can get the latest release from Github Releases [Github Releases](https://github.com/ResaloliPT/HydroModTool/releases).

If you suspect anything feel free to build from source and take a look for yourself.

## Usage

> In progress

## Licence

This is licenced under [MIT License](https://github.com/ResaloliPT/HydroModTool/blob/master/Licence.txt)

## Donate

If you have some cents pay me coffee, consider donating!

[![Donate](https://www.paypalobjects.com/en_US/i/btn/btn_donate_SM.gif)](https://paypal.me/ResaloliPT)

## Credits

Thanks to [Rexxar-tc](https://github.com/rexxar-tc) where i forked this project from, as it served basicly as the foundation of ideas.

Thanks to [KaiHeilos](https://github.com/kaiheilos) where he shared the Asset editor tool used for packaging and unpacking game assets && get the assets GUIDs from.

Thanks to [Rhino](https://github.com/RHlNO) for sharing the HydroUMH and giving massive help in the proccess.

Thanks to F4T4L and [Gamerkuipers](https://github.com/Gamerkuipers) for testing my PR versions.

The rest of all modders in the discord #hydro-modding channel, without their help, I would understand nothing of how the building a mod pipeline worked and being there to help.

## Author

Roberto "ResaloliPT" Pires
