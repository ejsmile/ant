using System;
using System.Collections.Generic;

namespace Ants {
	
	public class Location {
		
		public int row { get; private set; }
		public int col { get; private set; }
		
		public Location (int row, int col) {
			this.row = row;
			this.col = col;
		}
		public override string ToString ()
		{
			return string.Format ("[Location: row={0}, col={1}]", row, col);
		}
		public override int GetHashCode ()
		{
			return row * int.MaxValue + col;
		}
		public override bool Equals (object obj)
		{
			Location loc = (Location)obj;
			return (col == loc.col) ;
		}
	}
	
	public class AntLoc : Location {
		public int team { get; private set; }
		
		public AntLoc (int row, int col, int team) : base (row, col) {
			this.team = team;
		}
	}
	
	public class LocationComparer : IEqualityComparer<Location> {
		public bool Equals(Location loc1, Location loc2) {
			return (loc1.row == loc2.row && loc1.col == loc2.col);
		}
	
		public int GetHashCode(Location loc) {
			return loc.row * int.MaxValue + loc.col;
		}
	}
	
	public class Node
	{
		public Location Parent;
		public Location Size;
		
		public Node()
		{
			Parent = new Location(0,0);
			Size = new Location(0,0);
		}
		
		public Node (Location Parent, Location Size)
		{
			this.Parent = Parent;
			this.Size = Size;
		}
	}
}

