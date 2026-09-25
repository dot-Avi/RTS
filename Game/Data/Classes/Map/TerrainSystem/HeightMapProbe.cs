using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

[GlobalClass]
public partial class HeightMapProbe : RefCounted
{
	// Constant variables
	/// <summary>
	/// Probe types. An int of value <b>2</b> or more is considered a corner.
	/// </summary>
	public enum ProbeType // What type is the probe
	{
		Flat,    //0: Flat tile.
		Edge,    //1: Contains an edge, but otherwise the same as a flat tile.
		Corner,  //2: Contains a corner. Used to generate polygons.
		Junction, //3: Contains a corner and an edge. Used to generate polygons.
		FourWay //4: No two points make flat space.. Used to generate polygons.
	};

	[Flags]
	public enum EdgeDir // Helps me to understand, plus I think C# has built-in bitflag-byte conversion
	{
		None = 0,
		East = 1,
		SouthEast = 2,
		South = 4,
		SouthWest = 8,
		West = 16,
		NorthWest = 32,
		North = 64,
		NorthEast = 128
	};
	public BitArray Edges;
	public byte EdgeCount;

	// Standard variables
	public Vector2I Position; // Position relative to the owner chunk.
	public bool IsRamp; // unneeded for now, might be used later
	public bool IsStraightEdge; // Probably will work better on its own than the next two would have
	public bool IsCorner;
	//public bool IsFlat;
	private byte?[,] localHeightmap; // Local heightmap. Should be a 4x4 grid.
	//private ProbeType probeType; // Likely unneeded

	// Might not be needed after all
	//public Vector2I InChunk; // What chunk the probe is in.
	//private bool isCorner; // If the probe is a corner, use for quick checks

	// Static variables
#pragma warning disable CA2211 // Non-constant fields should not be visible
	public static ulong Probes; // Debug value for counting how many terrain probes are created.
#pragma warning restore CA2211 // Non-constant fields should not be visible


	/// <summary>
	/// Returns the type of the probe. Also updates the type of the probe.
	/// </summary>
	/// <typeparam name="T">Must be either ProbeType or int.</typeparam>
	/// <returns>What type the probe is. Either an enum or int.</returns>
	/// <exception cref="InvalidOperationException">Type must be int or enum.</exception>
	public T GetProbeType<T>()
	{
		var pType = CheckType();
		//probeType = pType; // Previously used as return, likely unneeded now
		if (typeof(T) == typeof(int))
		{
			return (T)(object)(int)pType; // Returns as an int
		}
		else if (typeof(T) == typeof(ProbeType) || typeof(T) == null)
		{
			return (T)(object)pType; // Returns as an enum
		}
		throw new InvalidOperationException("Unsupported type: must be a 'ProbeType' or 'Int");
	}


	// TODO Unfinished Update Probe method
	/// <summary>
	/// Updates the probe's edges.
	/// </summary>
	// INFO Optimization Potential Optimization Here
	public void UpdateProbe()
	{
		CheckEdges();
		IsStraightEdge = CheckIfPerfectEdge();
		if (EdgeCount > 2 || (EdgeCount == 2 && !CheckIfPerfectEdge()))
		{
			IsCorner = true;
		}
	}


	/// <summary>
	/// 
	/// </summary>
	/// <param name="fromEdge"></param>
	/// <returns>How many positions away the next edge is from the first edge clockwise.</returns>
	public byte GetNextEdge(byte fromEdge)
	{
		for (byte dir = 1; dir < 8; dir++)
		{
			if (Edges[(dir + fromEdge) % 8])
			{
				return dir;
			}
		}
		GD.PushWarning("No edge found.");
		GD.PrintErr("No edge found.");
		return 0;
	}


	public byte GetLastEdge(byte fromEdge)
	{
		for (byte dir = 7; dir >= 0; dir--)
		{
			if (Edges[(dir + fromEdge) % 8])
			{
				return dir;
			}
		}
		GD.PushWarning("No edge found.");
		GD.PrintErr("No edge found.");
		return 0;
	}


	/// <summary>
	/// Checks to make sure if the point the probe is on makes a perfectly straight edge.
	/// </summary>
	/// <returns>Returns "true" if it is a straight edge, otherwise returns false.</returns>
	private bool CheckIfPerfectEdge()
	{
		if (EdgeCount != 2) // It can only be a perfectly straight edge 
		{
			return false;
		}
		for (byte edge = 0; edge < 4; edge++)
		{
			// This loop and if statement ensures it is not a corner/junction with only two edges
			if (Edges[edge] && Edges[edge + 4])
			{
				return true;
			}
		}
		return false;
	}


