using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DbWizard
{
    public partial class GridView : UserControl
    {

        public GridView(DataTable table) : this("", table) { }

        public GridView(string headerText, DataTable table)
        {
            InitializeComponent();

            this.labHeader.Text = headerText;
            this.dgvData.DataSource = table;
        }

        public bool HeaderVisiable { get; set; }

        //绘制行号
        private void dgvData_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = sender as DataGridView;
            var rowIdx = (e.RowIndex + 1).ToString();

            var centerFormat = new StringFormat()
            {
                // right alignment might actually make more sense for numbers
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIdx, grid.Font, SystemBrushes.ControlText, headerBounds, centerFormat);
        }

        private void dgvData_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            return;
        }

        private void GridView_Load(object sender, EventArgs e)
        {
            if (this.HeaderVisiable)
            {
                this.pContainer.Visible = true;
            }
            else {
                this.pContainer.Visible = false;
            }
        }
    }
}
