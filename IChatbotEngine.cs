namespace AiAssisanceUI
{
    public interface IChatbotEngine
    {
        event Action<string> OnMemoryUpdate;

        ChatbotResponse ProcessUserInput(string userInput);
    }
}