	/// <summary>
	/// Checks if the point forms an edge in a certain direction.
	/// </summary>
	/// <param name="source"></param>
	/// <returns></returns>
	public bool CheckIfEdgeInDir(byte source)
	{
		if (Edges[source] && Edges[(source + 4) % 8]
		&& !(Edges[(source + 1) % 8] || Edges[(source + 2) % 8] || Edges[(source + 3) % 8]))
		{
			return true;
		}

		return false;
	}


	private void CheckEdges()
	{
		// Checks if there is a horizontal edge on the tile
		// rootPos is the top left corner of the tile
		bool checkHorizontal(Vector2I rootPos)
		{
			var posOne = localHeightmap[rootPos.X, rootPos.Y];
			var posTwo = localHeightmap[rootPos.X + 1, rootPos.Y];
			var posThree = localHeightmap[rootPos.X, rootPos.Y + 1];
			var posFour = localHeightmap[rootPos.X + 1, rootPos.Y + 1];

			if (posOne == posTwo
			&& posThree == posFour
			&& posOne != posThree)
			{
				return true;
			}
			return false;
		}

		// Checks if there is a vertical edge on the checked tile
		// rootPos is the top left corner of the tile
		bool checkVertical(Vector2I rootPos)
		{
			var posOne = localHeightmap[rootPos.X, rootPos.Y];
			var posTwo = localHeightmap[rootPos.X + 1, rootPos.Y];
			var posThree = localHeightmap[rootPos.X, rootPos.Y + 1];
			var posFour = localHeightmap[rootPos.X + 1, rootPos.Y + 1];

			if (posOne == posThree
			&& posTwo == posFour
			&& posOne != posTwo)
			{
				return true;
			}
			return false;
		}

		// Checks if there is a diagonal edge going from the bottom left to the top right, forward slash (/) pattern
		// rootPos is the top left corner of the tile
		bool checkDiagonalInequalCoordinates(Vector2I rootPos)
		{
			var posOne = localHeightmap[rootPos.X, rootPos.Y];
			var posTwo = localHeightmap[rootPos.X + 1, rootPos.Y];
			var posThree = localHeightmap[rootPos.X, rootPos.Y + 1];
			var posFour = localHeightmap[rootPos.X + 1, rootPos.Y + 1];

			if (posTwo == posThree
			&& (posTwo == posOne || posTwo == posFour)
			&& posOne != posFour)
			{
				return true;
			}
			return false;
		}

		// Checks if there is a diagonal edge going from the top left to the bottom right, backwards slash (\) pattern
		// rootPos is the top left corner of the tile
		bool checkDiagonalEqualCoordinates(Vector2I rootPos)
		{
			var posOne = localHeightmap[rootPos.X, rootPos.Y];
			var posTwo = localHeightmap[rootPos.X + 1, rootPos.Y];
			var posThree = localHeightmap[rootPos.X, rootPos.Y + 1];
			var posFour = localHeightmap[rootPos.X + 1, rootPos.Y + 1];

			if (posOne == posFour
			&& (posOne == posTwo || posOne == posThree)
			&& posTwo != posFour)
			{
				return true;
			}
			return false;
		}

		// Checks if the tile is flat
		// rootPos is the top left corner of the tile
		bool checkIfFlat(Vector2I rootPos)
		{
			var posOne = localHeightmap[rootPos.X, rootPos.Y];
			var posTwo = localHeightmap[rootPos.X + 1, rootPos.Y];
			var posThree = localHeightmap[rootPos.X, rootPos.Y + 1];
			var posFour = localHeightmap[rootPos.X + 1, rootPos.Y + 1];

			if (posOne == posTwo && posTwo == posThree && posThree == posFour)
			{
				return true; // All four tiles are equal, therefore this is flat
			}
			return false;
		}

		Vector2I center = new Vector2I(1, 1); // Used for both of the next code blocks
		EdgeCount = 0; // Reset for the next blocks

		// The central tiles must have inequal heights for this probe to connect to other probes
		if (checkIfFlat(center))
		{
			Edges.SetAll(false);
			return; // No edges to count
		}

		for (byte i = 0; i < 8; i++)
		{
			Vector2I tile = GridHelper.DirNeighborTiles[i];
			/*if (Position == new Vector2I(13, 15))
			{
				GD.Print($"{tile}: \\: {tile.X == tile.Y}. Corner/Diagonal {tile.Abs()}: {Math.Abs(tile.X) == Math.Abs(tile.Y)}");
			}*/

			if (Math.Abs(tile.X) == Math.Abs(tile.Y % 2))
			{
				// ~BUG~ EdgeDection DownRight seems to be doing the wrong check (the other diagonal check) or something
				// It's returning true when it shouldn't
				// I think I fixed it by moving the check inside the if code block rather than in the if () statement

				// Corner (\ Diagonal /)
				if (tile.X == tile.Y)// && checkDiagonalEqualCoordinates(tile + center)) // \ UpLeft \ DownRight \
				{
					if (checkDiagonalEqualCoordinates(tile + center))
					{
						// Only ran if previous is true. Inside previous as to not cause the next part to run if this block is false.
						Edges[i] = true;//checkDiagonalEqualCoordinates(tile + center);
						EdgeCount++;
					}
				}
				else if (checkDiagonalInequalCoordinates(tile + center)) // && (tile.X != tile.Y) / UpRight / DownLeft /
				{
					Edges[i] = true;//checkDiagonalInequalCoordinates(tile + center);
					EdgeCount++;
				}
				continue;
			} 
			
			// else Abs(tile.X % 2) != Abs(tile.y % 2)
			// Linear (+ Cardinal +)
			if (tile.X % 2 == 0)// && checkVertical(tile + center)) // | Vertical | Up | Down
			{
				if (checkVertical(tile + center))
				{
					// Only ran if previous is true. Inside previous as to not cause the next part to run if this block is false.
					Edges[i] = true;//checkVertical(tile + center);
					EdgeCount++;
				}
			}
			else if (checkHorizontal(tile + center)) // && (tile.Y == 0) - Horizontal - Left - Right
			{
				Edges[i] = true;//checkHorizontal(tile + center);
				EdgeCount++;
			}
		}
	}


