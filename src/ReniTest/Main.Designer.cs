namespace ReniTest
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
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView1.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeView1.Location = new System.Drawing.Point(13, 13);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(807, 549);
            this.treeView1.TabIndex = 0;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(832, 565);
            this.Controls.Add(this.treeView1);
            this.Name = "Main";
            this.Text = "Main";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeView1;


    }
}