using KModkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering;
using Rnd = UnityEngine.Random;
public class Wanderlust : MonoBehaviour
{
	private KMBombInfo Bomb;
	private KMBombModule Module;
	private KMAudio Audio;

	enum StatusLightPosition
	{ 
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight
	}

    [Header("Debug Mode")]
    [SerializeField]
    private bool debugMode;
    [SerializeField]
    private StatusLightPosition debugStatusPosition;


    [Header("Module Stuff")]
    public GameObject statuslight;
    [SerializeField]
    private Vector3 topLeftPosition;
    [SerializeField]
    private Vector3 bottomRightPosition;
    [SerializeField]
    private Vector3 bottomLeftPosition;

	public KMSelectable buttonL, buttonR, buttonU, buttonD, buttonF, buttonB;
	private KMSelectable statusLightKMS;

	private string SerialNumber;
	private int BatteryNum;
	private int PortNum;
	private int ModuleId;
	private static int ModuleIdCounter = 1;
	private List<string[]> pairs;

    private readonly string[,] edgeworkGrid = new string[3, 4]
{
	{"LL", "GB", "GU", "LF"},
	{"GR", "LD", "LB", "GL"},
	{"GD", "GF", "LR", "LU"},
};

	private LocalCube cube;
	private bool moduleSolved;
	private int[] SerialNumberToNum;
	private int bellRingCount;
	private int keyIndex;
	public static Maze[] mazes = new Maze[13];
	private int startingMazeIndex;
	private int currentMazeIndex;
	private int startingRow;
	private int startingCol;
	private int currentRow;
	private int currentCol;
	private List<Key> keys;
	private List<Face> localInputs;

	private bool statusHeld = false;
	private bool resetSoundPlayed = false;
	private float resetTimer = 0.0f;

	private enum MoveResult
	{ 
		BellStrike,
		WallStrike,
		KeyStrike,
		StartStrike,
		MoveLeft,
		MoveRight,
		MoveDown,
		MoveUp,
		RingBell,
		CollectKey,
		SubmitStart
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

	void Awake()
	{
        moduleSolved = false;
        

        buttonL.OnInteract += delegate () { Move(Face.Left, cube.Left, buttonL); return false; };
		buttonR.OnInteract += delegate () { Move(Face.Right, cube.Right, buttonR); return false; };
		buttonU.OnInteract += delegate () { Move(Face.Up, cube.Up, buttonU); return false; };
		buttonD.OnInteract += delegate () { Move(Face.Down, cube.Down, buttonD); return false; };
		buttonF.OnInteract += delegate () { Move(Face.Front, cube.Front, buttonF); return false; };
		buttonB.OnInteract += delegate () { Move(Face.Back, cube.Back, buttonB); return false; };

        statusLightKMS = statuslight.GetComponent<KMSelectable>();
        statusLightKMS.OnInteract += delegate () { statusHeld = true; return false;  };
        statusLightKMS.OnInteractEnded += delegate () { statusHeld = false; resetTimer = 0.0f; resetSoundPlayed = false;  };

        Bomb = GetComponent<KMBombInfo>();
		Audio = GetComponent<KMAudio>();
		Module = GetComponent<KMBombModule>();

		ModuleId = ModuleIdCounter++;
	}
	void Start()
	{
		SetUpModule();

    }

	void Update()
	{
		if (statusHeld && !resetSoundPlayed)
		{
			resetTimer += Time.deltaTime;
			if (resetTimer >= 5)
			{
				//play reeset sound here
				resetSoundPlayed = true;
				Log("Status light was held for 5 seconds. Resetting the module...");
				SetUpModule();
            }
		}
	}

