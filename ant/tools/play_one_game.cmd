@echo off
move /y ..\bin\Debug\ant.exe ant.exe
move /y ..\bin\Release\ant.exe ant.exe


python %~dp0playgame.py --engine_seed 42 --player_seed 42 --end_wait=0.25 --verbose --log_dir game_logs --turns 300 --map_file %~dp0maps\symmetric_random_walk\random_walk_05.map %* "ant.exe" "python %~dp0sample_bots\python\LeftyBot.py" "python %~dp0sample_bots\python\HunterBot.py" "python %~dp0sample_bots\python\GreedyBot.py"

rem maps\symmetric_random_walk\random_walk_05.map
rem maps\maze\maze_11.ma