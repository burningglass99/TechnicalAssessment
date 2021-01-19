
namespace TechnicalAssessment.Models
{
    /// <summary>
    /// Class to represent the json payload received when the body is a string
    /// </summary>
    public class Payload
    {
        public int id { get; set; }
        public string msg { get; set; }
        public string body { get; set; }
    }
}
