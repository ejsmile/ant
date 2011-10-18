rm -f log.log
rm -f debug.log
cp -f ../bin/Debug/ant.exe ant.exe

./playgame.py --player_seed 42 --end_wait=0.25 --verbose --log_dir game_logs --turns 1000 --map_file maps/example/tutorial1.map "$@" "ant.exe"  "python sample_bots/python/GreedyBot.py"
