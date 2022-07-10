# StefToys - Powertoys Plugins

## How to get started

1. Install PowerToys
2. Go to `C:\Program Files\PowerToys\modules\launcher` and you'll find a bunch of interesting .dlls there.
  - `PowerToys.Common.UI`
  - `PowerToys.ManagedCommon`
  - `Wox.Infrastructure`
  - `Wox.Plugin`
3. Copy them to a `libs` folder and add a reference to them. Sadly they aren't on Nuget, so we'll have to make do with this cave-people approach. With Visual Studio, you do this by adding a "Project Reference" and clicking on "Browse". 
4. Check out plugins such as 
  - https://github.com/microsoft/PowerToys/tree/main/src/modules/launcher/Plugins
  - https://github.com/skttl/ptrun-guid
  - https://github.com/hlaueriksson/GEmojiSharp/tree/master/src/GEmojiSharp.PowerToysRun
5. Build the plugin
6. Copy it to a folder like "C:\Program Files\PowerToys\modules\launcher\Plugins\NAME_GOES_HERE"
7. Automate the copy-ing with `xcopy /E /Y "$(TargetDir)" "C:\Program Files\PowerToys\modules\launcher\Plugins\NAME_GOES_HERE"`
  - Note that using `$(TargetDir)` means that we don't have to worry about accidentally copying the wrong build, like copying the debug build when we actually want to try out the release build.
8. If it fails, that usually means that the Windows folder permissions are wrong. In that case, just create the correct folder and let users modify it.

## Documentation

https://github.com/microsoft/PowerToys/tree/main/doc/devdocs/modules/launcher

## Tasks

Powertoy run
- [x] open vscode in current directory 
- [ ] yt-dl "url" (only available if yt-dl is on the path)
- [x] ip: shows current local ip https://github.com/microsoft/PowerToys/issues/19284
- [ ] brighter, darker, fullbright: integrated and use the current monitor, also shows the current brightness in the text


Add custom command plugin
- [ ] %appdata% & %temp%
- [ ] start file HTTP server using current directory 

Extra
- [ ] alarm clock
- [ ] do not disturb
- [ ] brightness: gives you a proper brightness slider or two key to press to quickly adjust the brightness 
- [ ] file browser (uses current explorer window)
- [ ] Music lyrics and title search
- [ ] color converter?
- [ ] better calculator?
- [ ] add to (Google) calendar