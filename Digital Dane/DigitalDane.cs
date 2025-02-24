using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digital_Dane
{
    internal class DigitalDane
    {
        Position currentPosition;
        Communication Communicator;
        public void Start()
        {
            // 1. look/scan
            // 2. aim at nearest enemy
            // 3. go within reach
            // 4. attack
            // 5. repeat
        }

        private async Task MoveTo(Position target)
        {
            while (currentPosition != target)
            {
                // Compute differences along the X and Y axes.
                int? dx = target.Left - currentPosition!.Left;
                int? dy = target.Top - currentPosition!.Top;

                // If we're already at the target, break out of the loop.
                if (dx == 0 && dy == 0)
                    break;

                // Choose the primary direction based on the larger difference.
                MoveDirections primaryDirection;
                if (Math.Abs((decimal)dx!) >= Math.Abs((decimal)dy!))
                {
                    primaryDirection = dx > 0 ? MoveDirections.Right : MoveDirections.Left;
                }
                else
                {
                    primaryDirection = dy > 0 ? MoveDirections.Down : MoveDirections.Up;
                }

                // Attempt to move in the primary direction.
                MoveResponse response = Communicator.Move(primaryDirection);

                // If the move wasn't successful, try an alternative direction.
                if (!response.Success)
                {
                    MoveDirections alternativeDirection = primaryDirection;
                    // If we tried horizontal, attempt vertical if needed.
                    if ((primaryDirection == MoveDirections.Left || primaryDirection == MoveDirections.Right) && dy != 0)
                    {
                        alternativeDirection = dy > 0 ? MoveDirections.Down : MoveDirections.Up;
                    }
                    // If we tried vertical, attempt horizontal.
                    else if ((primaryDirection == MoveDirections.Up || primaryDirection == MoveDirections.Down) && dx != 0)
                    {
                        alternativeDirection = dx > 0 ? MoveDirections.Right : MoveDirections.Left;
                    }

                    // Only try the alternative if it is different.
                    if (alternativeDirection != primaryDirection)
                    {
                        response = Communicator.Move(alternativeDirection);
                        // If the alternative move fails -> break to avoid an infinite loop.
                        if (!response.Success)
                        {
                            break;
                        }
                    }
                    else
                    {
                        // If no alternative move exists -> break.
                        break;
                    }
                }

                // Update the bot's current position.
                currentPosition = response.NewPosition;

                await Task.Delay(1000);

                // Check for game over.
                if (response.GameOver)
                {
                    break;
                }
            }
        }

    }
}
