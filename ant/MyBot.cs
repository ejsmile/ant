using System;
using System.IO;
using System.Collections.Generic;

namespace Ants
{
	class MyBot : Bot
	{
		
		private Random rng;
		private HashSet<Location> destinations;
		private IDictionary<AntLoc, Location> ready;
		private IDictionary<AntLoc, Location> aims;
		private FileStream fs;
		private StreamWriter sw;
		private int number = 1;
		
		public MyBot ()
		{
			rng = new Random (42);
			destinations = new HashSet<Location> (new LocationComparer ());
			ready = new Dictionary<AntLoc, Location> ();
			aims = new Dictionary<AntLoc, Location> ();
			fs = new FileStream ("log.log", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
			sw = new StreamWriter (fs);
		}
		
		// doTurn is run once per turn
		public override void doTurn (GameState state)
		{
			sw.WriteLine ("!Turn " + number++);
			destinations.Clear ();
			ready.Clear ();	
			//Clear fantom aims
			/*
			string st = "";
			foreach(var item in state.MyAnts)
			{
				st = st + " | " + item;
			}
			sw.WriteLine(st);
			
			st = "";
			foreach(var item in state.MyHills)
			{
				st = st + " X " + item;
			}
			sw.WriteLine(st);
			
			
			sw.WriteLine(state.MyAnts[1]);
			IEnumerable<char> directions1 = state.direction_algor_A (state.MyAnts[1], new Location(4,5));
			
			st = "path ";
			foreach(char c in directions1){
				st = st + c;
			}
			sw.WriteLine(st);
			/**/
			//clear my hill
			foreach (var hill in state.MyHills) {
				foreach (var item in state.MyAnts) {
					if ((hill.row == item.row) && (hill.col == item.col)) {
						foreach (char c in Ants.Aim.Keys) {
							Location loc = state.destination (hill, c);
							if (state.unoccupied (loc) && !destinations.Contains (loc)) {
								if (!aims.ContainsKey (item))
									aims.Add (item, loc);
								//ready.Add(item, loc);
								break;//add one moves
							}
						}
						break;
					}
				}
			}

			/**/
			
			//find aim for ants in enemy
			foreach (Location aim in state.EnemyAnts) {
				//TODO emeny can move ??? what find myant is this emeny
				//This enemy is aim myants
				
				/*
				//list variuos enemy was past turn
				
				IList<AntLoc> enemys = new List<AntLoc>();
				enemys.Add(aim);
				
				foreach(var item in Ants.Aim.Keys)
				{
					enemys(destination (loc, c));
				}
				
				bool find = false;
				foreach(var item in enemys) {
					foreach(var ant in aims.keys)
					{
						if ((aims[ant].row == item.row) && (aims[ant].col == item.col))
						{
						 //find
							// not aims.Remove(ant);
						}
					}
					if (find) break;
				}
				if (find) continue;
				//this new enemy
				/**/
				int minDist = int.MaxValue;
				AntLoc _ant = null;
				foreach (AntLoc ant in state.MyAnts) {
					int dist = state.distance (ant, aim);
					if (dist < minDist) {
						_ant = ant;
						minDist = dist;
					}
				}
				if (_ant != null) {	
					sw.WriteLine (_ant + " to enemy " + aim);
					if (!ready.ContainsKey (_ant))
						ready.Add (_ant, aim);
					state.MyAnts.Remove (_ant);
					if (aims.ContainsKey (_ant))
						aims.Remove (_ant);
				}
				//}
			}
			
			
			//find aim for ants in food
			foreach (Location aim in state.FoodTiles) {
				//This food is ants aim?
				if (!aims.Values.Contains (aim)) {
					//Not
					int minDist = int.MaxValue;
					AntLoc _ant = null;
				
					foreach (AntLoc ant in state.MyAnts) {
						int dist = state.distance (ant, aim);
						if (dist < minDist) {
							_ant = ant;
							minDist = dist;
						}					
					}				
					if (_ant != null) {
						sw.WriteLine (_ant + " to food " + aim);					
						if (!ready.ContainsKey (_ant))
							ready.Add (_ant, aim);
						state.MyAnts.Remove (_ant);
						if (aims.ContainsKey (_ant))
							aims.Remove (_ant);
					}
				}
			}

			
			//Find Aim  for other Ants
			
			foreach (var ant in state.MyAnts) {	

				Location aim;
				switch (rng.Next (3)) {
				default:
					aim = new Location ((ant.row + state.ViewRadius2 * 4) % state.Width 
							, (ant.col + state.ViewRadius2 * 4) % state.Height);
					break;
				case 0:
					aim = new Location (ant.row, (ant.col + state.ViewRadius2 * 4) % state.Height);
					break;
				case 1:
					aim = new Location ((ant.row + state.ViewRadius2 * 4) % state.Width, ant.col);
					break;					
				}
				
				if (aims.ContainsKey (ant)) {
					
					
					if ((ant.row == aims [ant].row) && (ant.col == aims [ant].col)) {
						sw.WriteLine (ant + " change " + aim);
						aims [ant] = aim;
						if (!ready.ContainsKey (ant))
							ready.Add (ant, aims [ant]);
					} else {
						sw.WriteLine (ant + " aim:" + aim);
						if (!ready.ContainsKey (ant))
							ready.Add (ant, aims [ant]);
					}/**/
					
					
				} else {
					sw.WriteLine ("new aim" + aim);
					aims.Add (ant, aim);
					if (!ready.ContainsKey (ant))
						ready.Add (ant, aim);
				}				
			}/**/
			
						
			//move ant to thei aim
			foreach (AntLoc ant in ready.Keys) {
				
				
				IEnumerable<char > directions = state.direction_algor_A (ant, ready [ant]);
				
				//path is null remove in famtom aim
				/*if (!directions.GetEnumerator().MoveNext()) {
					if (aims.ContainsKey (ant)) aims.Remove (ant);
					continue;
				}
				
				/**/
				string st = "";
				foreach (char cc in directions) {
					st = st + cc;
				}
				sw.WriteLine (ant + " aim " + ready [ant] + " goto " + st);
				/**/
				
				foreach (char direction in directions) {
					AntLoc NewAnt = new AntLoc (state.destination (ant, direction), 0);
					
					if (!destinations.Contains (NewAnt)) {
						destinations.Add (NewAnt);
					
						if (aims.ContainsKey (ant)) {
							if (!aims.ContainsKey (NewAnt))
								aims.Add (NewAnt, aims [ant]);
							aims.Remove (ant);
						}
					}
					issueOrder (ant, direction);
					break;
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