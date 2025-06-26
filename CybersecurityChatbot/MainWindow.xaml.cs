using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;
using System.Globalization;
using System.Media;
using System.Text;

namespace CybersecurityChatbot
{
    public partial class MainWindow : Window
    {
        
        private List<IResponseHandler> responseHandlers;
        private SentimentResponseHandler sentimentHandler;
        private string userName;

        // Task management
        private ObservableCollection<CybersecurityTask> tasks = new ObservableCollection<CybersecurityTask>();

        // Quiz management
        private List<QuizQuestion> quizQuestions = new List<QuizQuestion>();
        private int currentQuizQuestionIndex = -1;
        private int quizScore = 0;
        private bool quizInProgress = false;

        // Activity log
        private ObservableCollection<string> activityLog = new ObservableCollection<string>();

        public MainWindow()
        {
            InitializeComponent();

            // Play welcome sound and show ASCII art
            Loaded += async (sender, e) =>
            {
                PlayWelcomeMessage();
                await DisplayAnimatedAsciiArt();

                string name;
                do
                {
                    name = await GetUserNameAsync();
                    if (string.IsNullOrEmpty(name))
                    {

                        var result = MessageBox.Show("You must enter a name to continue. Try again?",
                                                   "Name Required",
                                                   MessageBoxButton.YesNo,
                                                   MessageBoxImage.Warning);

                        if (result == MessageBoxResult.No)
                        {
                            Close();
                            return;
                        }
                    }
                } while (string.IsNullOrEmpty(name));


                userName = name;
                UserNameDisplay.Text = $"User: {userName}";
                AddToActivityLog($"User session started for {userName}");


                TaskList.ItemsSource = tasks;
                ActivityLog.ItemsSource = activityLog;
                InitializeResponseHandlers();
                InitializeQuizQuestions();

                AddChatMessage("Cybersecurity Bot", $"I'm here to help you with cybersecurity awareness.", Colors.Green);
                AddChatMessage("Cybersecurity Bot", "You can ask me about passwords, phishing, safe browsing, social engineering, or two-factor authentication.", Colors.Blue);
            };
        }
      

        private async void PlayWelcomeMessage()
        {
            try
            {
                AddChatMessage("System", "Initializing cybersecurity assistant...", Colors.Gray);

                await Task.Run(() =>
                {
                    using (var player = new SoundPlayer("C:\\Users\\lab_services_student\\source\\repos\\CybersecurityChatbot\\Audio\\Welcome.wav"))
                    {
                        player.PlaySync();
                    }
                });
            }
            catch (Exception ex)
            {
                AddChatMessage("System", $"Audio note: {ex.Message}", Colors.Red);
            }
        }

        private async Task DisplayAnimatedAsciiArt()
        {
            var colors = new[] { Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Cyan, Colors.Magenta };

            AddChatMessage("System", "Loading cybersecurity protocols...", Colors.Gray);

            string asciiArt = @"
 ██████╗ ██╗   ██╗██████╗ ███████╗██████╗ 
██╔════╝ ╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗
██║       ╚████╔╝ ██████╔╝█████╗  ██████╔╝
██║        ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗
╚██████╗    ██║   ██████ ║███████╗██║  ██║
 ╚═════╝    ╚═╝   ╚══════╝╚══════╝╚═╝  ╚═╝
███████╗███████╗ ██████╗██╗   ██╗██████╗ ██╗████████╗██╗   ██╗
██╔════╝██╔════╝██╔════╝██║   ██║██╔══██╗██║╚══██╔══╝╚██╗ ██╔╝
███████╗█████╗  ██║     ██║   ██║██████╔╝██║   ██║    ╚████╔╝ 
╚════██║██╔══╝  ██║     ██║   ██║██╔══██╗██║   ██║     ╚██╔╝  
███████║███████╗╚██████╗╚██████╔╝██║  ██║██║   ██║      ██║   
╚══════╝╚══════╝ ╚═════╝ ╚═════╝ ╚═╝  ╚═╝╚═╝   ╚═╝      ╚═╝   
!!!CYBERSECURITY!!!CYBERSECURITY!!!CYBERSECURITY!!!";

            for (int i = 0; i < 5; i++)
            {
                AddChatMessage("System", asciiArt, colors[i % colors.Length]);
                await Task.Delay(300);
            }
            
            AddChatMessage("System", asciiArt, Colors.Black);
        }
        private async Task<string> GetUserNameAsync()
        {
            var inputDialog = new NameInputDialog { Owner = this };

            if (inputDialog.ShowDialog() == true)
            {
                string name = inputDialog.UserName;
                AddChatMessage("System", $"Nice to meet you, {name}!", Colors.Green);
                return name;
            }

            return string.Empty;
        }


        private void InitializeResponseHandlers()
        {
            sentimentHandler = new SentimentResponseHandler();
            responseHandlers = new List<IResponseHandler>
            {
                new TaskHandler(this),
                new GreetingHandler(),
                new PurposeHandler(),
                new PasswordSafetyHandler(),
                new PhishingHandler(),
                new BrowsingHandler(),
                new SocialEngineeringHandler(),
                new TwoFactorAuthHandler(),
                new DefaultHandler()
            };

            foreach (var handler in responseHandlers.OfType<ResponseHandlerBase>())
            {
                handler.SentimentDetected += sentimentHandler.HandleSentiment;
                handler.MessageAdded += (sender, message, color) =>
                {
                    Dispatcher.Invoke(() => AddChatMessage("Cybersecurity Bot", message, color));
                };
            }
        }

