namespace WindowsFormsApp4
{
    partial class Form1
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.id_box = new System.Windows.Forms.TextBox();
            this.pw_box = new System.Windows.Forms.TextBox();
            this.id_label = new System.Windows.Forms.Label();
            this.pw_label = new System.Windows.Forms.Label();
            this.login_button = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.account_box = new System.Windows.Forms.TextBox();
            this.axGiExpertControl1 = new AxGIEXPERTCONTROLLib.AxGiExpertControl();
            ((System.ComponentModel.ISupportInitialize)(this.axGiExpertControl1)).BeginInit();
            this.SuspendLayout();
            // 
            // id_box
            // 
            this.id_box.Location = new System.Drawing.Point(107, 36);
            this.id_box.Name = "id_box";
            this.id_box.Size = new System.Drawing.Size(100, 21);
            this.id_box.TabIndex = 0;
            // 
            // pw_box
            // 
            this.pw_box.Location = new System.Drawing.Point(107, 72);
            this.pw_box.Name = "pw_box";
            this.pw_box.Size = new System.Drawing.Size(100, 21);
            this.pw_box.TabIndex = 1;
            // 
            // id_label
            // 
            this.id_label.AutoSize = true;
            this.id_label.Location = new System.Drawing.Point(51, 41);
            this.id_label.Name = "id_label";
            this.id_label.Size = new System.Drawing.Size(41, 12);
            this.id_label.TabIndex = 2;
            this.id_label.Text = "아이디";
            // 
            // pw_label
            // 
            this.pw_label.AutoSize = true;
            this.pw_label.Location = new System.Drawing.Point(51, 77);
            this.pw_label.Name = "pw_label";
            this.pw_label.Size = new System.Drawing.Size(53, 12);
            this.pw_label.TabIndex = 3;
            this.pw_label.Text = "비밀번호";
            // 
            // login_button
            // 
            this.login_button.Cursor = System.Windows.Forms.Cursors.Hand;
            this.login_button.Location = new System.Drawing.Point(232, 36);
            this.login_button.Name = "login_button";
            this.login_button.Size = new System.Drawing.Size(75, 23);
            this.login_button.TabIndex = 4;
            this.login_button.Text = "로그인";
            this.login_button.UseVisualStyleBackColor = true;
            this.login_button.Click += new System.EventHandler(this.loginbutton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 112);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 12);
            this.label1.TabIndex = 6;
            this.label1.Text = "공인인증비밀번호";
            // 
            // account_box
            // 
            this.account_box.Location = new System.Drawing.Point(107, 108);
            this.account_box.Name = "account_box";
            this.account_box.Size = new System.Drawing.Size(100, 21);
            this.account_box.TabIndex = 5;
            // 
            // axGiExpertControl1
            // 
            this.axGiExpertControl1.Enabled = true;
            this.axGiExpertControl1.Location = new System.Drawing.Point(1587, 736);
            this.axGiExpertControl1.Name = "axGiExpertControl1";
            this.axGiExpertControl1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axGiExpertControl1.OcxState")));
            this.axGiExpertControl1.Size = new System.Drawing.Size(136, 50);
            this.axGiExpertControl1.TabIndex = 7;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(361, 200);
            this.Controls.Add(this.axGiExpertControl1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.account_box);
            this.Controls.Add(this.login_button);
            this.Controls.Add(this.pw_label);
            this.Controls.Add(this.id_label);
            this.Controls.Add(this.pw_box);
            this.Controls.Add(this.id_box);
            this.Name = "Form1";
            this.Text = "x";
            ((System.ComponentModel.ISupportInitialize)(this.axGiExpertControl1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox id_box;
        private System.Windows.Forms.TextBox pw_box;
        private System.Windows.Forms.Label id_label;
        private System.Windows.Forms.Label pw_label;
        private System.Windows.Forms.Button login_button;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox account_box;
        private AxGIEXPERTCONTROLLib.AxGiExpertControl axGiExpertControl1;
    }
}

