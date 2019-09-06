﻿namespace Yuzu.UI.Forms
{
    partial class VersionInfoForm
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
            this.pictureBoxIcon = new System.Windows.Forms.PictureBox();
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelVersion = new System.Windows.Forms.Label();
            this.labelProduct = new System.Windows.Forms.Label();
            this.buttonClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxIcon
            // 
            this.pictureBoxIcon.Location = new System.Drawing.Point(27, 27);
            this.pictureBoxIcon.Name = "pictureBoxIcon";
            this.pictureBoxIcon.Size = new System.Drawing.Size(36, 36);
            this.pictureBoxIcon.TabIndex = 0;
            this.pictureBoxIcon.TabStop = false;
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Location = new System.Drawing.Point(86, 17);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(35, 12);
            this.labelTitle.TabIndex = 1;
            this.labelTitle.Text = "label1";
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Location = new System.Drawing.Point(86, 39);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(35, 12);
            this.labelVersion.TabIndex = 2;
            this.labelVersion.Text = "label2";
            // 
            // labelProduct
            // 
            this.labelProduct.AutoSize = true;
            this.labelProduct.Location = new System.Drawing.Point(86, 61);
            this.labelProduct.Name = "labelProduct";
            this.labelProduct.Size = new System.Drawing.Size(35, 12);
            this.labelProduct.TabIndex = 3;
            this.labelProduct.Text = "label3";
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(297, 12);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 4;
            this.buttonClose.Text = "OK";
            this.buttonClose.UseVisualStyleBackColor = true;
            // 
            // VersionInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 93);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.labelProduct);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.pictureBoxIcon);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VersionInfoForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "バージョン情報";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxIcon;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelProduct;
        private System.Windows.Forms.Button buttonClose;
    }
}