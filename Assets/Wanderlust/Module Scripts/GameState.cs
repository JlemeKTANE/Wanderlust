using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//State to help with BFS
public class GameState
{
    public int CurrentMazeIndex { get; private set; }
    public int BellRingCount { get; private set; }
    public Cell CurrentCell { get; private set; }
    public GameState ParentState { get; private set; }
    public static int[] SerialNumberToNum;

    public GameState(int currentMazeIndex, int bellRingCount, Cell currentCell, GameState parentState)
    {
        BellRingCount = bellRingCount;
        CurrentMazeIndex = currentMazeIndex;
        CurrentCell = currentCell;
        ParentState = parentState;
    }

    public static bool StatesAreSame(GameState state1, GameState state2)
    {
        return state1.CurrentMazeIndex == state2.CurrentMazeIndex &&
               state1.BellRingCount == state2.BellRingCount &&
               state1.CurrentCell.Row == state2.CurrentCell.Row &&
               state1.CurrentCell.Column == state2.CurrentCell.Column;
    }

    public List<GameState> GetAvaiableGameStatesNieghbors()
    {
        List<GameState> nextStates = new List<GameState>();

        //get all the neighbor cells within the same maze
        List<Cell> neighbors = new List<Cell>();

        
        if (!CurrentCell.UpWall && CurrentCell.Row != 0)
        {
            neighbors.Add(Wanderlust.mazes[CurrentMazeIndex].grid[CurrentCell.Row - 1, CurrentCell.Column]);

        }
        if (!CurrentCell.DownWall && CurrentCell.Row != 5)
        {
            neighbors.Add(Wanderlust.mazes[CurrentMazeIndex].grid[CurrentCell.Row + 1, CurrentCell.Column]);
        }
        if (!CurrentCell.RightWall && CurrentCell.Column != 5)
        {
            neighbors.Add(Wanderlust.mazes[CurrentMazeIndex].grid[CurrentCell.Row, CurrentCell.Column + 1]);
        }
        if (!CurrentCell.LeftWall && CurrentCell.Column != 0)
        {
            neighbors.Add(Wanderlust.mazes[CurrentMazeIndex].grid[CurrentCell.Row, CurrentCell.Column - 1]);
        }

        foreach (Cell cell in neighbors)
        {
            nextStates.Add(new GameState(CurrentMazeIndex, BellRingCount, cell, this));
        }

        //If there is a bell, add a state upon we would press to go to a different maze
        if (CurrentCell.Bell)
        {
            //increment the number of bell rings
            int newBellRingCount = BellRingCount + 1;

            //figure out what the new maze index would be
            int newCurrntMazeIndex = Wanderlust.GetMazeIndex(SerialNumberToNum, newBellRingCount);

            //figure out what the new currnt cell would be
            Cell newCurrentCell = Wanderlust.mazes[newCurrntMazeIndex].grid[CurrentCell.Row, CurrentCell.Column];

            nextStates.Add(new GameState(newCurrntMazeIndex, newBellRingCount, newCurrentCell, this));
        }

        return nextStates;
    }

    public override string ToString()
    {
        return "Current Maze Index: " + CurrentMazeIndex + " | Row: " + CurrentCell.Row + " | Column: " + CurrentCell.Column + "| Bell Ring Count: " + BellRingCount; 
    }
}
