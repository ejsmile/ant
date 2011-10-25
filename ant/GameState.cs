using System;
using System.Collections.Generic;

namespace Ants
{
	public class GameState
	{
		
		public int Width { get; private set; }

		public int Height { get; private set; }
		
		public int LoadTime { get; private set; }

		public int TurnTime { get; private set; }
		
		private DateTime turnStart;

		public int TimeRemaining {
			get {
				TimeSpan timeSpent = DateTime.Now - turnStart;
				return TurnTime - timeSpent.Milliseconds;
			}
		}

		public int ViewRadius2 { get; private set; }

		public int AttackRadius2 { get; private set; }

		public int SpawnRadius2 { get; private set; }
		
		public List<AntLoc> MyAnts;
		public List<AntLoc> EnemyAnts;
		public List<AntLoc> MyHills;
		public List<AntLoc> EnemyHills;
		public List<Location> DeadTiles;
		public List<Location> FoodTiles;
		private Tile[,] map;
		
		public GameState (int width, int height, 
		                  int turntime, int loadtime, 
		                  int viewradius2, int attackradius2, int spawnradius2)
		{
			
			Width = width;
			Height = height;
			
			LoadTime = loadtime;
			TurnTime = turntime;
			
			ViewRadius2 = viewradius2;
			AttackRadius2 = attackradius2;
			SpawnRadius2 = spawnradius2;
			
			MyAnts = new List<AntLoc> ();
			EnemyAnts = new List<AntLoc> ();
			MyHills = new List<AntLoc> ();
			EnemyHills = new List<AntLoc> ();
			DeadTiles = new List<Location> ();
			FoodTiles = new List<Location> ();
			
			map = new Tile[height, width];
			for (int row = 0; row < height; row++) {
				for (int col = 0; col < width; col++) {
					map [row, col] = Tile.Land;
				}
			}
		}
		
		public void startNewTurn ()
		{
			// start timer
			turnStart = DateTime.Now;
			
			// clear ant data
			foreach (AntLoc loc in MyAnts) {
				map [loc.row, loc.col] = Tile.Land;
				if (EnemyHills.Contains(loc)) map [loc.row, loc.col] = Tile.Land;
			}
			//foreach (Location loc in EnemyAnts)
			//	map [loc.row, loc.col] = Tile.Land;
			foreach (Location loc in MyHills)
				map [loc.row, loc.col] = Tile.Land;
			foreach (Location loc in EnemyHills)
				map [loc.row, loc.col] = Tile.Land;
			foreach (Location loc in DeadTiles)
				map [loc.row, loc.col] = Tile.Land;
			
			MyAnts.Clear ();
			EnemyAnts.Clear ();
			MyHills.Clear ();
			EnemyHills.Clear ();			
			DeadTiles.Clear ();
			
			// set all known food to unseen
			foreach (Location loc in FoodTiles)
				map [loc.row, loc.col] = Tile.Land;
			FoodTiles.Clear ();
		}
		
		public void addAnt (int row, int col, int team)
		{
			map [row, col] = Tile.Ant;
			
			AntLoc ant = new AntLoc (row, col, team);
			if (team == 0) {
				MyAnts.Add (ant);
			} else {
				EnemyAnts.Add (ant);
			}
		}

		public void addHill (int row, int col, int team)
		{
			map [row, col] = Tile.Hill;
			
			AntLoc ant = new AntLoc (row, col, team);
			if (team == 0) {
				MyHills.Add (ant);
			} else {
				EnemyHills.Add (ant);
			}
		}
		
		public void addFood (int row, int col)
		{
			map [row, col] = Tile.Food;
			FoodTiles.Add (new Location (row, col));
		}
		
		public void removeFood (int row, int col)
		{
			// an ant could move into a spot where a food just was
			// don't overwrite the space unless it is food
			if (map [row, col] == Tile.Food) {
				map [row, col] = Tile.Land;
			}
			FoodTiles.Remove (new Location (row, col));
		}
		
		public void addWater (int row, int col)
		{
			map [row, col] = Tile.Water;
		}
		
		public void deadAnt (int row, int col)
		{
			// food could spawn on a spot where an ant just died
			// don't overwrite the space unless it is land
			if (map [row, col] == Tile.Land) {
				map [row, col] = Tile.Dead;
			}
			
			// but always add to the dead list
			DeadTiles.Add (new Location (row, col));
		}
		
		public bool passable (Location loc)
		{
			// true if not water
			return map [loc.row, loc.col] != Tile.Water;// && map [loc.row, loc.col] != Tile.Hill;
		}
		
		public bool unoccupied (Location loc)
		{
			// true if no ants are at the location
			return passable (loc) && map [loc.row, loc.col] != Tile.Ant;// && map [loc.row, loc.col] != Tile.Hill;
		}
		
		public Location destination (Location loc, Location delta)
		{
			// calculate a new location given the direction and wrap correctly
			int row = (loc.row + delta.row) % Height;
			if (row < 0)
				row += Height; // because the modulo of a negative number is negative
			
			int col = (loc.col + delta.col) % Width;
			if (col < 0)
				col += Width;
			
			return new Location (row, col);
		}
		
