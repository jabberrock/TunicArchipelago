@if not exist "%ProgramFiles(x86)%\Steam\steamapps\common\TUNIC\BepInEx\plugins\" (
	@echo Please follow the instructions to install BepInEx first:
	@echo https://github.com/silent-destroyer/tunic-randomizer#installation
	@pause
	@exit
)

@echo Installing Tunic Archipelago Mod:
@copy *.dll "%ProgramFiles(x86)%\Steam\steamapps\common\TUNIC\BepInEx\plugins\"
@copy *.json "%ProgramFiles(x86)%\Steam\steamapps\common\TUNIC\BepInEx\plugins\"
@pause
