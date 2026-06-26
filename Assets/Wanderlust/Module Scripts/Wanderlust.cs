using KModkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Rnd = UnityEngine.Random;
public class Wanderlust : MonoBehaviour
{

	//what the goal is for the path finding. Helps with finding the last input
	private enum Goal
	{
		Key
	}
	private KMBombInfo Bomb;
	private KMAudio Audio;
	private KMBombModule Module;
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
	private int keyIndex;
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

		Bomb = GetComponent<KMBombInfo>();
		Audio = GetComponent<KMAudio>();
		Module = GetComponent<KMBombModule>();

		ModuleId = ModuleIdCounter++;
	}
	void Start()
	{
		keyIndex = -1;
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
		GenerateKeyLocation();
		LogKey(0);
		Key key = keys[0];
		GetPathToGoal(currentMazeIndex, key.MazeNum, bellRingCount, mazes[currentMazeIndex].grid[currentPlayerRow, currentPlayerCol], mazes[key.MazeNum].grid[key.Row, key.Col], cube, Goal.Key, "One path to the key:");
	}

	private void GetPathToGoal(int currentMazeIndex, int goalMazeIndex, int bellRingCount, Cell currentCell, Cell goalCell, LocalCube startingCube, Goal goal, string startingStr)
	{
		List<GameState> gameStatePath = FindPath(currentMazeIndex, goalMazeIndex, bellRingCount, currentCell, goalCell);
		List<Face>[] localPaths = ParseGameStateToLocalFace(gameStatePath, startingCube, goal);
		LogPath(startingStr, ParseGameStatesToGlobalFaces(gameStatePath, Goal.Key), localPaths[0], localPaths[1]);
	}

	private void GenerateKeyLocation()
	{
		keyIndex++;
		int mazeNum, row, column;
		switch (keyIndex)
		{
			case 0:
				mazeNum = Bomb.GetSolvableModuleNames().Count % 13;
				row = (int)cube.Front;
				column = currentMazeIndex % 6;
				break;
			case 1:
			case 2:
				mazeNum = bellRingCount % 13;
				row = (int)cube.GetLocalFaceOfGlobalFace(Face.Front);
				column = (localInputs.Count() - bellRingCount - keyIndex) % 6;
				break;

			default:
				throw new ArgumentException("Invalid key num");
		}

		keys.Add(new Key(mazeNum, row, column));
	}

    private bool ValidMove(Face globalFace, out string strikeReason)
    {
        Cell currentCell = GetCurrnetCell();

        if (globalFace == Face.Back)
        {
            if (!currentCell.Bell)
            {
                strikeReason = "Global back pressed when not on a bell";
                return false;
            }
        }
        else if (globalFace == Face.Front)
        {
            Key desiredKey = keys.Last();

            if (!(keyIndex <= 2 &&
                  SameMazeAndCell(currentMazeIndex,
                                  currentCell.Row,
                                  currentCell.Column,
                                  desiredKey.MazeNum,
                                  desiredKey.Row,
                                  desiredKey.Col)))
            {
                strikeReason = "Global front pressed when not on the key";
                return false;
            }
        }
        else
        {
            bool hasWall = false;

            switch (globalFace)
            {
                case Face.Left: hasWall = currentCell.LeftWall; break;
                case Face.Right: hasWall = currentCell.RightWall; break;
                case Face.Up: hasWall = currentCell.UpWall; break;
                case Face.Down: hasWall = currentCell.DownWall; break;
            }

            if (hasWall)
            {
                strikeReason = "Hit a wall";
                return false;
            }
        }

        strikeReason = null;
        return true;
    }

    private void Move(Face localFace, Face globalFace)
	{
		string localFaceStr = localFace.ToString().ToLower();
        string globalFaceStr = globalFace.ToString().ToLower();

        Log("Local " + localFaceStr + " pressed which equates to global " + globalFaceStr);

		localInputs.Add(localFace);

		Cell currentCell = GetCurrnetCell();

		string strikeReason = null;

		//check to see if this move is valid
        if (!ValidMove(globalFace, out strikeReason))
        {
            Strike(strikeReason);
            return;
        }

		//Apply the move
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
				int solvedModNum = Bomb.GetSolvedModuleIDs().Count;
				Log("Number of solved mods: " + solvedModNum);
				cube.Rotate(bellPair[0], bellPair[1], solvedModNum % 2 == 0);
                Log("Current Cube Orientation\n" + cube);
            }
		}

		else if (globalFace == Face.Front)
		{
			Key desiredKey = keys.Last();

			if (keyIndex <= 2 && SameMazeAndCell(currentMazeIndex, currentCell.Row, currentCell.Column, desiredKey.MazeNum, desiredKey.Row, desiredKey.Col))
            {
				Log("Collected the " + Ordinal(keyIndex + 1) + " key");

				if (keyIndex == 0)
				{
					if (new Face[] { cube.Left, cube.Right, cube.Up }.Any(f => f == Face.Front))
					{
						Log("Global front is either local left, right, or up. Swapping local up and local right");
						cube.Swap("LU", "LR");
					}
					else
					{
						Log("Global front is not either local left, right, or up. Swapping local up and local front");
						cube.Swap("LU", "LF");

					}
				}

				else if (keyIndex == 1)
				{
                    if (currentMazeIndex % 2 == 0)
                    {
                        Log("The key was in an even maze. Swapping local down with local up and local right with local left.");
                        cube.Swap("LD", "LU");
                        cube.Swap("LR", "LL");
                    }
                    else
                    {
                        Log("The key was in an odd maze. Swapping local front with local back and local back with local down.");
                        cube.Swap("LF", "LB");
                        cube.Swap("LB", "LD");
                    }
                }


				Log("Current Cube Orientation\n" + cube);

				//if not thhe last key, generate the next one.
				if (keyIndex < 2)
				{
					GenerateKeyLocation();
					Key key = keys[keyIndex];
					LogKey(keyIndex);
					GetPathToGoal(currentMazeIndex, key.MazeNum, bellRingCount, mazes[currentMazeIndex].grid[currentPlayerRow, currentPlayerCol], mazes[key.MazeNum].grid[key.Row, key.Col], cube, Goal.Key, "One path to the key:");
				}
            }
            else
            {
                Strike("Global front pressed when not on the key.");
                localInputs.RemoveAt(localInputs.Count - 1);
            }
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
		Log("Strike! " + s);
        Module.HandleStrike();
	}

    private void Log(object s)
    {
        Debug.LogFormat("[Wanderlust #{0}] {1}", ModuleId, s);
    }

	private void LogKey(int index)
	{
		Key key = keys[index];
        Log(Ordinal(index + 1) + " key is in maze " + key.MazeNum + " at " + GetBattshipCoorinate(key.Row, key.Col));
    }

	private void LogPath(string startingStr, List<Face> globalPath, List<Face> localEvenPath, List<Face> localOddPath)
	{
		Log(startingStr);
        Log("Global path: " + Join(globalPath.Select(f => f.ToString())));
        Log("Local path for even solved modules: " + Join(localOddPath.Select(f => f.ToString())));
        Debug.Log(Join(localEvenPath.Select(f => f.ToString()[0].ToString()), ""));
        Log("Local path for odd solved modules: " + Join(localOddPath.Select(f => f.ToString())));
		Debug.Log(Join(localOddPath.Select(f => f.ToString()[0].ToString()), ""));
    }

    private string Join(IEnumerable<string> collection, string seperator = ", ")
	{
		return collection.ToArray().Join(seperator);
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
		while (queue.Count > 0 && !vistedStates.Any(s => SameMazeAndCell(s.CurrentMazeIndex, s.CurrentCell.Row, s.CurrentCell.Column, goalMazeIndex, goalCell.Row, goalCell.Column)))
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
        GameState endNode = vistedStates.FirstOrDefault(s => SameMazeAndCell(s.CurrentMazeIndex, s.CurrentCell.Row, s.CurrentCell.Column, goalMazeIndex, goalCell.Row, goalCell.Column));

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

    private List<Face> ParseGameStatesToGlobalFaces(List<GameState> path, Goal goal)
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

		//if the goal is a key, add global front to collect the key
		if (goal == Goal.Key)
		{ 
			globalFace.Add(Face.Front);
		}

		return globalFace;
	}

	private List<Face>[] ParseGameStateToLocalFace(List<GameState> path, LocalCube startingCube, Goal goal)
	{
		//get even path first then odd path
		return new bool[] { true, false }.Select(b => ParseGameStateToLocalFace(path, b, startingCube, goal)).ToArray();
    }

    private List<Face> ParseGameStateToLocalFace(List<GameState> path, bool evenModules, LocalCube startingCube, Goal goal)
	{
		List<Face> globalFacePath =	ParseGameStatesToGlobalFaces(path, goal);

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
			List<Face> globalFaces = l.Select(t => t.Item1).ToList();
			List<Face> localFaces = globalFaces.Select(globalFace => currentCube.GetLocalFaceOfGlobalFace(globalFace)).ToList();
            localFacePath.AddRange(localFaces);

            //calculate what the new cube orientation is
            Cell lastCell = l.Last().Item2;
			string[] pair = GetBellRotationPair(lastCell.Row, localFacePath);
			currentCube.Rotate(pair[0], pair[1], evenModules);
		}

		return localFacePath;
    }

	private bool SameMazeAndCell(int currentmazeIndex, int currentRow, int currentColumn, int goalMazeIndex, int goalRow, int goalColumn)
	{
		return currentmazeIndex == goalMazeIndex &&
				currentRow == goalRow &&
				currentColumn == goalColumn;
    }

    private Match Match(string input, string pattern)
    {
        return Regex.Match(input, pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use `!{0} reset` to reset the module. Use `!{0}` followed by `LRUDFB` to press the corresponding faces. These can be chained together with no spaces. The chain of commands will stop when the modules strikes or solves. If the former happens, a message will be sent showing the position of which press caused the strike.";
#pragma warning restore 414

	IEnumerator ProcessTwitchCommand(string Command)
	{
        Command = Command.ToUpper().Trim();
		yield return null;

		//Press face
		Match match = Match(Command, @"^([LRUDFB]+)$");

		if (match.Success)
		{
			string faces = match.Groups[1].Value;
			for (int i = 0; i < faces.Length; i++)
			{ 
				char c = faces[i];
				KMSelectable button = null;
				Face globalFace = Face.Up; //value don't matter
                switch (c)
                {
                    case 'L':
						globalFace = cube.Left;
						button = ButtonL;
                        break;
                    case 'R':
                        globalFace = cube.Right;
                        button = ButtonR;
                        break;
                    case 'U':
                        globalFace = cube.Up;
                        button = ButtonU;
                        break;
                    case 'D':
                        globalFace = cube.Down;
                        button = ButtonD;
                        break;
                    case 'F':
                        globalFace = cube.Front;
                        button = ButtonF;
                        break;
                    case 'B':
                        globalFace = cube.Back;
                        button = ButtonB;
                        break;
                }

                string strikeReason;
                bool strikeIncoming = !ValidMove(globalFace, out strikeReason);

                if (strikeIncoming)
                    yield return "strikemessage The " + Ordinal(i + 1) + " press (" + c + ") caused the strike";

                button.OnInteract();

                if (strikeIncoming)
                    yield break;
            }
            yield break;
		}
		else
		{
			yield return "sendtochaterror Invalid Command: \"" + Command + "\"";
			yield break;
		}

	}
    IEnumerator TwitchHandleForcedSolve()
	{
		yield return null;
	}
}
