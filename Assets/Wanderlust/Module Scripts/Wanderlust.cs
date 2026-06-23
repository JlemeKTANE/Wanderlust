using KModkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Rnd = UnityEngine.Random;
public class Wanderlust : MonoBehaviour
{
	public KMBombInfo Bomb;
	public KMAudio Audio;
	public GameObject statuslight;
	public Vector3 TopRightPosition;
	public Vector3 TopLeftPosition;
	public Vector3 BottomRightPosition;
	public Vector3 BottomLeftPosition;

	public KMSelectable ButtonL, ButtonR, ButtonU, ButtonD, ButtonF, ButtonB;

	private string SerialNumber;
	private int BatteryNum;
	private int PortNum;

	private int ModuleId;

	private static int ModuleIdCounter = 1;

	private List<string[]> pairs = new List<string[]>();

	private string[,] edgeworkGrid = new string[3, 4]
{
	{"LL", "GB", "GU", "LF"},
	{"GR", "LD", "LB", "GL"},
	{"GD", "GF", "LR", "LU"},
};
	private LocalCube cube;

	private int[] SerialNumberToNum;
	private int bellRingCount;

	public static Maze[] mazes = new Maze[13];
	private int startingMazeIndex;
	private int currentMazeIndex;
	private int startingPlayerRow;
	private int startingPlayerCol;
	private int currentPlayerRow;
	private int currentPlayerCol;
	private List<Key> keys;
	private List<Face> localInputs;

    private enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    /// <summary>
    /// Converts c# modulo from (-modulo, modulo) to [0, modulo)
    /// </summary>
    /// <param name="value">what is being moduloed</param>
    /// <param name="modulo">the value that value is being moduloed</param>
    /// <returns></returns>
    /// thank u hawker
    private int Modulo(int value, int modulo)
	{
		return ((value % modulo) + modulo) % modulo;
	}
	void buttonpressed()
	{
		Debug.Log("Button Pressed");
	}

