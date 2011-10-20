#!/usr/bin/env sh
rm -f log.log
rm -f debug.log

./playgame.py --player_seed 42 --end_wait=0.25 --verbose --log_dir game_logs --turns 1000 --map_file maps/multi_hill_maze/multi_maze_03.map "$@" "ant.exe" "python sample_bots/python/LeftyBot.py" 

#"python sample_bots/python/HunterBot.py" "python sample_bots/python/GreedyBot.py"