        private void InitializeQuizQuestions()
        {
            // Clear any existing questions first
            quizQuestions.Clear();

            // Password Security
            quizQuestions.Add(new QuizQuestion
            {
                Question = "🔐 What's the BEST way to create a strong password?",
                Options = new List<string> {
            "Use your pet's name with 123",
            "A random mix of letters, numbers & symbols",
            "Same password for all accounts but add 'secure'",
            "Write it down on a sticky note"
        },
                CorrectAnswerIndex = 1,
                Explanation = "💡 Correct! Random, complex passwords are hardest to crack. Try using a password manager!"
            });

            quizQuestions.Add(new QuizQuestion
            {
                Question = "🔄 How often should you change important passwords?",
                Options = new List<string> {
            "Never if they're strong",
            "Every 3-6 months",
            "Only after a breach",
            "Every January 1st"
        },
                CorrectAnswerIndex = 1,
                Explanation = "⏱️ Good practice! Regular changes limit exposure if a password is compromised."
            });

            // Phishing
            quizQuestions.Add(new QuizQuestion
            {
                Question = "📧 You get an email from 'IT Support' asking for your password. What do you do?",
                Options = new List<string> {
            "Reply with your password immediately",
            "Forward it to your real IT department",
            "Click the link to verify their identity",
            "Check if the email looks professional"
        },
                CorrectAnswerIndex = 1,
                Explanation = "✅ Right! Never share passwords via email. This is a classic phishing attempt!"
            });

            quizQuestions.Add(new QuizQuestion
            {
                Question = "📱 Your phone gets a text: 'Urgent! Click to claim your prize!' You:",
                Options = new List<string> {
            "Click immediately - free stuff!",
            "Forward to friends first",
            "Delete and report as spam",
            "Reply 'STOP' to unsubscribe"
        },
                CorrectAnswerIndex = 2,
                Explanation = "🛑 Smart move! This is 'smishing' - SMS phishing. Never engage with suspicious texts!"
            });

            // Authentication
            quizQuestions.Add(new QuizQuestion
            {
                Question = "🔒 What makes Two-Factor Authentication (2FA) more secure?",
                Options = new List<string> {
            "It uses two passwords",
            "It requires something you know AND something you have",
            "It doubles your login time",
            "It works on two devices simultaneously"
        },
                CorrectAnswerIndex = 1,
                Explanation = "🌟 2FA combines knowledge (password) with possession (phone/device) for extra security."
            });

            quizQuestions.Add(new QuizQuestion
            {
                Question = "📲 Which 2FA method is most secure?",
                Options = new List<string> {
            "SMS text messages",
            "Authenticator apps",
            "Email codes",
            "Security questions"
        },
                CorrectAnswerIndex = 1,
                Explanation = "🔐 Authenticator apps are more secure than SMS which can be intercepted via SIM swapping."
            });

            // Network Security
            quizQuestions.Add(new QuizQuestion
            {
                Question = "💻 On public WiFi, you should avoid:",
                Options = new List<string> {
            "Checking social media",
            "Online banking or shopping",
            "Reading news websites",
            "All of the above"
        },
                CorrectAnswerIndex = 1,
                Explanation = "⚠️ Careful! Public WiFi is risky for sensitive activities. Use a VPN if you must access important accounts!"
            });

            quizQuestions.Add(new QuizQuestion
            {
                Question = "🌐 What does a VPN protect against?",
                Options = new List<string> {
            "Eavesdropping on public WiFi",
            "Location tracking",
            "Some ISP monitoring",
            "All of the above"
        },
                CorrectAnswerIndex = 3,
                Explanation = "🔐 VPNs encrypt your connection, making all these protections possible!"
            });

            // Social Engineering
            quizQuestions.Add(new QuizQuestion
            {
                Question = "🤔 What's 'social engineering'?",
                Options = new List<string> {
            "A type of computer virus",
            "Manipulating people to reveal sensitive info",
            "A new social media platform",
            "Engineering degree program"
        },
                CorrectAnswerIndex = 1,
                Explanation = "🎯 Bullseye! Social engineering tricks people, not systems. Always verify requests for sensitive info!"
            });

            quizQuestions.Add(new QuizQuestion
            {
                Question = "📛 You get a call from 'Microsoft' about a virus. They:",
                Options = new List<string> {
            "Ask for remote access to 'fix' it",
            "Demand payment immediately",
            "Use scare tactics about your data",
            "All of the above"
        },
                CorrectAnswerIndex = 3,
                Explanation = "🚫 Scam alert! Legitimate companies won't call unsolicited about tech issues."
            });

            // Device Security
            quizQuestions.Add(new QuizQuestion
            {
                Question = "💾 You find a USB drive in the parking lot. Do you:",
                Options = new List<string> {
            "Plug it in to find the owner",
            "Give it to security/lost & found",
            "Format it and use it yourself",
            "Check for cool files first"
        },
                CorrectAnswerIndex = 1,
                Explanation = "🚨 Danger! This could be a 'USB drop attack'. Never plug in unknown devices!"
            });

            quizQuestions.Add(new QuizQuestion
            {
                Question = "📱 How often should you update your phone's OS?",
                Options = new List<string> {
            "Only when new features appear",
            "Whenever updates are available",
            "When the phone gets slow",
            "Never - updates break things"
        },
                CorrectAnswerIndex = 1,
                Explanation = "🛡️ Security patches in updates often fix critical vulnerabilities!"
            });

            // Website Security
            quizQuestions.Add(new QuizQuestion
            {
                Question = "🔍 How can you spot a fake website?",
                Options = new List<string> {
            "Check for HTTPS and padlock icon",
            "Look for poor spelling/grammar",
            "Verify the URL is correct",
            "All of the above"
        },
                CorrectAnswerIndex = 3,
                Explanation = "🔎 Detective skills! All these help identify fake sites trying to steal your info."
            });

            quizQuestions.Add(new QuizQuestion
            {
                Question = "🛒 Safe online shopping requires:",
                Options = new List<string> {
            "Checking for HTTPS",
            "Reading reviews first",
            "Using credit cards not debit",
            "All of the above"
        },
                CorrectAnswerIndex = 3,
                Explanation = "🛍️ All these practices help protect your money and information when shopping online."
            });

            // Data Protection
            quizQuestions.Add(new QuizQuestion
            {
                Question = "📅 How often should you back up important files?",
                Options = new List<string> {
            "Only when you remember",
            "After ransomware attacks",
            "On a regular schedule",
            "Never - cloud storage is enough"
        },
                CorrectAnswerIndex = 2,
                Explanation = "💾 3-2-1 Rule: 3 copies, 2 different media, 1 offsite. Schedule automatic backups!"
            });

            quizQuestions.Add(new QuizQuestion
            {
                Question = "☁️ Cloud storage is:",
                Options = new List<string> {
            "Always more secure than local",
            "Only safe with strong passwords",
            "Convenient but requires trust in provider",
            "Never secure for sensitive files"
        },
                CorrectAnswerIndex = 2,
                Explanation = "⚖️ Cloud has pros/cons. Enable 2FA and encrypt sensitive files before uploading."
            });

            // Malware
            quizQuestions.Add(new QuizQuestion
            {
                Question = "🤖 What's a 'botnet'?",
                Options = new List<string> {
            "Robot vacuum network",
            "Hacked devices controlled by attackers",
            "New social media bot trend",
            "Blockchain technology"
        },
                CorrectAnswerIndex = 1,
                Explanation = "👾 Scary but true! Botnets can launch massive attacks using everyday infected devices."
            });

            quizQuestions.Add(new QuizQuestion
            {
                Question = "💀 Ransomware does what?",
                Options = new List<string> {
            "Encrypts files until you pay",
            "Steals passwords silently",
            "Shows endless popup ads",
            "Slows down your computer"
        },
                CorrectAnswerIndex = 0,
                Explanation = "💰 Regular backups are your best defense against ransomware extortion!"
            });

            // Privacy
            quizQuestions.Add(new QuizQuestion
            {
                Question = "👀 Browser cookies can:",
                Options = new List<string> {
            "Only remember login sessions",
            "Track your activity across sites",
            "Give you viruses",
            "Physically damage your computer"
        },
                CorrectAnswerIndex = 1,
                Explanation = "🍪 While useful, cookies enable tracking. Regularly clear them or use private browsing."
            });

            quizQuestions.Add(new QuizQuestion
            {
                Question = "📱 App permissions should be:",
                Options = new List<string> {
            "Always accepted to use the app",
            "Granted only when needed",
            "Ignored - they don't matter",
            "The same for all apps"
        },
                CorrectAnswerIndex = 1,
                Explanation = "🛡️ Limit permissions to only what's essential for the app's function."
            });

            // Security Concepts
            quizQuestions.Add(new QuizQuestion
            {
                Question = "👑 What's the 'principle of least privilege'?",
                Options = new List<string> {
            "Give everyone admin access",
            "Only grant necessary permissions",
            "Make passwords easy to remember",
            "Let employees share accounts"
        },
                CorrectAnswerIndex = 1,
                Explanation = "🎓 Exactly! Limit access to only what's needed - reduces damage from breaches."
            });

            quizQuestions.Add(new QuizQuestion
            {
                Question = "🎣 What's the goal of phishing?",
                Options = new List<string> {
            "Steal login credentials",
            "Install malware",
            "Gain financial info",
            "All of the above"
        },
                CorrectAnswerIndex = 3,
                Explanation = "🎣 Phishing tries to hook ALL these prizes! Always think before you click."
            });

            // Physical Security
            quizQuestions.Add(new QuizQuestion
            {
                Question = "🏢 At work, you should:",
                Options = new List<string> {
            "Share badges with trusted coworkers",
            "Always lock your computer when away",
            "Write passwords on your desk calendar",
            "Use the same login as your neighbor"
        },
                CorrectAnswerIndex = 1,
                Explanation = "🔒 Physical security matters! Never share access credentials or leave devices unlocked."
            });

            quizQuestions.Add(new QuizQuestion
            {
                Question = "📝 Paper records should be:",
                Options = new List<string> {
            "Recycled immediately when done",
            "Stored in open bins for easy access",
            "Shredded when containing sensitive info",
            "Left on printers for others to collect"
        },
                CorrectAnswerIndex = 2,
                Explanation = "🗑️ 'Dumpster diving' is a real threat! Always properly destroy sensitive documents."
            });

            // Incident Response
            quizQuestions.Add(new QuizQuestion
            {
                Question = "🚨 You suspect a malware infection. First step?",
                Options = new List<string> {
            "Post about it on social media",
            "Disconnect from the network",
            "Keep using the device normally",
            "Run every antivirus program at once"
        },
                CorrectAnswerIndex = 1,
                Explanation = "🔌 Isolation prevents spread! Then contact IT professionals for help."
            });

            quizQuestions.Add(new QuizQuestion
            {
                Question = "⚠️ After a data breach, you should:",
                Options = new List<string> {
            "Change affected passwords immediately",
            "Monitor accounts for suspicious activity",
            "Check credit reports if financial data leaked",
            "All of the above"
        },
                CorrectAnswerIndex = 3,
                Explanation = "🛡️ All these steps help protect yourself after a breach. Consider credit freezes too."
            });

            // Emerging Threats
            quizQuestions.Add(new QuizQuestion
            {
                Question = "🤖 AI security risks include:",
                Options = new List<string> {
            "More convincing phishing emails",
            "Automated password guessing",
            "Deepfake voice scams",
            "All of the above"
        },
                CorrectAnswerIndex = 3,
                Explanation = "⚠️ AI empowers attackers too! Stay extra vigilant about verifying unusual requests."
            });

            quizQuestions.Add(new QuizQuestion
            {
                Question = "📶 5G networks bring:",
                Options = new List<string> {
            "Only benefits, no new risks",
            "Faster speeds but more attack surfaces",
            "Complete immunity to hacking",
            "Built-in antivirus protection"
        },
                CorrectAnswerIndex = 1,
                Explanation = "⚡ New tech often introduces new vulnerabilities alongside improvements."
            });

            // Home Security
            quizQuestions.Add(new QuizQuestion
            {
                Question = "🏠 Smart home devices need:",
                Options = new List<string> {
            "Default passwords kept for convenience",
            "Regular firmware updates",
            "No security - they're harmless",
            "Connection to public WiFi only"
        },
                CorrectAnswerIndex = 1,
                Explanation = "🏡 IoT devices are common attack targets! Change defaults and keep them updated."
            });

            quizQuestions.Add(new QuizQuestion
            {
                Question = "👶 Child online safety includes:",
                Options = new List<string> {
            "Sharing their photos publicly",
            "Using parental controls",
            "Letting them browse unsupervised",
            "Giving full social media access at age 5"
        },
                CorrectAnswerIndex = 1,
                Explanation = "🧒 Start security education early! Monitor activity and use age-appropriate restrictions."
            });

            // Business Security
            quizQuestions.Add(new QuizQuestion
            {
                Question = "💼 Employees should receive:",
                Options = new List<string> {
            "No security training",
            "Annual cybersecurity awareness training",
            "Only technical staff need training",
            "One training session when hired"
        },
                CorrectAnswerIndex = 1,
                Explanation = "🏢 Regular training reduces human error - the cause of 90% of breaches!"
            });

            ShuffleQuestions();
        }

