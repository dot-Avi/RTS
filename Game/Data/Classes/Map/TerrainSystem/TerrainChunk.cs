using Godot;
using System;

[GlobalClass]
public partial class TerrainChunk : RefCounted
{
#region TODO list
	// Terrain
	// TODO Terrain Generate the terrain
	// [ ] Terrain Generate regions
	// [ ] Terrain Generate floors
	// [ ] Terrain Generate ramps
	// [ ] Terrain Generate walls
	// [ ] Terrain (?)Optimization by adjusting existing meshes rather than regenerating new ones

	// NavMesh
	// TODO NavMesh Generate the navmesh
	// [ ] NavMesh Take the points and convert them into regions
	// [ ] NavMesh (?)Adjust the regions to account for cliffs
	// [ ] NavMesh Add tags to the regions
	// [ ] NavMesh Triangulate the regions
	// [ ] NavMesh Perform a Constrained Delaunay Triangulation
	// XXX NavMesh The rest should be done in other things
	// TODO NavMesh I think the Navmesh needs to be its own thing and not based on chunks
	// only updating from changes on the map with a main layer based on chunks
	// INFO NavMesh Tags will be given to triangles within regions.
	// These tags will determine where units can pathfind (flying, cliff, ground, etc)
	// INFO NavMesh Triangles should contain how long the shared edge between it and a neighboring triangle is.
	// INFO NavMesh Triangles can connect to any other triangle.
	// If a triangle is connected to one that its edges do not border, the length of the shared edge is the radius from the center needed to teleport.
	// Alternatively, something could be set up so that once an agent enters the triangle (as in the center crosses the border into the triangle)
	// the agent gets teleported to the edge of the next triangle.
	// In this case, the distance between the two triangles should be "0" as they technically overlap.
	// Additionally, the triangles may need to be 1:1 scale, or teleport the agent to a point as close as possible on the exit triangle
	// to the point they entered on the first triangle.
	// INFO NavMesh Each edge within a triangle should contain the data on the length of the shared edge.
	// So I guess that means the second case in the previous info is the only possible case.
	// INFO NavMesh The navmesh may need to be contained in another object.
	// Only update from each chunk for optimization. I dunno honestly, something like this.
#endregion

	public Vector2I Position; // Position relative to other chunks/to the map.
	public byte[,] LocalHeightMap; // The heightmap that is contained on the chunk.
	public HeightMapProbe[,] Probes;
	private HeightMapProbe[,] terrainCorners;


	/// <summary>
	/// Converts a tile position into a heightmap position.<br/>
	/// Vice Versa for overload.
	/// </summary>
	/// <param name="pos">Tile position we want to find the heightmap position of.</param>
	/// <returns>Heightmap position of the given tile.
	/// If the tile is not on the map, returns 0.</returns>
	private uint ConvertPos(Vector2I pos)
	{
		if (pos.X < 0 || pos.X >= 16)
		{
			GD.PrintErr($"Could not find a tile at position ({pos.X}, {pos.Y})!");
			GD.PushError($"Could not find a tile at position ({pos.X}, {pos.Y})!");
			return 0;
		}
		if (pos.Y < 0 || pos.Y >= 16)
		{
			GD.PrintErr($"Could not find a tile at position ({pos.X}, {pos.Y})!");
			GD.PushError($"Could not find a tile at position ({pos.X}, {pos.Y})!");
			return 0;
		}

		return (uint)(pos.X + (pos.Y * 16));
	}


	/// <summary>
	/// Converts a heightmap position to a map position.<br/>
	/// Vice Versa for overload.
	/// </summary>
	/// <param name="pos">Position on the heightmap to convert to a map position.</param>
	/// <returns>Returns the tile position of the given heightmap position.
	/// Returns the (-1, -1) if the given position does not exist..</returns>
	private Vector2I ConvertPos(uint pos)
	{
		if (pos >= 256)
		{
			GD.PrintErr($"Position ''{pos}'' is out of range. Max range: {255}");
			GD.PushError($"Position ''{pos}'' is out of range. Max range: {255}");
			return new Vector2I(-1, -1);
		}

		return new Vector2I((int)pos % 16, (int)Math.Floor((double)pos / 16));
	}


