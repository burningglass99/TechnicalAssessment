
namespace TechnicalAssessment.Models
{
    /// <summary>
    /// Class to represent the response object to be serialized and sent back to the client
    /// </summary>
    public class ResponsePayload
    {
        public int id { get; set; }
        public string msg { get; set; }
        public StateUpdate body { get; set; }
    }

}
