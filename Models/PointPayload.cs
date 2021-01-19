
namespace TechnicalAssessment.Models
{
    /// <summary>
    /// Class to represent the json payload object received from the client when a node is clicked
    /// </summary>
    public class PointPayload
    {
        public int id { get; set; }
        public string msg { get; set; }
        public Point body { get; set; }
    }
}




