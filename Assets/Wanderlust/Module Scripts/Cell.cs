using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell  {

	public int Row { get; private set; }
	public int Column { get; private set; }
	public bool UpWall { get; set; }
	public bool DownWall { get; set; }
    public bool LeftWall { get; set; }
    public bool RightWall { get; set; }
	public bool Bell { get; set; }

	public Cell(int row, int column)
	{
		Row = row;
		Column = column;
    }
}
