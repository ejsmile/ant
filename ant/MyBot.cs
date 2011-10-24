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
			Location[] hod = new Location[] {new Location (0, -state.AttackRadius2), new Location (0, state.AttackRadius2),	new Location (-state.AttackRadius2, 0),
				new Location (state.AttackRadius2, 0) ,new Location (-state.AttackRadius2 / 2, -state.AttackRadius2 / 2), 
				new Location (state.AttackRadius2 / 2, state.AttackRadius2 / 2),	new Location (state.AttackRadius2 / 2, -state.AttackRadius2 / 2),
				new Location (-state.AttackRadius2 / 2, state.AttackRadius2 / 2)};
			

			//Food
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

			
			//Add Enemy Hill to AIM
			state.EnemyHills.AddRange (state.EnemyHills);
			
			//find Enemy aim 
			foreach (var ant in state.MyAnts) {
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
					if (dist < state.AttackRadius2)
						attakEnemy++;
				}
				
				if (aimEnemy != null) {
				
					//find frinds in  state.AttackRadius2
					int attakFrinds = 0;
					int saveDist = state.AttackRadius2 + 1;	
					if (aimEnemy != null)
						foreach (var friends in state.MyAnts) {
							dist = state.distance (aimEnemy, friends);
							if (dist < saveDist) { 
								attakFrinds++;
							}
						}
					#if DEBUG
					sw.WriteLine (" we " + attakFrinds + " they " + attakEnemy);
					#endif
					
					//I am alone
					if ((attakFrinds <= attakEnemy) && (aimEnemy != null) && (distEnemy < state.AttackRadius2 + 2)) {
					
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
			
			//move other ants
			foreach (var ant in state.MyAnts) {
				if (!currentTurn.ContainsKey (ant)) {
					int shaftRow = (state.ViewRadius2 + (state.ViewRadius2)) * ((-1) ^ rng.Next (4));
					int shaftCol = (state.ViewRadius2 + rng.Next (state.ViewRadius2)) * ((-1) ^ rng.Next (4));
					Location aim = state.destination (ant, new Location (shaftRow, shaftCol));
				
					if (oldTurn.ContainsKey (ant)) 
					if ((ant.row != oldTurn [ant].row) && (ant.col != oldTurn [ant].col))
						aim = oldTurn [ant];
					currentTurn.Add (ant, aim);
					#if DEBUG
					sw.WriteLine (" ant " + ant + " move " + aim);
					#endif	
				}			
			}
			
			#if DEBUG
			sw.WriteLine("runs ANTS");
			#endif	
			
			oldTurn = new Dictionary<AntLoc, Location> ();
			
			foreach (AntLoc ant in currentTurn.Keys) {
				List<char> directions = state.direction_algor_A (ant, currentTurn [ant]);
				
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
					}
				}/**/
				
				if (state.TimeRemaining < 50) {
					return;
				}
			}
			
			
			
		}
		
		public static void Main (string[] args)
		{
			new Ants ().playGame (new MyBot ());
		}

	}
	
}