    private void SetUpModule()
    {
        keyIndex = -1;
        localInputs = new List<Face>();
		pairs = new List<string[]>();
        for (int i = 0; i < 13; i++)
        {
            mazes[i] = new Maze(i);
        }
        keys = new List<Key>();
        bellRingCount = 0;
        StatusLightPosition statusPosition = (StatusLightPosition)Rnd.Range(0, 4);
        if (debugMode)
        {
            statusPosition = debugStatusPosition;
        }
        string statusPositionStr = "";
        string[] statusLightPair = null;
        switch (statusPosition)
        {
            case StatusLightPosition.TopLeft:
                statuslight.transform.localPosition = topLeftPosition;
                statusLightPair = new string[] { "LU", "LL" };
                statusPositionStr = "top left";
                break;
            case StatusLightPosition.BottomRight:
                statuslight.transform.localPosition = bottomRightPosition;
                statusLightPair = new string[] { "LU", "LR" };
                statusPositionStr = "bottom right";
                break;
            case StatusLightPosition.BottomLeft:
                statuslight.transform.localPosition = bottomLeftPosition;
                statusLightPair = new string[] { "LU", "LD" };
                statusPositionStr = "bottom left";
                break;
            case StatusLightPosition.TopRight:
                statusPositionStr = "top right";
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
            indexCalculationLogs.Add(string.Format("{0} | Row: ({1} + {2}) % 4 = {3} | Col: ({4} - {5}) % 3 = {6}", edgeworkGrid[Row, Column], SerialNumberToNum[i], BatteryNum, Column, SerialNumberToNum[i], PortNum, Row));
        }
        for (int i = 0; i < pairs.Count; i++)
        {
            Log(string.Format("Pair {0}: {1},{2}", i + 1, pairs[i][0], pairs[i][1]));
        }

        Log(string.Format("Status Light is in the {0} conrner.", statusPositionStr));
        if (statusLightPair != null)
        {
            Log(string.Format("Letter Pair 1: {0}, {1}", statusLightPair[0], statusLightPair[1]));
            cube.Rotate(statusLightPair[0], statusLightPair[1], true);
            Log("Current Cube Orientation\n" + cube);
        }

        for (int i = 0; i < pairs.Count; i++)
        {
            string[] pair = pairs[i];
            Log(string.Format("Letter Pair {0}: {1}, {2}", i + 1 + (statusLightPair != null ? 1 : 0), pair[0], pair[1]));
            Log(indexCalculationLogs[i * 2]);
            Log(indexCalculationLogs[(i * 2) + 1]);

            cube.Rotate(pair[0], pair[1], true);
            Log("Current Cube Orientation\n" + cube);
        }

        startingRow = (int)cube.Left % 6;
        startingCol = Bomb.GetSolvableModuleNames().Count % 6;
        currentCol = startingCol;
        currentRow = startingRow;
        startingMazeIndex = GetMazeIndex(SerialNumberToNum, bellRingCount);
        currentMazeIndex = startingMazeIndex;
        Log(string.Format("Starting in maze {0} at {1}", currentMazeIndex, GetBattshipCoorinate(startingRow, startingCol)));
        GenerateKeyLocation();
        LogKey(0);
        Key key = keys[0];
        GetPathToGoal(currentMazeIndex, key.MazeNum, bellRingCount, mazes[currentMazeIndex].grid[currentRow, currentCol], mazes[key.MazeNum].grid[key.Row, key.Col], cube, "One path to the key:");
    }

