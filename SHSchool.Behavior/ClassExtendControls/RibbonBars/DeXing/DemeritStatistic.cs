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
using Framework.Feature;
using SHSchool.Behavior.StudentExtendControls.Reports.�ǥͼ��y����;

namespace SHSchool.Behavior.ClassExtendControls
{
    public partial class DemeritStatistic : UserControl, IDeXingExport
    {
        private string[] _classList;
        public DemeritStatistic(string[] classList)
        {
            _classList = classList;
            InitializeComponent();
        }

        #region IDeXingExport ����
        public Control MainControl
        {
            get { return this.groupPanel1; }
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
            //�g��
            int DemeritAB = int.Parse(h.GetText("Demerit/AB"));
            int DemeritBC = int.Parse(h.GetText("Demerit/BC"));
            //���y by dylan(100/03/16)
            int MeritAB = int.Parse(h.GetText("Merit/AB"));
            int MeritBC = int.Parse(h.GetText("Merit/BC"));

            int wa = int.Parse(txtA.Tag.ToString());
            int wb = int.Parse(txtB.Tag.ToString());
            int wc = int.Parse(txtC.Tag.ToString());
            int want = (wa * DemeritAB * DemeritBC) + (wb * DemeritBC) + wc;

            List<string> _studentIDList = new List<string>();

            List<string> studentUbeIDList = new List<string>(); //�Q�C�L���ǥ�,�ΥH�L���Ӯɪ��P�_

            Workbook book = new Workbook();
            Worksheet sheet = book.Worksheets[0];
            string schoolName = School.ChineseName;
            string A1Name = "";

            string wantString = wa + "�j�L " + wb + " �p�L " + wc + " ĵ�i";
            if (rbType1.Checked)
            {
                #region ��@�Ǵ�
                DSXmlHelper helperRequest = new DSXmlHelper("Request");
                helperRequest.AddElement("Condition");
                foreach (string classid in _classList)
                    helperRequest.AddElement("Condition", "ClassID", classid);

                helperRequest.AddElement("Condition", "SchoolYear", cboSchoolYear.SelectedItem.ToString());
                helperRequest.AddElement("Condition", "Semester", cboSemester.SelectedItem.ToString());
                helperRequest.AddElement("Order");
                helperRequest.AddElement("Order", "GradeYear", "ASC");
                helperRequest.AddElement("Order", "DisplayOrder", "ASC");
                helperRequest.AddElement("Order", "ClassName", "ASC");
                helperRequest.AddElement("Order", "SeatNo", "ASC");
                helperRequest.AddElement("Order", "Name", "ASC");

                //���o�g�ٲέp
                DSResponse dsrsp = QueryDiscipline.GetDemeritStatistic(new DSRequest(helperRequest));
                if (!dsrsp.HasContent)
                {
                    MsgBox.Show("�d���g�ٲέp����:" + dsrsp.GetFault().Message, "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                DSXmlHelper helper = dsrsp.GetContent();

                //���o���y�έp
                DSResponse MeritDsrsp = QueryDiscipline.GetMeritStatistic(new DSRequest(helperRequest));
                if (!dsrsp.HasContent)
                {
                    MsgBox.Show("�d�߼��y�έp����:" + dsrsp.GetFault().Message, "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                DSXmlHelper MeritHelper = MeritDsrsp.GetContent();

                Cell A1 = sheet.Cells["A1"];
                A1.Style.Borders.SetColor(Color.Black);

                A1Name = schoolName + "  (" + cboSchoolYear.SelectedItem.ToString() +
                    "/" + cboSemester.SelectedItem.ToString() + ") �g�ٯS���{";

                sheet.Name = "�g�ٯS���{";
                A1.PutValue(A1Name);
                A1.Style.HorizontalAlignment = TextAlignmentType.Center;

                FormatCell(sheet.Cells["A2"], "�Z��");
                FormatCell(sheet.Cells["B2"], "�y��");
                FormatCell(sheet.Cells["C2"], "�m�W");
                FormatCell(sheet.Cells["D2"], "�Ǹ�");
                FormatCell(sheet.Cells["E2"], "�j�L");
                FormatCell(sheet.Cells["F2"], "�p�L");
                FormatCell(sheet.Cells["G2"], "ĵ�i");
                if (cbxIsMeritAndDemerit.Checked) //�i��\�L�۩�
                {
                    FormatCell(sheet.Cells["H2"], "�j�\");
                    FormatCell(sheet.Cells["I2"], "�p�\");
                    FormatCell(sheet.Cells["J2"], "�ż�");
                    FormatCell(sheet.Cells["K2"], "���(��)");
                    sheet.Cells.Merge(0, 0, 1, 11);
                }
                else
                {
                    FormatCell(sheet.Cells["H2"], "���(��)");
                    sheet.Cells.Merge(0, 0, 1, 8);
                }


                int index = 1;
                foreach (XmlElement e in helper.GetElements("Student"))
                {
                    _studentIDList.Add(e.GetAttribute("StudentID"));
                    //�g��
                    string Demeritda = e.SelectSingleNode("DemeritA").InnerText;
                    string Demeritdb = e.SelectSingleNode("DemeritB").InnerText;
                    string Demeritdc = e.SelectSingleNode("DemeritC").InnerText;
                    string Meritda = "0";
                    string Meritdb = "0";
                    string Meritdc = "0";
                    int a, b, c, DemeritTotal;
                    if (!int.TryParse(Demeritda, out a)) a = 0;
                    if (!int.TryParse(Demeritdb, out b)) b = 0;
                    if (!int.TryParse(Demeritdc, out c)) c = 0;
                    DemeritTotal = (a * DemeritAB * DemeritBC) + (b * DemeritBC) + c;

                    if (cbxIsMeritAndDemerit.Checked) //�i��\�L�۩�
                    {
                        foreach (XmlElement o in MeritHelper.GetElements("Student"))
                        {
                            if (e.GetAttribute("StudentID") != o.GetAttribute("StudentID"))
                                continue;

                            //���y
                            Meritda = o.SelectSingleNode("MeritA").InnerText;
                            Meritdb = o.SelectSingleNode("MeritB").InnerText;
                            Meritdc = o.SelectSingleNode("MeritC").InnerText;
                            int x, y, z, MeritTotal;
                            if (!int.TryParse(Meritda, out x)) x = 0;
                            if (!int.TryParse(Meritdb, out y)) y = 0;
                            if (!int.TryParse(Meritdc, out z)) z = 0;
                            MeritTotal = (x * DemeritAB * DemeritBC) + (y * DemeritBC) + z;
                            //�۴�
                            DemeritTotal -= MeritTotal;
                        }
                    }

                    //�[�`�p��ϥΪ̦ۭq�ƭ�
                    if (DemeritTotal < want || DemeritTotal == 0) continue;

                    studentUbeIDList.Add(e.GetAttribute("StudentID"));

                    int rowIndex = index + 2;
                    FormatCell(sheet.Cells["A" + rowIndex], e.SelectSingleNode("ClassName").InnerText);
                    FormatCell(sheet.Cells["B" + rowIndex], e.SelectSingleNode("SeatNo").InnerText);
                    FormatCell(sheet.Cells["C" + rowIndex], e.SelectSingleNode("Name").InnerText);
                    FormatCell(sheet.Cells["D" + rowIndex], e.SelectSingleNode("StudentNumber").InnerText);
                    FormatCell(sheet.Cells["E" + rowIndex], Demeritda);
                    FormatCell(sheet.Cells["F" + rowIndex], Demeritdb);
                    FormatCell(sheet.Cells["G" + rowIndex], Demeritdc);
                    if (cbxIsMeritAndDemerit.Checked) //�i��\�L�۩�
                    {
                        FormatCell(sheet.Cells["H" + rowIndex], Meritda);
                        FormatCell(sheet.Cells["I" + rowIndex], Meritdb);
                        FormatCell(sheet.Cells["J" + rowIndex], Meritdc);
                        FormatCell(sheet.Cells["K" + rowIndex], DemeritTotal.ToString());
                    }
                    else
                    {
                        FormatCell(sheet.Cells["H" + rowIndex], DemeritTotal.ToString());
                    }
                    index++;
                } 
                #endregion
            }
            else // �Y�έp�֭p�ɪ��B�z
            {
                #region �h�Ǵ�
                DSXmlHelper RequestHelper = new DSXmlHelper("Request");
                RequestHelper.AddElement("Condition");
                foreach (string classid in _classList)
                    RequestHelper.AddElement("Condition", "ClassID", classid);
                RequestHelper.AddElement("Order");
                RequestHelper.AddElement("Order", "GradeYear", "ASC");
                RequestHelper.AddElement("Order", "DisplayOrder", "ASC");
                RequestHelper.AddElement("Order", "ClassName", "ASC");
                RequestHelper.AddElement("Order", "SeatNo", "ASC");
                RequestHelper.AddElement("Order", "Name", "ASC");

                //���o�g�ٲέp
                DSResponse dsrsp = QueryDiscipline.GetDemeritStatistic(new DSRequest(RequestHelper));
                if (!dsrsp.HasContent)
                {
                    MsgBox.Show("�d���g�ٲέp����:" + dsrsp.GetFault().Message, "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                DSXmlHelper helper = dsrsp.GetContent();

                //���o���y�έp
                DSResponse MeritDsrsp = QueryDiscipline.GetMeritStatistic(new DSRequest(RequestHelper));
                if (!dsrsp.HasContent)
                {
                    MsgBox.Show("�d�߼��y�έp����:" + dsrsp.GetFault().Message, "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                DSXmlHelper MeritHelper = MeritDsrsp.GetContent();


                Cell A1 = sheet.Cells["A1"];
                A1.Style.Borders.SetColor(Color.Black);

                A1Name = schoolName + "�@�g�ٯS���{(�h�Ǵ�)";

                sheet.Name = "�g�ٯS���{";
                A1.PutValue(A1Name);
                A1.Style.HorizontalAlignment = TextAlignmentType.Center;

                FormatCell(sheet.Cells["A2"], "�Z��");
                FormatCell(sheet.Cells["B2"], "�y��");
                FormatCell(sheet.Cells["C2"], "�m�W");
                FormatCell(sheet.Cells["D2"], "�Ǹ�");
                FormatCell(sheet.Cells["E2"], "�j�L");
                FormatCell(sheet.Cells["F2"], "�p�L");
                FormatCell(sheet.Cells["G2"], "ĵ�i");
                if (cbxIsMeritAndDemerit.Checked) //�i��\�L�۩�
                {
                    FormatCell(sheet.Cells["H2"], "�j�\");
                    FormatCell(sheet.Cells["I2"], "�p�\");
                    FormatCell(sheet.Cells["J2"], "�ż�");
                    FormatCell(sheet.Cells["K2"], "���(��)");
                    sheet.Cells.Merge(0, 0, 1, 11);
                }
                else
                {
                    FormatCell(sheet.Cells["H2"], "���(��)");
                    sheet.Cells.Merge(0, 0, 1, 8);
                }

                int index = 3;
                foreach (XmlElement e in helper.GetElements("Student"))
                {
                    _studentIDList.Add(e.GetAttribute("StudentID"));

                    string da = e.SelectSingleNode("DemeritA").InnerText;
                    string db = e.SelectSingleNode("DemeritB").InnerText;
                    string dc = e.SelectSingleNode("DemeritC").InnerText;
                    string Meritda = "0";
                    string Meritdb = "0";
                    string Meritdc = "0";

                    int a, b, c, DemeritTotal;
                    if (!int.TryParse(da, out a)) a = 0;
                    if (!int.TryParse(db, out b)) b = 0;
                    if (!int.TryParse(dc, out c)) c = 0;
                    DemeritTotal = (a * DemeritAB * DemeritBC) + (b * DemeritBC) + c;

                    if (cbxIsMeritAndDemerit.Checked) //�i��\�L�۩�
                    {
                        foreach (XmlElement o in MeritHelper.GetElements("Student"))
                        {
                            if (e.GetAttribute("StudentID") != o.GetAttribute("StudentID"))
                                continue;

                            //���y
                            Meritda = o.SelectSingleNode("MeritA").InnerText;
                            Meritdb = o.SelectSingleNode("MeritB").InnerText;
                            Meritdc = o.SelectSingleNode("MeritC").InnerText;
                            int x, y, z, MeritTotal;
                            if (!int.TryParse(Meritda, out x)) x = 0;
                            if (!int.TryParse(Meritdb, out y)) y = 0;
                            if (!int.TryParse(Meritdc, out z)) z = 0;
                            MeritTotal = (x * DemeritAB * DemeritBC) + (y * DemeritBC) + z;
                            //�۴�
                            DemeritTotal -= MeritTotal;
                        }
                    }

                    //�i��P�_
                    if (DemeritTotal < want || DemeritTotal == 0) continue;

                    //�Q�C�L���ǥ�
                    studentUbeIDList.Add(e.GetAttribute("StudentID"));

                    FormatCell(sheet.Cells["A" + index], e.SelectSingleNode("ClassName").InnerText);
                    FormatCell(sheet.Cells["B" + index], e.SelectSingleNode("SeatNo").InnerText);
                    FormatCell(sheet.Cells["C" + index], e.SelectSingleNode("Name").InnerText);
                    FormatCell(sheet.Cells["D" + index], e.SelectSingleNode("StudentNumber").InnerText);
                    FormatCell(sheet.Cells["E" + index], e.SelectSingleNode("DemeritA").InnerText);
                    FormatCell(sheet.Cells["F" + index], e.SelectSingleNode("DemeritB").InnerText);
                    FormatCell(sheet.Cells["G" + index], e.SelectSingleNode("DemeritC").InnerText);
                    if (cbxIsMeritAndDemerit.Checked) //�i��\�L�۩�
                    {
                        FormatCell(sheet.Cells["H" + index], Meritda);
                        FormatCell(sheet.Cells["I" + index], Meritdb);
                        FormatCell(sheet.Cells["J" + index], Meritdc);
                        FormatCell(sheet.Cells["K" + index], DemeritTotal.ToString());
                    }
                    else
                    {
                        FormatCell(sheet.Cells["H" + index], DemeritTotal.ToString());
                    }
                    index++;
                }
                #endregion
            }

            #region ���o�g�٩���
            h = new DSXmlHelper("Request");
            h.AddElement("Field");
            h.AddElement("Field", "All");
            h.AddElement("Condition");
            h.AddElement("Condition", "Or");
            h.AddElement("Condition/Or", "MeritFlag", "0");
            h.AddElement("Condition/Or", "MeritFlag", "2");
            
            h.AddElement("Condition", "RefStudentID", "-1"); //�o�u�O����!!
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
            sheet.Name = "�g�٩���";
            Cell titleCell = sheet.Cells["A1"];
            titleCell.Style.Borders.SetColor(Color.Black);

            titleCell.PutValue(sheet.Name);
            titleCell.Style.HorizontalAlignment = TextAlignmentType.Center;
            sheet.Cells.Merge(0, 0, 1, 12);

            FormatCell(sheet.Cells["A2"], "�Z��");
            FormatCell(sheet.Cells["B2"], "�y��");
            FormatCell(sheet.Cells["C2"], "�m�W");
            FormatCell(sheet.Cells["D2"], "�Ǹ�");
            FormatCell(sheet.Cells["E2"], "�Ǧ~��");
            FormatCell(sheet.Cells["F2"], "�Ǵ�");
            FormatCell(sheet.Cells["G2"], "���");
            FormatCell(sheet.Cells["H2"], "�j�L");
            FormatCell(sheet.Cells["I2"], "�p�L");
            FormatCell(sheet.Cells["J2"], "ĵ�i");
            FormatCell(sheet.Cells["K2"], "�d��");
            FormatCell(sheet.Cells["L2"], "�ƥ�");

            int ri = 3;
            foreach (XmlElement e in h.GetElements("Discipline"))
            {
                if (!studentUbeIDList.Contains(e.SelectSingleNode("RefStudentID").InnerText))
                    continue;

                XmlElement xml = (XmlElement)e.SelectSingleNode("Detail/Discipline/Demerit");
                if (xml != null)
                {
                    if (xml.GetAttribute("Cleared") == "�O")
                        continue;
                }
                //if(e.SelectSingleNode("Demerit")GetAttribute("Cleared") == "�O")
                //    continue;

                FormatCell(sheet.Cells["A" + ri], e.SelectSingleNode("ClassName").InnerText);
                FormatCell(sheet.Cells["B" + ri], e.SelectSingleNode("SeatNo").InnerText);
                FormatCell(sheet.Cells["C" + ri], e.SelectSingleNode("Name").InnerText);
                FormatCell(sheet.Cells["D" + ri], e.SelectSingleNode("StudentNumber").InnerText);
                FormatCell(sheet.Cells["E" + ri], e.SelectSingleNode("SchoolYear").InnerText);
                FormatCell(sheet.Cells["F" + ri], e.SelectSingleNode("Semester").InnerText);
                FormatCell(sheet.Cells["G" + ri], e.SelectSingleNode("OccurDate").InnerText);
                FormatCell(sheet.Cells["H" + ri], e.SelectSingleNode("Detail/Discipline/Demerit/@A").InnerText);
                FormatCell(sheet.Cells["I" + ri], e.SelectSingleNode("Detail/Discipline/Demerit/@B").InnerText);
                FormatCell(sheet.Cells["J" + ri], e.SelectSingleNode("Detail/Discipline/Demerit/@C").InnerText);
                FormatCell(sheet.Cells["K" + ri], e.SelectSingleNode("MeritFlag").InnerText == "2" ? "�O" : "�_");
                FormatCell(sheet.Cells["L" + ri], e.SelectSingleNode("Reason").InnerText);
                ri++;
            } 
            #endregion

            if (cbxIsMeritAndDemerit.Checked)
            {
                #region ���y����

                h = new DSXmlHelper("Request");
                h.AddElement("Field");
                h.AddElement("Field", "All");
                h.AddElement("Condition");
                h.AddElement("Condition", "Or");
                h.AddElement("Condition/Or", "MeritFlag", "1");

                h.AddElement("Condition", "RefStudentID", "-1"); //�o�u�O����!!
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
                sheet.Name = "���y����";
                titleCell = sheet.Cells["A1"];
                titleCell.Style.Borders.SetColor(Color.Black);

                titleCell.PutValue(sheet.Name);
                titleCell.Style.HorizontalAlignment = TextAlignmentType.Center;
                sheet.Cells.Merge(0, 0, 1, 11);

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

                ri = 3;
                foreach (XmlElement e in h.GetElements("Discipline"))
                {
                    if (!studentUbeIDList.Contains(e.SelectSingleNode("RefStudentID").InnerText))
                        continue;

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
                #endregion
            }

            #region �C�L���
            foreach (Worksheet AutoFitsheet in book.Worksheets)
            {
                AutoFitsheet.AutoFitColumns();
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
            #endregion

        }

        #endregion

        private bool IsValid()
        {
            error.Clear();
            error.Tag = true;
            ValidInt(txtA, txtA, "������J�Ʀr");
            ValidInt(txtB, txtB, "������J�Ʀr");
            ValidInt(txtC, txtC, "������J�Ʀr");

            return bool.Parse(error.Tag.ToString());
        }

        private void ValidInt(Control intControl, Control showErrorControl, string message)
        {
            int i = 0;
            intControl.Tag = "0";
            if (intControl.Text == string.Empty) return;
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
