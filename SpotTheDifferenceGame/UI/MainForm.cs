using System;
using System.Drawing;
using System.Windows.Forms;
using SpotTheDifferenceGame.Utils;
using SpotTheDifferenceGame.Logic;

namespace SpotTheDifferenceGame.UI
{
    public partial class MainForm : Form
    {
        // UI Controls
        private PictureBox pictureBoxLeft;
        private PictureBox pictureBoxRight;
        private Label labelStatus;
        private Label labelFound;
        private Label labelRemaining;
        private ComboBox comboMode;
        private ComboBox comboDifficulty;
        private Button buttonStart;

        // Game Logic
        private State gameState;
        private ModeManager modeManager;

        private readonly string basePath = @"..\..\Assets\Images\";

        public MainForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            Console.WriteLine(basePath);
            this.Text = "Spot the Difference Game";
            this.ClientSize = new Size(800, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.WhiteSmoke;

            Font labelFont = new Font("Segoe UI", 11, FontStyle.Bold);
            Font comboFont = new Font("Segoe UI", 10);

            Label titleLabel = new Label()
            {
                Text = "Spot the Difference",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.DarkSlateBlue,
                Location = new Point(280, 10),
                AutoSize = true
            };
            this.Controls.Add(titleLabel);

            pictureBoxLeft = new PictureBox()
            {
                Location = new Point(30, 60),
                Size = new Size(350, 467),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                Enabled = false
            };
            this.Controls.Add(pictureBoxLeft);

            pictureBoxRight = new PictureBox()
            {
                Location = new Point(410, 60),
                Size = new Size(350, 467),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                Enabled = false
            };
            this.Controls.Add(pictureBoxRight);

            labelStatus = new Label()
            {
                Location = new Point(30, 540),
                Size = new Size(730, 30),
                Text = "Select difficulty and press Start.",
                Font = labelFont
            };
            this.Controls.Add(labelStatus);

            labelFound = new Label()
            {
                Location = new Point(30, 570),
                Size = new Size(150, 25),
                Font = labelFont,
                Text = "Found: 0"
            };
            this.Controls.Add(labelFound);

            labelRemaining = new Label()
            {
                Location = new Point(200, 570),
                Size = new Size(250, 25),
                Font = labelFont,
                Text = "Remaining: 0"
            };
            this.Controls.Add(labelRemaining);

            Label diffLabel = new Label()
            {
                Text = "Difficulty:",
                Location = new Point(30, 610),
                Size = new Size(80, 25),
                Font = labelFont
            };
            this.Controls.Add(diffLabel);

            comboDifficulty = new ComboBox()
            {
                Location = new Point(120, 610),
                Size = new Size(140, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = comboFont
            };
            comboDifficulty.Items.AddRange(new string[] { "Easy", "Medium", "Hard" });
            comboDifficulty.SelectedIndex = 0;
            this.Controls.Add(comboDifficulty);

            Label modeLabel = new Label()
            {
                Text = "Game Mode:",
                Location = new Point(300, 610),
                Size = new Size(100, 25),
                Font = labelFont
            };
            this.Controls.Add(modeLabel);

            comboMode = new ComboBox()
            {
                Location = new Point(410, 610),
                Size = new Size(140, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = comboFont
            };
            comboMode.Items.AddRange(new string[] { "Timer", "Attempts" });
            comboMode.SelectedIndex = 0;
            this.Controls.Add(comboMode);

            buttonStart = new Button()
            {
                Location = new Point(580, 610),
                Size = new Size(150, 35),
                Text = "Start Game",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.MediumSlateBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            buttonStart.FlatAppearance.BorderSize = 0;
            buttonStart.Click += ButtonStart_Click;
            this.Controls.Add(buttonStart);

            // Click events
            pictureBoxLeft.MouseClick += PictureBox_Click;
            pictureBoxRight.MouseClick += PictureBox_Click;
        }

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            string difficulty = comboDifficulty.SelectedItem.ToString();
            string mode = comboMode.SelectedItem.ToString();

            gameState = new State(totalDifferences: 5, maxAttempts: 10, maxLevels: 6);
            modeManager = new ModeManager();

            pictureBoxLeft.Enabled = true;
            pictureBoxRight.Enabled = true;
            comboDifficulty.Enabled = false;
            comboMode.Enabled = false;
            buttonStart.Enabled = false;

            labelStatus.Text = $"Difficulty: {difficulty} | Level {gameState.CurrentLevel}";
            labelFound.Text = "Found: 0";

            if (mode == "Timer")
            {
                modeManager.StartTimer(60);
                labelRemaining.Text = "Time Left: 60 sec";

                modeManager.Tick += () =>
                {
                    labelRemaining.Text = $"Time Left: {modeManager.TimeLeft} sec";
                };

                modeManager.TimeUp += () =>
                {
                    MessageBox.Show("⏰ Time's up!");
                    EndGame();
                };
            }
            else
            {
                labelRemaining.Text = $"Attempts Left: {gameState.RemainingAttempts}";
            }

            LoadLevelImages(difficulty, gameState.CurrentLevel);
        }

        private void LoadLevelImages(string difficulty, int level)
        {
            var (left, right) = ImageHelper.LoadImagePair(basePath, difficulty, level);
            if (left == null || right == null)
            {
                MessageBox.Show("Missing images for this level.");
                EndGame();
                return;
            }

            pictureBoxLeft.Image?.Dispose();
            pictureBoxRight.Image?.Dispose();
            pictureBoxLeft.Image = left;
            pictureBoxRight.Image = right;

            labelFound.Text = "Found: 0";
        }

        private void PictureBox_Click(object sender, MouseEventArgs e)
        {
            if (pictureBoxLeft.Image == null || pictureBoxRight.Image == null)
                return;

            PictureBox clickedBox = sender as PictureBox;

            int x = (int)(e.X * ((float)clickedBox.Image.Width / clickedBox.Width));
            int y = (int)(e.Y * ((float)clickedBox.Image.Height / clickedBox.Height));
            Point clickPoint = new Point(x, y);

            // TEMP: simulate correct click randomly
            bool isCorrect = new Random().Next(2) == 0;

            clickedBox.Image = FeedbackDrawer.DrawCircle(clickedBox.Image, clickPoint, isCorrect);

            if (isCorrect)
            {
                bool levelComplete = gameState.RegisterCorrect();
                labelFound.Text = $"Found: {gameState.FoundDifferences}";

                if (levelComplete)
                {
                    modeManager?.StopTimer();
                    MessageBox.Show("✅ Level complete!");

                    if (!gameState.AdvanceLevel())
                    {
                        MessageBox.Show("🎉 You completed all levels!");
                        EndGame();
                        return;
                    }

                    LoadLevelImages(comboDifficulty.SelectedItem.ToString(), gameState.CurrentLevel);
                    labelStatus.Text = $"Difficulty: {comboDifficulty.SelectedItem} | Level {gameState.CurrentLevel}";

                    if (comboMode.SelectedItem.ToString() == "Timer")
                    {
                        modeManager.StartTimer(60);
                        labelRemaining.Text = "Time Left: 60 sec";
                    }
                    else
                    {
                        labelRemaining.Text = $"Attempts Left: {gameState.RemainingAttempts}";
                    }
                }
            }
            else
            {
                if (comboMode.SelectedItem.ToString() == "Attempts")
                {
                    bool outOfAttempts = gameState.RegisterWrong();
                    labelRemaining.Text = $"Attempts Left: {gameState.RemainingAttempts}";

                    if (outOfAttempts)
                    {
                        MessageBox.Show("❌ No attempts left!");
                        EndGame();
                    }
                }
            }
        }

        private void EndGame()
        {
            pictureBoxLeft.Enabled = false;
            pictureBoxRight.Enabled = false;
            buttonStart.Enabled = true;
            comboDifficulty.Enabled = true;
            comboMode.Enabled = true;
            labelStatus.Text = "Game over. Press Start to try again.";
            modeManager?.StopTimer();
        }
    }
}