	void Awake()
	{
		ButtonL.OnInteract += delegate () { Move(Face.Left, cube.Left); return false; };
		ButtonR.OnInteract += delegate () { Move(Face.Right, cube.Right); return false; };
		ButtonU.OnInteract += delegate () { Move(Face.Up, cube.Up); return false; };
		ButtonD.OnInteract += delegate () { Move(Face.Down, cube.Down); return false; };
		ButtonF.OnInteract += delegate () { Move(Face.Front, cube.Front); return false; };
		ButtonB.OnInteract += delegate () { Move(Face.Back, cube.Back); return false; };

		ModuleId = ModuleIdCounter++;
	}
	void Start()
	{
		localInputs = new List<Face>();
		for (int i = 0; i < 13; i++)
		{
			mazes[i] = new Maze(i);
		}
		keys = new List<Key>();
		bellRingCount = 0;
		int statusPositionIndex = Rnd.Range(0, 4);
		statusPositionIndex = 0;
		string statusLightPosition = "";
		string[] statusLightPair = null;
		switch (statusPositionIndex)
		{
			case 0:
				statuslight.transform.localPosition = TopLeftPosition;
				statusLightPair = new string[] { "LU", "LL" };
				statusLightPosition = "top left";
				break;
			case 1:
				statuslight.transform.localPosition = BottomRightPosition;
				statusLightPair = new string[] { "LU", "LR" };
				statusLightPosition = "bottom right";
				break;
			case 2:
				statuslight.transform.localPosition = BottomLeftPosition;
				statusLightPair = new string[] { "LU", "LD" };
				statusLightPosition = "bottom left";
				break;
			case 3:
				statusLightPosition = "top right";
				break;
		}
		cube = new LocalCube();
		BatteryNum = Bomb.GetBatteryCount();
		PortNum = Bomb.GetPortCount();
		SerialNumber = Bomb.GetSerialNumber().ToUpper();
		SerialNumberToNum = SerialNumber.Select(x => char.IsDigit(x) ? int.Parse("" + x) : x - 'A' + 1).ToArray();
		List<string> indexCalculationLogs = new List<string>();

		for (int i = 0; i < SerialNumberToNum.Length; i++)
		{
			int Column = Modulo(SerialNumberToNum[i] + BatteryNum, 4);
			int Row = Modulo(SerialNumberToNum[i] - PortNum, 3);
			if (i % 2 == 0)
			{
				pairs.Add(new string[2]);
			}
			pairs[i / 2][i % 2] = edgeworkGrid[Row, Column];

			//store for logging purposes
			indexCalculationLogs.Add(edgeworkGrid[Row, Column] + " | Row: (" + SerialNumberToNum[i] + " + " + BatteryNum + ") % 4 = " + Column + " | Col: (" + SerialNumberToNum[i] + " - " + PortNum + ") % 3 = " + Row);
		}
		for (int i = 0; i < pairs.Count; i++)
		{
			Log("Pair " + (i + 1) + ": " + pairs[i][0] + "," + pairs[i][1]);

		}

		Log("Status Light is in the " + statusLightPosition + " corner.");
		if (statusLightPair != null)
		{
			Log("Letter Pair 1:" + statusLightPair[0] + ", " + statusLightPair[1]);
			cube.Rotate(statusLightPair[0], statusLightPair[1], true);
			Log("Current Cube Orientation\n" + cube);
		}

		for (int i = 0; i < pairs.Count; i++)
		{
			string[] pair = pairs[i];
			Log("Letter Pair " + (i + 1 + (statusLightPair != null ? 1 : 0)) + ": " + pair[0] + ", " + pair[1]);
			Log(indexCalculationLogs[i * 2]);
			Log(indexCalculationLogs[(i * 2) + 1]);

			cube.Rotate(pair[0], pair[1], true);
			Log("Current Cube Orientation\n" + cube);
		}

		startingPlayerRow = (int)cube.Left % 6;
		startingPlayerCol = Bomb.GetSolvableModuleNames().Count % 6;
		currentPlayerCol = startingPlayerCol;
		currentPlayerRow = startingPlayerRow;
		startingMazeIndex = GetMazeIndex(SerialNumberToNum, bellRingCount);
		currentMazeIndex = startingMazeIndex;
		Log("Starting in maze " + currentMazeIndex + " at " + GetBattshipCoorinate(startingPlayerRow, startingPlayerCol));
		Log(mazes[currentMazeIndex]);
		Key key = new Key(Bomb.GetSolvableModuleNames().Count % 13, (int)cube.Front, currentMazeIndex % 6);
		Log("First key is in maze " + key.MazeNum + " at " + GetBattshipCoorinate(key.Row, key.Col));
		keys.Add(key);

	 	

		

        List<GameState> gameStatePath = FindPath(currentMazeIndex, key.MazeNum, bellRingCount, mazes[currentMazeIndex].grid[currentPlayerRow, currentPlayerCol], mazes[key.MazeNum].grid[key.Row, key.Col]);
		LogPath("One path to the key:", ParseGameStatesToGlobalFaces(gameStatePath), ParseGameStateToLocalFace(gameStatePath, true, cube), ParseGameStateToLocalFace(gameStatePath, false, cube));

    }

