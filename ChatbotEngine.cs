using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AiAssisanceUI
{
    public class ChatbotResponse
    {
        public string Message { get; set; }
        public string Sentiment { get; set; }
        public bool ShouldSpeak { get; set; }
    }

    public class ChatbotEngine
    {
        private Dictionary<string, UserInfo> userMemory;
        private Dictionary<string, List<string>> keywordResponses;
        private Dictionary<string, List<string>> randomResponseCategories;
        private Dictionary<string, ConversationContext> conversationContexts;
        private string currentTopic;
        private string lastResponse;

        public ChatbotEngine()
        {
            userMemory = new Dictionary<string, UserInfo>();
            conversationContexts = new Dictionary<string, ConversationContext>();
            InitializeKeywordResponses();
            InitializeRandomResponses();
            currentTopic = "";
            lastResponse = "";
        }

        private void InitializeKeywordResponses()
        {
            keywordResponses = new Dictionary<string, List<string>>
            {
                ["password"] = new List<string>
                {
                    "🔐 Password Security Tip: Use strong, unique passwords for each account. Consider using a password manager to generate and store complex passwords.",
                    "🔑 Password Best Practice: Enable two-factor authentication whenever possible. This adds an extra layer of security beyond just your password.",
                    "🛡️ Password Safety: Avoid using personal information like birthdays or names in your passwords. Use a mix of uppercase, lowercase, numbers, and symbols."
                },
                ["scam"] = new List<string>
                {
                    "⚠️ Scam Alert: Never share personal information or send money to unsolicited requests. Legitimate organizations won't ask for sensitive information via email or phone.",
                    "🕵️ Scam Detection: Be wary of urgent requests, too-good-to-be-true offers, and pressure tactics. Take time to verify before taking action.",
                    "🚨 Avoiding Scams: Research before you invest, verify charities before donating, and never click suspicious links. Trust your instincts - if it feels wrong, it probably is."
                },
                ["privacy"] = new List<string>
                {
                    "🔒 Privacy Protection: Review privacy settings on all your social media accounts regularly. Limit what you share publicly.",
                    "🛡️ Data Privacy: Use encrypted messaging apps for sensitive conversations. Be mindful of what personal data you share online.",
                    "👁️ Privacy Best Practices: Regularly clear your browser cache and cookies. Consider using a VPN for additional privacy protection."
                },
                ["phish"] = new List<string>
                {
                    "🎣 Phishing Prevention: Always check email sender addresses carefully. Hover over links before clicking to see the actual URL.",
                    "🛡️ Spotting Phishing: Look for spelling errors, generic greetings, and urgent demands. These are common signs of phishing attempts.",
                    "⚠️ Email Safety: Never download attachments or click links from unknown senders. When in doubt, contact the organization directly through official channels."
                },
                ["malware"] = new List<string>
                {
                    "🦠 Malware Protection: Keep your antivirus software updated and run regular system scans.",
                    "🔒 Safe Browsing: Don't download software from untrusted sources. Always verify the legitimacy of websites before entering information.",
                    "⚡ Prevention Tips: Enable automatic updates for your operating system and applications. Be cautious with USB drives from unknown sources."
                },
                ["encrypt"] = new List<string>
                {
                    "🔐 Encryption Explained: Encryption scrambles your data so only authorized parties can read it. Use encrypted connections (HTTPS) whenever possible.",
                    "📱 Data Security: Enable device encryption on your smartphones and computers. This protects your data if your device is lost or stolen."
                }
            };
        }

        private void InitializeRandomResponses()
        {
            randomResponseCategories = new Dictionary<string, List<string>>
            {
                ["greeting"] = new List<string>
                {
                    "Hello! How can I help you with cybersecurity today?",
                    "Hi there! Ready to boost your online security?",
                    "Greetings! What cybersecurity topic would you like to learn about?"
                },
                ["thanks"] = new List<string>
                {
                    "You're welcome! Stay safe online!",
                    "Happy to help! Cybersecurity is important for everyone.",
                    "Glad I could assist! Remember to practice these tips daily."
                },
                ["general_tip"] = new List<string>
                {
                    "💡 Quick Tip: Always update your software and apps promptly to get the latest security patches.",
                    "💡 Pro Tip: Back up your important data regularly to protect against ransomware and hardware failure.",
                    "💡 Security Tip: Use different passwords for different accounts to prevent credential stuffing attacks."
                },
                ["encouragement"] = new List<string>
                {
                    "You're doing great! Cybersecurity awareness is a journey, not a destination.",
                    "Keep learning! Every security measure you implement makes you safer online.",
                    "Excellent question! Being curious about security shows you care about your digital safety."
                }
            };
        }

        public ChatbotResponse ProcessUserInput(string userInput)
        {
            string lowerInput = userInput.ToLower();
            ChatbotResponse response = new ChatbotResponse();

            // Detect sentiment
            response.Sentiment = DetectSentiment(lowerInput);

            // Check for conversation continuation
            if (IsContinuationRequest(lowerInput))
            {
                response.Message = HandleContinuation(lowerInput);
                response.ShouldSpeak = true;
                return response;
            }

            // Check for memory recall (asking about stored info)
            if (IsMemoryRecallRequest(lowerInput))
            {
                response.Message = RecallUserInfo(lowerInput);
                if (!string.IsNullOrEmpty(response.Message))
                {
                    response.ShouldSpeak = true;
                    return response;
                }
            }

            // Check for user info storage (name, interests, etc.)
            StoreUserInfo(lowerInput);

            // Check for cybersecurity keywords
            string responseMessage = GetKeywordResponse(lowerInput);
            if (!string.IsNullOrEmpty(responseMessage))
            {
                response.Message = responseMessage;
                response.ShouldSpeak = true;
                return response;
            }

            // Default responses for various inputs
            if (ContainsAny(lowerInput, new[] { "hello", "hi", "hey", "greetings" }))
            {
                response.Message = GetRandomResponse("greeting");
            }
            else if (ContainsAny(lowerInput, new[] { "thanks", "thank you", "appreciate" }))
            {
                response.Message = GetRandomResponse("thanks");
            }
            else if (ContainsAny(lowerInput, new[] { "help", "what can you do", "capabilities" }))
            {
                response.Message = GetHelpMessage();
            }
            else
            {
                response.Message = GetDefaultResponse();
            }

            response.ShouldSpeak = true;
            lastResponse = response.Message;
            return response;
        }

        private string GetKeywordResponse(string input)
        {
            foreach (var keyword in keywordResponses.Keys)
            {
                if (input.Contains(keyword))
                {
                    currentTopic = keyword;
                    var responses = keywordResponses[keyword];
                    string selectedResponse = responses[new Random().Next(responses.Count)];

                    // Personalize with user info
                    selectedResponse = PersonalizeResponse(selectedResponse);

                    return selectedResponse;
                }
            }
            return null;
        }

        private string GetRandomResponse(string category)
        {
            if (randomResponseCategories.ContainsKey(category))
            {
                var responses = randomResponseCategories[category];
                return responses[new Random().Next(responses.Count)];
            }
            return "I'm here to help with cybersecurity! What would you like to know?";
        }

        private string DetectSentiment(string input)
        {
            string[] worriedKeywords = { "worried", "scared", "anxious", "nervous", "fear", "concerned", "unsafe" };
            string[] frustratedKeywords = { "frustrated", "annoyed", "confused", "difficult", "hard", "complicated" };
            string[] curiousKeywords = { "curious", "wondering", "interesting", "learn", "tell me", "explain" };

            if (ContainsAny(input, worriedKeywords))
                return "worried";
            if (ContainsAny(input, frustratedKeywords))
                return "frustrated";
            if (ContainsAny(input, curiousKeywords))
                return "curious";

            return "neutral";
        }

        private bool ContainsAny(string input, string[] keywords)
        {
            return keywords.Any(keyword => input.Contains(keyword));
        }

        private void StoreUserInfo(string input)
        {
            // Extract and store name
            Match nameMatch = Regex.Match(input, @"my name is (\w+)", RegexOptions.IgnoreCase);
            if (nameMatch.Success)
            {
                string name = nameMatch.Groups[1].Value;
                if (!userMemory.ContainsKey("name"))
                    userMemory["name"] = new UserInfo { InfoType = "name", Value = name };

                AddBotNote($"📝 I'll remember your name is {name}!");
            }

            // Extract and store interests
            string[] topics = { "password", "privacy", "scam", "phish", "malware", "encrypt" };
            foreach (string topic in topics)
            {
                if (input.Contains(topic) && input.Contains("interested in"))
                {
                    if (!userMemory.ContainsKey("interest"))
                        userMemory["interest"] = new UserInfo { InfoType = "interest", Value = topic };

                    AddBotNote($"📝 I'll remember you're interested in {topic}!");
                }
            }
        }

        private string RecallUserInfo(string input)
        {
            if (input.Contains("my name") && userMemory.ContainsKey("name"))
            {
                return $"Your name is {userMemory["name"].Value}! That's a great name for someone interested in cybersecurity. 😊";
            }

            if (input.Contains("interested in") && userMemory.ContainsKey("interest"))
            {
                return $"You're interested in {userMemory["interest"].Value}. That's an important topic in cybersecurity! Would you like to learn more about it?";
            }

            return null;
        }

        private string PersonalizeResponse(string response)
        {
            if (userMemory.ContainsKey("name"))
            {
                string name = userMemory["name"].Value;
                if (new Random().Next(3) == 0) // 33% chance to personalize
                {
                    return $"{name}, {response.ToLower()}";
                }
            }

            if (userMemory.ContainsKey("interest") && response.Contains(currentTopic))
            {
                string interest = userMemory["interest"].Value;
                if (interest == currentTopic)
                {
                    return $"Since you're interested in {interest}, {response.ToLower()}";
                }
            }

            return response;
        }

        private bool IsContinuationRequest(string input)
        {
            string[] continuationKeywords = { "more", "another", "continue", "tell me more", "explain more", "elaborate" };
            return ContainsAny(input, continuationKeywords);
        }

        private string HandleContinuation(string input)
        {
            if (!string.IsNullOrEmpty(currentTopic) && keywordResponses.ContainsKey(currentTopic))
            {
                var responses = keywordResponses[currentTopic];
                string newResponse = responses[new Random().Next(responses.Count)];
                return $"Here's another tip about {currentTopic}: {newResponse}";
            }

            return GetRandomResponse("general_tip");
        }

        private bool IsMemoryRecallRequest(string input)
        {
            return input.Contains("remember") || input.Contains("recall") || input.Contains("my name") || input.Contains("interested in");
        }

        private string GetHelpMessage()
        {
            return @"🤖 I can help you with various cybersecurity topics:
            
                   • 🔐 Password Safety - Learn about strong passwords and 2FA
                   • ⚠️ Scam Detection - Recognize and avoid online scams
                   • 🔒 Privacy Protection - Keep your personal data secure
                   • 🎣 Phishing Prevention - Spot fake emails and messages
                   • 🦠 Malware Protection - Defend against viruses and malware

                   Try asking:
                  'Tell me about password safety'
                  'How to avoid scams?'
                  'What are privacy tips?'
                  'Give me phishing tips'
                  'Explain malware protection'

                  I also remember your name and interests if you share them!";
        }

        private string GetDefaultResponse()
        {
            string[] defaultResponses = {
                "I'm not sure I understand. Can you try rephrasing? I can help with passwords, scams, privacy, phishing, and malware protection.",
                "I want to help with cybersecurity, but I didn't quite catch that. Could you ask about password safety, scam prevention, or privacy tips?",
                "Hmm, I'm not familiar with that topic. Would you like to learn about passwords, scams, privacy, or phishing prevention instead?",
                "I'm still learning! Could you rephrase your question? I specialize in cybersecurity topics like password safety and scam detection."
            };

            return defaultResponses[new Random().Next(defaultResponses.Length)];
        }

        private void AddBotNote(string note)
        {
            // This would be handled by the UI to show system messages
            System.Diagnostics.Debug.WriteLine(note);
        }
    }

    public class UserInfo
    {
        public string InfoType { get; set; }
        public string Value { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class ConversationContext
    {
        public string CurrentTopic { get; set; }
        public string LastQuestion { get; set; }
        public List<string> History { get; set; } = new List<string>();
    }
}