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
		//private IDictionary<AntLoc, Location> oldTurn;
		private FileStream fs;
		private StreamWriter sw;
		private int number = 1;
		
		public MyBot ()
		{
			rng = new Random (42);
			destinations = new HashSet<Location> (new LocationComparer ());
			currentTurn = new Dictionary<AntLoc, Location> ();
			//oldTurn = new Dictionary<AntLoc, Location> ();
			fs = new FileStream ("log.log", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
			sw = new StreamWriter (fs);
		}
		
		// doTurn is run once per turn
		public override void doTurn (GameState state)
		{
			sw.WriteLine ("!Turn " + number++);
			destinations.Clear ();
			currentTurn.Clear ();
			state.EnemyHills.AddRange(state.EnemyHills);
			
			foreach (var ant in state.MyAnts) {
				//TODO find new Aim
				
				int distEnemy = int.MaxValue;
				Location aimEnemy = null;
					
				foreach (var enemy in state.EnemyAnts) {
					if (state.distance (ant, enemy) < distEnemy) {
						distEnemy = state.distance (ant, enemy);
						aimEnemy = enemy;
					}			
				}
				
				
				int distFood = int.MaxValue;
				Location aimFood = null;
					
				foreach (var food in state.FoodTiles) {
					if (state.distance (ant, food) < distFood) {
						distFood = state.distance (ant, food);
						aimFood = food;
					}			
				}
				
				if ((aimFood == null) && (aimEnemy == null)) {
					int shaftRow = (state.ViewRadius2 + rng.Next (state.ViewRadius2 * 2)) * ((-1) ^ rng.Next (2));
					int shaftCol = (state.ViewRadius2 + rng.Next (state.ViewRadius2 * 2)) * ((-1) ^ rng.Next (2));
					
					aimFood = state.destination (ant, new Location (ant.row + shaftRow, ant.col + shaftCol));
					sw.WriteLine (ant + " goto new random " + aimFood);
					currentTurn.Add (ant, aimFood);
					continue;
				}
				
				if ((aimFood != null) && (aimEnemy != null)) {
					if (distEnemy < distFood)
						currentTurn.Add (ant, aimEnemy);
					else
						currentTurn.Add (ant, aimFood);
					continue;
				}
				if (aimFood == null) {
					currentTurn.Add (ant, aimEnemy);
					continue;
				} else {
					currentTurn.Add (ant, aimFood);
				}
				
			}
			
			//Clear oldTurn for new data
			//move ant to thei aim
			foreach (AntLoc ant in currentTurn.Keys) {
			
				IEnumerable<char> directions = state.direction_algor_A (ant, currentTurn [ant]);
				
				//FIXME clearn after write
				string st = "";
				foreach (char cc in directions) {
					st = st + cc;
				}
				sw.WriteLine (ant + " aim " + currentTurn [ant] + " goto " + st);
				/**/
				//FIXME HACK
				if (st.Length == 0) {
					destinations.Add (ant);
				} else {
					
					foreach (char direction in directions) {
						AntLoc NewAnt = new AntLoc (state.destination (ant, direction), 0);
					
						if (!destinations.Contains (NewAnt)) {
							destinations.Add (NewAnt);
							issueOrder (ant, direction);
						}
						break;
					}
				}
				if (state.TimeRemaining < 10)
					break;
			}	
		}
		
		public static void Main (string[] args)
		{
			new Ants ().playGame (new MyBot ());
		}

	}
	
}