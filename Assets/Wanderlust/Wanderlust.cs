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
	private int ModuleNum;

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
		ButtonL.OnInteract += delegate () { buttonpressed(); return false; };
		ButtonR.OnInteract += delegate () { buttonpressed(); return false; };
		ButtonU.OnInteract += delegate () { buttonpressed(); return false; };
		ButtonD.OnInteract += delegate () { buttonpressed(); return false; };
		ButtonF.OnInteract += delegate () { buttonpressed(); return false; };
		ButtonB.OnInteract += delegate () { buttonpressed(); return false; };
		Debug.Log("hi");

        ModuleId = ModuleIdCounter++;
    }
	void Start()
	{
		int statusPositionIndex = Rnd.Range(0, 4);
        pairs.Add(new string[2]);
        switch (statusPositionIndex)
		{
			case 0:
				statuslight.transform.localPosition = TopLeftPosition;
				pairs[0]=new string[] {"LU","LL"};
				Log("Top Left Status Light");
				break;
            case 1:
                statuslight.transform.localPosition = BottomRightPosition;
                pairs[0] = new string[] { "LU", "LR" };
                Log("Bottom Right Status Light");
                break;
            case 2:
                statuslight.transform.localPosition = BottomLeftPosition;
                pairs[0] = new string[] { "LU", "LD" };
                Log("Bottom Left Status Light");
                break;
			case 3:
				pairs.RemoveAt(0);
                Log("Top Right Status Light");
                break;
        }
		cube = new LocalCube();
		BatteryNum = Bomb.GetBatteryCount();
		PortNum = Bomb.GetPortCount();
		ModuleNum = Bomb.GetSolvableModuleIDs().Count();
		SerialNumber = Bomb.GetSerialNumber().ToUpper();
		int[] SerialNumberToNum = SerialNumber.Select(x => char.IsDigit(x) ? int.Parse(""+x):x-'A'+1 ).ToArray();

		for (int i = 0; i < SerialNumberToNum.Length; i++)
		{
			int Column = Modulo(SerialNumberToNum[i] + BatteryNum, 4);
			int Row = Modulo(SerialNumberToNum[i] - PortNum, 3);
			if (i % 2 == 0)
			{
				pairs.Add(new string[2]);
			}
			int pairIndex = statusPositionIndex == 3 ? i / 2 : (i / 2) + 1;
            pairs[pairIndex][i % 2] = edgeworkGrid[Row, Column];


        }
		for (int i = 0; i < pairs.Count; i++)
		{
			Log("Pair " + (i + 1) + ": " + pairs[i][0] + "," + pairs[i][1]);

        }
    }
    private void Log(object s)
    {
        Debug.LogFormat("[Wanderlust #{0}] {1}", ModuleId, s);
    }
}
