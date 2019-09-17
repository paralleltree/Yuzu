namespace Yuzu.UI.Forms
{
    partial class SoundSourceSelector
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.fileBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.offsetBox = new System.Windows.Forms.NumericUpDown();
            this.browseButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.offsetBox)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "音源ファイル";
            // 
            // fileBox
            // 
            this.fileBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileBox.Location = new System.Drawing.Point(18, 30);
            this.fileBox.Name = "fileBox";
            this.fileBox.Size = new System.Drawing.Size(237, 19);
            this.fileBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "オフセット [s]";
            // 
            // offsetBox
            // 
            this.offsetBox.DecimalPlaces = 3;
            this.offsetBox.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.offsetBox.Location = new System.Drawing.Point(90, 68);
            this.offsetBox.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.offsetBox.Minimum = new decimal(new int[] {
            60,
            0,
            0,
            -2147483648});
            this.offsetBox.Name = "offsetBox";
            this.offsetBox.Size = new System.Drawing.Size(83, 19);
            this.offsetBox.TabIndex = 4;
            // 
            // browseButton
            // 
            this.browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseButton.Location = new System.Drawing.Point(261, 28);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(75, 23);
            this.browseButton.TabIndex = 3;
            this.browseButton.Text = "参照";
            this.browseButton.UseVisualStyleBackColor = true;
            // 
            // SoundSourceSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.offsetBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.fileBox);
            this.Controls.Add(this.label1);
            this.Name = "SoundSourceSelector";
            this.Size = new System.Drawing.Size(350, 99);
            ((System.ComponentModel.ISupportInitialize)(this.offsetBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox fileBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown offsetBox;
        private System.Windows.Forms.Button browseButton;
    }
}
