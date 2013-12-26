using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using FISCA.DSAUtil;
using SmartSchool.Common;
using FISCA.Presentation.Controls;
using SmartSchool;
using SHSchool.Data;

namespace �w�榨�Z�պ��
{
    public partial class SelectTypeForm : FISCA.Presentation.Controls.BaseForm
    {
        Campus.Configuration.ConfigData cd;
        private string _ConfigPrint;
        private string name = "�`���]�w";
        private BackgroundWorker _BGWAbsenceAndPeriodList;

        private List<string> typeList = new List<string>();
        private List<string> absenceList = new List<string>();

        public SelectTypeForm(string name)
        {
            InitializeComponent();

            _ConfigPrint = name; //�]�w�ɦW��

            _BGWAbsenceAndPeriodList = new BackgroundWorker();
            _BGWAbsenceAndPeriodList.DoWork += new DoWorkEventHandler(_BGWAbsenceAndPeriodList_DoWork);
            _BGWAbsenceAndPeriodList.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_BGWAbsenceAndPeriodList_RunWorkerCompleted);
            _BGWAbsenceAndPeriodList.RunWorkerAsync();
        }

        void _BGWAbsenceAndPeriodList_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            #region ��J�w�]�e��
            System.Windows.Forms.DataGridViewTextBoxColumn colName = new DataGridViewTextBoxColumn();
            colName.HeaderText = "�`������";
            colName.MinimumWidth = 70;
            colName.Name = "colName";
            colName.ReadOnly = true;
            colName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colName.Width = 70;
            this.dataGridViewX1.Columns.Add(colName);

            foreach (string absence in absenceList)
            {
                System.Windows.Forms.DataGridViewCheckBoxColumn newCol = new DataGridViewCheckBoxColumn();
                newCol.HeaderText = absence;
                newCol.Width = 55;
                newCol.ReadOnly = false;
                newCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
                newCol.Tag = absence;
                newCol.ValueType = typeof(bool);
                this.dataGridViewX1.Columns.Add(newCol);
            }
            foreach (string type in typeList)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridViewX1, type);
                row.Tag = type;
                dataGridViewX1.Rows.Add(row);
            }
            #endregion

            #region Ū���C�L�]�w Preference

            cd = Campus.Configuration.Config.User[_ConfigPrint];
            string StringLine = cd[name];
            if (!string.IsNullOrEmpty(StringLine))
            {
                XmlElement config = DSXmlHelper.LoadXml(StringLine);

                if (config != null)
                {
                    //�w���]�w�ɫh�N�]�w�ɤ��e��^�e���W
                    foreach (XmlElement type in config.SelectNodes("Type"))
                    {
                        string typeName = type.GetAttribute("Text");
                        foreach (DataGridViewRow row in dataGridViewX1.Rows)
                        {
                            if (typeName == ("" + row.Tag))
                            {
                                foreach (XmlElement absence in type.SelectNodes("Absence"))
                                {
                                    string absenceName = absence.GetAttribute("Text");
                                    foreach (DataGridViewCell cell in row.Cells)
                                    {
                                        if (cell.OwningColumn is DataGridViewCheckBoxColumn && ("" + cell.OwningColumn.Tag) == absenceName)
                                        {
                                            cell.Value = true;
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }

            #endregion
        }

        void _BGWAbsenceAndPeriodList_DoWork(object sender, DoWorkEventArgs e)
        {
            foreach (SHPeriodMappingInfo info in SHPeriodMapping.SelectAll())
            {
                if (!typeList.Contains(info.Type))
                    typeList.Add(info.Type);
            }

            foreach (SHAbsenceMappingInfo info in SHAbsenceMapping.SelectAll())
            {
                if (!absenceList.Contains(info.Name))
                    absenceList.Add(info.Name);
            }
        }

        internal bool CheckColumnNumber()
        {
            int limit = 253;
            int columnNumber = 0;
            int block = 9;

            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value is bool && ((bool)cell.Value))
                        columnNumber++;
                }
            }

            if (columnNumber * block > limit)
            {
                FISCA.Presentation.Controls.MsgBox.Show("�z�ҿ�ܪ����O�W�X Excel ���̤j���A�д�ֳ������O");
                return false;
            }
            else
                return true;
        }

        private void checkBoxX1_CheckedChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.ColumnIndex == 0)
                        continue;

                    if (checkBoxX1.Checked)
                    {
                        cell.Value = "true";
                    }
                    else
                    {
                        cell.Value = "false";
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //�e���ˬd
            if (!CheckColumnNumber())
                return;

            #region �x�s

            XmlElement config = new XmlDocument().CreateElement("AbsenceList");
            foreach (XmlElement var in config.SelectNodes("Type"))
                config.RemoveChild(var);
            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                bool needToAppend = false;
                XmlElement type = config.OwnerDocument.CreateElement("Type");
                type.SetAttribute("Text", "" + row.Tag);
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.ColumnIndex == 0)
                        continue;
                    XmlElement absence = config.OwnerDocument.CreateElement("Absence");
                    absence.SetAttribute("Text", "" + cell.OwningColumn.Tag);
                    bool te = false;
                    if (bool.TryParse("" + cell.Value, out te))
                    {
                        if (te)
                        {
                            needToAppend = true;
                            type.AppendChild(absence);
                        }
                    }
                }
                if (needToAppend)
                    config.AppendChild(type);
            }

            cd[name] = config.OuterXml;
            cd.Save();

            #endregion

            this.Close();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}