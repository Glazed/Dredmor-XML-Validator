namespace DredmorXmlCapitalizationFixer
{
	partial class Main
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if ( disposing && ( components != null ) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.inputPath = new System.Windows.Forms.TextBox();
			this.browseForInput = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.outputPath = new System.Windows.Forms.TextBox();
			this.browseForOutput = new System.Windows.Forms.Button();
			this.go = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(56, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "InputPath:";
			// 
			// inputPath
			// 
			this.inputPath.Location = new System.Drawing.Point(13, 26);
			this.inputPath.Name = "inputPath";
			this.inputPath.Size = new System.Drawing.Size(509, 20);
			this.inputPath.TabIndex = 1;
			// 
			// browseForInput
			// 
			this.browseForInput.Location = new System.Drawing.Point(528, 26);
			this.browseForInput.Name = "browseForInput";
			this.browseForInput.Size = new System.Drawing.Size(65, 20);
			this.browseForInput.TabIndex = 3;
			this.browseForInput.Text = "Browse...";
			this.browseForInput.UseVisualStyleBackColor = true;
			this.browseForInput.Click += new System.EventHandler(this.browseForInput_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(10, 64);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(67, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Output Path:";
			// 
			// outputPath
			// 
			this.outputPath.Location = new System.Drawing.Point(12, 80);
			this.outputPath.Name = "outputPath";
			this.outputPath.Size = new System.Drawing.Size(509, 20);
			this.outputPath.TabIndex = 5;
			// 
			// browseForOutput
			// 
			this.browseForOutput.Location = new System.Drawing.Point(527, 79);
			this.browseForOutput.Name = "browseForOutput";
			this.browseForOutput.Size = new System.Drawing.Size(65, 20);
			this.browseForOutput.TabIndex = 6;
			this.browseForOutput.Text = "Browse...";
			this.browseForOutput.UseVisualStyleBackColor = true;
			this.browseForOutput.Click += new System.EventHandler(this.browseForOutput_Click);
			// 
			// go
			// 
			this.go.Location = new System.Drawing.Point(12, 106);
			this.go.Name = "go";
			this.go.Size = new System.Drawing.Size(65, 23);
			this.go.TabIndex = 7;
			this.go.Text = "Go";
			this.go.UseVisualStyleBackColor = true;
			this.go.Click += new System.EventHandler(this.go_Click);
			// 
			// Main
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(601, 142);
			this.Controls.Add(this.go);
			this.Controls.Add(this.browseForOutput);
			this.Controls.Add(this.outputPath);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.browseForInput);
			this.Controls.Add(this.inputPath);
			this.Controls.Add(this.label1);
			this.Name = "Main";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Dredmor Capitalization Fixer";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.TextBox inputPath;
		private System.Windows.Forms.Button browseForInput;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox outputPath;
		private System.Windows.Forms.Button browseForOutput;
		private System.Windows.Forms.Button go;
	}
}

