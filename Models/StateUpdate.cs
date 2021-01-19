
namespace TechnicalAssessment.Models
{
    /// <summary>
    /// Class to represent the body of the payload sent back to the client
    /// </summary>
    public class StateUpdate
    {
        public Line newLine { get; set; }
        public string heading { get; set; }
        public string message { get; set; }
    }
}
