using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key {

	public int MazeNum;
	public int Row;
	public int Col;

	public Key(int mazeNum, int row, int col)
	{ 
		MazeNum = mazeNum;
		Row = row;
		Col = col;
	}
}
