using UnityEngine;

public class LocalCube
{
    public enum Face { Left, Right, Up, Down, Front, Back }
    //vairable is the local face, the value is the global face
    public Face Left { get; private set; }
    public Face Right { get; private set; }
    public Face Up { get; private set; }
    public Face Down { get; private set; }
    public Face Front { get; private set; }
    public Face Back { get; private set; }
    public LocalCube()
    {
        Left = Face.Left;
        Right = Face.Right;
        Up = Face.Up;
        Down = Face.Down;
        Front = Face.Front;
        Back = Face.Back;
    }
    //I hate this so much - Hawker
    /// <summary>
    /// 
    /// </summary>
    /// <param name="desiredFace">The local face we want to move</param>
    /// <param name="targetFace">The "position" of the local face we want to move to</param>
    /// <param name="evenModules">If there are an even amount of modules</param>
    public void Rotate(Face desiredFace, Face targetFace, bool evenModules)
    {
        LocalCube oldCube = this.Clone();
        //if they are the same, then we don't need to do anything
        if (desiredFace == targetFace)
        {
            return;
        }
        #region Example
        //[R, L, F, B, U, D]
        //[B, F, R, L, U, D]
        //what is currently at index 3 should be at index 0
        //what is currently local Down, should be local Left
        //Left = oldCube.Down;
        #endregion

        //Left,Up OR
        //Up,Right OR
        //Right,Down OR
        //Down,Left ->
        //(Down, Up, Left, Right, Front, Back)
        if ((desiredFace == Face.Left && targetFace == Face.Up) ||
            (desiredFace == Face.Up && targetFace == Face.Right) ||
            (desiredFace == Face.Right && targetFace == Face.Down) ||
            (desiredFace == Face.Down && targetFace == Face.Left))
        {
            Left = oldCube.Down;
            Right = oldCube.Up;
            Up = oldCube.Left;
            Down = oldCube.Right;
            Front = oldCube.Front;
            Back = oldCube.Back;
        }

        //Left,Up OR
        //Up,Right OR
        //Right,Down OR
        //Down,Left ->
        //(Down, Up, Left, Right, Front, Back)
        else if
            ((desiredFace == Face.Left && targetFace == Face.Up) ||
            (desiredFace == Face.Up && targetFace == Face.Right) ||
            (desiredFace == Face.Right && targetFace == Face.Down) ||
            (desiredFace == Face.Down && targetFace == Face.Left))
        {
            Left = oldCube.Down;
            Right = oldCube.Up;
            Up = oldCube.Left;
            Down = oldCube.Right;
            Front = oldCube.Front;
            Back = oldCube.Back;
        }

        //Left,Down OR
        //Down,Right OR
        //Right,Up OR
        //Up,Left ->
        //(Up, Down, Right, Left, Front, Back)
        else if
            ((desiredFace == Face.Left && targetFace == Face.Down) ||
            (desiredFace == Face.Down && targetFace == Face.Right) ||
            (desiredFace == Face.Right && targetFace == Face.Up) ||
            (desiredFace == Face.Up && targetFace == Face.Left))
        {
            Left = oldCube.Up;
            Right = oldCube.Down;
            Up = oldCube.Right;
            Down = oldCube.Left;
            Front = oldCube.Front;
            Back = oldCube.Back;
        }

        //Left,Front OR
        //Front,Right OR
        //Right,Back OR
        //Back,Left ->
        //(Back, Front, Up, Down, Left, Right)
        else if
            ((desiredFace == Face.Left && targetFace == Face.Front) ||
            (desiredFace == Face.Front && targetFace == Face.Right) ||
            (desiredFace == Face.Right && targetFace == Face.Back) ||
            (desiredFace == Face.Back && targetFace == Face.Left))
        {
            Left = oldCube.Back;
            Right = oldCube.Front;
            Up = oldCube.Up;
            Down = oldCube.Down;
            Front = oldCube.Left;
            Back = oldCube.Right;
        }

        //Left,Back OR
        //Back,Right OR
        //Right,Front OR
        //Front,Left->
        //(Front, Back, Up, Down, Right, Left)
        else if
            ((desiredFace == Face.Left && targetFace == Face.Back) ||
            (desiredFace == Face.Back && targetFace == Face.Right) ||
            (desiredFace == Face.Right && targetFace == Face.Front) ||
            (desiredFace == Face.Front && targetFace == Face.Left))
        {
            Left = oldCube.Front;
            Right = oldCube.Back;
            Up = oldCube.Up;
            Down = oldCube.Down;
            Front = oldCube.Right;
            Back = oldCube.Left;
        }

        //Up,Front OR
        //Front,Down OR
        //Down,Back OR
        //Back,Up ->
        //(Left, Right, Back, Front, Up, Down)
        else if
            ((desiredFace == Face.Up && targetFace == Face.Front) ||
            (desiredFace == Face.Front && targetFace == Face.Down) ||
            (desiredFace == Face.Down && targetFace == Face.Back) ||
            (desiredFace == Face.Back && targetFace == Face.Up))
        {
            Left = oldCube.Left;
            Right = oldCube.Right;
            Up = oldCube.Back;
            Down = oldCube.Front;
            Front = oldCube.Up;
            Back = oldCube.Down;
        }

        //Up,Back OR
        //Back,Down OR
        //Down,Front OR
        //Front,Up ->
        //(Left, Right, Front, Back, Down, Up)

        else if
            ((desiredFace == Face.Up && targetFace == Face.Back) ||
            (desiredFace == Face.Back && targetFace == Face.Down) ||
            (desiredFace == Face.Down && targetFace == Face.Front) ||
            (desiredFace == Face.Front && targetFace == Face.Up))
        {
            Left = oldCube.Left;
            Right = oldCube.Right;
            Up = oldCube.Front;
            Down = oldCube.Back;
            Front = oldCube.Down;
            Back = oldCube.Up;
        }

        //Left,Right OR Right,Left -> 
        //EVEN(Right, Left, Up, Down, Back, Front)
        //ODD(Right, Left, Down, Up, Front, Back)

        else if
            ((desiredFace == Face.Left && targetFace == Face.Right) ||
            (desiredFace == Face.Right && targetFace == Face.Left))
        {
            Left = oldCube.Right;
            Right = oldCube.Left;

            if (evenModules)
            {
                Up = oldCube.Up;
                Down = oldCube.Down;
                Front = oldCube.Back;
                Back = oldCube.Front;
            }

            else
            {
                Up = oldCube.Down;
                Down = oldCube.Up;
                Front = oldCube.Front;
                Back = oldCube.Back;
            }
        }

        //Up,Down OR Down,Up ->
        //EVEN(Right, Left, Down, Up, Front, Back)
        //ODD(Left, Right, Down, Up, Back, Front)

        else if
            ((desiredFace == Face.Up && targetFace == Face.Down) ||
            (desiredFace == Face.Down && targetFace == Face.Up))
        {
            Up = oldCube.Down;
            Down = oldCube.Up;

            if (evenModules)
            {
                Left = oldCube.Right;
                Right = oldCube.Left;
                Front = oldCube.Front;
                Back = oldCube.Back;
            }

            else
            {
                Left = oldCube.Left;
                Right = oldCube.Right;
                Front = oldCube.Back;
                Back = oldCube.Front;
            }
        }

        //Front,Back OR Back,Front ->
        //EVEN(Left, Right, Down, Up, Back, Front)
        //ODD(Right, Left, Up, Down, Back, Front)

        else if
            ((desiredFace == Face.Front && targetFace == Face.Back) ||
            (desiredFace == Face.Back && targetFace == Face.Front))
        {
            Front = oldCube.Back;
            Back = oldCube.Front;

            if (evenModules)
            {
                Left = oldCube.Left;
                Right = oldCube.Right;
                Up = oldCube.Down;
                Down = oldCube.Up;
            }

            else
            {
                Left = oldCube.Right;
                Right = oldCube.Left;
                Up = oldCube.Up;
                Down = oldCube.Down;
            }
        }

        //(Left, Right, Up, Down, Front, Back)



    }
    public Face GetFace(string letters)
    {
        if (letters[0] == 'L')
        {
            switch (letters[1])
            {
                case 'L':
                    return Left;
                case 'R':
                    return Right;
                case 'U':
                    return Up;
                case 'D':
                    return Down;
                case 'F':
                    return Front;
                case 'B':
                    return Back;

            }
        }
        else
        {
            switch (letters[1])
            {
                case 'L':
                    return Face.Left;
                case 'R':
                    return Face.Right;
                case 'U':
                    return Face.Up;
                case 'D':
                    return Face.Down;
                case 'F':
                    return Face.Front;
                case 'B':
                    return Face.Back;

            }
        }
        return Face.Left;
    }

    public LocalCube Clone()
    {
        return new LocalCube
        {
            Left = this.Left,
            Right = this.Right,
            Up = this.Up,
            Down = this.Down,
            Front = this.Front,
            Back = this.Back
        };
    }
    public string ToString()
    {
        return "Local Left = Global " + Left + "\n"
            + "Local Right = Global " + Right + "\n"
            + "Local Up = Global " + Up + "\n"
            + "Local Down = Global " + Down + "\n"
            + "Local Front = Global " + Front + "\n"
            + "Local Back = Global " + Back + "\n";
    }
}
