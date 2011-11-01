using System;
using System.IO;
using System.Collections.Generic;

namespace Ants
{
	class MyBot : Bot
	{
		
		private Random rng;
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
			
			#region setup turn
			currentTurn.Clear ();
			
			Location[] hod = new Location[] {new Location (0, -state.ViewRadius2 - 1), new Location (0, state.ViewRadius2 + 1),	
				new Location (-state.ViewRadius2 - 1, 0), new Location (state.ViewRadius2 + 1, 0) ,
				new Location (-state.ViewRadius2 / 2 - 1, -state.ViewRadius2 / 2 + 1), 
				new Location (state.ViewRadius2 / 2 + 1, state.ViewRadius2 / 2 + 1),	
				new Location (state.ViewRadius2 / 2 + 1, -state.ViewRadius2 / 2 - 1),
				new Location (-state.ViewRadius2 / 2 - 1, state.ViewRadius2 / 2 + 1)};
			/**/
			Location[] smeshnie = new Location[] {new Location (0, -state.ViewRadius2), new Location (0, state.ViewRadius2),	
				new Location (-state.ViewRadius2, 0), new Location (state.ViewRadius2, 0)};
			#endregion
			
			#region Setup Discovery Aims
			HashSet<Location > Discovery = new HashSet<Location> ();
			for (int row = 0; row <= state.Height / (state.ViewRadius2 / 2); row++) {
				for (int col = 0; col <= state.Width / (state.ViewRadius2 / 2); col++) {
					Discovery.Add (new Location (row, col));
				}
			}
			
			foreach (var ant in state.MyAnts) {
				Location tmp = new Location (ant.row / (state.ViewRadius2 / 2), ant.col / (state.ViewRadius2 / 2));
				if (Discovery.Contains (tmp))
					Discovery.Remove (tmp);
			}
			#endregion
			
			#region Find Guard for my Hills
			Location[] g = new Location[4] {new Location (-1, -1), new Location (-1, 1), new Location (1, -1), new Location (1, 1)};
			
			IDictionary<AntLoc, Location > Guard = new Dictionary<AntLoc, Location> ();
			foreach (var hill in state.MyHills) {
				//foreach (var dest in Ants.Aim.Keys) {
				foreach (var dest in g) {
					Location loc = state.destination (hill, dest);
					if (state.passable (loc)) {
						Guard.Add (hill, loc);
						break;
					}
				}
			}
			#endregion
			
			#region clear oldturn from deadants
			foreach (Location dead in state.DeadTiles) {
				AntLoc ant = new AntLoc (dead, 0);
				if (oldTurn.ContainsKey (ant))
					oldTurn.Remove (ant);
			}
			#endregion
			
			#region aim ants to food + Enemy hills
			
			HashSet<Location> tmpFood = new HashSet<Location> ();		
			
			//Add Enemy Hill to AIM food :) and remove aim food from oldTurn
			foreach (var hill in state.EnemyHills) {
				Location tmp = new Location (hill.row, hill.col);
				tmpFood.Add (tmp);
			}
			foreach (var food in state.FoodTiles) {
				Location tmp = new Location (food.row, food.col);
				if (!oldTurn.Values.Contains (tmp))
					tmpFood.Add (tmp);
			}
			
			HashSet<Location> hungryAnts = new HashSet<Location> ();
			IDictionary<AntLoc, int> distToFood = new Dictionary<AntLoc, int> ();
			foreach (var ant in state.MyAnts) {
				if (oldTurn.ContainsKey (ant))
				if (state.FoodTiles.Contains (oldTurn [ant])) {
					currentTurn.Add (ant, oldTurn [ant]);
					distToFood.Add (ant, state.distance (ant, oldTurn [ant]));
				} else
					hungryAnts.Add (ant);
			}
			
			
			//find distance ants to food
			IDictionary<int, HashSet<Node>> foodFind = new Dictionary<int, HashSet<Node>> ();
			
			
			foreach (Location food in tmpFood) {
				foreach (AntLoc ant in hungryAnts) {
					int dist = state.distance (ant, food);
					if (dist > (state.ViewRadius2 * 2))
						continue;
					if (!foodFind.ContainsKey (dist))
						foodFind.Add (dist, new HashSet<Node> ());
					foodFind [dist].Add (new Node (ant, food));
				}
			}
			//sort distans
			int[] sortDist = new int[foodFind.Keys.Count];
			foodFind.Keys.CopyTo (sortDist, 0);
			Array.Sort (sortDist);
			
			//add optimum to aim
			foreach (int dist in sortDist) {
				foreach (Node node in foodFind[dist]) {
					AntLoc ant = new AntLoc (node.Parent, 0);
					if (!currentTurn.ContainsKey (ant) && !currentTurn.Values.Contains (node.Size)) {
						currentTurn.Add (ant, node.Size);
						distToFood.Add (ant, dist);
						#if DEBUG
						sw.WriteLine (" ant " + ant + " food " + currentTurn[ant]);
						#endif	
					}
				}
			}

			#endregion
			
			
			
			#region Enemy aim
			IDictionary<int, HashSet<Node>> enemyFind = new Dictionary<int, HashSet<Node>> ();
			IDictionary<Location, int> enemeys = new Dictionary<Location, int> ();
			IDictionary<Location, int> friends = new Dictionary<Location, int> ();
			
