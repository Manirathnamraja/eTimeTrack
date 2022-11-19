namespace eTimeTrack.Models
{
    public enum InfoMessageType
    {
        Success,
        Warning,
        Failure
    }

    public class InfoMessage
    {
        public InfoMessageType MessageType { get; set; }
        public string MessageContent { get; set; }

        public InfoMessage() { }

        public InfoMessage(InfoMessageType messageType, string content)
        {
            MessageType = messageType;
            MessageContent = content;
        }
    }
}