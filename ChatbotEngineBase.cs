namespace AiAssisanceUI
{
    public class ChatbotEngineBase
    {

        private bool ContainsAny(string input, string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                if (input.Contains(keyword))
                {
                    return true;
                }
            }
            return false;
        }
    }
}