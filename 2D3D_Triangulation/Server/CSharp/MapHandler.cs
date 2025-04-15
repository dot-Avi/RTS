using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

[GlobalClass]
public partial class MapHandler : Node2D
{
	public Godot.Collections.Array<MapLayer> mapLayers;
	public Vector2I mapSize = new Vector2I(0, 0);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Vector2[] tileOne = new Vector2[4]
		{
			new Vector2(0, 0),
			new Vector2(100, 0),
			new Vector2(100, -99),
			new Vector2(0, -99)
		};
		Vector2[] tileTwo = new Vector2[4]
		{
			new Vector2(100, 0),
			new Vector2(199, 0),
			new Vector2(199, -99),
			new Vector2(100, -99)
		};
		GD.Print(Geometry2D.MergePolygons(tileOne, tileTwo)[0]);

		GD.Print(mapSize);
		mapLayers = new Godot.Collections.Array<MapLayer>();
		foreach (Node child in GetChildren())
		{
			if (child is MapLayer)
			{
				mapLayers.Add(child as MapLayer);
			}
		}

		// Make sure that each child is properly set up and ready
		while (true)
		{
			int childrenReadyCount = 0;
			foreach (MapLayer layer in mapLayers)
			{
				if (layer.isReady && false)
				{
					break;
				}
				childrenReadyCount += 1;
			}
			GD.Print(childrenReadyCount);
			if (childrenReadyCount >= mapLayers.Count)
			{
				break;
			}
			GD.Print("Not all children ready");
			
		}
		GD.Print("All children ready");

		LoadMap();
		EraseMissingCells();

		for (int layerLevel = 1; layerLevel <= mapLayers.Count; layerLevel++)
		{
			MapLayer layer = GetNode<MapLayer>("Layer" + layerLevel.ToString());
			if (mapSize.X < layer.GetUsedRect().Size.X + layer.GetUsedRect().Position.X)
			{
				mapSize.X = layer.GetUsedRect().Size.X + layer.GetUsedRect().Position.X;
			}
			if (mapSize.Y < layer.GetUsedRect().Size.Y + layer.GetUsedRect().Position.Y)
			{
				mapSize.Y = layer.GetUsedRect().Size.Y + layer.GetUsedRect().Position.Y;
			}
			GD.Print(mapSize);
			

			/* TODO */
			/*
			make a region out of tiles, send the region to layers above and below
			for bottom layer, check for entirely missing tiles and make a region there too
			send that to bottom layer
			send each region to the highest layer
			for each layer's new regions, combine all of them where possible to prevent overlap
			
			yeah this'll be easy I definitely know how to make regions in the first place
			*/
		}

		// REMINDER //
		// MapLayer.GetSurroundingCells(Vector2I Cell) gets the four neighboring cells on the sides
		// NOT the corners
		foreach (MapLayer currentLayer in mapLayers)
		{
			GenerateLayerRegions(currentLayer);
		}
		


		GD.Print();
		GD.Print("LOADING DONE");
		GD.Print();
	}

	public override void _Draw()
	{
		base._Draw();
		Vector2[] tileOne = new Vector2[4]
		{
			new Vector2(0, 0),
			new Vector2(100, 0),
			new Vector2(100, 99),
			new Vector2(0, 99)
		};
		Vector2[] tileTwo = new Vector2[4]
		{
			new Vector2(100, 0),
			new Vector2(199, 0),
			new Vector2(199, 99),
			new Vector2(100, 99)
		};
		DrawColoredPolygon(Geometry2D.MergePolygons(tileOne, tileTwo)[0], new Color(0.941176f, 0.972549f, 1f, 1f));
	}

	#region Cell Management
	public void EraseMissingCells()
	{
		Godot.Collections.Array<Vector2I> cellsUsed = new Godot.Collections.Array<Vector2I>();
		foreach (MapLayer layer in mapLayers)
		{
			GD.Print("Checking Layer: ", layer.Name);
			foreach (Godot.Vector2I cell in layer.GetUsedCells())
			{
				if (cellsUsed.Contains(cell) || isBehindPosition(cell))
				{
					GD.Print(
						"Cell ",
						cell,
						" already exists: ",
						cellsUsed.Contains(cell),
						". Cell is behind (0, 0): ",
						isBehindPosition(cell)
					);
					layer.EraseCell(cell);
					continue;
				}
				if (layer.GetCellAtlasCoords(cell) == new Vector2I(2, 0))
				{
					GD.Print("Cell ", cell, " was a bridge");
					continue;
				}
				cellsUsed.Add(cell);
			}
			GD.Print(cellsUsed);
			GD.Print();
			GD.Print("Test");
		}
	}
	#endregion

//empty region space

//so it's readable in vscode on the right

#region Cell Related Checking
	public bool isBehindPosition(Vector2I currentPosition, Vector2I targetPosition = default)
	{
		if (currentPosition.X < targetPosition.X || currentPosition.Y < targetPosition.Y)
		{
			return true;
		}
		return false;
	}
#endregion

//empty region space

//so it's readable in vscode on the right

#region Loading Only
	public void LoadMap()
	{
		return;
	}

	Godot.Collections.Array<Vector2I> uncheckedTiles;
	Godot.Collections.Array<Vector2I> tilesToCheck;
	Godot.Collections.Array<Vector2I> checkedTiles;
	public void GenerateLayerRegions(MapLayer layer)
	{
		// Set up the tiles to check
		uncheckedTiles = new Godot.Collections.Array<Vector2I>(VectorRange(mapSize));
		tilesToCheck = new Godot.Collections.Array<Vector2I>();
		checkedTiles = new Godot.Collections.Array<Vector2I>();

		tilesToCheck.Add(uncheckedTiles[0]);
		while (uncheckedTiles.Count > 0 || tilesToCheck.Count > 0)
		{
			// GD.Print("Checking layer " + layer.Name);
			CheckTile(tilesToCheck[0]);
			
		}
	}
#endregion

//empty region space

//so it's readable in vscode on the right

#region Generate Navmesh
	private void CheckTile(Vector2I tile)
	{
		uncheckedTiles.RemoveAt(0);
		if (uncheckedTiles.Count != 0)
		{
			tilesToCheck.Add(uncheckedTiles[0]);
		}
		tilesToCheck.RemoveAt(0);
	}

	private godot_packed_vector2_array GenerateARegion(Vector2I startingTile)
	{
		
		return new godot_packed_vector2_array();
	}
#endregion

//empty region space

//so it's readable in vscode on the right

#region Global-ish stuff
	// Returns an array of Vector2Is, going left to right, then down a row
	public Godot.Collections.Array<Vector2I> VectorRange(Vector2I size)
	{
		Godot.Collections.Array<Vector2I> outputArr = new Godot.Collections.Array<Vector2I>();	
		for (int x = 0; x < size.X; x++)
		{
			for (int y = 0; y < size.X; y++)
			{
				outputArr.Add(new Vector2I(x, y));
			}
		}
		return outputArr;
	}
#endregion
}
