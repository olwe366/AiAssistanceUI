using AiAssisanceUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AiAssisanceUI
{
    public partial class MainWindow : Window
    {
        private ChatbotEngine chatbot;
        private SpeechSynthesizer speechSynthesizer;
        private SpeechRecognitionEngine speechRecognizer;
        private bool isListening = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeChatbot();
            InitializeSpeech();
            ShowWelcomeMessage();
        }

        private void InitializeChatbot()
        {
            chatbot = new ChatbotEngine();
        }

        private void InitializeSpeech()
        {
            // Initialize text-to-speech
            speechSynthesizer = new SpeechSynthesizer();
            speechSynthesizer.SetOutputToDefaultAudioDevice();
            speechSynthesizer.Rate = 1;
            speechSynthesizer.Volume = 100;

            // Initialize speech recognition
            try
            {
                speechRecognizer = new SpeechRecognitionEngine();
                speechRecognizer.LoadGrammar(new DictationGrammar());
                speechRecognizer.SetInputToDefaultAudioDevice();
                speechRecognizer.SpeechRecognized += SpeechRecognizer_SpeechRecognized;
            }
            catch (Exception ex)
            {
                AddBotMessage("⚠️ Voice recognition is not available on this system.", false);
            }
        }

        private void ShowWelcomeMessage()
        {
            // ASCII art greeting in chat
            string asciiGreeting = @"
╔════════════════════════════════════════╗
║  Welcome to Cybersecurity Guardian!    ║
║  Your personal cybersecurity assistant ║
║                                        ║
║  I can help you with:                  ║
║  • Password safety                     ║
║  • Scam detection                      ║
║  • Privacy protection                  ║
║  • Phishing prevention                 ║
║  • Malware protection                  ║
║                                        ║
║  How can I help you stay safe today?   ║
╚════════════════════════════════════════╝";

            AddBotMessage(asciiGreeting, true);

            // Voice greeting
            Task.Delay(1000).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    speechSynthesizer.SpeakAsync("Welcome to Cybersecurity Guardian. Your personal cybersecurity assistant. How can I help you stay safe today?");
                });
            });
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessUserInput();
        }

        private void UserInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                ProcessUserInput();
            }
        }

        private void VoiceInputButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isListening)
            {
                StartVoiceRecognition();
            }
            else
            {
                StopVoiceRecognition();
            }
        }

        private void StartVoiceRecognition()
        {
            try
            {
                if (speechRecognizer != null)
                {
                    speechRecognizer.RecognizeAsync(RecognizeMode.Multiple);
                    isListening = true;
                    VoiceInputButton.Content = "🔴 Stop";
                    VoiceInputButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));
                    AddBotMessage("🎤 Listening... Please speak now.", false);
                }
            }
            catch (Exception ex)
            {
                AddBotMessage("❌ Voice recognition error: " + ex.Message, false);
            }
        }

        private void StopVoiceRecognition()
        {
            if (speechRecognizer != null)
            {
                speechRecognizer.RecognizeAsyncStop();
                isListening = false;
                VoiceInputButton.Content = "🎤 Voice";
                VoiceInputButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));
            }
        }

        private void SpeechRecognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string recognizedText = e.Result.Text;
            Dispatcher.Invoke(() =>
            {
                UserInputTextBox.Text = recognizedText;
                StopVoiceRecognition();
                ProcessUserInput();
            });
        }

        private void ProcessUserInput()
        {
            string userInput = UserInputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(userInput))
                return;

            // Add user message to chat
            AddUserMessage(userInput);

            // Process with chatbot
            ChatbotResponse response = chatbot.ProcessUserInput(userInput);

            // Update sentiment indicator
            UpdateSentimentIndicator(response.Sentiment);

            // Add bot response
            AddBotMessage(response.Message, true);

            // Optional: Speak response
            if (response.ShouldSpeak)
            {
                speechSynthesizer.SpeakAsync(response.Message);
            }

            // Clear input
            UserInputTextBox.Clear();

            // Auto-scroll to bottom
            ScrollToBottom();
        }

        private void AddUserMessage(string message)
        {
            Border messageBorder = new Border
            {
                Style = (Style)FindResource("ChatBubbleUser"),
                Margin = new Thickness(10, 5, 50, 5)
            };

            TextBlock messageText = new TextBlock
            {
                Text = message,
                Style = (Style)FindResource("MessageTextUser"),
                TextWrapping = TextWrapping.Wrap
            };

            messageBorder.Child = messageText;
            ChatMessagesPanel.Children.Add(messageBorder);
        }

        private void AddBotMessage(string message, bool withAvatar = true)
        {
            Border messageBorder = new Border
            {
                Style = (Style)FindResource("ChatBubbleBot"),
                Margin = new Thickness(50, 5, 10, 5)
            };

            StackPanel contentStack = new StackPanel();

            if (withAvatar)
            {
                TextBlock avatar = new TextBlock
                {
                    Text = "🤖 Guardian: ",
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50")),
                    Margin = new Thickness(0, 0, 0, 5)
                };
                contentStack.Children.Add(avatar);
            }

            TextBlock messageText = new TextBlock
            {
                Text = message,
                Style = (Style)FindResource("MessageTextBot"),
                TextWrapping = TextWrapping.Wrap
            };

            contentStack.Children.Add(messageText);
            messageBorder.Child = contentStack;
            ChatMessagesPanel.Children.Add(messageBorder);
        }

        private void UpdateSentimentIndicator(string sentiment)
        {
            SentimentIndicator.Text = sentiment;

            switch (sentiment.ToLower())
            {
                case "worried":
                case "frustrated":
                    SentimentIndicator.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));
                    break;
                case "curious":
                    SentimentIndicator.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB"));
                    break;
                default:
                    SentimentIndicator.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60"));
                    break;
            }
        }

        private void ScrollToBottom()
        {
            ChatScrollViewer.ScrollToBottom();
        }

        private void UserInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Auto-resize logic can be added here if needed
        }
    }
}