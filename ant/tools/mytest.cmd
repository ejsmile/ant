@echo off
del /f log.log
del /f debug.log
copy /y ..\bin\Debug\ant.exe ant.exe

python %~dp0playgame.py --engine_seed 42 --player_seed 42 --end_wait=0.25 --verbose --log_dir game_logs --turns 500 --map_file %~dp0maps\example\tutorial1.map %* "ant.exe" "python %~dp0sample_bots\python\GreedyBot.py"

