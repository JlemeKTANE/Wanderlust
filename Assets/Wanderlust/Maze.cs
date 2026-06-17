using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Maze {
    public int Index { get; private set; }
    public Cell[,] grid { get; private set;}

    //used to generate the string grids
    string[][][] stringGrids =
    {
        new string[][]
        {
            new[] {"R","R*","D","","D","D*"},
            new[] {"R","R","","RD","D*",""},
            new[] {"","RD","D","D","RD*",""},
            new[] {"R","D","","","RD",""},
            new[] {"D","RD*","R","RD*","",""},
            new[] {"*","","R","","R","*"}
        },
        new string[][]
        {
            new[] {"D","D","R","D*","D",""},
            new[] {"R*","D*","R","","RD*",""},
            new[] {"","RD","D","RD","D",""},
            new[] {"R","D*","D","","R",""},
            new[] {"","D","RD*","R","R",""},
            new[] {"","R","","R","R*","*"}
        },
        new string[][]
        {
           new[] {"","D","","RD*","D",""},
           new[] {"R","R*","R","","",""},
           new[] {"RD*","R","RD","RD*","R","D*"},
           new[] {"","D","RD","R*","D","D"},
           new[] {"","R","","D","D",""},
           new[] {"R","R*","","","R*",""}
        },
        new string[][]
        {
           new[] {"","D","R","","D",""},
           new[] {"D","R","RD*","D","R","D*"},
           new[] {"D*","D","RD","D","D","D*"},
           new[] {"","D","R","","",""},
           new[] {"D","R","RD*","RD*","R","D*"},
           new[] {"*","","R","","",""}
        },
        new string[][]
        {
           new[] {"","D","D","","D",""},
           new[] {"RD*","","RD*","RD*","R","D"},
           new[] {"","","D","RD","R","*"},
           new[] {"RD*","R","D","D","","D"},
           new[] {"R","RD","R*","D*","RD","*"},
           new[] {"","","","","",""}
        },
        new string[][]
        {
           new[] {"R","","D","","D","D*"},
           new[] {"R","D","RD*","R","R*",""},
           new[] {"R","D*","R","RD","D",""},
           new[] {"","RD*","","D","RD*",""},
           new[] {"R","D","D","R","",""},
           new[] {"","","R*","R","R*",""}
        },
        new string[][]
        {
           new[] {"D","D","","RD*","","D*"},
           new[] {"D*","R","D","D","RD",""},
           new[] {"","D","RD","R*","",""},
           new[] {"R","D*","R","R","RD*",""},
           new[] {"R","D","R","R","",""},
           new[] {"","R*","","R","R","*"}
        },
        new string[][]
        {
           new[] {"D","","D","RD*","R","*"},
           new[] {"","D","RD","D","D",""},
           new[] {"D","RD*","R*","D","R",""},
           new[] {"","D","RD","","R",""},
           new[] {"R","","RD*","R","RD*",""},
           new[] {"","R","*","","R","*"}
        },
        new string[][]
        {
           new[] {"D","","D","D","RD*","*"},
           new[] {"R*","R","","RD*R","D",""},
           new[] {"D","RD","D","D","D","D"},
           new[] {"","R","","D","D",""},
           new[] {"R","RD*","D","RD*","D",""},
           new[] {"","","","","R*","*"}
        },
        new string[][]
        {
           new[] {"","","R","R*","R",""},
           new[] {"R","R","R","R","D",""},
           new[] {"RD*","RD","RD*","R","D*",""},
           new[] {"D*","D","D","","RD",""},
           new[] {"D","D","R","RD","R*",""},
           new[] {"*","","","","R","*"}
        },
        new string[][]
        {
           new[] {"D","D","","D","","D*"},
           new[] {"","RD*","R","R*","RD",""},
           new[] {"","RD","RD*","","D",""},
           new[] {"R","D*","D","RD","R*","D"},
           new[] {"","R","","D","RD","*"},
           new[] {"R*","R","","","",""}
        },
        new string[][]
        {
           new[] {"R","R*","D","","D","D*"},
           new[] {"","RD","R","R","","D*"},
           new[] {"","D","RD","D","RD",""},
           new[] {"RD*","","R","D*","","D"},
           new[] {"","RD","","RD","R","*"},
           new[] {"R*","*","R","","",""}
        },
        new string[][]
        {
           new[] {"","","D","D","RD*",""},
           new[] {"R","R","D*","D","","D"},
           new[] {"RD*","RD","","RD*","R","*"},
           new[] {"","","RD","R*","D","D"},
           new[] {"R","R","","","R","*"},
           new[] {"R","R*","R","R","",""}
        }

    };


    public Maze(int index)
    { 
        Index = index;

        grid = new Cell[6, 6];

        //populate the grid with cells
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 6; col++)
            {
                grid[row, col] = new Cell(row, col);
            }
        }

        //set outer walls
        for (int i = 0; i < 6; i++)
        {
            grid[i, 0].LeftWall = true;
            grid[i, 5].RightWall = true;
            grid[0, i].UpWall = true;
            grid[5, i].DownWall = true;
        }


        string[][] mazeStr = stringGrids[index];

        //set maze walls and bells
        for (int row = 0; row < 6; row++)
        {
            for(int col =  0; col < 6; col++)
            {
                string strs = mazeStr[row][col];
                if (strs.Contains("L"))
                {
                    grid[row, col].LeftWall = true;
                    grid[row, col - 1].RightWall = true;
                }

                if (strs.Contains("R"))
                {
                    grid[row, col].RightWall = true;
                    grid[row, col + 1].LeftWall = true;
                }

                if (strs.Contains("U"))
                {
                    grid[row, col].UpWall = true;
                    grid[row - 1, col].DownWall = true;
                }

                if (strs.Contains("D"))
                {
                    grid[row, col].DownWall = true;
                    grid[row + 1, col].UpWall = true;
                }

                if (strs.Contains("*"))
                {
                    grid[row, col].Bell = true;
                }
            }
        }
    }

    //prits out grid
    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine();

        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 6; col++)
            {
                Cell cell = grid[row, col];

                sb.Append(cell.LeftWall ? 'L' : '.');
                sb.Append(cell.RightWall ? 'R' : '.');
                sb.Append(cell.UpWall ? 'U' : '.');
                sb.Append(cell.DownWall ? 'D' : '.');
                sb.Append(cell.Bell ? '*' : '.');

                if (col < 5)
                    sb.Append("  ");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

}
