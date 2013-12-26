using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using FISCA.DSAUtil;
using DevComponents.DotNetBar;
using Aspose.Cells;
using System.Xml;
using System.IO;
using System.Diagnostics;
using SmartSchool.Common;
using K12.Data;
using SHSchool.Behavior.StudentExtendControls.Reports.�ǥͼ��y����;
using Framework.Feature;

namespace SHSchool.Behavior.ClassExtendControls
{
    public partial class MeritStatistic : UserControl, IDeXingExport
    {
        private string[] _classList;

        public MeritStatistic(string[] classList)
        {
            _classList = classList;
            InitializeComponent();
        }

        #region IDeXingExport ����

        public Control MainControl
        {
            get { return this.panel1; }
        }

        public void LoadData()
        {
            cboSemester.Items.Add("1");
            cboSemester.Items.Add("2");
            cboSemester.SelectedIndex = int.Parse(School.DefaultSemester) - 1;

            int schoolYear = int.Parse(School.DefaultSchoolYear);
            for (int i = schoolYear; i > schoolYear - 4; i--)
            {
                cboSchoolYear.Items.Add(i);
            }
            if (cboSchoolYear.Items.Count > 0)
                cboSchoolYear.SelectedIndex = 0;


            // rbType1.Text = rbType1.Text.Replace("@@", _schoolYear).Replace("!!", _semester);
            rbType1.Checked = true;
        }

