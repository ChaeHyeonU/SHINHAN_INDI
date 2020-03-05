namespace WindowsFormsApp4
{
    partial class select_code_form
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
            this.code_list = new System.Windows.Forms.ListBox();
            this.select_code_btn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // code_list
            // 
            this.code_list.FormattingEnabled = true;
            this.code_list.ItemHeight = 12;
            this.code_list.Location = new System.Drawing.Point(12, 12);
            this.code_list.Name = "code_list";
            this.code_list.Size = new System.Drawing.Size(263, 412);
            this.code_list.TabIndex = 0;
            // 
            // select_code_btn
            // 
            this.select_code_btn.Location = new System.Drawing.Point(12, 430);
            this.select_code_btn.Name = "select_code_btn";
            this.select_code_btn.Size = new System.Drawing.Size(263, 23);
            this.select_code_btn.TabIndex = 1;
            this.select_code_btn.Text = "선택";
            this.select_code_btn.UseVisualStyleBackColor = true;
            this.select_code_btn.Click += new System.EventHandler(this.select_code_btn_Click);
            // 
            // select_code_form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(287, 460);
            this.Controls.Add(this.select_code_btn);
            this.Controls.Add(this.code_list);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "select_code_form";
            this.Text = "종목선택";
            this.Load += new System.EventHandler(this.select_code_form_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox code_list;
        private System.Windows.Forms.Button select_code_btn;
    }
}