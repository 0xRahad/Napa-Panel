using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KeyAuth;

namespace Napa_Panel
{
    public partial class LoginScreen : Form
    {
        private const int ParticleCount = 100;
        private const int DrawCount = 90; // Number of particles to draw
        private readonly Random _random = new Random();
        private readonly PointF[] _particlePositions = new PointF[ParticleCount];
        private readonly PointF[] _particleTargetPositions = new PointF[ParticleCount];
        private readonly float[] _particleSpeeds = new float[ParticleCount];
        private readonly float[] _particleSizes = new float[ParticleCount];
        private readonly float[] _particleRadii = new float[ParticleCount];
        private readonly float[] _particleRotations = new float[ParticleCount];
        private readonly PointF[] _vertices = new PointF[3]; // Reuse vertices array
        public LoginScreen()
        {
           
            InitializeComponent();
            DoubleBuffered = true;
            KeyAuthApp.init();

            InitializeParticles();
            Timer timer = new Timer
            {
                Interval = 3 // Roughly 60 FPS
            };
            timer.Tick += (sender, args) =>
            {
                UpdateParticles();
                Invalidate(); // Causes the form to be redrawn
            };
            timer.Start();

        }

        public static api KeyAuthApp = new api(
            name: "Napa Panel",
            ownerid: "N9gP1OMIcd",
            version: "1.0",
            secret: "82a270f6abd2ff1acf278f223b4db56039b7880f9568413d9ec201cdd660d84c");

        private PointF Lerp(PointF start, PointF end, float t)
        {
            return new PointF(start.X + (end.X - start.X) * t, start.Y + (end.Y - start.Y) * t);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            for (int i = 0; i < DrawCount; i++)
            {
                DrawTriangleWithGlow(e.Graphics, _particlePositions[i], _particleSizes[i], _particleRotations[i]);
            }
        }

        private void DrawTriangleWithGlow(Graphics graphics, PointF position, float size, float rotation)
        {
            float angle = (float)(Math.PI * 2 / 3); // 120 degrees for equilateral triangle
            PointF[] vertices = new PointF[3];

            for (int i = 0; i < 3; i++)
            {
                vertices[i] = new PointF(
                    position.X + size * (float)Math.Cos(rotation + i * angle),
                    position.Y + size * (float)Math.Sin(rotation + i * angle)
                );
            }

            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw glow effect
            int maxGlowLayers = 10;
            for (int j = 0; j < maxGlowLayers; j++)
            {
                int alpha = 25 - 2 * j; // Gradually decrease alpha for each layer
                using (Brush glowBrush = new SolidBrush(Color.FromArgb(alpha, 52, 80, 255))) // Semi-transparent red
                {
                    float glowSize = size + j * 2; // Gradually increase the glow size
                    graphics.FillEllipse(glowBrush, position.X - glowSize / 2, position.Y - glowSize / 2, glowSize, glowSize);
                }
            }

            // Draw triangle
            using (Brush brush = new SolidBrush(Color.FromArgb(52, 80, 255))) // Solid red color for the triangle
            {
                graphics.FillPolygon(brush, vertices);
            }
        }

        private void UpdateParticles()
        {
            Size screenSize = Screen.PrimaryScreen.Bounds.Size;
            for (int i = 0; i < ParticleCount; i++)
            {
                if (_particlePositions[i].X == 0 || _particlePositions[i].Y == 0)
                {
                    _particlePositions[i] = new PointF(_random.Next(screenSize.Width + 1), 15f);
                    _particleSpeeds[i] = 1 + _random.Next(25);
                    _particleRadii[i] = _random.Next(4);
                    _particleSizes[i] = _random.Next(8);
                    _particleTargetPositions[i] = new PointF(_random.Next(screenSize.Width), screenSize.Height * 2);
                }

                float deltaTime = 2.0f / 60; // Assuming 60 FPS
                _particlePositions[i] = Lerp(_particlePositions[i], _particleTargetPositions[i], deltaTime * (_particleSpeeds[i] / 60));
                _particleRotations[i] += deltaTime;

                if (_particlePositions[i].Y > screenSize.Height)
                {
                    _particlePositions[i] = new PointF(0, 0);
                    _particleRotations[i] = 0;
                }
            }
        }


        private void InitializeParticles()
        {
            Size screenSize = Screen.PrimaryScreen.Bounds.Size;
            for (int i = 0; i < ParticleCount; i++)
            {
                _particlePositions[i] = new PointF(0, 0);
                _particleTargetPositions[i] = new PointF(_random.Next(screenSize.Width), screenSize.Height * 2);
                _particleSpeeds[i] = 1 + _random.Next(25);
                _particleSizes[i] = _random.Next(8);
                _particleRadii[i] = _random.Next(4);
                _particleRotations[i] = 0;
            }
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            string username = loginUser.Text.Trim();
            string password = loginPassword.Text.Trim();

            // Validate inputs
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                authStatus.Text = "Please enter username and password.";
                SystemSounds.Exclamation.Play(); // Play error sound
                return; // Stop further execution
            }

            // Call authentication method
            KeyAuthApp.login(username, password);

            // Check authentication response
            if (KeyAuthApp.response.success)
            {
                authStatus.Text = KeyAuthApp.response.message;
                SystemSounds.Asterisk.Play(); // Play success sound
                timer1.Start();
            
                
            }
            else
            {
                authStatus.Text = KeyAuthApp.response.message;
                SystemSounds.Exclamation.Play(); // Play error sound
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.progressBar.Increment(10);
            if(progressBar.Value == progressBar.Maximum)
            {
              
                new Home().Show();
                this.Hide();
                timer1.Stop();
            }
        }
    }
}