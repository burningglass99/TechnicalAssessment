
namespace TechnicalAssessment.Models
{
    /// <summary>
    /// Class to represent the active board
    /// </summary>
    public class Board
    {
        public Node[,] nodes;

        public bool GameStarted { get; set; }

        /// <summary>
        /// Constructor initializes all of the nodes and flags that the game has not been started yet
        /// </summary>
        public Board()
        {
            nodes = new Node[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for(int j = 0; j < 4; j++)
                {
                    nodes[i,j] = new Node();
                }
            }
            this.GameStarted = false;
        }
    }
}
