using Godot;
using System;
using System.ComponentModel;

[GlobalClass]
public partial class MapHandler : Node
{
	// General Variables
	public MapData Data;
	private TerrainChunk[,] chunks;

	// Export items
	[ExportGroup("Standard")]
	[Export]
	public byte ChunkSize = 16;
	[ExportGroup("Debug")]
	[Export]
	public bool Debug;
	[Export]
	public Node DebugParent;
	[Export]
	public Vector2I MapSize = new Vector2I(16, 16);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ulong startTime = Time.GetTicksUsec();
		Console.WriteLine("Console.WriteLine test");
		if (Data is null) // If there is no map data, generate a new one.
		{
			Data = MapData.NewMap(MapSize, 4, ChunkSize);
		}
		
		Vector2I size = Data.MapSize;
		Vector2I chunkMapSize = size / ChunkSize;
		chunks = new TerrainChunk[chunkMapSize.X, chunkMapSize.Y];
		GD.Print(chunkMapSize);
		// Create chunks
		for (uint chunkPosX = 0; chunkPosX < chunks.GetLength(0); chunkPosX++)
		{
			for (uint chunkPosY = 0; chunkPosY < chunks.GetLength(1); chunkPosY++)
			{
				TerrainChunk newChunk = TerrainChunk.NewChunk(
					new Vector2I((int)chunkPosX, (int)chunkPosY),
					Data.HeightMap,
					ChunkSize
				);
				//newChunk.Position = new Vector2I((int)chunkPosX, (int)chunkPosY);
				chunks[chunkPosX, chunkPosY] = newChunk; // Store the new chunk in the Chunk Array
			}
		}
		GD.Print($"Total Terrain Probes: {HeightMapProbe.Probes}");

		// Print how long the process took
		ulong endTime = Time.GetTicksUsec();
		GD.Print($"Time elapsed:\n{endTime-startTime} microseconds");
		GD.Print($"{Math.Round((endTime-startTime) / 100.0) / 10} milliseconds");
		GD.Print($"{Math.Round(Math.Round((endTime-startTime) / 1000.0) / 100) / 10} seconds");
		GD.Print();
		//ArrayHelper.Print2DArray(ArrayHelper.Slice2DArray(Data.HeightMap, 0, 15, 0, 15));
		//ArrayHelper.Print2DArray(Data.HeightMap);

		// just generates the texture the sprite2d uses
		if (Debug)
		{
			Image img = new Image();
			byte[] imgData = ArrayHelper.Flatten2DArray(Data.HeightMap); // Flattens the heightmap
			img.SetData(Data.MapSize.X, Data.MapSize.Y, false, Image.Format.R8, imgData);
			// prints the whole heightmap
			
			/*byte[] imgData = new byte[chunks[0, 0].LocalHeightMap.Length];
			// Flattens the heightmap
			Buffer.BlockCopy(chunks[0, 0].LocalHeightMap, 0, imgData, 0, chunks[0, 0].LocalHeightMap.Length);
			img.SetData(16, 16, false, Image.Format.R8, imgData);
			// prints the chunk at (0, 0)*/

			//GD.Print(Data.HeightMap.Length);
			img.SavePng("res://test/map.png");
			GetNode<Sprite2D>("%HeightMap").Texture = ImageTexture.CreateFromImage(img);
			//GetNode<Sprite2D>("%map").Texture = ImageTexture.CreateFromImage(img);

			PackedScene marker = GD.Load<PackedScene>("uid://dlw113xqvyiof");
			Node2D debugRoot = GetNode<Node2D>("%RootDebug");
			foreach (TerrainChunk chunk in chunks)
			{
				/*Node2D newMarker = marker.Instantiate<Node2D>();
				newMarker.Position = chunk.Position * 64;
				AddChild(newMarker);
				ArrayHelper.Print2DArray(chunk.probes);*/
				chunk.GenerateTerrain();
				foreach (HeightMapProbe probe in chunk.Probes)
				{
					if (probe.IsCorner)//GetProbeType<int>() > 1)
					{
						if (chunk.Position == Vector2I.Zero && probe.Position == new Vector2I(13, 15))
						{
							GD.Print($"Probe is corner somehow:");
							for (byte edge = 0; edge < 8; edge++)
							{
								GD.Print($"Dir {edge} is edge: {probe.Edges[edge]}");
							}
						}
						Vector2I probePos = chunk.Position * 16 + probe.Position;
						Node2D newMarker = marker.Instantiate<Node2D>();
						newMarker.Position = probePos;
						newMarker.Name = probePos.ToString();
						//GD.Print($"probe {newMarker.Name} of type {probe.CheckType()} at {probe.Position} in chunk {chunk.Position}");
						debugRoot.AddChild(newMarker);
					}
				}
			}
		}

		chunks[0, 0].GenerateEdgeSegment(new Vector2I(0, 0), 0);
		for (byte i = 0; i < 8; i++)
		{
			GD.Print($"{chunks[0, 0].Probes[0, 0].Edges[i]}");
		}
		
	}

	public void LoadMap(MapData data)
	{
		
	}
}
