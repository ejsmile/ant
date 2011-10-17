using System;
using System.Collections.Generic;

namespace Ants {

	class MyBot : Bot {
		
		private Random rng;
		private HashSet<Location> destinations;
		private IDictionary<AntLoc, Location> ready;
		
		public MyBot()
		{
			rng = new Random(42);
			destinations = new HashSet<Location>(new LocationComparer());
			ready = new Dictionary<AntLoc, Location>();
		}
		
		// doTurn is run once per turn
		public override void doTurn (GameState state) {
			destinations.Clear();
			ready.Clear();			
			
			//find aim for ants in enemy
			foreach(Location aim in state.EnemyAnts) {
				int minDist = int.MaxValue;
				AntLoc _aim = null;
				foreach(AntLoc ant in state.MyAnts) {
					if (!ready.ContainsKey(ant)) {
						int dist = state.distance(ant, aim);
						if (dist < minDist) {
							_aim = ant;
							minDist = dist;
						}
					}
				}
				if (_aim != null) {
					ready.Add(_aim, aim);
					state.MyAnts.Remove(_aim);
				}
			}
			
			//find aim for ants in food
			foreach(Location aim in state.FoodTiles) {
				int minDist = int.MaxValue;
				AntLoc _aim = null;
				foreach(AntLoc ant in state.MyAnts) {
					if (!ready.ContainsKey(ant)) {
						int dist = state.distance(ant, aim);
						if (dist < minDist) {
							_aim = ant;
							minDist = dist;
						}
					}
				}
				if (_aim != null) {
					ready.Add(_aim, aim);
					state.MyAnts.Remove(_aim);
				}
			}
			
			
			foreach (AntLoc ant in ready.Keys)
			{
				bool pass = false;

				IEnumerable<char> directions = state.direction(ant, ready[ant]);
				foreach (char direction in directions) {
					Location newLoc = state.destination(ant, direction);
					
					if (state.unoccupied(newLoc) && !destinations.Contains(newLoc)) {
						issueOrder(ant, direction);
						destinations.Add(newLoc);
						pass = true;
						break;
					}
				}
				if (pass) state.MyAnts.Add(ant);
				if (state.TimeRemaining < 10) break;
			}
			
			foreach (AntLoc ant in state.MyAnts)
			{
				IEnumerable<char> directions = Ants.Aim.Keys.Shuffle(rng);
				
				foreach (char direction in directions) {
					Location newLoc = state.destination(ant, direction);
					
					if (state.unoccupied(newLoc) && !destinations.Contains(newLoc)) {
						issueOrder(ant, direction);
						destinations.Add(newLoc);
						break;
					}
				}
				if (state.TimeRemaining < 10) break;
			}
			
			
		}		
		
		public static void Main (string[] args) {
			new Ants().playGame(new MyBot());
		}

	}
	
}