	private void Move(Face localFace, Face globalFace)
	{
		string localFaceStr = localFace.ToString().ToLower();
        string globalFaceStr = globalFace.ToString().ToLower();


        Log("Local " + localFaceStr + " pressed which equates to global " + globalFaceStr);

		localInputs.Add(localFace);

		Cell currentCell = GetCurrnetCell();

        if (globalFace == Face.Back)
		{
			if (currentCell.Bell)
			{
				bellRingCount++;
				Log("Rang the bell. This is the " + Ordinal(bellRingCount) + " time");
				currentMazeIndex = GetMazeIndex(SerialNumberToNum, bellRingCount);
				Log("Now in maze " + currentMazeIndex);

				string[] bellPair = GetBellRotationPair(currentCell.Row, localInputs);

				if (localInputs.Count == 1) 
				{
                    Log("Only one input has been made. Using local " + localFaceStr);
				}
				else
				{
					Log("Second to last local input was " + localInputs[localInputs.Count - 2].ToString().ToLower());
				}

                Log("The bell rang is in row " + currentCell.Row + " which corresponds to " + ((Face)currentCell.Row).ToString());
				Log("Rotating " + bellPair[0] + " and " + bellPair[1]);
				cube.Rotate(bellPair[0], bellPair[1], Bomb.GetSolvableModuleIDs().Count % 2 == 0);
                Log("Current Cube Orientation\n" + cube);
            }
			else
			{
				Strike("Global back pressed when not on a bell");
			}
		}

		else if (globalFace == Face.Front)
		{

			Key desiredKey = keys.Last();
			
            if (currentCell.Row == desiredKey.Row &&
				currentCell.Column == desiredKey.Col &&
				desiredKey.MazeNum == currentMazeIndex)
            {
				int index = keys.IndexOf(desiredKey) + 1;



				Log("Collected the " + Ordinal(index) + " key");


				if (new Face[] { cube.Left, cube.Right, cube.Up }.Any(f => f == Face.Front))
				{
					Log("Global front is either Local left, right, or up. Swapping local up and local right");
					cube.Swap("LU", "LR");
				}
				else
				{
                    Log("Global front is not either Local left, right, or up. Swapping local up and local front");
                    cube.Swap("LU", "LF");

                }

                Log("Current Cube Orientation\n" + cube);
            }
            else
            {
                Strike("Global front pressed when not on the key.");
            }
        }

		else
		{
			Cell[,] maze = mazes[currentMazeIndex].grid;
			Cell cell = maze[currentPlayerRow, currentPlayerCol];

			bool hasWall = false;

			switch (globalFace)
			{
				case Face.Left:
					hasWall = cell.LeftWall;
					break;
				case Face.Right:
					hasWall = cell.RightWall;
					break;
				case Face.Up:
					hasWall = cell.UpWall;
					break;
				case Face.Down:
					hasWall = cell.DownWall;
					break;
			}

			if (hasWall)
			{
				Strike("Hit a wall.");
			}

			else
			{
				switch (globalFace)
				{
					case Face.Left:
						currentPlayerCol--;
						break;
					case Face.Right:
						currentPlayerCol++;
						break;
					case Face.Up:
						currentPlayerRow--;
						break;
					case Face.Down:
						currentPlayerRow++;
						break;
				}

				Log("Now at " + GetBattshipCoorinate(currentPlayerRow, currentPlayerCol));
			}
		}
    }

	private string[] GetBellRotationPair(int bellRow, List<Face> localFaceInputs)
	{
        Face lastLocalFace;
        if (localFaceInputs.Count == 1)
        {
            lastLocalFace = localFaceInputs.Last();
        }

        else
        {
            lastLocalFace = localFaceInputs[localFaceInputs.Count - 2];
        }

        string p1 = "L" + lastLocalFace.ToString()[0];

        Face faceIndex = (Face)bellRow;

        string p2 = "G" + faceIndex.ToString()[0];

		return new string[] { p1, p2 };
    }
    public static int GetMazeIndex(int[] serialNumberToNum, int bellRingCount)
	{
		return (serialNumberToNum.Sum() + (bellRingCount * (serialNumberToNum.Last() + 1))) % 13;
	}

	private string Ordinal(int num)
	{ 
		switch (num)
		{
			case 1:
				return num + "st";
			case 2:
				return num + "nd";
            case 3:
                return num + "rd";
			default:
				return num + "th";

        }
	}


	private string GetBattshipCoorinate(int row, int col)
	{
		return "" + (char)('A' + col) + (row + 1);
	}

	private void Strike(string s)
	{
        GetComponent<KMBombModule>().HandleStrike();
		Log("Strike! " + s);
	}

    private void Log(object s)
    {
        Debug.LogFormat("[Wanderlust #{0}] {1}", ModuleId, s);
    }

	private void LogPath(string startingStr, List<Face> globalPath, List<Face> localEvenPath, List<Face> localOddPath)
	{
		Log(startingStr);
        Log("Global Path: " + Join(globalPath.Select(f => f.ToString())));
        Log("Local Even Path: " + Join(localEvenPath.Select(f => f.ToString())));
        Log("Local Odd Path: " + Join(localOddPath.Select(f => f.ToString())));
	}

	private string Join(IEnumerable<string> collection)
	{
		return collection.ToArray().Join(", ");
	}

	private Cell GetCurrnetCell()
	{
		return mazes[currentMazeIndex].grid[currentPlayerRow, currentPlayerCol];
	}

