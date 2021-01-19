
namespace TechnicalAssessment.Models
{
    /// <summary>
    /// Class to represent a node on the board
    /// Activated means there is only one line attached to it
    /// Closed means there are two lines attached to it
    /// If activated and not closed, node is an endpoint of the line and a valid start node
    /// </summary>
    public class Node
    {
        public bool Activated { get; set; }
        public bool Closed { get; set; }
    }
}
