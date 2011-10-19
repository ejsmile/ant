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
			return row << 14 + col;
		}
		public override bool Equals (object obj)
		{
			if (obj == null) return false;
			//if (obj.GetType() != Ants.Location) return false;

			Location loc = (Location)obj;
			return (this.row == loc.row) && (this.col == loc.col) ;
		}

	}
	
	public class AntLoc : Location {
		public int team { get; private set; }
		
		public AntLoc (int row, int col, int team) : base (row, col) {
			this.team = team;
		}
		public AntLoc (Location loc, int team): base(loc.row, loc.col) {
			this.team = team;
		}
		
		public override int GetHashCode ()
		{
			return row << 14 + col;
		}
		public override bool Equals (object obj)
		{
			//TODO Может надо учитывать команду
			if (obj == null) return false;
			Location loc = obj as Location;
			if ((System.Object)loc == null) return false;
			return (this.row == loc.row) && (this.col == loc.col) ;
		}
	}
	
	public class LocationComparer : IEqualityComparer<Location> {
		public bool Equals(Location loc1, Location loc2) {
			return (loc1.row == loc2.row && loc1.col == loc2.col);
		}
	
		public int GetHashCode(Location loc) {
			return loc.row << 14 + loc.col;
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

