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
		private FileStream fs;
		private StreamWriter sw;
		private int number = 1;
		
		public MyBot ()
		{
			rng = new Random (42);
			destinations = new HashSet<Location> (new LocationComparer ());
			currentTurn = new Dictionary<AntLoc, Location> ();
			oldTurn = new Dictionary<AntLoc, Location> ();
			fs = new FileStream ("log.log", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
			sw = new StreamWriter (fs);
		}
		
		// doTurn is run once per turn
		public override void doTurn (GameState state)
		{
			sw.WriteLine ("!Turn " + number++);
			destinations.Clear ();
			currentTurn.Clear ();

			foreach (var ant in state.MyAnts) {
				if (oldTurn.ContainsKey (ant)) {
					if (state.FoodTiles.Contains (oldTurn [ant])) { 
						sw.WriteLine (ant + " old food aim" + oldTurn [ant]);
						//state.EnemyAnts.Contains (new AntLoc (oldTurn [ant], 0))) {
						currentTurn.Add (ant, oldTurn [ant]);
						//Цель есть она не достигнута и видна
						continue;
					}
					sw.WriteLine(!ant.Equals( oldTurn [ant]) + " " + ((ant.row != oldTurn [ant].row) && (ant.col != oldTurn [ant].col)) );
					//жратвы нет и но рандомная цель не достигнута
					if ((state.FoodTiles.Count == 0)
						//&& (ant != oldTurn [ant])) {
						&& ((ant.row != oldTurn [ant].row) && (ant.col != oldTurn [ant].col))) {
						sw.WriteLine (ant + " old random aim" + oldTurn [ant]);
						currentTurn.Add (ant, oldTurn [ant]);
						continue;
					}
				}

				//TODO find new Aim

				int dist = int.MaxValue;
				Location aim = null;
					
				foreach (var food in state.FoodTiles) {
					if (state.distance (ant, food) < dist) {
						dist = state.distance (ant, food);
						aim = food;
					}			
				}
				if (aim != null) {
					sw.WriteLine (ant + " goto new food " + aim);
					currentTurn.Add (ant, aim);
				} else {
					int shaftRow = (state.ViewRadius2 + rng.Next (state.ViewRadius2 * 2)) * ((-1) ^ rng.Next (2));
					int shaftCol = (state.ViewRadius2 + rng.Next (state.ViewRadius2 * 2)) * ((-1) ^ rng.Next (2));
					
					aim = state.destination (ant, new Location (ant.row + shaftRow, ant.col + shaftCol));
					sw.WriteLine (ant + " goto new random " + aim);
					currentTurn.Add (ant, aim);
				}
			}
			
			//Clear oldTurn for new data
			oldTurn = new Dictionary<AntLoc, Location> ();
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
							oldTurn.Add (NewAnt, currentTurn [ant]);
							issueOrder (ant, direction);
						} else {
							oldTurn.Add (ant, currentTurn [ant]);
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