		public Location destination (Location loc, char direction)
		{
			return destination (loc, Ants.Aim [direction]);
			
		}
		
		public int distance (Location loc1, Location loc2)
		{
			// calculate the closest distance between two locations
			int d_row = Math.Abs (loc1.row - loc2.row);
			d_row = Math.Min (d_row, Height - d_row);
			
			int d_col = Math.Abs (loc1.col - loc2.col);
			d_col = Math.Min (d_col, Width - d_col);
			
			return d_row + d_col;
		}
		
		public ICollection<char> direction (Location loc1, Location loc2)
		{
			// determine the 1 or 2 fastest (closest) directions to reach a location

			
			List<char > directions = new List<char> ();
			if (loc1.row < loc2.row) {
				if (loc2.row - loc1.row >= Height / 2)
					directions.Add ('n');
				if (loc2.row - loc1.row <= Height / 2)
					directions.Add ('s');
			}
			if (loc2.row < loc1.row) {
				if (loc1.row - loc2.row >= Height / 2)
					directions.Add ('s');
				if (loc1.row - loc2.row <= Height / 2)
					directions.Add ('n');
			}
			
			if (loc1.col < loc2.col) {
				if (loc2.col - loc1.col >= Width / 2)
					directions.Add ('w');
				if (loc2.col - loc1.col <= Width / 2)
					directions.Add ('e');
			}
			if (loc2.col < loc1.col) {
				if (loc1.col - loc2.col >= Width / 2)
					directions.Add ('e');
				if (loc1.col - loc2.col <= Width / 2)
					directions.Add ('w');
			}
			
			return directions;
		}
		
		public List<char> direction_algor_A (Location loc1, Location loc2)
		{
			//Открытий список вершин по которым идет поиск
			IDictionary<Location, Node > OpenList = new Dictionary<Location, Node> ();
			//Закрытий список вершин поиск завершен
			IDictionary<Location, Node > CloseList = new Dictionary<Location, Node> ();
			//Оптимизация поиска по массиву открытих вершин
			IDictionary<int,List<Location >> Tree = new Dictionary<int, List<Location>> (); 
			List<char > directions = new List<char> ();
			//начальная точка
			CloseList.Add (loc1, new Node (loc1, new Location (0, distance (loc1, loc2))));
			
			Location loc = loc1;

			bool finish = true;
			DateTime start = DateTime.Now;
			int head = int.MaxValue;
			while (finish) {
				
				foreach (char c in Ants.Aim.Keys) {
					
					Location newLoc = destination (loc, c);
						
					if (unoccupied (newLoc) && !CloseList.Keys.Contains (newLoc)) {
						Location size = new Location (CloseList [loc].Size.row + 1, distance (newLoc, loc2));
						
						int f = size.row + size.col;
						
						if (OpenList.ContainsKey (newLoc)) {
							
							if (size.row < OpenList [newLoc].Size.row) {
								//Смена значание Node требует смены индекса в дереве
								int ff = OpenList [newLoc].Size.row + OpenList [newLoc].Size.col; 
								Tree [ff].Remove (newLoc);
								if (Tree [ff].Count == 0)
									Tree.Remove (ff);
								if (ff == head) {
									head = int.MaxValue;
									foreach (var item in Tree.Keys) {
									if (item < head)
									head = item;
									}
								}
								
								OpenList [newLoc] = new Node (loc, size);

								if (f < head) head = f;
								if (!Tree.ContainsKey (f)) Tree.Add (f, new List<Location> ());								
								Tree [f].Add (newLoc);
							}
						} else {
							OpenList.Add (newLoc, new Node (loc, size));
							if (f < head)	head = f;
							if (!Tree.ContainsKey (f)) Tree.Add (f, new List<Location> ());
						
							Tree [f].Add (newLoc);	
						}
					}
				}
				if (Tree.Keys.Count > 0) {
					//loc = Tree [head] [Tree[head].Count - 1];
					loc = Tree [head] [0];
					CloseList.Add (loc, OpenList [loc]);
					OpenList.Remove (loc);
					Tree [head].Remove (loc);
					if (Tree [head].Count == 0) {
						Tree.Remove (head);
						head = int.MaxValue;
						foreach (var item in Tree.Keys) {
							if (item < head)
								head = item;
						}
					}
				} else {
					break;
				}

				if ((TimeRemaining < 30) || ((DateTime.Now - start).Milliseconds > TurnTime / 10))
					//return new List<char> ();
					break;
				
				if (CloseList.ContainsKey (loc2))
					finish = false;
			}
			
			Location find = loc1;
			Location old;
			
			
			if (CloseList.ContainsKey (loc2)) {
				//Точка достижима
				find = loc2;
			} else {
				//Точка не достижима
				int h = int.MaxValue;
				foreach (var item in CloseList.Keys) {
					if ((CloseList [item].Size.col < h) && ((item.row != loc1.row) || (item.col != loc1.col))) {
						h = CloseList [item].Size.col;
						find = CloseList [item].Parent;
					}
				}
				
			}
			
			while (find != loc1) {
				old = CloseList [find].Parent;
				directions.InsertRange (0, direction (old, find));
				find = old;
			}

			
			return directions;
			/*
			//Вышли т.к. не пути
			
			/**/

		}
	}
}

