using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using FISCA.DSAUtil;
using System.Xml;
using Aspose.Cells;
using System.IO;
using DevComponents.DotNetBar;
using System.Diagnostics;
using SmartSchool.Common;
using K12.Data;
using SHSchool.Behavior.StudentExtendControls;

namespace SHSchool.Behavior.ClassExtendControls
{
    public partial class NoAbsenceStatistic : UserControl, IDeXingExport
    {
        private string[] _classidList;
        private string _schoolYear;
        private string _semester;

        public NoAbsenceStatistic(string[] classidList)
        {
            InitializeComponent();
            _classidList = classidList;
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

            //_schoolYear = SmartSchool.Common.CurrentUser.Instance.SchoolYear.ToString();
            //_semester = SmartSchool.Common.CurrentUser.Instance.Semester.ToString();

            //checkBoxX1.Text = checkBoxX1.Text.Replace("@@", _schoolYear).Replace("!!", _semester);
            checkBoxX1.Checked = true;
        }

        public void Export()
        {
            //���o�Z��ID
            List<string> ClassIDList = K12.Presentation.NLDPanels.Class.SelectedSource;
            List<string> �|�v�T���Ԫ����O = new List<string>();
            foreach (AbsenceMappingInfo each in AbsenceMapping.SelectAll())
            {
                if (!each.Noabsence)
                {
                    �|�v�T���Ԫ����O.Add(each.Name);
                }
            }

            //���o�Z�žǥ�
            List<StudentRecord> StudentList = Student.SelectByClassIDs(ClassIDList);

            //�Ҧ��ǥ�
            Dictionary<string, StudentRecord> StudentDic = new Dictionary<string, StudentRecord>();
            //�����Ծǥ�
            Dictionary<string, StudentRecord> noOK_StudentList = new Dictionary<string, StudentRecord>();
            //���Ԫ��ǥ�
            Dictionary<string, StudentRecord> OK_StudentList = new Dictionary<string, StudentRecord>();

            foreach (StudentRecord each in StudentList)
            {
                if (each.Status == StudentRecord.StudentStatus.�@�� || each.Status == StudentRecord.StudentStatus.����)
                {
                    if (!StudentDic.ContainsKey(each.ID))
                    {
                        StudentDic.Add(each.ID, each);
                    }
                }
            }

            //���o�ǥͩҦ������m���
            //�p�G�S�����m�O����
            //�N��O����
            List<AttendanceRecord> TestList = Attendance.SelectByStudentIDs(StudentDic.Keys);
            List<AttendanceRecord> AttendanceList = new List<AttendanceRecord>();

            if (checkBoxX1.Checked)
            {
                //�ۦP�Ǧ~��/�Ǵ�
                foreach (AttendanceRecord attendnace in TestList)
                {
                    int schoolYear = int.Parse(cboSchoolYear.SelectedItem.ToString());
                    int semester = int.Parse(cboSemester.SelectedItem.ToString());
                    if (attendnace.SchoolYear == schoolYear && attendnace.Semester == semester)
                    {
                        AttendanceList.Add(attendnace);
                    }
                }
            }
            else
            {
                AttendanceList.AddRange(TestList);
            }

            foreach (AttendanceRecord each in AttendanceList)
            {
                foreach (K12.Data.AttendancePeriod period in each.PeriodDetail)
                {
                    if (�|�v�T���Ԫ����O.Contains(period.AbsenceType))
                    {
                        if (!noOK_StudentList.ContainsKey(period.RefStudentID))
                        {
                            noOK_StudentList.Add(period.RefStudentID, StudentDic[period.RefStudentID]);
                        }
                    }
                }
            }

            foreach (StudentRecord each in StudentDic.Values)
            {
                if (!noOK_StudentList.ContainsKey(each.ID))
                {
                    if (!OK_StudentList.ContainsKey(each.ID))
                    {
                        OK_StudentList.Add(each.ID, each);
                    }
                }
            }
            List<StudentRecord> StudentRecordList = new List<StudentRecord>();
            foreach (StudentRecord each in OK_StudentList.Values)
            {
                StudentRecordList.Add(each);
            }

            StudentRecordList = SortClassIndex.K12Data_StudentRecord(StudentRecordList);

            Workbook book = new Workbook();
            Worksheet sheet = book.Worksheets[0];

            string schoolName = School.ChineseName;
            Cell A1 = sheet.Cells["A1"];
            A1.Style.Borders.SetColor(Color.Black);
            string A1Name = schoolName + "  ";
            if (checkBoxX1.Checked)
            {
                A1Name += "(" + cboSchoolYear.SelectedItem.ToString() + "/" + cboSemester.SelectedItem.ToString() + ") ";
            }

            A1Name += "���ԾǥͦW��";
            sheet.Name = "���ԾǥͦW��";
            A1.PutValue(A1Name);
            A1.Style.HorizontalAlignment = TextAlignmentType.Center;
            sheet.Cells.Merge(0, 0, 1, 5);

            FormatCell(sheet.Cells["A2"], "�s��");
            FormatCell(sheet.Cells["B2"], "�Z��");
            FormatCell(sheet.Cells["C2"], "�y��");
            FormatCell(sheet.Cells["D2"], "�m�W");
            FormatCell(sheet.Cells["E2"], "�Ǹ�");

            int index = 1;
            foreach (StudentRecord e in StudentRecordList)
            {
                int rowIndex = index + 2;
                FormatCell(sheet.Cells["A" + rowIndex], index.ToString());
                FormatCell(sheet.Cells["B" + rowIndex], string.IsNullOrEmpty(e.RefClassID) ? "" : e.Class.Name);
                FormatCell(sheet.Cells["C" + rowIndex], e.SeatNo.HasValue ? e.SeatNo.Value.ToString() : "");
                FormatCell(sheet.Cells["D" + rowIndex], e.Name);
                FormatCell(sheet.Cells["E" + rowIndex], e.StudentNumber);
                index++;
            }
            string path = Path.Combine(Application.StartupPath, "Reports");
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
            catch (IOException)
            {
                try
                {
                    FileInfo file = new FileInfo(path);
                    string nameTempalte = file.FullName.Replace(file.Extension, "") + "{0}.xls";
                    int count = 1;
                    string fileName = string.Format(nameTempalte, count);
                    while (File.Exists(fileName))
                        fileName = string.Format(nameTempalte, count++);

                    book.Save(fileName);
                    path = fileName;
                }
                catch (Exception ex)
                {
                    MsgBox.Show("�ɮ��x�s����:" + ex.Message, "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
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

        private void FormatCell(Cell cell, string value)
        {
            cell.PutValue(value);
            cell.Style.Borders.SetStyle(CellBorderType.Hair);
            cell.Style.Borders.SetColor(Color.Black);
            cell.Style.Borders.DiagonalStyle = CellBorderType.None;
            cell.Style.HorizontalAlignment = TextAlignmentType.Center;
        }

        #endregion


    }
}