    private void GetPathToGoal(int currentMazeIndex, int goalMazeIndex, int bellRingCount, Cell currentCell, Cell goalCell, LocalCube startingCube, string startingStr)
	{
		List<GameState> gameStatePath = FindPath(currentMazeIndex, goalMazeIndex, bellRingCount, currentCell, goalCell);
		List<Face>[] localPaths = ParseGameStateToLocalFace(gameStatePath, startingCube);
		LogPath(startingStr, ParseGameStatesToGlobalFaces(gameStatePath), localPaths[0], localPaths[1]);
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

	/// <summary>
	/// Logic on handling what the result of the move was. Just to remove duplication checks for TP and button presses
	/// </summary>
	/// <param name="globalFace">the global direction going to moved to</param>
	/// <returns></returns>
    private MoveResult GetMoveResult(Face globalFace)
    {
        Cell currentCell = GetCurrnetCell();

		if (globalFace == Face.Back)
		{
			if (!currentCell.Bell)
			{
				return MoveResult.BellStrike;
			}

			else
			{
				return MoveResult.RingBell;
			}
		}
		else if (globalFace == Face.Front)
		{
			Key desiredKey = keys.Last();
			int targetMaze;
			int targetRow;
			int targetCol;
			MoveResult strikeResult, correctResult;

			//if current goal is the key
			if (keyIndex <= 2)
			{
				targetMaze = desiredKey.MazeNum;
				targetRow = desiredKey.Row;
				targetCol = desiredKey.Col;
				strikeResult = MoveResult.KeyStrike;
				correctResult = MoveResult.CollectKey;

            }

            //if current goal is the starting position
            else
            {
				targetMaze = startingMazeIndex;
				targetRow = startingRow;
				targetCol = startingCol;
				strikeResult = MoveResult.StartStrike;
				correctResult = MoveResult.SubmitStart;
			}

			return SameMazeAndCell(currentMazeIndex, currentCell.Row, currentCell.Column, targetMaze, targetRow, targetCol) ? correctResult : strikeResult;
        }

        else
        {
            bool hasWall;
			MoveResult direction;

            switch (globalFace)
            {
                case Face.Left:
					hasWall = currentCell.LeftWall;
					direction = MoveResult.MoveLeft;
                    break;
                case Face.Right: 
					hasWall = currentCell.RightWall;
                    direction = MoveResult.MoveRight;
                    break;
                case Face.Up: 
					hasWall = currentCell.UpWall;
                    direction = MoveResult.MoveUp;
                    break;
                case Face.Down: 
					hasWall = currentCell.DownWall;
                    direction = MoveResult.MoveDown;
                    break;
				default:
					throw new ArgumentException(string.Format("Invalid global face {0}", globalFace));
            }

			return hasWall ? MoveResult.WallStrike : direction;
        }
    }

	private void Move(Face localFace, Face globalFace, KMSelectable selectable)
	{
		selectable.AddInteractionPunch();

		if (moduleSolved)
		{
			return;
		}
		string localFaceStr = localFace.ToString().ToLower();

		Log(string.Format("Local {0} pressed which equates to global {1}", localFaceStr, globalFace.ToString().ToLower()));
		localInputs.Add(localFace);

		Cell currentCell = GetCurrnetCell();
		MoveResult result = GetMoveResult(globalFace);
		bool strike = false;
		bool move = false;
		string strikeReason = "";

		switch (result)
		{
			case MoveResult.WallStrike:
				strike = true;
				strikeReason = "Hit a wall";
				break;

			case MoveResult.BellStrike:
				strike = true;
				strikeReason = "Global back pressed when not on a bell";
				break;

			case MoveResult.KeyStrike:
				strike = true;
				strikeReason = string.Format("Global front pressed when not on the key");
				break;

			case MoveResult.StartStrike:
				strike = true;
				strikeReason = string.Format("Global front pressed when not on the starting cell");
				break;

			case MoveResult.RingBell:
				bellRingCount++;

				Log(string.Format("Rang the bell. This is the {0} time", Ordinal(bellRingCount)));

				currentMazeIndex = GetMazeIndex(SerialNumberToNum, bellRingCount);

				Log(string.Format("Now in maze {0}", currentMazeIndex));

				string[] bellPair = GetBellRotationPair(currentCell.Row, localInputs);

				if (localInputs.Count == 1)
				{
					Log(string.Format("Only one input has been made. Using local {0}", localFaceStr));
				}
				else
				{
					Log(string.Format("Second to last local input was {0}", localInputs[localInputs.Count - 2].ToString().ToLower()));
				}

				Log(string.Format("The bell rang is in row {0} which corresponds to {1}", currentCell.Row, (Face)currentCell.Row).ToString());
				Log(string.Format("Rotating {0} and {1}", bellPair[0], bellPair[1]));

				int solvedModNum = Bomb.GetSolvedModuleIDs().Count;

				Log(string.Format("Number of solved mods: {0}", solvedModNum));

				cube.Rotate(bellPair[0], bellPair[1], solvedModNum % 2 == 0);

				Log("Current Cube Orientation\n" + cube);
				break;

			case MoveResult.CollectKey:
				Log(string.Format("Collected the {0} key", Ordinal(keyIndex + 1)));

				switch (keyIndex)
				{
					case 0:
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
						break;
					case 1:
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
						break;
					case 2:
						foreach (string[] pair in pairs)
						{
							Log(string.Format("Swapping {0} and {1}", pair[0], pair[1]));
							cube.Swap(pair[0], pair[1]);
						}
						break;
				}

				Log("Current Cube Orientation\n" + cube);

				//if not thhe last key, generate the next one.
				if (keyIndex < 2)
				{
					GenerateKeyLocation();
					Key key = keys[keyIndex];
					LogKey(keyIndex);
					GetPathToGoal(currentMazeIndex, key.MazeNum, bellRingCount, mazes[currentMazeIndex].grid[currentRow, currentCol], mazes[key.MazeNum].grid[key.Row, key.Col], cube, "One path to the key:");
				}

				//If it is the last key, go back to the start position
				else
				{
					keyIndex++;
					Log(string.Format("Going back to starting position which is in maze {0} {1}", startingMazeIndex, GetBattshipCoorinate(startingRow, startingCol)));
					GetPathToGoal(currentMazeIndex, startingMazeIndex, bellRingCount, mazes[currentMazeIndex].grid[currentRow, currentCol], mazes[startingMazeIndex].grid[startingRow, startingCol], cube, "One path to the start:");
				}
				break;

			case MoveResult.SubmitStart:
				Log("Moule Solved");
				Module.HandlePass();
				moduleSolved = true;
				break;

			case MoveResult.MoveLeft:
				move = true;
				currentCol--;
				break;

			case MoveResult.MoveRight:
				move = true;
				currentCol++;
				break;

			case MoveResult.MoveUp:
				move = true;
				currentRow--;
				break;

			case MoveResult.MoveDown:
				move = true;
				currentRow++;
				break;
		}

		if (strike)
		{
			Strike(strikeReason);
			localInputs.RemoveAt(localInputs.Count - 1);
		}

		else if (move)
		{
            Log(string.Format("Now at {0}", GetBattshipCoorinate(currentRow, currentCol)));
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

	public static string GetBattshipCoorinate(int row, int col)
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
        Log(string.Format("{0} key is in the maze {1} at {2}", Ordinal(index + 1), key.MazeNum, GetBattshipCoorinate(key.Row, key.Col)));
    }

    private void LogPath(string startingStr, List<Face> globalPath, List<Face> localEvenPath, List<Face> localOddPath)
    {
        Log(startingStr);

		bool evenSolves = Bomb.GetSolvedModuleIDs().Count() % 2 == 0;
        LogPathVariant("Global path", globalPath, false, false);
        LogPathVariant("Local path for even solved modules", localEvenPath, true, evenSolves);
        LogPathVariant("Local path for odd solved modules", localOddPath, true, !evenSolves);
    }

    private void LogPathVariant(string label, List<Face> path, bool logAbbreviated, bool copyToKeyboard)
    {
        Log(label + ": " + Join(path));

        //copy to keybaord for tp
		if (logAbbreviated && debugMode)
		{
			string abbreviation = Join(path.Select(p => "" + p.ToString()[0]), "");
            Debug.Log(abbreviation);

			if(copyToKeyboard)
			{
				GUIUtility.systemCopyBuffer = string.Format("!1 {0}", abbreviation);
			}
		}
    }

    private string Join(IEnumerable<Face> collection, string seperator = ", ")
    {
        return Join(collection.Select(f => f.ToString()), seperator);
    }

    private string Join(IEnumerable<string> collection, string seperator = ", ")
	{
		return collection.ToArray().Join(seperator);
	}

	private Cell GetCurrnetCell()
	{
		return mazes[currentMazeIndex].grid[currentRow, currentCol];
	}

	//Find the shortest path from one cell to another
	private List<GameState> FindPath(int currentMazeIndex, int goalMazeIndex, int bellRingCount, Cell currentCell, Cell goalCell)
	{
		//help for logging game states for debugging
		bool debug = currentMazeIndex == 9 && currentCell.Row == 2 && currentCell.Column == 0 && goalMazeIndex == 6 && goalCell.Row == 3 && goalCell.Column == 1;

		debug = false;
        //set the serial number
        GameState.SerialNumberToNum = SerialNumberToNum;

        GameState startingState = new GameState(currentMazeIndex, bellRingCount, currentCell, null);

        List<GameState> vistedStates = new List<GameState>() { startingState };
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

		//add global front to submit
		globalFace.Add(Face.Front);

		return globalFace;
	}

	private List<Face>[] ParseGameStateToLocalFace(List<GameState> path, LocalCube startingCube)
	{
		//get even path first then odd path
		return new bool[] { true, false }.Select(b => ParseGameStateToLocalFace(path, b, startingCube, localInputs)).ToArray();
    }

    private List<Face> ParseGameStateToLocalFace(List<GameState> path, bool evenModules, LocalCube startingCube, List<Face> currentLocalInputs)
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

		//need to take previous moves from other find path calls in account to get adequate bell rotations
		List<Face> localFacePathWithCurrentInputs = currentLocalInputs.ToList();
        List<Face> localFacePath = new List<Face>();

        //Calcuclate the local face directions broken up when pressing back due to cube rotations
        LocalCube currentCube = startingCube.Clone();

        foreach (List<Tuple<Face, Cell>> l in brokenFacePaths)
		{
			List<Face> globalFaces = l.Select(t => t.Item1).ToList();
			List<Face> localFaces = globalFaces.Select(globalFace => currentCube.GetLocalFaceOfGlobalFace(globalFace)).ToList();
            localFacePath.AddRange(localFaces);
			localFacePathWithCurrentInputs.AddRange(localFaces);

            //calculate what the new cube orientation is
            Cell lastCell = l.Last().Item2;
			string[] pair = GetBellRotationPair(lastCell.Row, localFacePathWithCurrentInputs);

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
						button = buttonL;
                        break;
                    case 'R':
                        globalFace = cube.Right;
                        button = buttonR;
                        break;
                    case 'U':
                        globalFace = cube.Up;
                        button = buttonU;
                        break;
                    case 'D':
                        globalFace = cube.Down;
                        button = buttonD;
                        break;
                    case 'F':
                        globalFace = cube.Front;
                        button = buttonF;
                        break;
                    case 'B':
                        globalFace = cube.Back;
                        button = buttonB;
                        break;
                }

                bool strikeIncoming = GetMoveResult(globalFace).ToString().Contains("Strike");

                if (strikeIncoming)
                    yield return string.Format("strikemessage The {0} press ({1}) caused the strike", Ordinal(i + 1), c);

                button.OnInteract();

                if (strikeIncoming)
                    yield break;
            }
            yield break;
		}
		else
		{
			yield return string.Format("sendtochaterror Invalid Command: {0}", Command);
            yield break;
		}

	}
    IEnumerator TwitchHandleForcedSolve()
	{
		yield return null;
	}

	//helper function to copy the tp path to clipboard
    public void CopyTextToClipboard(string textToCopy)
    {
        
    }
}
