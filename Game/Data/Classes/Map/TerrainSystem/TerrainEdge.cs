using Godot;
using System;

public partial class TerrainEdge : RefCounted
{
	#region CHECKLIST
	// TODO EntiretyOfThis Literally everything needs doing

	// TODO Storage
	// [ ] Storage Store the data on what height either side is
	// [ ] Storage Store the height on both ends (for ramps, i.e if the ends are different, the edge is sloped)
	
	// TODO Logic Two straight edges become one straight edge, then that edge is passed to a region to form a polygon
	// [ ] Logic Determine Straightness 
	// When checking that the edge is straight, first verify there is another edge this edge leads into (not into the edge of the chunk)
	// Next verify that edges sides are the same height as this edge's sides
	// Finally, once I'm doing ramp logic, verify that the matching ends (connecting ends between the edges) are the same height
	//in case of t-junctions
	// Might be best to store slope data in tiles? Perhaps even tiles are flat and odd tiles are slopes between the other two
	// Terrain deformation will be another heightmap using signed integers/bytes and probably processed via a shader instead of actuall polygons/regions
	//on the map

	// DIAGONALS
	// [ ] Logic Diagonals&Corners
	// To check for diagonals, store the previous change in polygon detection
	// If it was straight, do nothing
	// If it was a turn, check if it was in the same direction as the current turn
	// L->R == diagonal, L->L or R->R == corner
	// Additionally, store the previous segment's length
	// If it's greater than one, always do nothing
	// If it is one, perform the diagonal check
	// If it fails and the current segment's distance is only one as well, turn it into a corner
	//so that instead of having [diagonal segment -> one unit long segment -> diagonal segment], we have [diagonal segment -> different diagonal segment]
	// It would be nice to make it possible so 1x1 tiles can be either diagonal tiles or horizontal/vertical tiles

	// !!! ensure that every time a change in the edge is detected (straight -> turn, turn -> straight) a new edge is begun and the previous edge
	//is pushed to the region
	// And in "|_|" style turns, keep it "|_|" instead of making it "\/" by reading the data of the previous turn
	//But allow for overwrites too, maybe keep specific data for turns
	#endregion

	private byte direction;	// 0 is horizontal (-) while 1 is vertical (|)
							// Honestly a bool could be used for this but I feel that this is simpler to understand
	/*protected byte endHeightNeg; // In the negative direction. Left for direction 0, up for direction 1.
	protected byte endHeightPos; // In the positive direction. Right for direction 0, down for direction 1.
	public byte tileHeightNeg; //	In the negative direction. Up for direction 0, left for direction 1.
	public byte tileHeightPos; // 	In the positive direction. Down for direction 0, right for direction 1.*/

	public byte[] dirHeights = new byte[4]; //	Can work in place of the previous solution.
	//	((2 * direction) + dir) % 4 to get the desired direction.
	//	In direction 0, left/right (0/2) are the edge's end points. Up/down (1/3) are the tiles the edge is between.
	//	In direction 1, up/down (1/3) are the edge's end points. Left/right (0/2) are the tiles the edge is between.
}
