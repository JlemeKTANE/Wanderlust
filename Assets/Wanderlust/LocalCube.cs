public class LocalCube
{


    public enum Face { Left, Right, Up, Down, Front, Back }

    public Face left, right, up, down, front, back;

    public LocalCube()
    {
        left = Face.Left;
        right = Face.Right;
        up = Face.Up;
        down = Face.Down;
        front = Face.Front;
        back = Face.Back;
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
        //what is currently local down, should be local left
        //left = oldCube.down;
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
            left = oldCube.down;
            right = oldCube.up;
            up = oldCube.left;
            down = oldCube.right;
            front = oldCube.front;
            back = oldCube.back;
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
            left = oldCube.down;
            right = oldCube.up;
            up = oldCube.left;
            down = oldCube.right;
            front = oldCube.front;
            back = oldCube.back;
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
            left = oldCube.up;
            right = oldCube.down;
            up = oldCube.right;
            down = oldCube.left;
            front = oldCube.front;
            back = oldCube.back;
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
            left = oldCube.back;
            right = oldCube.front;
            up = oldCube.up;
            down = oldCube.down;
            front = oldCube.left;
            back = oldCube.right;
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
            left = oldCube.front;
            right = oldCube.back;
            up = oldCube.up;
            down = oldCube.down;
            front = oldCube.right;
            back = oldCube.left;
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
            left = oldCube.left;
            right = oldCube.right;
            up = oldCube.back;
            down = oldCube.front;
            front = oldCube.up;
            back = oldCube.down;
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
            left = oldCube.left;
            right = oldCube.right;
            up = oldCube.front;
            front = oldCube.back;
            back = oldCube.down;
            down = oldCube.up;
        }

        //Left,Right OR Right,Left -> 
        //EVEN(Right, Left, Up, Down, Back, Front)
        //ODD(Right, Left, Down, Up, Front, Back)

        else if
            ((desiredFace == Face.Left && targetFace == Face.Right) ||
            (desiredFace == Face.Right && targetFace == Face.Left))
        {
            left = oldCube.right;
            right = oldCube.left;

            if (evenModules)
            {
                up = oldCube.up;
                front = oldCube.down;
                back = oldCube.back;
                down = oldCube.front;
            }

            else
            {
                up = oldCube.down;
                front = oldCube.up;
                back = oldCube.front;
                down = oldCube.back;
            }
        }

        //Up,Down OR Down,Up ->
        //EVEN(Right, Left, Down, Up, Front, Back)
        //ODD(Left, Right, Down, Up, Back, Front)

        else if
            ((desiredFace == Face.Left && targetFace == Face.Right) ||
            (desiredFace == Face.Right && targetFace == Face.Left))
        {
            up = oldCube.down;
            front = oldCube.up;

            if (evenModules)
            {
                left = oldCube.right;
                right = oldCube.left;
                front = oldCube.up;
                back = oldCube.front;
                down = oldCube.back;
            }

            else
            {
                left = oldCube.left;
                right = oldCube.right;
                back = oldCube.back;
                down = oldCube.front;
            }
        }

        //Front,Back OR Back,Front ->
        //EVEN(Left, Right, Down, Up, Back, Front)
        //ODD(Right, Left, Up, Down, Back, Front)

        else if
            ((desiredFace == Face.Front && targetFace == Face.Back) ||
            (desiredFace == Face.Back && targetFace == Face.Front))
        {
            back = oldCube.back;
            down = oldCube.front;

            if (evenModules)
            {
                left = oldCube.left;
                right = oldCube.right;
                up = oldCube.down;
                front = oldCube.up;
                back = oldCube.back;
                down = oldCube.front;
            }

            else
            {
                left = oldCube.right;
                right = oldCube.left;
                up = oldCube.up;
                front = oldCube.down;
            }
        }

        //(Left, Right, Up, Down, Front, Back)



    }

    public LocalCube Clone()
    {
        return new LocalCube
        {
            left = this.left,
            right = this.right,
            up = this.up,
            down = this.down,
            front = this.front,
            back = this.back
        };
    }
}