	/// <summary>
	/// Creates a new chunk from a position and the local heightmap.
	/// </summary>
	/// <param name="position">The chunk's position on the map</param>
	/// <param name="heightmap">The chunk's local heightmap</param>
	/// <returns>The new chunk</returns>
	public static TerrainChunk NewChunk(Vector2I position, byte[,] heightmap, byte size)
	{
		// Prepares the new chunk and sets its stored variables
		TerrainChunk newChunk = new TerrainChunk
		{
			Position = position,
			LocalHeightMap = ArrayHelper.Slice2DArray(heightmap, position.X, size, position.Y, size)
		};

		// Initializes the new chunk's probe array
		newChunk.Probes = new HeightMapProbe[
			newChunk.LocalHeightMap.GetLength(0) + 1,
			newChunk.LocalHeightMap.GetLength(1) + 1
		];

		// Iterate over every position in the probe array to generate a probe
		// Something seems wrong here, like the Probes heightmap is off by the chunks position
		// Maybe in the probe's script?
		for (int x = 0; x < newChunk.LocalHeightMap.GetLength(0) + 1; x++)
		{
			for (int y = 0; y < newChunk.LocalHeightMap.GetLength(1) + 1; y++)
			{
				newChunk.Probes[x, y] = HeightMapProbe.NewProbe(new Vector2I(x, y), position * 16, heightmap);
				newChunk.Probes[x, y].UpdateProbe();

				/*Vector2I probeWorldPos = newChunk.Probes[x, y].Position + newChunk.Position * 16;
				if (probeWorldPos == new Vector2I(16, 3))
				{
					GD.Print($"chunk {newChunk.Position}: {probeWorldPos}");
				}*/
			}
		}

		// Prepare the probes
		//for (int x = 0; x < newChunk.Probes.GetLength(0); x++)
		//{
		//	for (int y = 0; y < newChunk.Probes.GetLength(1); y++)
		//	{
		//		newChunk.Probes[x, y].UpdateProbe();
		//	}
		//}
		//GD.Print("Probes Created!");
		return newChunk;
	}


	/// <summary>
	/// Finds an edge's end point from the origin and direction, as well as what direction it continues in.
	/// </summary>
	/// <param name="origin">The origin vector to find the next point</param>
	/// <param name="edgeDir">A number from 0-8 determining what direction the edge search is in</param>
	/// <returns></returns>
	public (Vector2I result, Byte edgeDir) GenerateEdgeSegment(Vector2I origin, byte edgeDir)
	{
		GD.Print("test starting");
		Vector2I nextTile = origin + GridHelper.DirNeighborTiles[edgeDir];
		while (true)
		{
			if (nextTile.X < 0 || nextTile.X > 16 || nextTile.Y < 0 || nextTile.Y > 16)
			{
				nextTile -= GridHelper.DirNeighborTiles[edgeDir];
				GD.Print($"Hit a corner at {nextTile}, next dir is {Probes[nextTile.X, nextTile.Y].GetNextEdge(edgeDir)}");
				return (nextTile, Probes[nextTile.X, nextTile.Y].GetLastEdge(edgeDir));
			}

			HeightMapProbe probe = Probes[nextTile.X, nextTile.Y];
			if (probe.IsStraightEdge)
			{
				nextTile += GridHelper.DirNeighborTiles[edgeDir];
				GD.Print($"{probe.Position} is Straight Edge");
				continue;
			}

			byte fromDir = (byte)((edgeDir + 4) % 8);
			byte nextEdgeDir = probe.GetNextEdge(fromDir);
			// If nextEdge != 4 is a bad result and we have found a corner
			if (nextEdgeDir != 4)
			{
				GD.Print($"{origin} -> {nextTile}\nNext edge dir on polygon: {nextEdgeDir + edgeDir}");
				return (nextTile, (byte)(nextEdgeDir + edgeDir));
			}
			GD.Print($"{probe.Position}in dir right is not straight edge and edge is Equal to 4");
			nextTile += GridHelper.DirNeighborTiles[edgeDir];
		}
		
	}


	// XXX IMPORTANT Generate the terrain
	/// TODO: GenTerrain Generate the terrain
	/// [ ] GenTerrain make this method work
	/// [ ] GenTerrain finish it so it generates everything, not just the first region
	/// [ ] GenTerrain make it generate ramps
	/// [ ] GenTerrain make it generate walls
	/// <summary>
	/// Regenerates the entirety of the chunk's 3D terrain.
	/// May be laggy as it regenerates the entire chunk rather than updating terrain.
	/// I'll probably try to optimize that later, especially if it ends up being too slow.
	/// </summary>
	public void GenerateTerrain()
	{
		// Generate each edge, find each connected edge, keep going until returning to root corner
		// Re-check each corner that has multiple junctions until fully checked
		// Maybe try re-using edges that are shared between two regions?
		// I have no idea what I'm doing :(
	}
}
