using System;

namespace View
{
    /// <summary>
    /// The core of the GUI for the game Snake. This partial class contains the declarations
    /// and initializations of GUI objects used in the form.
    /// </summary>
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.connectButton = new System.Windows.Forms.Button();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.nameLabel = new System.Windows.Forms.Label();
            this.serverTextBox = new System.Windows.Forms.TextBox();
            this.serverLabel = new System.Windows.Forms.Label();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.scoreboardLabel = new System.Windows.Forms.Label();
            this.drawingPanel1 = new View.DrawingPanel();
            this.writingPanel1 = new View.WritingPanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.splitContainer1.Panel1.Controls.Add(this.connectButton);
            this.splitContainer1.Panel1.Controls.Add(this.nameTextBox);
            this.splitContainer1.Panel1.Controls.Add(this.nameLabel);
            this.splitContainer1.Panel1.Controls.Add(this.serverTextBox);
            this.splitContainer1.Panel1.Controls.Add(this.serverLabel);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(864, 508);
            this.splitContainer1.SplitterDistance = 49;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 0;
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(595, 11);
            this.connectButton.Margin = new System.Windows.Forms.Padding(4);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(100, 28);
            this.connectButton.TabIndex = 4;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(424, 14);
            this.nameTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(132, 22);
            this.nameTextBox.TabIndex = 3;
            this.nameTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.sendConnectRequestOnEnterPress_KeyDown);
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nameLabel.Location = new System.Drawing.Point(343, 14);
            this.nameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(70, 25);
            this.nameLabel.TabIndex = 2;
            this.nameLabel.Text = "Name:";
            // 
            // serverTextBox
            // 
            this.serverTextBox.Location = new System.Drawing.Point(124, 12);
            this.serverTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.serverTextBox.Name = "serverTextBox";
            this.serverTextBox.Size = new System.Drawing.Size(209, 22);
            this.serverTextBox.TabIndex = 1;
            this.serverTextBox.Text = "localhost";
            // 
            // serverLabel
            // 
            this.serverLabel.AutoSize = true;
            this.serverLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.serverLabel.Location = new System.Drawing.Point(37, 11);
            this.serverLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.serverLabel.Name = "serverLabel";
            this.serverLabel.Size = new System.Drawing.Size(76, 25);
            this.serverLabel.TabIndex = 0;
            this.serverLabel.Text = "Server:";
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.IsSplitterFixed = true;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.drawingPanel1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.splitContainer2.Panel2.Controls.Add(this.writingPanel1);
            this.splitContainer2.Panel2.Controls.Add(this.scoreboardLabel);
            this.splitContainer2.Size = new System.Drawing.Size(864, 454);
            this.splitContainer2.SplitterDistance = 642;
            this.splitContainer2.SplitterWidth = 5;
            this.splitContainer2.TabIndex = 0;
            // 
            // scoreboardLabel
            // 
            this.scoreboardLabel.AutoSize = true;
            this.scoreboardLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, ((System.Drawing.FontStyle)(((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic) 
                | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.scoreboardLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.scoreboardLabel.Location = new System.Drawing.Point(54, 28);
            this.scoreboardLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.scoreboardLabel.Name = "scoreboardLabel";
            this.scoreboardLabel.Size = new System.Drawing.Size(161, 29);
            this.scoreboardLabel.TabIndex = 0;
            this.scoreboardLabel.Text = "Scoreboard:";
            // 
            // drawingPanel1
            // 
            this.drawingPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.drawingPanel1.Location = new System.Drawing.Point(0, 0);
            this.drawingPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.drawingPanel1.Name = "drawingPanel1";
            this.drawingPanel1.Size = new System.Drawing.Size(642, 454);
            this.drawingPanel1.TabIndex = 0;
            // 
            // writingPanel1
            // 
            this.writingPanel1.Location = new System.Drawing.Point(59, 80);
            this.writingPanel1.Name = "writingPanel1";
            this.writingPanel1.Size = new System.Drawing.Size(192, 331);
            this.writingPanel1.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(864, 508);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Snake 2.0";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ArrowKey_KeyDown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }


        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.TextBox serverTextBox;
        private System.Windows.Forms.Label serverLabel;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Label scoreboardLabel;
        private DrawingPanel drawingPanel1;
        private WritingPanel writingPanel1;
    }
}

