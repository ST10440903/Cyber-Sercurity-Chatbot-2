using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Media;
using System.IO;

namespace Cyber_Security_Chatbot_2
{
    class Program
    {
        // Memory to store user details and recent topic
        static Dictionary<string, string> userMemory = new Dictionary<string, string>();
        static string lastTopic = "";

        // Dictionary for dynamic keyword recognition with multiple responses
        static Dictionary<string, List<string>> keywordResponses = new Dictionary<string, List<string>>()
        {
            { "password safety", new List<string>
                {
                    "Use a mix of letters, numbers, and symbols. Never reuse old passwords!",
                    "Strong passwords should be 12+ characters with a great mix of letters, numbers, and symbols."
                }
            },
            { "phishing", new List<string>
                {
                    "Avoid clicking on suspicious links and always verify email senders.",
                    "Phishing scams often impersonate trusted sources. Stay highly cautious."
                }
            },
            { "safe browsing", new List<string>
                {
                    "Keep your browser updated and use reputable security extensions.",
                    "Always check for HTTPS and avoid shady pop-ups for secure browsing."
                }
            },
            { "two-factor authentication", new List<string>
                {
                    "Always enable 2FA for extra security. Use an authenticator app instead of SMS.",
                    "2FA is essential—add an extra layer to your account security."
                }
            },
            { "malware protection", new List<string>
                {
                    "Install reliable antivirus software and avoid downloading files from unknown sources.",
                    "Schedule regular scans to keep malware at bay."
                }
            },
            { "public wifi risks", new List<string>
                {
                    "Avoid accessing sensitive accounts on public WiFi; use a VPN for added security.",
                    "Public networks are risky—protect your data when you're on the go."
                }
            },
            { "social engineering", new List<string>
                {
                    "Be cautious of unsolicited messages asking for personal info. Always verify identities.",
                    "Social engineering attacks rely on trickery. Stay alert and safeguard your info."
                }
            },
            { "ransomware", new List<string>
                {
                    "Regularly back up important data and never pay ransom demands. Keep your system updated.",
                    "Backups and updated software are your best defense against ransomware."
                }
            },
            { "software updates", new List<string>
                {
                    "Keep your OS and applications updated to patch security vulnerabilities.",
                    "Regular updates help you stay protected against emerging threats."
                }
            },
            { "email security", new List<string>
                {
                    "Never open attachments from unknown senders. Check email addresses for spoofing.",
                    "Strong email security starts with cautious handling of every unexpected email."
                }
            },
            { "secure websites", new List<string>
                {
                    "Only enter your personal information on sites that use HTTPS and have valid security certificates.",
                    "Look for that little padlock icon before sharing any sensitive data online."
                }
            },
            { "data privacy", new List<string>
                {
                    "Limit the personal information you share online and adjust privacy settings on social media.",
                    "Your digital footprint matters—be selective about what you share."
                }
            }
        };

        // Words used for very basic sentiment analysis
        static string[] positiveWords = { "good", "great", "awesome", "happy", "love", "excellent", "fantastic" };
        static string[] negativeWords = { "bad", "hate", "terrible", "sad", "awful", "problem", "error" };

        static Random rnd = new Random();

        static void Main(string[] args)
        {
            GreetingMessage();
            DisplayAsciiArt();
            StartChat();
        }

        // Plays an audio greeting if available
        static void GreetingMessage()
        {
            try
            {
                SoundPlayer player = new SoundPlayer("welcomeUser.wav");
                player.PlaySync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: Audio file not found. " + ex.Message);
            }
        }

        // Displays ASCII art for a polished welcome screen
        static void DisplayAsciiArt()
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("  WELCOME TO THE PHANTOM TECH BOT  ");
            Console.WriteLine("========================================");
            Console.WriteLine(@"  
         ooooooooo.   ooooo   ooooo       .o.       ooooo      ooo ooooooooooooo   .oooooo.   ooo        ooooo 
         `888   `Y88. `888'   `888'      .888.      `888b.     `8' 8'   888   `8  d8P'  `Y8b  `88.       .888' 
          888   .d88'  888     888      .8""888.     8 `88b.    8       888      888      888  888b     d'888  
          888ooo88P'   888ooooo888     .8' `888.     8   `88b.  8       888      888      888  8 Y88. .P  888  
          888          888     888    .88ooo8888.    8     `88b.8       888      888      888  8  `888'   888  
          888          888     888   .8'     `888.   8       `888       888      `88b    d88'  8    Y     888  
         o888o        o888o   o888o o88o     o8888o o8o        `8      o888o      `Y8bood8P'  o8o        o888o 

         [Phantom Tech - Digital Security Experts]");
            Console.WriteLine("========================================");
        }

