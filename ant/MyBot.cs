using System;
using System.IO;
using System.Collections.Generic;

namespace Ants
{
	class MyBot : Bot
	{
		
		private Random rng;
		private HashSet<Location> destinations;
		private IDictionary<AntLoc, Location> currentTurn;
		private IDictionary<AntLoc, Location> oldTurn;
		#if DEBUG
		private FileStream fs;
		private StreamWriter sw;
		private int number = 1;
		#endif
		
		public MyBot ()
		{
			rng = new Random (42);
			destinations = new HashSet<Location> (new LocationComparer ());
			currentTurn = new Dictionary<AntLoc, Location> ();
			oldTurn = new Dictionary<AntLoc, Location> ();
			#if DEBUG
			fs = new FileStream ("log.log", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
			sw = new StreamWriter (fs);
			#endif
		}
		
		// doTurn is run once per turn
		public override void doTurn (GameState state)
		{
			#if DEBUG
			sw.WriteLine ("!Turn " + number++);
			#endif
			destinations.Clear ();
			currentTurn.Clear ();
			//Add Enemy Hill to AIM
			state.EnemyHills.AddRange (state.EnemyHills);
			
			foreach (var ant in state.MyAnts) {
				
				//
				//int distEnemy = int.MaxValue;
				int distEnemy = state.ViewRadius2 * 2;
				Location aimEnemy = null;				
				int dist;
				int attakEnemy = 0;
				
				foreach (var enemy in state.EnemyAnts) {
					dist = state.distance (ant, enemy);
					if (dist < distEnemy) {
						distEnemy = dist;
						aimEnemy = enemy;
					}
					if (dist <= state.AttackRadius2 + 2)
						attakEnemy++;
				}
				
				#if DEBUG
				if (aimEnemy != null) sw.WriteLine (ant + " view enemy " + aimEnemy + " dist " + distEnemy);
				#endif	
				
				//find frinds in  state.AttackRadius2
				int attakFrinds = 0;
				int saveDist = state.AttackRadius2 - 1;	
				foreach (var friends in state.MyAnts) {
					dist = state.distance (ant, friends);
					if ((dist <= saveDist) && (ant.row != friends.row) && (ant.col != friends.col)) {
						attakFrinds++;
					}
				}
				#if DEBUG
				sw.WriteLine (" we " + attakFrinds + " they " + attakEnemy);
				#endif
				//I am alone
				if ((attakFrinds <= attakEnemy) && (aimEnemy != null) && (distEnemy < state.AttackRadius2 + 1)) {
					
					int row = 0;
					if (ant.row < aimEnemy.row) {
						row = -2;
					} else if (ant.row == aimEnemy.row) {
						row = 0;
					} else {
						row = 2;
					}
					int col = 0;
					if (ant.col < aimEnemy.col) {
						col = -2;
					} else if (ant.col == aimEnemy.col) {
						col = 0;
					} else {
						col = 2;
					}
					aimEnemy = state.destination (ant, new Location (row, col));
					//aimEnemy = state.destination (aimEnemy, new Location (row, col));
					
					distEnemy = state.distance (ant, aimEnemy);
					
					//
					#if DEBUG
					sw.WriteLine (ant + " run " + aimEnemy + " dist ");
					#endif	
					//continue;

				}
				
				/**/
				//int distFood = int.MaxValue;
				int distFood = state.ViewRadius2 * 2;
				Location aimFood = null;
				
				foreach (var food in state.FoodTiles) {
					if (state.distance (ant, food) < distFood) {
						distFood = state.distance (ant, food);
						aimFood = food;
					}			
				}

				#if DEBUG
				if (aimFood != null) sw.WriteLine (ant + " view food " + aimFood + " dist " + distFood);
				sw.WriteLine ("view " + (aimFood == null) + " enemy " + (aimEnemy == null));
				#endif	
				
				
				if ((aimFood == null) && (aimEnemy == null)) {
					if (oldTurn.ContainsKey (ant)) {
						#if DEBUG
						sw.WriteLine (ant + " goto old aim " + oldTurn[ant]);
						#endif											
					} else {
						int shaftRow = 0;
						int shaftCol = 0;
						
						shaftRow = (state.ViewRadius2 + rng.Next (state.ViewRadius2 * 2)) * ((-1) ^ rng.Next (2));
						shaftCol = (state.ViewRadius2 + rng.Next (state.ViewRadius2 * 2)) * ((-1) ^ rng.Next (2));
						aimFood = state.destination (ant, new Location (shaftRow, shaftCol));
						#if DEBUG
						sw.WriteLine (ant + " goto new random " + aimFood);
						#endif					
						oldTurn.Add (ant, aimFood);
					}
					continue;
				}
				
				if (oldTurn.ContainsKey(ant)) oldTurn.Remove(ant);
				
				if ((aimFood != null) && (aimEnemy != null)) {
					
					if (distEnemy < distFood) {
						
						currentTurn.Add (ant, aimEnemy);
						#if DEBUG
						sw.WriteLine (ant + " goto enemy " + aimEnemy);
						#endif	
					}
					else {
						currentTurn.Add (ant, aimFood);
						#if DEBUG
						sw.WriteLine (ant + " goto food " + aimFood);
						#endif	
					}
					continue;
				}
				if (aimFood == null) {
					currentTurn.Add (ant, aimEnemy);
					continue;
				} else {
					sw.WriteLine ("ant " + ant + " " + aimFood);
					currentTurn.Add (ant, aimFood);
				}
				
			}
			
			#if DEBUG
			sw.WriteLine("runs ANTS");
			#endif	
			
			
			//move ant to thei aim
			//FIX
			IDictionary<AntLoc, Location> tempTurn = new Dictionary<AntLoc, Location> ();
			
			foreach (AntLoc ant in oldTurn.Keys) {
				if (currentTurn.ContainsKey (ant))
					currentTurn.Remove (ant);
				
				List<char> directions = state.direction_algor_A (ant, oldTurn [ant]);
				if (directions.Count == 0) {
					destinations.Add (ant);
				} else {
					AntLoc NewAnt = new AntLoc (state.destination (ant, directions[0]), 0);
					if (!destinations.Contains (NewAnt)) {
						destinations.Add (NewAnt);
						issueOrder (ant, directions[0]);
						tempTurn.Add (NewAnt, oldTurn [ant]);
					} else {
						tempTurn.Add (ant, oldTurn [ant]);
					}
	
				}/**/
				if (state.TimeRemaining < 50)					
					return;
			}
			
			
			oldTurn = tempTurn;
			
			foreach (AntLoc ant in currentTurn.Keys) {
				
				List<char> directions = state.direction_algor_A (ant, currentTurn [ant]);
				
				//FIXME clearn after write
				string st = "";
				foreach (char cc in directions) {
					st = st + cc;
					break;
				}
				//FIXME HACK
				if (directions.Count == 0) {
					destinations.Add (ant);
				} else {
					AntLoc NewAnt = new AntLoc (state.destination (ant, directions [0]), 0);
				
					if (!destinations.Contains (NewAnt)) {
						destinations.Add (NewAnt);
						issueOrder (ant, directions[0]);
					}
				}
				
				if (state.TimeRemaining < 50)					
					return;
			}
			
		}
		
		public static void Main (string[] args)
		{
			new Ants ().playGame (new MyBot ());
		}

	}
	
}