	/// <summary>
	/// Overload for GetType that takes no type enforcer.
	/// </summary>
	/// <returns>The type of the probe as an enum.</returns>
	public ProbeType GetProbeType()
	{
		//probeType = CheckType(); // Previously used as return, likely unneeded now
		var type = CheckType();
		return type;
	}


	/// <summary>
	/// Returns the type of the probe and sets if it is a corner or not.
	/// Might need to be public but was private before
	/// </summary>
	/// <param name="pos">What position (0,0) - (2,2) to check on the local heightmap</param>
	/// <returns></returns>
	public ProbeType CheckType()
	{
		// The main 4 tiles the probe covers
		byte?[,] primaryTiles = ArrayHelper.Slice2DArray(localHeightmap, 1, 2, 1, 2);

		// debug print stuff
		//GD.Print(Position);
		//ArrayHelper.Print2DArray(localHeightmap);
		//ArrayHelper.Print2DArray(primaryTiles);
		
		// Ensures the probe remains as a corner if it is on the edge of a chunk
		if ((Position.X == 0 || Position.X == 16)
		  &&(Position.Y == 0 || Position.Y == 16))
		{
			return ProbeType.Corner;
		}

		// If each position is the same height, it is flat
		if (CheckisFlat(new Vector2I(1, 1)))
		{
			return ProbeType.Flat;
		}

		// If two positions equal each other and another two positions equal each other
		// it is an edge
		if (CheckIsEdge(new Vector2I(1, 1)))
		{
			return ProbeType.Edge;
		}

		//GD.Print(Position);
		// Now to figure out if it's a corner or diagonal
		Func<int, int> xPos = n => Math.Clamp(n % 3, 0, 1);
		Func<int, int> yPos = n => n / 2;
		for (byte i = 0; i < 4; i++)
		{
			var posOne = primaryTiles[xPos(i), yPos(i)];
			var posTwo = primaryTiles[xPos((i + 1) % 4), yPos((i + 1) % 4)];
			var posThree = primaryTiles[xPos((i + 2) % 4), yPos((i + 2) % 4)];
			var posFour = primaryTiles[xPos((i + 3) % 4), yPos((i + 3) % 4)]; 

			if (posOne is null
			|| posTwo is null
			|| posThree is null
			|| posFour is null)
			{
				return ProbeType.Corner;
			}

			// Not a single tile forms an edge
			if (posOne != posTwo
			&& posTwo != posFour
			&& posFour != posThree
			&& posThree != posOne)
			{
				return ProbeType.FourWay;
			}

			// Three-way junction
			if ((posOne != posTwo // Vertical check
			&& posTwo != posThree
			&& posThree != posOne
			&& posTwo == posFour)
			|| (posOne == posTwo // Horizontal/Diagonal check
			&& posTwo != posThree // Technically diagonal would be a four-way junction
			&& posThree != posFour
			&& posFour != posOne))
			{
				return ProbeType.Junction;
			}
			
			// three are equal and one isn't
			if (posOne != posTwo
			&& posFour == posTwo
			&& posThree == posFour)
			{
				byte flatCount = 0;
				for (byte j = 0; j < 4; j++)
				{
					float dir = j * ((float)Math.PI/2);
					Vector2I normalizedDir = (Vector2I)Vector2.FromAngle(dir).Round();
					/*if (Position == new Vector2I(0, 0))
					{
						GD.Print($"{j}: Vec2{Vector2.FromAngle((float)dir)}, Vec2I{normalizedDir}");// + new Vector2I(1, 1));
					}*/// Wasn't working at first

					// Early return as if there's a single edge, it should be a corner
					if (CheckIsEdge(new Vector2I(1, 1) + normalizedDir, Math.Abs(normalizedDir.Y)))
					{
						return ProbeType.Corner;
					}
					if (CheckisFlat(new Vector2I(1, 1) + normalizedDir))
					{
						flatCount++;
					}
				}
				// A diagonal edge has two flat neighbors and two other "corner" neighbors
				if (flatCount > 2)
				{
					return ProbeType.Corner;
				}
			}
		}

		return ProbeType.Flat;
	//}


		bool CheckisFlat(Vector2I pos)
		{
			byte?[,] primaryTiles = ArrayHelper.Slice2DArray(localHeightmap, pos.X, 2, pos.Y, 2);

			if (primaryTiles[0, 0] == primaryTiles [0, 1]
			&& primaryTiles[0, 1] == primaryTiles[1, 1]
			&& primaryTiles[1, 1] == primaryTiles[1, 0])
			{
				return true;
			}
			return false;
		}


		bool CheckIsEdge(Vector2I pos, int? dir = null)
		{
			byte?[,] primaryTiles = ArrayHelper.Slice2DArray(localHeightmap, pos.X, 2, pos.Y, 2);
			if (dir == null) // Direction doesn't matter, check both
			{
				if ((primaryTiles[0, 0] == primaryTiles[1, 0]
					&& primaryTiles[0, 1] == primaryTiles[1, 1]
					&& primaryTiles[0, 0] != primaryTiles[0, 1]
					) || (
					primaryTiles[0, 0] == primaryTiles[0, 1]
					&& primaryTiles[1, 0] == primaryTiles[1, 1]
					&& primaryTiles[0, 0] != primaryTiles[1, 0])
				)
				{
					return true;
				}
			} else // Only runs if there is a direction to check
			{
				if (dir == 0) // Check the horizontal
				{
					if (primaryTiles[0, 0] == primaryTiles[1, 0]
						&& primaryTiles[0, 1] == primaryTiles[1, 1]
						&& primaryTiles[0, 0] != primaryTiles[0, 1]
					)
					{
						return true;
					}
				}
				if (dir == 1)
				{ // Check the vertical
					if (primaryTiles[0, 0] == primaryTiles[0, 1]
						&& primaryTiles[1, 0] == primaryTiles[1, 1]
						&& primaryTiles[0, 0] != primaryTiles[1, 0]
					)
					{
						return true;
					}
				}
			}
			
			// Not an edge
			return false;
		}
	}


