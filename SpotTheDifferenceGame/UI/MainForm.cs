// MainForm.cs
using System;
using System.Collections.Generic;
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
        private List<Difference> differences;
        private List<Difference> foundDifferences = new List<Difference>();
        private Bitmap originalLeft;
        private Bitmap originalRight;


        private readonly string basePath = @"D:\SpotTheDifferenceGame\SpotTheDifferenceGame\Assets\Images\";
        private bool debugMode = true; // Toggle to draw rectangles

        public MainForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
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
            pictureBoxRight = new PictureBox()
            {
                Location = new Point(410, 60),
                Size = new Size(350, 467),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                Enabled = false
            };
            this.Controls.Add(pictureBoxLeft);
            this.Controls.Add(pictureBoxRight);

            labelStatus = new Label()
            {
                Location = new Point(30, 540),
                Size = new Size(730, 30),
                Text = "Select difficulty and press Start.",
                Font = labelFont
            };
            labelFound = new Label()
            {
                Location = new Point(30, 570),
                Size = new Size(150, 25),
                Font = labelFont,
                Text = "Found: 0"
            };
            labelRemaining = new Label()
            {
                Location = new Point(200, 570),
                Size = new Size(250, 25),
                Font = labelFont,
                Text = "Remaining: 0"
            };
            this.Controls.Add(labelStatus);
            this.Controls.Add(labelFound);
            this.Controls.Add(labelRemaining);

            Label diffLabel = new Label()
            {
                Text = "Difficulty:",
                Location = new Point(30, 610),
                Size = new Size(80, 25),
                Font = labelFont
            };
            comboDifficulty = new ComboBox()
            {
                Location = new Point(120, 610),
                Size = new Size(140, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = comboFont
            };
            comboDifficulty.Items.AddRange(new string[] { "Easy", "Medium", "Hard" });
            comboDifficulty.SelectedIndex = 0;
            this.Controls.Add(diffLabel);
            this.Controls.Add(comboDifficulty);

            Label modeLabel = new Label()
            {
                Text = "Game Mode:",
                Location = new Point(300, 610),
                Size = new Size(100, 25),
                Font = labelFont
            };
            comboMode = new ComboBox()
            {
                Location = new Point(410, 610),
                Size = new Size(140, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = comboFont
            };
            comboMode.Items.AddRange(new string[] { "Timer", "Attempts" });
            comboMode.SelectedIndex = 0;
            this.Controls.Add(modeLabel);
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

            pictureBoxLeft.MouseClick += PictureBox_Click;
            pictureBoxRight.MouseClick += PictureBox_Click;
        }

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            string difficulty = comboDifficulty.SelectedItem.ToString();
            string mode = comboMode.SelectedItem.ToString();

            gameState = new State(totalDifferences: 0, maxAttempts: 10, maxLevels: 6);
            gameState.GameMode = mode;
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
                modeManager.Tick += () => labelRemaining.Text = $"Time Left: {modeManager.TimeLeft} sec";
                modeManager.TimeUp += () => { MessageBox.Show("⏰ Time's up!"); EndGame(); };
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
            pictureBoxLeft.Image = new Bitmap(left);
            pictureBoxRight.Image = new Bitmap(right);

            originalLeft = new Bitmap(left);
            originalRight = new Bitmap(right);


            
            var detector = new DifferenceDetector();
            differences = detector.GetDifferences(originalLeft, originalRight);
            foundDifferences.Clear();
            gameState.SetTotalDifferences(differences.Count);

            labelStatus.Text += $" | Differences detected: {differences.Count}";

            if (debugMode)
            {
                DrawDebugRectangles(pictureBoxLeft.Image, differences);
                DrawDebugRectangles(pictureBoxRight.Image, differences);
            }
        }

        private void DrawDebugRectangles(Image img, List<Difference> diffs)
        {
            using (Graphics g = Graphics.FromImage(img))
            using (Pen pen = new Pen(Color.Red, 3))
            {
                foreach (var diff in diffs)
                    g.DrawRectangle(pen, diff.BoundingBox);
            }
            pictureBoxLeft.Refresh();
            pictureBoxRight.Refresh();
        }

        private void PictureBox_Click(object sender, MouseEventArgs e)
        {
            if (pictureBoxLeft.Image == null || pictureBoxRight.Image == null)
                return;

            var clickedBox = (PictureBox)sender;
            int x = (int)(e.X * ((float)clickedBox.Image.Width / clickedBox.Width));
            int y = (int)(e.Y * ((float)clickedBox.Image.Height / clickedBox.Height));
            var clickPoint = new Point(x, y);

            bool isCorrect = false;
            Difference foundDiff = null;
            foreach (var diff in differences)
            {
                if (!foundDifferences.Contains(diff) && diff.IsNear(x, y))
                {
                    foundDifferences.Add(diff);
                    foundDiff = diff;
                    isCorrect = true;
                    break;
                }
            }

            clickedBox.Image = FeedbackDrawer.DrawCircle(clickedBox.Image, clickPoint, isCorrect);
            var otherBox = clickedBox == pictureBoxLeft ? pictureBoxRight : pictureBoxLeft;

            if (isCorrect && foundDiff != null)
            {
                otherBox.Image = FeedbackDrawer.DrawCircle(otherBox.Image, foundDiff.CenterPoint, true);
                SoundPlayerHelper.PlayCorrect();
                bool levelComplete = gameState.RegisterCorrect();
                labelFound.Text = $"Found: {foundDifferences.Count}";

                if (levelComplete || foundDifferences.Count == differences.Count)
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
                SoundPlayerHelper.PlayWrong();
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
            pictureBoxLeft.Enabled = pictureBoxRight.Enabled = false;
            buttonStart.Enabled = comboDifficulty.Enabled = comboMode.Enabled = true;
            labelStatus.Text = "Game over. Press Start to try again.";
            modeManager?.StopTimer();
        }
    }
}
