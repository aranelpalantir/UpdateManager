namespace SelfUpdaterDesktopSample
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnCheckUpdates = new Button();
            lblCurrentVersion = new Label();
            SuspendLayout();
            // 
            // btnCheckUpdates
            // 
            btnCheckUpdates.Location = new Point(2, 43);
            btnCheckUpdates.Name = "btnCheckUpdates";
            btnCheckUpdates.Size = new Size(224, 23);
            btnCheckUpdates.TabIndex = 0;
            btnCheckUpdates.Text = "Check Updates";
            btnCheckUpdates.UseVisualStyleBackColor = true;
            btnCheckUpdates.Click += btnCheckUpdates_Click;
            // 
            // lblCurrentVersion
            // 
            lblCurrentVersion.AutoSize = true;
            lblCurrentVersion.Location = new Point(105, 9);
            lblCurrentVersion.Name = "lblCurrentVersion";
            lblCurrentVersion.Size = new Size(22, 15);
            lblCurrentVersion.TabIndex = 1;
            lblCurrentVersion.Text = "---";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(228, 84);
            Controls.Add(lblCurrentVersion);
            Controls.Add(btnCheckUpdates);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button btnCheckUpdates;
        private Label lblCurrentVersion;
    }
}