	/// <summary>
	/// Checks if the probe is a corner or not.
	/// If it is, mark it as one.
	/// If not, mark that it isn't.
	/// </summary>
	/// <returns>The new corner status of the probe.</returns>
	/*public bool UpdateProbeCornerStatus()
	{
		if (GetProbeType<int>() >= 2)
		{
			IsCorner = true;
		}
		else
		{
			IsCorner = false;
		}
		return IsCorner;
	}*/


	/// <summary>
	/// Creates a new HeightMapProbe instance. Performs its own slicing on a heightmap.
	/// </summary>
	/// <param name="position">The chunk's position on the map</param>
	/// <param name="heightmap">The chunk's local heightmap</param>
	/// <returns>A terrain probe.</returns>
	public static HeightMapProbe NewProbe(Vector2I pos, Vector2I chunkPos, byte[,] heightmap)
	//, bool doPreCheck = false)
	{
		Probes += 1;

		HeightMapProbe probe = new HeightMapProbe();
		probe.Position = pos;
		probe.localHeightmap = new byte?[4, 4];
		probe.Edges = new BitArray(8);
		probe.IsStraightEdge = false;
		probe.IsCorner = false;
		//probe.IsFlat = true;
		//GD.Print(pos);

		// Something might be wrong here as the heightmap seems off by one, relative to the chunk's position
		for (int x = -2; x < 2; x++)
		{
			for (int y = -2; y < 2; y++)
			{
				// Ensures the position actually exists.
				if (chunkPos.Y + y + pos.Y < 0 || chunkPos.Y + y + pos.Y >= heightmap.GetLength(1))
				{ // Switched from just (chunkPos.V) to (chunkPos.V * 2), seems more broken
					probe.localHeightmap[x + 2, y + 2] = null;
					continue;
				}
				if (chunkPos.X + x + pos.X < 0 || chunkPos.X + x + pos.X >= heightmap.GetLength(0))
				{
					probe.localHeightmap[x + 2, y + 2] = null;
					continue;
				}

				probe.localHeightmap[x + 2, y + 2] = heightmap[chunkPos.X + x + pos.X, chunkPos.Y + y + pos.Y];
			}
		}
		//ArrayHelper.Print2DArray(probe.localHeightmap);
		return probe;
	}
}