        // Begins the conversation, storing the user’s name and guiding them
        static void StartChat()
        {
            Console.Write("Hello! What's your name? ");
            string name = Console.ReadLine().Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "User";
            }
            Console.WriteLine($"Welcome, {name}! I am your CyberSecurity Assistant.");
            // Store user name for memory recall
            userMemory["name"] = name;

            Console.WriteLine("You can ask me about cybersecurity topics like 'password safety', 'phishing', 'safe browsing', etc.");
            Console.WriteLine("Type 'remember' to recall our last conversation, or 'exit' to end the chat.");

            while (true)
            {
                Console.Write("\nYou: ");
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("[Chatbot] Sorry, I didn't catch that. Could you please rephrase?");
                    continue;
                }

                string lowerInput = input.ToLower().Trim();

                if (lowerInput == "exit")
                {
                    Console.WriteLine("[Chatbot] Goodbye! Stay safe online.");
                    break;
                }

                if (lowerInput.Contains("remember"))
                {
                    RecallMemory();
                    continue;
                }

                ProcessInput(input);
            }
        }

        // Processes the input by checking for keywords and analyzing sentiment
        static void ProcessInput(string input)
        {
            string lowerInput = input.ToLower().Trim();
            string sentiment = AnalyzeSentiment(lowerInput);
            bool foundKeyword = false;

            // Check each keyword using regex search for flexibility
            foreach (var entry in keywordResponses)
            {
                string keyword = entry.Key;
                if (Regex.IsMatch(lowerInput, Regex.Escape(keyword), RegexOptions.IgnoreCase))
                {
                    foundKeyword = true;
                    lastTopic = keyword;  // Save the last topic discussed for memory
                    // Get a random response from the list of responses for that keyword
                    List<string> responses = entry.Value;
                    string response = responses[rnd.Next(responses.Count)];
                    Console.WriteLine("[Chatbot] " + response);
                    ProvideFollowUp(sentiment, keyword);
                    break;
                }
            }

            // If no specific keyword is found, return a generic response based on sentiment
            if (!foundKeyword)
            {
                if (sentiment == "negative")
                {
                    Console.WriteLine("[Chatbot] It seems you're experiencing some issues. Would you like some cybersecurity tips?");
                }
                else if (sentiment == "positive")
                {
                    Console.WriteLine("[Chatbot] Great to see your enthusiasm! Do you have a specific cybersecurity topic you want to explore?");
                }
                else
                {
                    Console.WriteLine("[Chatbot] I'm not exactly sure what you mean. Could you please specify a topic like 'password safety' or 'phishing'?");
                }
            }
        }

        // Performs a basic sentiment analysis by counting positive and negative indicator words
        static string AnalyzeSentiment(string input)
        {
            int score = 0;
            foreach (var word in positiveWords)
            {
                if (input.Contains(word))
                    score++;
            }
            foreach (var word in negativeWords)
            {
                if (input.Contains(word))
                    score--;
            }
            if (score > 0) return "positive";
            if (score < 0) return "negative";
            return "neutral";
        }

        // Offers follow-up questions based on the detected sentiment and topic
        static void ProvideFollowUp(string sentiment, string topic)
        {
            if (sentiment == "negative")
                Console.WriteLine($"[Chatbot] It seems like {topic} might be a concern. Would you like additional tips?");
            else if (sentiment == "positive")
                Console.WriteLine($"[Chatbot] I'm glad you find {topic} interesting! Would you like more advanced advice?");
            else
                Console.WriteLine($"[Chatbot] Do you need more details on {topic}?");
        }

        // Recalls saved memory (user name, last discussed topic) for context continuity
        static void RecallMemory()
        {
            Console.WriteLine("[Chatbot] Here’s what I remember so far:");
            if (userMemory.ContainsKey("name"))
            {
                Console.WriteLine($"[Chatbot] Your name is {userMemory["name"]}.");
            }
            if (!string.IsNullOrEmpty(lastTopic))
            {
                Console.WriteLine($"[Chatbot] We last talked about: {lastTopic}.");
            }
            else
            {
                Console.WriteLine("[Chatbot] We haven't discussed a particular topic yet.");
            }
        }
    }
}