        private void ShuffleQuestions()
        {
            var rng = new Random();
            quizQuestions = quizQuestions.OrderBy(x => rng.Next()).Take(10).ToList();
        }

        private void AddChatMessage(string sender, string message, Color color)
        {
            ChatHistory.Items.Add(new ChatMessage
            {
                Sender = sender,
                Message = message,
                TextColor = Brushes.White,
                BackgroundColor = new SolidColorBrush(color)
            });


            if (ChatHistory.Items.Count > 0)
            {
                ChatHistory.ScrollIntoView(ChatHistory.Items[ChatHistory.Items.Count - 1]);
            }
        }

        private void AddToActivityLog(string activity)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            activityLog.Insert(0, $"[{timestamp}] {activity}");


            if (activityLog.Count > 100)
            {
                activityLog.RemoveAt(activityLog.Count - 1);
            }
        }


        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessUserInput();
        }

        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
            {
                ProcessUserInput();
                e.Handled = true;
            }
        }

        private void ProcessUserInput()
        {
            string input = UserInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            AddChatMessage(userName, input, Colors.MidnightBlue);
            AddToActivityLog($"User input: {input}");


            if (input.Equals("what have you done for me?", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("show activity log", StringComparison.OrdinalIgnoreCase))
            {
                ShowActivitySummary();
                UserInput.Clear();
                return;
            }


            var handler = responseHandlers.FirstOrDefault(h => h.CanHandle(input)) ?? responseHandlers.Last();
            handler.Handle(input, userName);

            UserInput.Clear();
        }

        private void ShowActivitySummary()
        {
            AddChatMessage("Cybersecurity Bot", "Here's a summary of recent actions:", Colors.Cyan);

            if (tasks.Any())
            {
                string taskSummary = string.Join("\n", tasks.Select((t, i) =>
                    $"{i + 1}. Task: '{t.Title}' {(t.HasReminder ? $"(Reminder set for {t.ReminderDate?.ToString("d")})" : "")}"));
                AddChatMessage("Cybersecurity Bot", taskSummary, Colors.SaddleBrown);
            }

            if (quizInProgress)
            {
                AddChatMessage("Cybersecurity Bot", $"Quiz in progress: {quizScore} correct answers so far.", Colors.SaddleBrown);
            }
        }


        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskTitle.Text))
            {
                MessageBox.Show("Please enter a task title.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var task = new CybersecurityTask
            {
                Title = TaskTitle.Text,
                Description = TaskDescription.Text,
                HasReminder = TaskReminderCheck.IsChecked ?? false,
                ReminderDate = TaskReminderCheck.IsChecked ?? false ? TaskReminderDate.SelectedDate : null
            };

            tasks.Add(task);
            AddToActivityLog($"Task added: {task.Title}");

            string response = $"Task added: '{task.Title}'. ";
            if (task.HasReminder && task.ReminderDate.HasValue)
            {
                response += $"I'll remind you on {task.ReminderDate.Value.ToString("d")}.";
            }
            AddChatMessage("Cybersecurity Bot", response, Colors.Green);

 
            TaskTitle.Clear();
            TaskDescription.Clear();
            TaskReminderCheck.IsChecked = false;
        }

        private void CompleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is CybersecurityTask task)
            {
                tasks.Remove(task);
                AddToActivityLog($"Task completed: {task.Title}");
                AddChatMessage("Cybersecurity Bot", $"Marked task '{task.Title}' as completed.", Colors.Green);
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is CybersecurityTask task)
            {
                tasks.Remove(task);
                AddToActivityLog($"Task deleted: {task.Title}");
                AddChatMessage("Cybersecurity Bot", $"Deleted task '{task.Title}'.", Colors.Green);
            }
        }

        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            currentQuizQuestionIndex = -1;
            quizScore = 0;
            quizInProgress = true;

            QuizScore.Text = $"Score: {quizScore}/{quizQuestions.Count}";
            NextQuestionButton.IsEnabled = true;
            StartQuizButton.IsEnabled = false;

            AddToActivityLog("Quiz started");
            ShowNextQuizQuestion();
        }

        private void ShowNextQuizQuestion()
        {
            currentQuizQuestionIndex++;

            if (currentQuizQuestionIndex < quizQuestions.Count)
            {
                var question = quizQuestions[currentQuizQuestionIndex];
                QuizQuestionText.Text = $"Question {currentQuizQuestionIndex + 1}: {question.Question}";

                QuizOptions.ItemsSource = question.Options;
                QuizFeedback.Text = string.Empty;


                foreach (var item in QuizOptions.Items)
                {
                    var container = QuizOptions.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                    if (container != null)
                    {
                        var radioButton = FindVisualChild<RadioButton>(container);
                        if (radioButton != null)
                        {
                            radioButton.IsChecked = false;
                        }
                    }
                }
            }
            else
            {
                // Quiz completed
                QuizCompleted();
            }
        }

        private void QuizOption_Checked(object sender, RoutedEventArgs e)
        {
            if (currentQuizQuestionIndex >= 0 && currentQuizQuestionIndex < quizQuestions.Count)
            {
                var radioButton = sender as RadioButton;
                if (radioButton != null)
                {
                    int selectedIndex = QuizOptions.Items.IndexOf(radioButton.Content);
                    var question = quizQuestions[currentQuizQuestionIndex];

                    if (selectedIndex == question.CorrectAnswerIndex)
                    {
                        quizScore++;
                        QuizFeedback.Text = $"Correct! {question.Explanation}";
                        QuizFeedback.Foreground = Brushes.LightGreen;
                    }
                    else
                    {
                        string correctAnswer = question.Options[question.CorrectAnswerIndex];
                        QuizFeedback.Text = $"Incorrect! The correct answer was: {correctAnswer}\n{question.Explanation}";
                        QuizFeedback.Foreground = Brushes.LightPink;
                    }

                    QuizScore.Text = $"Score: {quizScore}/{quizQuestions.Count}";
                }
            }
        }

        private void NextQuestion_Click(object sender, RoutedEventArgs e)
        {
            ShowNextQuizQuestion();
        }
        

        private void QuizCompleted()
        {
            quizInProgress = false;
            NextQuestionButton.IsEnabled = false;
            StartQuizButton.IsEnabled = true;

            QuizQuestionText.Text = "Quiz Completed!";
            QuizOptions.ItemsSource = null;

            string feedback;
            if (quizScore == quizQuestions.Count)
            {
                feedback = "Perfect score! You're a cybersecurity expert!";
            }
            else if (quizScore >= quizQuestions.Count * 0.8)
            {
                feedback = "Great job! You have excellent cybersecurity knowledge.";
            }
            else if (quizScore >= quizQuestions.Count * 0.5)
            {
                feedback = "Good effort! Consider reviewing more cybersecurity topics.";
            }
            else
            {
                feedback = "Keep learning! Cybersecurity is important for everyone.";
            }

            QuizFeedback.Text = $"Final Score: {quizScore}/{quizQuestions.Count}\n{feedback}";
            QuizFeedback.Foreground = Brushes.White;

            AddToActivityLog($"Quiz completed with score: {quizScore}/{quizQuestions.Count}");
            AddChatMessage("Cybersecurity Bot", $"Quiz completed! You scored {quizScore} out of {quizQuestions.Count}. {feedback}", Colors.Cyan);
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            activityLog.Clear();
            AddToActivityLog("Activity log cleared");
        }

        private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

       
        private class InputDialog : Window
        {
            public string Answer { get; private set; }

            public InputDialog(string title, string message)
            {
                Title = title;
                Width = 400;
                Height = 200;
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                ResizeMode = ResizeMode.NoResize;

                var grid = new Grid { Margin = new Thickness(10) };
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var messageText = new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                Grid.SetRow(messageText, 0);

                var inputBox = new TextBox
                {
                    Name = "InputBox",
                    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(5)
                };
                Grid.SetRow(inputBox, 1);

                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                Grid.SetRow(buttonPanel, 2);

                var okButton = new Button
                {
                    Content = "OK",
                    Width = 80,
                    Margin = new Thickness(0, 0, 10, 0),
                    IsDefault = true
                };
                okButton.Click += (s, e) =>
                {
                    Answer = inputBox.Text;
                    DialogResult = true;
                };

                var cancelButton = new Button
                {
                    Content = "Cancel",
                    Width = 80,
                    IsCancel = true
                };
                cancelButton.Click += (s, e) => DialogResult = false;

                buttonPanel.Children.Add(okButton);
                buttonPanel.Children.Add(cancelButton);

                grid.Children.Add(messageText);
                grid.Children.Add(inputBox);
                grid.Children.Add(buttonPanel);

                Content = grid;
            }
        }


        public class ChatMessage
        {
            public string Sender { get; set; }
            public string Message { get; set; }
            public Brush TextColor { get; set; }
            public Brush BackgroundColor { get; set; }
        }

        public class CybersecurityTask
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public bool HasReminder { get; set; }
            public DateTime? ReminderDate { get; set; }

            public string ReminderText => HasReminder ?
                $"Reminder: {ReminderDate?.ToString("d")}" :
                "No reminder set";
        }

        public class QuizQuestion
        {
            public string Question { get; set; }
            public List<string> Options { get; set; }
            public int CorrectAnswerIndex { get; set; }
            public string Explanation { get; set; }
        }

        public delegate void SentimentEventHandler(string sentiment, string userInput, string userName);
        public delegate void MessageAddedEventHandler(object sender, string message, Color color);

        public interface IResponseHandler
        {
            bool CanHandle(string input);
            void Handle(string input, string userName);
            event SentimentEventHandler SentimentDetected;
        }

        public abstract class ResponseHandlerBase : IResponseHandler
        {
            protected List<string> Keywords { get; set; } = new List<string>();
            protected Color ResponseColor { get; set; } = Colors.White;
            protected Random Random { get; } = new Random();
            protected Dictionary<string, List<string>> ResponseVariations { get; } = new Dictionary<string, List<string>>();

            public event SentimentEventHandler SentimentDetected = delegate { };
            public event MessageAddedEventHandler MessageAdded = delegate { };

            protected Dictionary<string, string> UserMemory { get; private set; } = new Dictionary<string, string>();

            public virtual bool CanHandle(string input)
            {
                return Keywords.Any(keyword => input.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            public abstract void Handle(string input, string userName);

            protected void AddMessage(string message, Color color)
            {
                MessageAdded?.Invoke(this, message, color);
            }

            protected void CheckForSentiment(string input, string userName)
            {
                var negativeWords = new List<string> { "worried", "scared", "afraid", "nervous", "frustrated" };
                var positiveWords = new List<string> { "happy", "excited", "great", "good", "interested" };

                if (negativeWords.Any(w => input.Contains(w, StringComparison.OrdinalIgnoreCase)))
                    SentimentDetected?.Invoke("negative", input, userName);
                else if (positiveWords.Any(w => input.Contains(w, StringComparison.OrdinalIgnoreCase)))
                    SentimentDetected?.Invoke("positive", input, userName);
            }


            protected void Remember(string key, string value)
            {
                if (UserMemory.ContainsKey(key))
                    UserMemory[key] = value;
                else
                    UserMemory.Add(key, value);
            }

            protected string Recall(string key)
            {
                return UserMemory.ContainsKey(key) ? UserMemory[key] : null;
            }

            public string RecallMemory(string key) => Recall(key);

            protected string GetRandomResponse(string variationKey)
            {
                if (ResponseVariations.ContainsKey(variationKey) && ResponseVariations[variationKey].Count > 0)
                {
                    var responses = ResponseVariations[variationKey];
                    string memoryKey = $"last_response_{variationKey}";
                    int lastIndex = Recall(memoryKey) != null ? int.Parse(Recall(memoryKey)) : -1;

                    var availableResponses = responses
                        .Select((r, i) => new { Response = r, Index = i })
                        .Where(x => x.Index != lastIndex)
                        .ToList();

                    if (availableResponses.Count == 0)
                        availableResponses = responses.Select((r, i) => new { Response = r, Index = i }).ToList();

                    var selected = availableResponses[Random.Next(availableResponses.Count)];
                    Remember(memoryKey, selected.Index.ToString());

                    return selected.Response;
                }
                return string.Empty;
            }
        }

        public class GreetingHandler : ResponseHandlerBase
        {
            public GreetingHandler()
            {
                Keywords = new List<string> { "hello", "hey", "how are you" };
                ResponseColor = Colors.Green;
                ResponseVariations.Add("greeting", new List<string> {
                    "I'm doing well, {0}! Ready to help with your cybersecurity questions.",
                    "All systems functioning optimally, {0}! How can I assist you today?",
                    "Hello {0}! I'm here and ready to discuss cybersecurity with you.",
                    "Greetings {0}! Your digital safety is my top priority. What can I help with?",
                    "Hi there {0}! Let's talk about keeping you secure online."
                });
            }

            public override void Handle(string input, string userName)
            {
                CheckForSentiment(input, userName);
                string response = string.Format(GetRandomResponse("greeting"), userName);
                AddMessage(response, ResponseColor);
            }
        }

        public class PurposeHandler : ResponseHandlerBase
        {
            public PurposeHandler()
            {
                Keywords = new List<string> { "purpose", "what are you", "why are you here", "function", "Goal" };
                ResponseColor = Colors.Cyan;
                ResponseVariations.Add("purpose", new List<string> {
                    "{0}, my purpose is to:\n- Educate about cyber threats\n- Provide safety tips\n- Help recognize scams\n- Promote digital safety",
                    "{0}, I'm here to:\n• Teach cybersecurity basics\n• Warn about online dangers\n• Suggest protection methods\n• Answer your questions",
                    "{0}, I was created to:\n1. Increase security awareness\n2. Prevent cyber crimes\n3. Share best practices\n4. Make the web safer",
                    "{0}, my mission includes:\n~ Cybersecurity education\n~ Threat prevention\n~ Safe browsing guidance\n~ Password protection",
                    "{0}, I exist to:\n> Identify digital risks\n> Recommend security measures\n> Explain security concepts\n> Help you stay safe online"
                });
            }

            public override void Handle(string input, string userName)
            {
                CheckForSentiment(input, userName);
                string response = string.Format(GetRandomResponse("purpose"), userName);
                AddMessage(response, ResponseColor);
            }
        }

        public class PasswordSafetyHandler : ResponseHandlerBase
        {
            public PasswordSafetyHandler()
            {
                Keywords = new List<string> { "password", "passwords", "secure password" };
                ResponseColor = Colors.Magenta;
                ResponseVariations.Add("main", new List<string> {
                    "Strong Password Guidelines:\n- Use at least 12 characters\n- Combine uppercase, lowercase, numbers & symbols\n- Avoid personal information\n- Use unique passwords for each account\n\nPro Tip: Use passphrases like 'Coffee@RainyCapeTown2023!'",
                    "Password Safety Tips:\n1. Never share your passwords\n2. Change passwords every 3-6 months\n3. Use a password manager\n4. Enable two-factor authentication\n\nRemember, a strong password is your first defense!",
                    "Creating Secure Passwords:\n• Length is more important than complexity\n• Avoid dictionary words\n• Consider using the first letters of a sentence\n• Example: 'I love hiking in Table Mountain every Sunday!' becomes 'IlhiTMeS!'",
                    "Password Security Facts:\n- 80% of hacking breaches involve weak passwords\n- Adding just one special character makes a password 10x harder to crack\n- The most common password is still '123456' - don't be that person!",
                    "Advanced Password Tips:\n1. Use a different password for every account\n2. Consider using passwordless authentication where available\n3. Regularly change your password to ensure ultimate safety"
                });
            }

            public override void Handle(string input, string userName)
            {
                CheckForSentiment(input, userName);
                Remember("last_topic", "password safety");
                string response;
                if (Recall("interest") == "password safety")
                {
                    response = $"Since you're interested in password safety, {userName}, here's more:\n" +
                              GetRandomResponse("main");
                }
                else
                {
                    response = GetRandomResponse("main");
                }

                AddMessage(response, ResponseColor);

                if (input.Contains("remember", StringComparison.OrdinalIgnoreCase))
                {
                    Remember("interest", "password safety");
                    AddMessage($"\nI'll remember you're interested in password safety, {userName}!", ResponseColor);
                }
            }
        }

        public class PhishingHandler : ResponseHandlerBase
        {
            public PhishingHandler()
            {
                Keywords = new List<string> { "phishing", "scam", "email scam" };
                ResponseColor = Colors.Magenta;
                ResponseVariations.Add("main", new List<string> {
                    "How to Spot Phishing Attempts:\n- Check sender email addresses carefully\n- Hover over links to see actual URLs\n- Look for poor grammar/spelling\n- Be wary of urgent requests\n- Verify unexpected attachments\n\nREMEMBER banks will NEVER ask for your PIN via email/SMS",
                    "Phishing Red Flags:\n1. Generic greetings like 'Dear Customer'\n2. Threats of account closure\n3. Requests for immediate action\n4. Suspicious attachments\n\nWhen in doubt, contact the organization directly!",
                    "Anti-Phishing Tips:\n• Don't click links in unsolicited emails\n• Bookmark important sites instead of clicking links\n• Check for HTTPS in URLs\n• Keep your browser updated",
                    "Advanced Phishing Defense:\n~ Enable email filtering\n~ Use email authentication \n~ Verify sender phone numbers independently\n~ Report phishing attempts to your IT department",
                    "Phishing Statistics:\n- 90% of data breaches start with phishing\n- Employees receive 14 malicious emails per year on average\n- Spear phishing accounts for 65% of targeted attacks"
                });
            }

            public override void Handle(string input, string userName)
            {
                CheckForSentiment(input, userName);
                Remember("last_topic", "phishing");
                string response = Recall("important_topic") == "phishing"
                    ? $"{userName}, since phishing is important to you:\n{GetRandomResponse("main")}"
                    : GetRandomResponse("main");

                AddMessage(response, ResponseColor);

                if (input.Contains("remember", StringComparison.OrdinalIgnoreCase))
                {
                    Remember("important_topic", "phishing");
                    AddMessage($"\nI'll remember that phishing is an important topic for you, {userName}!", ResponseColor);
                }
            }
        }

        public class BrowsingHandler : ResponseHandlerBase
        {
            public BrowsingHandler()
            {
                Keywords = new List<string> { "browsing", "safe internet", "https" };
                ResponseColor = Colors.Magenta;
                ResponseVariations.Add("main", new List<string> {
                    "Safe Browsing Practices:\n- Always look for HTTPS in website URLs\n- Keep your browser and plugins updated\n- Use a reputable antivirus program\n- Avoid public Wi-Fi for sensitive transactions\n- Clear browser cache and cookies regularly",
                    "Internet Safety Tips:\n1. Verify website security before entering credentials\n2. Use ad-blockers to avoid malicious ads\n3. Be cautious with downloads\n4. Check privacy policies of websites",
                    "Secure Web Browsing:\n• Use privacy-focused browsers when possible\n• Enable phishing protection in your browser\n• Review extension permissions regularly\n• Consider using a separate browser for financial transactions",
                    "Browser Security Enhancements:\n~ Enable automatic updates\n~ Use secure DNS \n~ Disable Flash and Java plugins\n~ Regularly clear browsing data",
                    "Dangerous Online Behaviors to Avoid:\n- Using the same password across sites\n- Ignoring browser security warnings\n- Downloading software from untrusted sources\n- Clicking 'Remember me' on public computers"
                });
            }

            public override void Handle(string input, string userName)
            {
                CheckForSentiment(input, userName);
                Remember("last_topic", "safe browsing");
                AddMessage(GetRandomResponse("main"), ResponseColor);
            }
        }

        public class SocialEngineeringHandler : ResponseHandlerBase
        {
            public SocialEngineeringHandler()
            {
                Keywords = new List<string> { "social engineering", "phone scam", "impersonation" };
                ResponseColor = Colors.Magenta;
                ResponseVariations.Add("main", new List<string> {
                    "Social Engineering Awareness:\n- Never share sensitive information over the phone\n- Verify identities before granting access\n- Be cautious of 'too good to be true' offers\n- Don't plug in unknown USB devices\n- Report suspicious requests to your IT department",
                    "Avoiding Social Engineering:\n1. Question unexpected requests for information\n2. Verify unusual requests through another channel\n3. Be wary of urgent or threatening language\n4. Educate family members about common scams",
                    "Common Social Engineering Tactics:\n• Pretexting (creating fake scenarios)\n• Baiting (offering something enticing)\n• Quid pro quo (offering something in exchange)\n• Tailgating (following into secure areas)",
                    "Real-World Social Engineering Examples:\n- Tech support scams claiming your computer is infected\n- Fake IT staff asking for your password\n- 'CEO fraud' emails requesting urgent wire transfers\n- Fake job offers requesting personal information",
                    "Psychological Triggers Exploited:\n~ Authority (pretending to be someone important)\n~ Urgency (creating time pressure)\n~ Familiarity (pretending to know you)\n~ Social proof (claiming others have complied)"
                });
            }

            public override void Handle(string input, string userName)
            {
                CheckForSentiment(input, userName);
                Remember("last_topic", "social engineering");
                string response = Recall("last_social_engineering_response") != null
                    ? $"{userName}, building on our previous talk about social engineering:\n{GetRandomResponse("main")}"
                    : GetRandomResponse("main");

                AddMessage(response, ResponseColor);
                Remember("last_social_engineering_response", "true");
            }
        }

        public class TwoFactorAuthHandler : ResponseHandlerBase
        {
            public TwoFactorAuthHandler()
            {
                Keywords = new List<string> { "2fa", "two factor", "authentication" };
                ResponseColor = Colors.Magenta;
                ResponseVariations.Add("main", new List<string> {
                    "Two-Factor Authentication (2FA):\n- Always enable 2FA where available\n- Use authenticator apps instead of SMS when possible\n- Keep backup codes in a secure place\n- Consider hardware security keys for high-value accounts\n- Review authorized devices regularly",
                    "Benefits of 2FA:\n1. Adds an extra layer of security beyond passwords\n2. Protects against credential stuffing attacks\n3. Can prevent unauthorized access even if password is compromised\n4. Available on most major platforms",
                    "Implementing 2FA:\n• Use apps like Google Authenticator \n• Set up backup methods in case you lose your primary\n• Be cautious of 2FA code requests (could be phishing)\n• Consider using biometric 2FA where available",
                    "2FA Statistics:\n- Reduces account takeover risk by 99.9%\n- Only 28% of users enable it when available\n- SMS 2FA is better than nothing but vulnerable to SIM swapping\n- Push notification 2FA has the highest user adoption",
                    "Advanced 2FA Tips:\n~ Use different 2FA methods for different accounts\n~ Store backup codes in a password manager\n~ Register multiple devices for critical accounts\n~ Periodically review active 2FA sessions"
                });
            }

            public override void Handle(string input, string userName)
            {
                CheckForSentiment(input, userName);
                Remember("last_topic", "two-factor authentication");
                string interest = Recall("2fa_interest");
                string response = interest == "yes"
                    ? $"{userName}, since you asked about 2FA before:\n{GetRandomResponse("main")}"
                    : GetRandomResponse("main");

                AddMessage(response, ResponseColor);

                if (input.Contains("remember", StringComparison.OrdinalIgnoreCase))
                {
                    Remember("2fa_interest", "yes");
                    AddMessage($"\nI'll remember your interest in two-factor authentication, {userName}!", ResponseColor);
                }
            }
        }

        public class DefaultHandler : ResponseHandlerBase
        {
            public DefaultHandler()
            {
                Keywords = new List<string>();
                ResponseVariations.Add("default", new List<string> {
                    "I didn't quite understand that. Could you rephrase?",
                    "I'm not sure I follow. Can you try asking differently?",
                    "That's not something I'm programmed to handle. Try asking about cybersecurity topics."
                });
            }

            public override bool CanHandle(string input) => true;

            public override void Handle(string input, string userName)
            {
                AddMessage(GetRandomResponse("default"), Colors.Red);
                AddMessage("Try asking about: passwords, phishing, safe browsing, social engineering, or 2FA.", Colors.Red);
            }
        }

        public class SentimentResponseHandler
        {
            public void HandleSentiment(string sentiment, string userInput, string userName)
            {
                if (sentiment == "negative")
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"I understand this might feel overwhelming, {userName}. Let's take it one step at a time.",
                            "I Sense Some Concern", MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                }
                else if (sentiment == "positive")
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Great to see your enthusiasm about cybersecurity, {userName}!",
                            "Positive Attitude", MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                }
            }
        }
        public class TaskHandler : ResponseHandlerBase
        {
            private MainWindow _mainWindow;
            public TaskHandler(MainWindow mainWindow)
            {
                _mainWindow = mainWindow;
                Keywords = new List<string> {
            "task", "remind", "remember", "todo",
            "add task", "create task", "set reminder"
        };
                ResponseColor = Colors.Orange;
                _mainWindow = mainWindow;
            }

            public override bool CanHandle(string input)
            {
                
                var taskTriggers = new[] { "task", "remind", "remember", "todo" };
                if (taskTriggers.Any(t => input.Contains(t, StringComparison.OrdinalIgnoreCase)))
                    return true;

                if (input.StartsWith("add ", StringComparison.OrdinalIgnoreCase) ||
                    input.StartsWith("set ", StringComparison.OrdinalIgnoreCase))
                    return true;

                return false;
            }

            public override void Handle(string input, string userName)
            {
                string taskTitle = ExtractTaskTitle(input);
                string timePhrase = ExtractTimePhrase(input);
                DateTime? reminderDate = ParseTimePhrase(timePhrase);

                var task = new CybersecurityTask
                {
                    Title = taskTitle,
                    Description = $"Created from chat: {input}",
                    HasReminder = reminderDate.HasValue,
                    ReminderDate = reminderDate
                };

                _mainWindow.Dispatcher.Invoke(() =>
                {
                    _mainWindow.tasks.Add(task);
                    _mainWindow.AddChatMessage("Cybersecurity Bot",
                        $"Task created: '{taskTitle}'" +
                        (reminderDate.HasValue ? $" (Reminder set for {reminderDate.Value:d})" : ""),
                        Colors.Green);

                    _mainWindow.AddToActivityLog($"Task created via chat: {taskTitle}");
                });
            }

            private string ExtractTaskTitle(string input)
            {
                
                if (input.Contains("add task") || input.Contains("create task"))
                    return input.Split(new[] { "add task", "create task" }, StringSplitOptions.RemoveEmptyEntries)
                               .Last()
                               .Trim();

                if (input.Contains("remind me to"))
                    return input.Split(new[] { "remind me to" }, StringSplitOptions.RemoveEmptyEntries)
                               .Last()
                               .Split(new[] { "tomorrow", "next week", "in" }, StringSplitOptions.RemoveEmptyEntries)
                               .First()
                               .Trim();

                return "Security task";
            }

            private string ExtractTimePhrase(string input)
            {
                var timeWords = new Dictionary<string, string>
        {
            {"tomorrow", "tomorrow"},
            {"next week", "next week"},
            {"in 3 days", "in 3 days"},
            {"today", "today"},
            {"next month", "next month"},
            {"in a week", "in 7 days"}
        };

                foreach (var word in timeWords)
                {
                    if (input.Contains(word.Key, StringComparison.OrdinalIgnoreCase))
                        return word.Value;
                }
                return "";
            }

            private DateTime? ParseTimePhrase(string phrase)
            {
                if (string.IsNullOrEmpty(phrase)) return null;

                return phrase.ToLower() switch
                {
                    "tomorrow" => DateTime.Now.AddDays(1),
                    "next week" => DateTime.Now.AddDays(7),
                    "in 3 days" => DateTime.Now.AddDays(3),
                    "today" => DateTime.Now,
                    "next month" => DateTime.Now.AddMonths(1),
                    "in 7 days" => DateTime.Now.AddDays(7),
                    _ => null
                };
            }
        }
    }
}