	//Find the shortest path from one cell to another
	private List<GameState> FindPath(int currentMazeIndex, int goalMazeIndex, int bellRingCount, Cell currentCell, Cell goalCell)
	{
		//set the serial number
		GameState.SerialNumberToNum = SerialNumberToNum;

        GameState startingState = new GameState(currentMazeIndex, bellRingCount, currentCell, null);

        List<GameState> vistedStates = new List<GameState>();
        Queue<GameState> queue = new Queue<GameState>();

        queue.Enqueue(startingState);

        //continue while the goal has not been found
        while (queue.Count > 0 && !vistedStates.Any(s => 
		s.CurrentMazeIndex == goalMazeIndex && 
		s.CurrentCell.Row == goalCell.Row && 
		s.CurrentCell.Column == goalCell.Column))
        {
            GameState currentState = queue.Dequeue();

            //get all the valid neighbors
            List<GameState> currentStateNieghbors = currentState.GetAvaiableGameStatesNieghbors();

            foreach (GameState neighbor in currentStateNieghbors)
            {
                //don't check states we already visted
                if (vistedStates.Any(v => GameState.StatesAreSame(v, neighbor)))
                {
                    continue;
                }

                vistedStates.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }

        //find the shortest path
        GameState endNode = vistedStates.FirstOrDefault(s => s.CurrentMazeIndex == goalMazeIndex && s.CurrentCell.Row == goalCell.Row && s.CurrentCell.Column == goalCell.Column);

        //a path to the goal cell could not be found
        if (endNode == null)
        {
            return new List<GameState>();
        }

        List<GameState> path = new List<GameState>();
        GameState current = endNode;

        while (current != null)
        {
            //add it to list
            path.Add(current);

            //set new current state
            current = current.ParentState;
        }

        path.Reverse();

        return path;
    }

    private List<Face> ParseGameStatesToGlobalFaces(List<GameState> path)
	{ 
		List<Face> globalFace = new List<Face>();

		for(int i  = 0; i < path.Count - 1; i++)
		{
			GameState currentGameState = path[i];
			GameState nextGameState = path[i + 1];

			//if the maze inicies are the same, check to see which directional face was pressed
			if (currentGameState.CurrentMazeIndex == nextGameState.CurrentMazeIndex)
			{ 
				Cell currentCell = currentGameState.CurrentCell;
				Cell nextCell = nextGameState.CurrentCell;

				//UP
				if (currentCell.Row - 1 == nextCell.Row)
				{
					globalFace.Add(Face.Up);
				}

				//Down
				else if (currentCell.Row + 1 == nextCell.Row)
				{
                    globalFace.Add(Face.Down);
                }

                //Left
                else if (currentCell.Column - 1 == nextCell.Column)
                {
                    globalFace.Add(Face.Left);
                }

                //Right
                else if (currentCell.Column + 1 == nextCell.Column)
                {
                    globalFace.Add(Face.Right);
                }
            }

			//if the maze indicies don't match, assume the player needs to press Back
			else if(currentGameState.CurrentMazeIndex !=  nextGameState.CurrentMazeIndex)
			{
				globalFace.Add(Face.Back);
			}
		}

		return globalFace;
	}

	private List<Face> ParseGameStateToLocalFace(List<GameState> path, bool evenModules, LocalCube startingCube)
	{

		List<Face> globalFacePath =	ParseGameStatesToGlobalFaces(path);

		//split the global path when there is a back button (add the current cell just in case we need it for calculating bell pair rotations)
		List<List<Tuple<Face, Cell>>> brokenFacePaths = new List<List<Tuple<Face, Cell>>>();
        List<Tuple<Face, Cell>> list = new List<Tuple<Face, Cell>>();

		for (int i = 0; i < globalFacePath.Count; i++)
		{
			Face f = globalFacePath[i];
			Cell c = path[i].CurrentCell;

            list.Add(new Tuple<Face, Cell>(f, c));

			if (f == Face.Back || i == globalFacePath.Count - 1)
			{
				brokenFacePaths.Add(list.ToList());
				list.Clear();
			}
        }

        List<Face> localFacePath = new List<Face>();

		//Calcuclate the local face directions broken up when pressing back due to cube rotations
		LocalCube currentCube = startingCube.Clone();

        foreach (List<Tuple<Face, Cell>> l in brokenFacePaths)
		{
			List<Face> faces = l.Select(t => t.Item1).ToList();
            localFacePath.AddRange(faces.Select(globalFace => currentCube.GetLocalFaceOfGlobalFace(globalFace)));
			
			//calculate what the new cube orientation is
			Cell lastCell = l.Last().Item2;
			string[] pair = GetBellRotationPair(lastCell.Row, localFacePath);
			currentCube.Rotate(pair[0], pair[1], evenModules);
		}

		return localFacePath;
    }


}