        public void Export()
        {
            if (!IsValid()) return;

            // ���o�����h
            DSResponse d = Config.GetMDReduce();
            if (!d.HasContent)
            {
                MsgBox.Show("���o���g����W�h����:" + d.GetFault().Message, "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DSXmlHelper h = d.GetContent();
            int ab = int.Parse(h.GetText("Merit/AB"));
            int bc = int.Parse(h.GetText("Merit/BC"));
            int wa = int.Parse(txtA.Tag.ToString());
            int wb = int.Parse(txtB.Tag.ToString());
            int wc = int.Parse(txtC.Tag.ToString());
            int want = (wa * ab * bc) + (wb * bc) + wc;

            List<string> _studentIDList = new List<string>();
            Workbook book = new Workbook();
            Worksheet sheet = book.Worksheets[0];
            string schoolName = School.ChineseName;
            string A1Name = "";

            if (rbType1.Checked)
            {
                DSXmlHelper helper = new DSXmlHelper("Request");
                helper.AddElement("Condition");
                foreach (string classid in _classList)
                    helper.AddElement("Condition", "ClassID", classid);

                helper.AddElement("Condition", "SchoolYear", cboSchoolYear.SelectedItem.ToString());
                helper.AddElement("Condition", "Semester", cboSemester.SelectedItem.ToString());
                helper.AddElement("Order");
                helper.AddElement("Order", "GradeYear", "ASC");
                helper.AddElement("Order", "DisplayOrder", "ASC");
                helper.AddElement("Order", "ClassName", "ASC");
                helper.AddElement("Order", "SeatNo", "ASC");
                helper.AddElement("Order", "Name", "ASC");

                DSResponse dsrsp = GetResponse(new DSRequest(helper));
                if (!dsrsp.HasContent)
                {
                    MsgBox.Show("�d�ߵ��G����:" + dsrsp.GetFault().Message, "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                helper = dsrsp.GetContent();
                
                Cell A1 = sheet.Cells["A1"];
                A1.Style.Borders.SetColor(Color.Black);

                A1Name = schoolName + " (" + cboSchoolYear.SelectedItem.ToString() +
                    "/" + cboSemester.SelectedItem.ToString() + ") ���y�S���{";

                sheet.Name = "���y�S���{";
                A1.PutValue(A1Name);
                A1.Style.HorizontalAlignment = TextAlignmentType.Center;
                sheet.Cells.Merge(0, 0, 1, 7);

                FormatCell(sheet.Cells["A2"], "�Z��");
                FormatCell(sheet.Cells["B2"], "�y��");
                FormatCell(sheet.Cells["C2"], "�m�W");
                FormatCell(sheet.Cells["D2"], "�Ǹ�");
                FormatCell(sheet.Cells["E2"], "�j�\");
                FormatCell(sheet.Cells["F2"], "�p�\");
                FormatCell(sheet.Cells["G2"], "�ż�");
                int index = 1;
                foreach (XmlElement e in helper.GetElements("Student"))
                {
                    string da = e.SelectSingleNode("MeritA").InnerText;
                    string db = e.SelectSingleNode("MeritB").InnerText;
                    string dc = e.SelectSingleNode("MeritC").InnerText;

                    int a, b, c, total;
                    if (!int.TryParse(da, out a)) a = 0;
                    if (!int.TryParse(db, out b)) b = 0;
                    if (!int.TryParse(dc, out c)) c = 0;
                    total = (a * ab * bc) + (b * bc) + c;
                    if (total < want) continue;

                    _studentIDList.Add(e.GetAttribute("StudentID"));

                    int rowIndex = index + 2;
                    FormatCell(sheet.Cells["A" + rowIndex], e.SelectSingleNode("ClassName").InnerText);
                    FormatCell(sheet.Cells["B" + rowIndex], e.SelectSingleNode("SeatNo").InnerText);
                    FormatCell(sheet.Cells["C" + rowIndex], e.SelectSingleNode("Name").InnerText);
                    FormatCell(sheet.Cells["D" + rowIndex], e.SelectSingleNode("StudentNumber").InnerText);

                    FormatCell(sheet.Cells["E" + rowIndex], e.SelectSingleNode("MeritA").InnerText);
                    FormatCell(sheet.Cells["F" + rowIndex], e.SelectSingleNode("MeritB").InnerText);
                    FormatCell(sheet.Cells["G" + rowIndex], e.SelectSingleNode("MeritC").InnerText);
                    index++;
                }
            }
            else // �Y�έp�֭p�ɪ��B�z
            {
                DSXmlHelper helper = new DSXmlHelper("Request");
                helper.AddElement("Condition");
                foreach (string classid in _classList)
                    helper.AddElement("Condition", "ClassID", classid);
                helper.AddElement("Order");
                helper.AddElement("Order", "GradeYear", "ASC");
                helper.AddElement("Order", "DisplayOrder", "ASC");
                helper.AddElement("Order", "ClassName", "ASC");
                helper.AddElement("Order", "SeatNo", "ASC");
                helper.AddElement("Order", "Name", "ASC");
                DSResponse dsrsp = GetResponse(new DSRequest(helper));
                if (!dsrsp.HasContent)
                {
                    MsgBox.Show("�d�ߵ��G����:" + dsrsp.GetFault().Message, "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                helper = dsrsp.GetContent();
                               
                Cell A1 = sheet.Cells["A1"];
                A1.Style.Borders.SetColor(Color.Black);

                A1Name = schoolName + "  ���y�֭p�@�ǥͲM��";

                sheet.Name = "���y�S���{";
                A1.PutValue(A1Name);
                A1.Style.HorizontalAlignment = TextAlignmentType.Center;
                sheet.Cells.Merge(0, 0, 1, 7);

                FormatCell(sheet.Cells["A2"], "�Z��");
                FormatCell(sheet.Cells["B2"], "�y��");
                FormatCell(sheet.Cells["C2"], "�m�W");
                FormatCell(sheet.Cells["D2"], "�Ǹ�");
                FormatCell(sheet.Cells["E2"], "�j�\");
                FormatCell(sheet.Cells["F2"], "�p�\");
                FormatCell(sheet.Cells["G2"], "�ż�");

                int index = 3;
                foreach (XmlElement e in helper.GetElements("Student"))
                {
                    _studentIDList.Add(e.GetAttribute("StudentID"));
                    string da = e.SelectSingleNode("MeritA").InnerText;
                    string db = e.SelectSingleNode("MeritB").InnerText;
                    string dc = e.SelectSingleNode("MeritC").InnerText;

                    int a, b, c, total;
                    if (!int.TryParse(da, out a)) a = 0;
                    if (!int.TryParse(db, out b)) b = 0;
                    if (!int.TryParse(dc, out c)) c = 0;
                    total = (a * ab * bc) + (b * bc) + c;
                    if (total < want) continue;

                    FormatCell(sheet.Cells["A" + index], e.SelectSingleNode("ClassName").InnerText);
                    FormatCell(sheet.Cells["B" + index], e.SelectSingleNode("SeatNo").InnerText);
                    FormatCell(sheet.Cells["C" + index], e.SelectSingleNode("Name").InnerText);
                    FormatCell(sheet.Cells["D" + index], e.SelectSingleNode("StudentNumber").InnerText);
                    FormatCell(sheet.Cells["E" + index], e.SelectSingleNode("MeritA").InnerText);
                    FormatCell(sheet.Cells["F" + index], e.SelectSingleNode("MeritB").InnerText);
                    FormatCell(sheet.Cells["G" + index], e.SelectSingleNode("MeritC").InnerText);
                    index++;
                }
            }

            h = new DSXmlHelper("Request");
            h.AddElement("Field");
            h.AddElement("Field", "All");
            h.AddElement("Condition");            
            h.AddElement("Condition", "MeritFlag", "1");
            h.AddElement("Condition", "RefStudentID", "-1");

            foreach (string sid in _studentIDList)
                h.AddElement("Condition", "RefStudentID", sid);
            if (rbType1.Checked)
            {
                h.AddElement("Condition", "SchoolYear", cboSchoolYear.Text);
                h.AddElement("Condition", "Semester", cboSemester.Text);
            }
            h.AddElement("Order");
            h.AddElement("Order", "GradeYear", "ASC");
            h.AddElement("Order", "ClassDisplayOrder", "ASC");
            h.AddElement("Order", "ClassName", "ASC");
            h.AddElement("Order", "SeatNo", "ASC");            
            h.AddElement("Order", "RefStudentID", "ASC");
            h.AddElement("Order", "OccurDate", "ASC");

            d = QueryDiscipline.GetDiscipline(new DSRequest(h));
            if (!d.HasContent)
            {
                MsgBox.Show("���o���Ӹ�ƿ��~:" + d.GetFault().Message, "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            h = d.GetContent();
            book.Worksheets.Add();
            sheet = book.Worksheets[book.Worksheets.Count - 1];
            sheet.Name = schoolName + "���y�֭p����";
            Cell titleCell = sheet.Cells["A1"];
            titleCell.Style.Borders.SetColor(Color.Black);

            titleCell.PutValue(sheet.Name);
            titleCell.Style.HorizontalAlignment = TextAlignmentType.Center;
            sheet.Cells.Merge(0, 0, 1, 7);

            FormatCell(sheet.Cells["A2"], "�Z��");
            FormatCell(sheet.Cells["B2"], "�y��");
            FormatCell(sheet.Cells["C2"], "�m�W");
            FormatCell(sheet.Cells["D2"], "�Ǹ�");
            FormatCell(sheet.Cells["E2"], "�Ǧ~��");
            FormatCell(sheet.Cells["F2"], "�Ǵ�");
            FormatCell(sheet.Cells["G2"], "���");
            FormatCell(sheet.Cells["H2"], "�j�\");
            FormatCell(sheet.Cells["I2"], "�p�\");
            FormatCell(sheet.Cells["J2"], "�ż�");            
            FormatCell(sheet.Cells["K2"], "�ƥ�");

            int ri = 3;
            foreach (XmlElement e in h.GetElements("Discipline"))
            {
                FormatCell(sheet.Cells["A" + ri], e.SelectSingleNode("ClassName").InnerText);
                FormatCell(sheet.Cells["B" + ri], e.SelectSingleNode("SeatNo").InnerText);
                FormatCell(sheet.Cells["C" + ri], e.SelectSingleNode("Name").InnerText);
                FormatCell(sheet.Cells["D" + ri], e.SelectSingleNode("StudentNumber").InnerText);
                FormatCell(sheet.Cells["E" + ri], e.SelectSingleNode("SchoolYear").InnerText);
                FormatCell(sheet.Cells["F" + ri], e.SelectSingleNode("Semester").InnerText);
                FormatCell(sheet.Cells["G" + ri], e.SelectSingleNode("OccurDate").InnerText);
                FormatCell(sheet.Cells["H" + ri], e.SelectSingleNode("Detail/Discipline/Merit/@A").InnerText);
                FormatCell(sheet.Cells["I" + ri], e.SelectSingleNode("Detail/Discipline/Merit/@B").InnerText);
                FormatCell(sheet.Cells["J" + ri], e.SelectSingleNode("Detail/Discipline/Merit/@C").InnerText);                
                FormatCell(sheet.Cells["K" + ri], e.SelectSingleNode("Reason").InnerText);
                ri++;
            }

            string path = Path.Combine(Application.StartupPath, "Reports");

            //�p�G�ؿ����s�b�h�إߡC
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            path = Path.Combine(path, book.Worksheets[0].Name + ".xls");
            int i = 1;
            while (true)
            {
                string newPath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + (i++) + Path.GetExtension(path);
                if (!File.Exists(newPath))
                {
                    path = newPath;
                    break;
                }
            }
            try
            {
                book.Save(path);
            }
            catch (Exception ex)
            {
                MsgBox.Show("�ɮ��x�s����:" + ex.Message, "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                Process.Start(path);
            }
            catch (Exception ex)
            {
                MsgBox.Show("�ɮ׶}�ҥ���:" + ex.Message, "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
        #endregion

        private DSResponse GetResponse(DSRequest request)
        {
            if (rbAll.Checked)
                return QueryDiscipline.GetMeritStatistic(request);
            if (rbNoDemert.Checked)
                return QueryDiscipline.GetMeritIgnoreDemerit(request);
            if (rbNoUnclearedDemert.Checked)
                return QueryDiscipline.GetMeritIgnoreUnclearedDemerit(request);
            return null;
        }

        private bool IsValid()
        {
            error.Tag = true;
            error.Clear();
            ValidInt(txtA, txtA, "������J�Ʀr");
            ValidInt(txtB, txtB, "������J�Ʀr");
            ValidInt(txtC, txtC, "������J�Ʀr");

            return (bool)error.Tag;
        }

        private void ValidInt(Control intControl, Control showErrorControl, string message)
        {
            int i = 0;
            intControl.Tag = "0";
            if (intControl.Text == string.Empty)
                return;

            if (!int.TryParse(intControl.Text, out i))
            {
                error.SetError(showErrorControl, message);
                error.Tag = false;                
            }
            intControl.Tag = intControl.Text;
        }

        private void FormatCell(Cell cell, string value)
        {
            cell.PutValue(value);
            cell.Style.Borders.SetStyle(CellBorderType.Hair);
            cell.Style.Borders.SetColor(Color.Black);
            cell.Style.Borders.DiagonalStyle = CellBorderType.None;
            cell.Style.HorizontalAlignment = TextAlignmentType.Center;
        }

        private void FormatCellWithStandard(Cell cell, string value, string standard)
        {
            int v = 0, s = 0;
            if (!int.TryParse(value, out v)) v = 0;
            if (!int.TryParse(standard, out s)) s = -1;
            if (v >= s && s != -1)
                cell.Style.Font.Color = Color.Red;
            cell.PutValue(value);
            cell.Style.Borders.SetStyle(CellBorderType.Hair);
            cell.Style.Borders.SetColor(Color.Black);
            cell.Style.Borders.DiagonalStyle = CellBorderType.None;
            cell.Style.HorizontalAlignment = TextAlignmentType.Center;
        }

        private string ConvertToValidName(string A1Name)
        {
            char[] invalids = Path.GetInvalidFileNameChars();

            string result = A1Name;
            foreach (char each in invalids)
                result = result.Replace(each, '_');

            return result;
        }

    }
}
