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
			rng = new Random (42 + 42);
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
			
			#region Setup Discovery Aims
			List<Location > Discovery = new List<Location> ();
			for (int row = 0; row < state.Height / (state.ViewRadius2 / 2); row++) {
				for (int col = 0; col < state.Width / (state.ViewRadius2 / 2); col++) {
					Discovery.Add (new Location (row, col));
				}
			}
			
			foreach (var ant in state.MyAnts) {
				Location tmp = new Location (ant.row / (state.ViewRadius2 / 2), ant.col / (state.ViewRadius2 / 2));
				if (Discovery.Contains (tmp))
					Discovery.Remove (tmp);
			}
			#endregion
			
			
			Location[] hod = new Location[] {new Location (0, -state.ViewRadius2 - 1), new Location (0, state.ViewRadius2 + 1),	
				new Location (-state.ViewRadius2 - 1, 0), new Location (state.ViewRadius2 + 1, 0) ,
				new Location (-state.ViewRadius2 / 2 - 1, -state.ViewRadius2 / 2 + 1), 
				new Location (state.ViewRadius2 / 2 + 1, state.ViewRadius2 / 2 + 1),	
				new Location (state.ViewRadius2 / 2 + 1, -state.ViewRadius2 / 2 - 1),
				new Location (-state.ViewRadius2 / 2 - 1, state.ViewRadius2 / 2 + 1)};
			Location[] smeshnie = new Location[] {new Location (0, -state.ViewRadius2 - rng.Next (state.ViewRadius2)), 
				new Location (0, state.ViewRadius2 + rng.Next (state.ViewRadius2)),	
				new Location (-state.ViewRadius2 - rng.Next (state.ViewRadius2), 0), 
				new Location (state.ViewRadius2 + rng.Next (state.ViewRadius2), 0)};
			
			#region Find Guard for my Hills
			IDictionary<AntLoc, Location > Guard = new Dictionary<AntLoc, Location> ();
			foreach (var hill in state.MyHills) {
				foreach (var dest in Ants.Aim.Keys) {
					Location loc = state.destination (hill, dest);
					if (state.passable (loc)) {
						Guard.Add (hill, loc);
						break;
					}
				}
			}
			#endregion

			#region aim ants to food + Enemy hills
			
			//Add Enemy Hill to AIM food :)
			foreach (var hill in state.EnemyHills) {
				state.FoodTiles.Add (new Location (hill.row, hill.col));
			}

			
			foreach (var food in state.FoodTiles) {
				int dist = int.MaxValue;
				AntLoc antFood = null;
			
				foreach (var ant in state.MyAnts) {
					if (!currentTurn.ContainsKey (ant))
					if (state.distance (ant, food) < dist) {
						dist = state.distance (ant, food);
						antFood = ant;
					}
					
				}
				if (antFood != null) {
					currentTurn.Add (antFood, food);
					#if DEBUG
					sw.WriteLine (" ant " + antFood + " food " + food);
					#endif	
					
				} else
					break;
			}
			#endregion
			
			
			
			#region Enemy aim
			foreach (var ant in state.MyAnts) {
				int distEnemy = state.ViewRadius2 * 3;
				Location aimEnemy = null;				
				int dist;
				int attakEnemy = 0;
				
				foreach (var enemy in state.EnemyAnts) {
					dist = state.distance (ant, enemy);
					if (dist < distEnemy) {
						distEnemy = dist;
						aimEnemy = enemy;
					}
					if (dist < state.AttackRadius2 + 1)
						attakEnemy++;
				}
				
				if (aimEnemy != null) {
				
					//find frinds in  state.AttackRadius2
					int attakFrinds = 0;
					int saveDist = state.AttackRadius2 + 1;	
					foreach (var friends in state.MyAnts) {
						dist = state.distance (aimEnemy, friends);
						if (dist < saveDist) { 
							attakFrinds++;
						}
					}
					#if DEBUG
					sw.WriteLine (" ant " + ant + " friends " + attakFrinds + " they " + attakEnemy + " aim " + aimEnemy);
					#endif
					
					//I am alone
					if ((attakFrinds <= attakEnemy) && (distEnemy < state.AttackRadius2 + 3)) {	
						int runDist = distEnemy;
						
						Location runLoc = null;
						foreach (Location loc in hod) {
							Location newLoc = state.destination (ant, loc);
							if (state.unoccupied (newLoc)) {
								if (runDist < state.distance (newLoc, aimEnemy)) {
									runDist = state.distance (newLoc, aimEnemy);
									runLoc = newLoc;
								}
							}
						}
						if (runLoc != null) {
							aimEnemy = runLoc;
							distEnemy = runDist;
						}
						#if DEBUG
						sw.WriteLine (" ant " + ant + " run  from enemy to " + aimEnemy);
						#endif				
					}
					/**/
					if (currentTurn.ContainsKey (ant)) {
						int tmp = state.distance (ant, currentTurn [ant]);
						if (tmp > distEnemy) {
							Location food = currentTurn [ant];
							currentTurn [ant] = aimEnemy;
							aimEnemy = null;
							tmp = int.MaxValue;
							
							foreach (var ants in state.MyAnts) {
								if (!currentTurn.ContainsKey (ants) && (state.distance (ant, food) < tmp)) {
									tmp = state.distance (ant, food);
									aimEnemy = ants;
								}
							}
							if (aimEnemy != null)
								currentTurn.Add (new AntLoc (aimEnemy, 0), food);
						}
					} else
						currentTurn.Add (ant, aimEnemy);
				}
			}
			#endregion
			
			#region Move other ants
			foreach (var ant in state.MyAnts) {
				if (!currentTurn.ContainsKey (ant)) {
					
					Location aim = smeshnie [rng.Next (4)];
					if (Discovery.Count > 0) { 
						int dist = int.MaxValue;
						foreach (var loc in Discovery) {
							Location aimTmp = new Location (loc.row * (state.ViewRadius2 / 2), loc.col * (state.ViewRadius2 / 2));
							int tmp = state.distance (aimTmp, ant);
							if (tmp < dist) {
								dist = tmp;
								aim = aimTmp;
							}
						}
					} 					
					if (oldTurn.ContainsKey (ant)) {
						if (state.distance (ant, oldTurn [ant]) > (state.ViewRadius2 / 2)) {
							aim = oldTurn [ant];
						}
					}
					
					currentTurn.Add (ant, aim);
					#if DEBUG
					sw.WriteLine (" ant " + ant + " move " + aim);
					#endif	
				}			
			}
			#endregion
			
			#region Setup guard
			if (state.MyAnts.Count > 5 * state.MyHills.Count) {
				foreach (var hill in Guard.Keys) {
					AntLoc tmp = new AntLoc (Guard [hill], 0);
					if (currentTurn.ContainsKey (tmp)) {
						currentTurn [tmp] = Guard [hill];
					} else {
					
						int dist = int.MaxValue;
						AntLoc antGuard = null;
						foreach (var ant in state.MyAnts) {
							if (state.distance (hill, ant) < dist) {
								dist = state.distance (hill, ant);
								antGuard = ant; 
							}
						}
						if (antGuard != null) 
						if (currentTurn.ContainsKey (antGuard))
							currentTurn [antGuard] = Guard [hill];
						else
							currentTurn.Add (antGuard, Guard [hill]);
					}
				}
			}
			#endregion
			/**/
			#if DEBUG
			sw.WriteLine("runs ANTS");
			#endif	
			
			#region runs Ants
			oldTurn = new Dictionary<AntLoc, Location> ();
			
			foreach (AntLoc ant in currentTurn.Keys) {
				List<char > directions = state.direction_algor_A (ant, currentTurn [ant]);
				
				if (directions.Count == 0) {
					//добавление препядствия
					state.addAnt (ant.row, ant.col, 0);
					#if DEBUG
					sw.WriteLine (" ant " + ant + " stop ");
					#endif	
				} else {
					AntLoc NewAnt = new AntLoc (state.destination (ant, directions [0]), 0);
					//странно пытаемся переститься на своего муровья
					if (!oldTurn.ContainsKey (NewAnt)) {
						issueOrder (ant, directions [0]);
						//добавление препядствия
						state.addAnt (NewAnt.row, NewAnt.col, 0);
						oldTurn.Add (NewAnt, currentTurn [ant]);
						#if DEBUG
						sw.WriteLine (" ant " + ant + " move " + NewAnt);
						#endif						
					} else {
						state.addAnt (ant.row, ant.col, 0);
					}
				}/**/
				
				if (state.TimeRemaining < 50) {
					return;
				}
			}
			#endregion			
		}
		
		public static void Main (string[] args)
		{
			new Ants ().playGame (new MyBot ());
		}

	}
	
}