			foreach (AntLoc enemy in state.EnemyAnts) {
				foreach (AntLoc ant in state.MyAnts) {
					int dist = state.distance (ant, enemy);
					if (dist > (3 * state.ViewRadius2) / 2)
						continue;
					
					if (!enemyFind.ContainsKey (dist))
						enemyFind.Add (dist, new HashSet<Node> ());
					
					if (!enemeys.ContainsKey (ant))
						enemeys.Add (ant, 0);
					if (!friends.ContainsKey (enemy))
						friends.Add (enemy, 0);
					
					enemyFind [dist].Add (new Node (ant, enemy));
					
					if (dist < state.AttackRadius2 + 2) {
						enemeys [ant]++;
					}
					if (dist < state.AttackRadius2 + 2) {
						friends [enemy]++;
					}

				}
			}
			sortDist = new int[enemyFind.Keys.Count];
			enemyFind.Keys.CopyTo (sortDist, 0);
			Array.Sort (sortDist);
			
			foreach (int dist in sortDist) {
				
				if (dist < state.AttackRadius2 + 4) {
					foreach (Node node in enemyFind[dist]) {
						AntLoc ant = new AntLoc (node.Parent, 0);
						Location enemy = node.Size;
						
						if (friends [enemy] <= enemeys [ant]) {
							int distRun = 0;
							foreach (Location loc in hod) {
								Location newLoc = state.destination (ant, loc);
								int tmp = state.distance (newLoc, enemy);
								if (distRun < tmp) {
									distRun = tmp;
									enemy = newLoc;
								}
							}
							
						}
						if (!currentTurn.ContainsKey (ant)) { 
							currentTurn.Add (ant, enemy);
						} else {
							currentTurn [ant] = enemy;
						}	
					}
				}
					
				
//				if (dist > state.AttackRadius2 + 2) {
//					foreach (Node node in enemyFind[dist]) {
//						AntLoc ant = new AntLoc (node.Parent, 0);
//						if (!currentTurn.ContainsKey (ant)) {
//							currentTurn.Add (ant, node.Size);
//							#if DEBUG
//							sw.WriteLine (" ant " + ant + " attack " + node.Size);
//							#endif	
//						} else {
//							if (distToFood.ContainsKey(ant)) {
//								if (dist < distToFood[ant]) {
//									currentTurn[ant] = node.Size;
//									#if DEBUG
//									sw.WriteLine (" ant " + ant + " attack " + node.Size);
//									#endif	
//								}
//
//							}
//						}
//					}
//				} else {
//					foreach (Node node in enemyFind[dist]) {
//						
//						AntLoc ant = new AntLoc (node.Parent, 0);
//						Location enemy = node.Size;
//						
//						#if DEBUG
//							sw.WriteLine ("friens " + friends[enemy] + " enemy " + enemeys[ant]);
//						#endif	
//						
//						if (friends[enemy] <=  enemeys[ant]) {
//							//мы под атакой но нас меньше
//							
//							Location tmpLoc = null; 
//							/*
//							int vektorRow = 0;
//							int vektorCol = 0;
//							if (ant.row - (state.Height / 2) < enemy.row - (state.Height / 2))
//								vektorRow = -state.ViewRadius2 - rng.Next (state.AttackRadius2);
//							else
//								vektorRow = state.ViewRadius2 + rng.Next (state.AttackRadius2);
//							
//							if (ant.col - (state.Height / 2) < enemy.col - (state.Height / 2))
//								vektorCol = -state.ViewRadius2 - rng.Next (state.AttackRadius2);
//							else
//								vektorCol = state.ViewRadius2 + rng.Next (state.AttackRadius2);
//							
//							tmpLoc = state.destination (ant, new Location(vektorRow, vektorCol));
//							/**/
//							//сваливаем
//							
//							
//							  int distRun = 0;
//							  foreach (Location loc in hod) {
//								Location newLoc = state.destination (ant, loc);
//								int tmp = state.distance (newLoc, enemy);
//								if (distRun < tmp) {
//									distRun = tmp;
//									tmpLoc = newLoc;
//								}
//							}/**/
//							
//							//самая дальния точка от врага
//							if (tmpLoc != null) enemy = tmpLoc;
//							#if DEBUG
//							sw.WriteLine (" ant " + ant + " run " + enemy + " friens ");
//							#endif	
//						}
//						if (!currentTurn.ContainsKey (ant)) { 
//							currentTurn.Add(ant, enemy);
//						} else {
//							currentTurn[ant] = enemy;
//						}
//						#if DEBUG
//						sw.WriteLine (" ant " + ant + " attack " + enemy);
//						#endif	
//					}
//				}
			}
			

			#endregion
			
			#region Move other ants
			foreach (var ant in state.MyAnts) {
				if (!currentTurn.ContainsKey (ant)) {
					
					Location aim = smeshnie [rng.Next (4)];
					if (Discovery.Count > 0) { 
						int dist = int.MaxValue;
						Location aiDiscovery = null;
						foreach (var loc in Discovery) {
							Location aimTmp = new Location (loc.row * (state.ViewRadius2 / 2), loc.col * (state.ViewRadius2 / 2));
							int tmp = state.distance (aimTmp, ant);
							if (tmp < dist) {
								dist = tmp;
								aim = aimTmp;
								aiDiscovery = loc;
							}
						}
						if (aiDiscovery != null)
							Discovery.Remove (aiDiscovery);
						aim = new Location (aim.row + rng.Next (state.ViewRadius2 / 2), aim.col + rng.Next (state.ViewRadius2 / 2));
					} 					
					if (oldTurn.ContainsKey (ant)) {
						if ((aim.col != oldTurn [ant].col) && (aim.row != oldTurn [ant].row))
							aim = oldTurn [ant];
					}
					
					currentTurn.Add (ant, aim);
					#if DEBUG
					sw.WriteLine (" ant " + ant + " move " + aim);
					#endif	
				}			
			}
			#endregion
			
			#region Setup guard
			if (state.MyAnts.Count > 4 * state.MyHills.Count) {
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

			
			#region runs Ants
			#if DEBUG
			sw.WriteLine("runs ANTS");
			#endif			
			
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
						state.ClearMap(ant.row, ant.col);
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