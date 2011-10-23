#!/usr/bin/env sh
rm -f log.log
rm -f debug.log
cp -f ../bin/Debug/ant.exe ant.exe


#./playgame.py --player_seed 42 --end_wait=0.25 --verbose --log_dir game_logs --turns 500 --map_file maps/symmetric_random_walk/random_walk_01.map "$@" "ant.exe" "python sample_bots/python/LeftyBot.py" "python sample_bots/python/HunterBot.py"  "python sample_bots/python/GreedyBot.py"
./playgame.py --player_seed 42 --end_wait=0.25 --verbose --log_dir game_logs --turns 500 --map_file maps/maze/maze_12.map "$@" "ant.exe" "python sample_bots/python/LeftyBot.py" "python sample_bots/python/HunterBot.py" "python sample_bots/python/GreedyBot.py"
#./playgame.py --player_seed 42 --end_wait=0.25 --verbose --log_dir game_logs --turns 300 --map_file maps/multi_hill_maze/multi_maze_09.map "$@" "ant.exe" "python sample_bots/python/LeftyBot.py" "python sample_bots/python/HunterBot.py" "python sample_bots/python/GreedyBot.py"
