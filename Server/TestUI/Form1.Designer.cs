namespace Exallon.TestUI
{
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxLogin = new System.Windows.Forms.TextBox();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageCatalogs = new System.Windows.Forms.TabPage();
            this.buttonGetCatalogs = new System.Windows.Forms.Button();
            this.tabPageCatalogItems = new System.Windows.Forms.TabPage();
            this.checkBoxGetAll = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxItemNumTo = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxItemNumFrom = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxParentId = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonGetCatalogItems = new System.Windows.Forms.Button();
            this.textBoxCatalogName = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.dataGridViewResults = new System.Windows.Forms.DataGridView();
            this.tabPageDocuments = new System.Windows.Forms.TabPage();
            this.buttonGetDocuments = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPageCatalogs.SuspendLayout();
            this.tabPageCatalogItems.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewResults)).BeginInit();
            this.tabPageDocuments.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Login:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(206, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Password:";
            // 
            // textBoxLogin
            // 
            this.textBoxLogin.Location = new System.Drawing.Point(50, 12);
            this.textBoxLogin.Name = "textBoxLogin";
            this.textBoxLogin.Size = new System.Drawing.Size(150, 20);
            this.textBoxLogin.TabIndex = 2;
            this.textBoxLogin.Text = "111";
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(268, 12);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(150, 20);
            this.textBoxPassword.TabIndex = 3;
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(424, 10);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(93, 23);
            this.buttonConnect.TabIndex = 4;
            this.buttonConnect.Text = "Подключиться";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageCatalogs);
            this.tabControl1.Controls.Add(this.tabPageCatalogItems);
            this.tabControl1.Controls.Add(this.tabPageDocuments);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabControl1.Enabled = false;
            this.tabControl1.Location = new System.Drawing.Point(5, 48);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(774, 112);
            this.tabControl1.TabIndex = 5;
            // 
            // tabPageCatalogs
            // 
            this.tabPageCatalogs.Controls.Add(this.buttonGetCatalogs);
            this.tabPageCatalogs.Location = new System.Drawing.Point(4, 22);
            this.tabPageCatalogs.Name = "tabPageCatalogs";
            this.tabPageCatalogs.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCatalogs.Size = new System.Drawing.Size(766, 86);
            this.tabPageCatalogs.TabIndex = 0;
            this.tabPageCatalogs.Text = "Справочники";
            this.tabPageCatalogs.UseVisualStyleBackColor = true;
            // 
            // buttonGetCatalogs
            // 
            this.buttonGetCatalogs.Location = new System.Drawing.Point(667, 57);
            this.buttonGetCatalogs.Name = "buttonGetCatalogs";
            this.buttonGetCatalogs.Size = new System.Drawing.Size(93, 23);
            this.buttonGetCatalogs.TabIndex = 5;
            this.buttonGetCatalogs.Text = "Обновить";
            this.buttonGetCatalogs.UseVisualStyleBackColor = true;
            this.buttonGetCatalogs.Click += new System.EventHandler(this.buttonGetCatalogs_Click);
            // 
            // tabPageCatalogItems
            // 
            this.tabPageCatalogItems.Controls.Add(this.checkBoxGetAll);
            this.tabPageCatalogItems.Controls.Add(this.label7);
            this.tabPageCatalogItems.Controls.Add(this.label6);
            this.tabPageCatalogItems.Controls.Add(this.textBoxItemNumTo);
            this.tabPageCatalogItems.Controls.Add(this.label5);
            this.tabPageCatalogItems.Controls.Add(this.textBoxItemNumFrom);
            this.tabPageCatalogItems.Controls.Add(this.label4);
            this.tabPageCatalogItems.Controls.Add(this.textBoxParentId);
            this.tabPageCatalogItems.Controls.Add(this.label3);
            this.tabPageCatalogItems.Controls.Add(this.buttonGetCatalogItems);
            this.tabPageCatalogItems.Controls.Add(this.textBoxCatalogName);
            this.tabPageCatalogItems.Location = new System.Drawing.Point(4, 22);
            this.tabPageCatalogItems.Name = "tabPageCatalogItems";
            this.tabPageCatalogItems.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCatalogItems.Size = new System.Drawing.Size(766, 86);
            this.tabPageCatalogItems.TabIndex = 1;
            this.tabPageCatalogItems.Text = "Элементы справочника";
            this.tabPageCatalogItems.UseVisualStyleBackColor = true;
            // 
            // checkBoxGetAll
            // 
            this.checkBoxGetAll.AutoSize = true;
            this.checkBoxGetAll.Checked = true;
            this.checkBoxGetAll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxGetAll.Location = new System.Drawing.Point(433, 60);
            this.checkBoxGetAll.Name = "checkBoxGetAll";
            this.checkBoxGetAll.Size = new System.Drawing.Size(142, 17);
            this.checkBoxGetAll.TabIndex = 14;
            this.checkBoxGetAll.Text = "Вернуть все элементы";
            this.checkBoxGetAll.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(301, 61);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(21, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "По";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(189, 61);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(14, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "С";
            // 
            // textBoxItemNumTo
            // 
            this.textBoxItemNumTo.Location = new System.Drawing.Point(328, 58);
            this.textBoxItemNumTo.Name = "textBoxItemNumTo";
            this.textBoxItemNumTo.Size = new System.Drawing.Size(86, 20);
            this.textBoxItemNumTo.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(4, 61);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(166, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Диапазон номеров элементов:";
            // 
            // textBoxItemNumFrom
            // 
            this.textBoxItemNumFrom.Location = new System.Drawing.Point(209, 58);
            this.textBoxItemNumFrom.Name = "textBoxItemNumFrom";
            this.textBoxItemNumFrom.Size = new System.Drawing.Size(86, 20);
            this.textBoxItemNumFrom.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 35);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(158, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "ИД родительского элемента:";
            // 
            // textBoxParentId
            // 
            this.textBoxParentId.Location = new System.Drawing.Point(176, 32);
            this.textBoxParentId.Name = "textBoxParentId";
            this.textBoxParentId.Size = new System.Drawing.Size(238, 20);
            this.textBoxParentId.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(154, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Наименование справочника:";
            // 
            // buttonGetCatalogItems
            // 
            this.buttonGetCatalogItems.Location = new System.Drawing.Point(667, 57);
            this.buttonGetCatalogItems.Name = "buttonGetCatalogItems";
            this.buttonGetCatalogItems.Size = new System.Drawing.Size(93, 23);
            this.buttonGetCatalogItems.TabIndex = 6;
            this.buttonGetCatalogItems.Text = "Обновить";
            this.buttonGetCatalogItems.UseVisualStyleBackColor = true;
            this.buttonGetCatalogItems.Click += new System.EventHandler(this.buttonGetCatalogItems_Click);
            // 
            // textBoxCatalogName
            // 
            this.textBoxCatalogName.Location = new System.Drawing.Point(176, 6);
            this.textBoxCatalogName.Name = "textBoxCatalogName";
            this.textBoxCatalogName.Size = new System.Drawing.Size(238, 20);
            this.textBoxCatalogName.TabIndex = 6;
            this.textBoxCatalogName.Text = "Номенклатура";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.textBoxPassword);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.buttonConnect);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.textBoxLogin);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(5, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(774, 43);
            this.panel1.TabIndex = 6;
            // 
            // dataGridViewResults
            // 
            this.dataGridViewResults.AllowUserToAddRows = false;
            this.dataGridViewResults.AllowUserToDeleteRows = false;
            this.dataGridViewResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewResults.Location = new System.Drawing.Point(5, 160);
            this.dataGridViewResults.Name = "dataGridViewResults";
            this.dataGridViewResults.ReadOnly = true;
            this.dataGridViewResults.Size = new System.Drawing.Size(774, 397);
            this.dataGridViewResults.TabIndex = 15;
            this.dataGridViewResults.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewResults_CellClick);
            // 
            // tabPageDocuments
            // 
            this.tabPageDocuments.Controls.Add(this.buttonGetDocuments);
            this.tabPageDocuments.Location = new System.Drawing.Point(4, 22);
            this.tabPageDocuments.Name = "tabPageDocuments";
            this.tabPageDocuments.Size = new System.Drawing.Size(766, 86);
            this.tabPageDocuments.TabIndex = 2;
            this.tabPageDocuments.Text = "Документы";
            this.tabPageDocuments.UseVisualStyleBackColor = true;
            // 
            // buttonGetDocuments
            // 
            this.buttonGetDocuments.Location = new System.Drawing.Point(659, 51);
            this.buttonGetDocuments.Name = "buttonGetDocuments";
            this.buttonGetDocuments.Size = new System.Drawing.Size(93, 23);
            this.buttonGetDocuments.TabIndex = 6;
            this.buttonGetDocuments.Text = "Обновить";
            this.buttonGetDocuments.UseVisualStyleBackColor = true;
            this.buttonGetDocuments.Click += new System.EventHandler(this.buttonGetDocuments_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.dataGridViewResults);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Тестовый клиент Exallon";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPageCatalogs.ResumeLayout(false);
            this.tabPageCatalogItems.ResumeLayout(false);
            this.tabPageCatalogItems.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewResults)).EndInit();
            this.tabPageDocuments.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxLogin;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageCatalogs;
        private System.Windows.Forms.TabPage tabPageCatalogItems;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonGetCatalogs;
        private System.Windows.Forms.Button buttonGetCatalogItems;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxCatalogName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxItemNumFrom;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxParentId;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxItemNumTo;
        private System.Windows.Forms.DataGridView dataGridViewResults;
        private System.Windows.Forms.CheckBox checkBoxGetAll;
        private System.Windows.Forms.TabPage tabPageDocuments;
        private System.Windows.Forms.Button buttonGetDocuments;
    }
}

