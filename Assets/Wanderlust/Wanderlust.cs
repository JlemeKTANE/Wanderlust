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

	private static Maze[] mazes = new Maze[13];
	private int startingMazeIndex;
	private int currentMazeIndex;
	private int startingPlayerRow;
	private int startingPlayerCol;
	private int currentPlayerRow;
	private int currentPlayerCol;
	private List<Key> keys;

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
		ButtonL.OnInteract += delegate () { Move("left", cube.Left); return false; };
		ButtonR.OnInteract += delegate () { Move("right", cube.Right); return false; };
		ButtonU.OnInteract += delegate () { Move("up", cube.Up); return false; };
		ButtonD.OnInteract += delegate () { Move("down", cube.Down); return false; };
		ButtonF.OnInteract += delegate () { buttonpressed(); return false; };
		ButtonB.OnInteract += delegate () { buttonpressed(); return false; };

		ModuleId = ModuleIdCounter++;
	}
	void Start()
	{
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
		Log("Intital Rotations");
		Log("Status Light is in the " + statusLightPosition + " corner.");
		if (statusLightPair != null)
		{
			Log("Letter Pair 1:" + statusLightPair[0] + ", " + statusLightPair[1]);
			cube.Rotate(statusLightPair[0], statusLightPair[1], true);
			Log("Current Cube Rotation\n" + cube);
		}

		for (int i = 0; i < pairs.Count; i++)
		{
			string[] pair = pairs[i];
			Log("Letter Pair " + (i + 1 + (statusLightPair != null ? 1 : 0)) + ": " + pair[0] + ", " + pair[1]);
			Log(indexCalculationLogs[i * 2]);
			Log(indexCalculationLogs[(i * 2) + 1]);

			cube.Rotate(pair[0], pair[1], true);
			Log("Current Cube Rotation\n" + cube);
		}

		startingPlayerRow = (int)cube.Left % 6;
		startingPlayerCol = Bomb.GetSolvableModuleNames().Count % 6;
		currentPlayerCol = startingPlayerCol;
		currentPlayerRow = startingPlayerRow;
		startingMazeIndex = GetMazeIndex();
		currentMazeIndex = startingMazeIndex;
		Log("Starting in maze " + currentMazeIndex + " at " + GetBattshipCoorinate(startingPlayerRow, startingPlayerCol));
		Log(mazes[currentMazeIndex]);
		Key key = new Key(Bomb.GetSolvableModuleNames().Count % 13, (int)cube.Front, currentMazeIndex % 6);
		Log("First key is in maze " + key.MazeNum + " at " + GetBattshipCoorinate(key.Row, key.Col));
		keys.Add(key);
	}

	private void Move(string localFace, LocalCube.Face globalFace)
	{ 
		Log("Local " + localFace + " pressed which equates to global " + globalFace);
        Cell[,] maze = mazes[currentMazeIndex].grid;
        Cell cell = maze[currentPlayerRow, currentPlayerCol];

        bool hasWall = false;

        switch (globalFace)
        {
            case LocalCube.Face.Left:
                hasWall = cell.LeftWall;
                break;
            case LocalCube.Face.Right:
                hasWall = cell.RightWall;
                break;
            case LocalCube.Face.Up:
                hasWall = cell.UpWall;
                break;
            case LocalCube.Face.Down:
                hasWall = cell.DownWall;
                break;
        }

        if (hasWall)
        {
            Strike("Hit a wall");
        }

        else
        {
            switch (globalFace)
            {
                case LocalCube.Face.Left:
                    currentPlayerCol--;
                    break;
                case LocalCube.Face.Right:
                    currentPlayerCol++;
                    break;
                case LocalCube.Face.Up:
                    currentPlayerRow--;
                    break;
                case LocalCube.Face.Down:
                    currentPlayerRow++;
                    break;
            }

			Log("Player is now at " + GetBattshipCoorinate(currentPlayerRow, currentPlayerCol));
        }
    }

    private int GetMazeIndex()
	{
		return Modulo(SerialNumberToNum.Sum() + (bellRingCount * (SerialNumberToNum.Last() + 1)), 13);
	}


	private string GetBattshipCoorinate(int row, int col)
	{
		return "" + (char)('A' + col) + (row + 1);
	}

	private void Strike(string s)
	{
        GetComponent<KMBombModule>().HandleStrike();
		Log(s);
	}

    private void Log(object s)
    {
        Debug.LogFormat("[Wanderlust #{0}] {1}", ModuleId, s);
    }
}
