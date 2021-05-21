namespace WtPUpdater
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.RefreshButton = new System.Windows.Forms.Button();
            this.ActionLog = new System.Windows.Forms.TextBox();
            this.VersionList = new System.Windows.Forms.ListBox();
            this.SaveButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // RefreshButton
            // 
            this.RefreshButton.Location = new System.Drawing.Point(13, 13);
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Size = new System.Drawing.Size(75, 23);
            this.RefreshButton.TabIndex = 0;
            this.RefreshButton.Text = "Refresh";
            this.RefreshButton.UseVisualStyleBackColor = true;
            this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            // 
            // ActionLog
            // 
            this.ActionLog.Location = new System.Drawing.Point(13, 43);
            this.ActionLog.Multiline = true;
            this.ActionLog.Name = "ActionLog";
            this.ActionLog.Size = new System.Drawing.Size(660, 395);
            this.ActionLog.TabIndex = 1;
            // 
            // VersionList
            // 
            this.VersionList.FormattingEnabled = true;
            this.VersionList.Location = new System.Drawing.Point(679, 43);
            this.VersionList.Name = "VersionList";
            this.VersionList.Size = new System.Drawing.Size(120, 394);
            this.VersionList.TabIndex = 2;
            this.VersionList.SelectedIndexChanged += new System.EventHandler(this.VersionList_SelectedIndexChanged);
            // 
            // SaveButton
            // 
            this.SaveButton.Enabled = false;
            this.SaveButton.Location = new System.Drawing.Point(94, 14);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(75, 23);
            this.SaveButton.TabIndex = 3;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(811, 450);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.VersionList);
            this.Controls.Add(this.ActionLog);
            this.Controls.Add(this.RefreshButton);
            this.Name = "MainForm";
            this.Text = "Downloader";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button RefreshButton;
        private System.Windows.Forms.TextBox ActionLog;
        private System.Windows.Forms.ListBox VersionList;
        private System.Windows.Forms.Button SaveButton;
    }
}

